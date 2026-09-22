using System.Threading.Channels;
using EsmatPlastic.Domain.Entities;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EsmatPlastic.API.Services;

public interface IDbSyncTrigger
{
    void TriggerSync();
}

public class DbSyncBackgroundService : BackgroundService, IDbSyncTrigger
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbConnectionManager _connectionManager;
    private readonly ILogger<DbSyncBackgroundService> _logger;
    private readonly Channel<bool> _syncChannel = Channel.CreateUnbounded<bool>(new UnboundedChannelOptions { SingleReader = true });

    public DbSyncBackgroundService(
        IServiceProvider serviceProvider,
        IDbConnectionManager connectionManager,
        ILogger<DbSyncBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public void TriggerSync()
    {
        _syncChannel.Writer.TryWrite(true);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Database Sync Background Service started (Option B - Local Network + Neon Backup).");

        // Ensure local primary schema is initialized
        await EnsureLocalSchemaCreatedAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _connectionManager.EvaluateConnectionAsync(stoppingToken);

                if (_connectionManager.IsNeonBackupAvailable)
                {
                    await PerformDatabaseSyncAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during background database synchronization cycle.");
            }

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                cts.CancelAfter(TimeSpan.FromSeconds(3));
                await _syncChannel.Reader.ReadAsync(cts.Token);
            }
            catch
            {
                // Timeout or canceled - continue loop
            }
        }
    }

    private async Task EnsureLocalSchemaCreatedAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var localConnStr = _connectionManager.ActiveConnectionString;

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            if (localConnStr.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            {
                optionsBuilder.UseNpgsql(localConnStr);
            }
            else
            {
                optionsBuilder.UseSqlite(localConnStr);
            }

            using var localDb = new AppDbContext(optionsBuilder.Options);
            await localDb.Database.EnsureCreatedAsync(cancellationToken);
            _logger.LogInformation("Local primary database schema verified & ready.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize local primary database schema.");
        }
    }

    public async Task PerformDatabaseSyncAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var neonRaw = config.GetConnectionString("NeonConnection")
                ?? Environment.GetEnvironmentVariable("DATABASE_URL");

            if (string.IsNullOrWhiteSpace(neonRaw))
            {
                return;
            }

            var neonFormatted = DbConnectionManagerExtensions.ConvertUrlToConnectionString(neonRaw);
            var neonOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(neonFormatted)
                .Options;

            var localConnStr = _connectionManager.ActiveConnectionString;
            var localOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            if (localConnStr.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            {
                localOptionsBuilder.UseNpgsql(localConnStr);
            }
            else
            {
                localOptionsBuilder.UseSqlite(localConnStr);
            }

            using var neonDb = new AppDbContext(neonOptions);
            using var localDb = new AppDbContext(localOptionsBuilder.Options);

            // Ensure schema exists on both
            await neonDb.Database.EnsureCreatedAsync(cancellationToken);
            await localDb.Database.EnsureCreatedAsync(cancellationToken);

            int pushedUsers = 0;
            int pulledUsers = 0;
            int pushedProducts = 0;

            // 1. Sync Users (Bi-directional by Username)
            var neonUsers = await neonDb.Users.AsNoTracking().ToListAsync(cancellationToken);
            var localUsers = await localDb.Users.AsNoTracking().ToListAsync(cancellationToken);

            var localUserMap = localUsers
                .GroupBy(u => u.Username.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            var neonUserMap = neonUsers
                .GroupBy(u => u.Username.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var user in neonUsers)
            {
                var key = user.Username.Trim().ToLowerInvariant();
                if (!localUserMap.ContainsKey(key))
                {
                    localDb.Users.Add(new User
                    {
                        Username = user.Username,
                        PasswordHash = user.PasswordHash,
                        FullName = user.FullName,
                        Role = user.Role,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt
                    });
                    pulledUsers++;
                }
                else
                {
                    var existing = await localDb.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == key, cancellationToken);
                    if (existing != null)
                    {
                        existing.FullName = user.FullName;
                        existing.PasswordHash = user.PasswordHash;
                        existing.Role = user.Role;
                        existing.IsActive = user.IsActive;
                    }
                }
            }

            foreach (var user in localUsers)
            {
                var key = user.Username.Trim().ToLowerInvariant();
                if (!neonUserMap.ContainsKey(key))
                {
                    neonDb.Users.Add(new User
                    {
                        Username = user.Username,
                        PasswordHash = user.PasswordHash,
                        FullName = user.FullName,
                        Role = user.Role,
                        IsActive = user.IsActive,
                        CreatedAt = EnsureUtc(user.CreatedAt)
                    });
                    pushedUsers++;
                }
                else
                {
                    var existing = await neonDb.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == key, cancellationToken);
                    if (existing != null)
                    {
                        existing.FullName = user.FullName;
                        existing.PasswordHash = user.PasswordHash;
                        existing.Role = user.Role;
                        existing.IsActive = user.IsActive;
                    }
                }
            }

            await localDb.SaveChangesAsync(cancellationToken);
            await neonDb.SaveChangesAsync(cancellationToken);

            // 2. Sync Products (Bi-directional by Name)
            var neonProducts = await neonDb.Products.AsNoTracking().ToListAsync(cancellationToken);
            var localProducts = await localDb.Products.AsNoTracking().ToListAsync(cancellationToken);

            var localProdMap = localProducts
                .GroupBy(p => p.Name.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            var neonProdMap = neonProducts
                .GroupBy(p => p.Name.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var prod in neonProducts)
            {
                var key = prod.Name.Trim().ToLowerInvariant();
                if (!localProdMap.ContainsKey(key))
                {
                    localDb.Products.Add(new Product
                    {
                        Name = prod.Name,
                        Description = prod.Description,
                        ImagePath = prod.ImagePath,
                        IsActive = prod.IsActive,
                        CreatedAt = prod.CreatedAt
                    });
                }
                else
                {
                    var existing = await localDb.Products.FirstOrDefaultAsync(p => p.Name.ToLower() == key, cancellationToken);
                    if (existing != null)
                    {
                        existing.Description = prod.Description;
                        existing.ImagePath = prod.ImagePath;
                        existing.IsActive = prod.IsActive;
                    }
                }
            }

            foreach (var prod in localProducts)
            {
                var key = prod.Name.Trim().ToLowerInvariant();
                if (!neonProdMap.ContainsKey(key))
                {
                    neonDb.Products.Add(new Product
                    {
                        Name = prod.Name,
                        Description = prod.Description,
                        ImagePath = prod.ImagePath,
                        IsActive = prod.IsActive,
                        CreatedAt = EnsureUtc(prod.CreatedAt)
                    });
                    pushedProducts++;
                }
                else
                {
                    var existing = await neonDb.Products.FirstOrDefaultAsync(p => p.Name.ToLower() == key, cancellationToken);
                    if (existing != null)
                    {
                        existing.Description = prod.Description;
                        existing.ImagePath = prod.ImagePath;
                        existing.IsActive = prod.IsActive;
                    }
                }
            }

            await localDb.SaveChangesAsync(cancellationToken);
            await neonDb.SaveChangesAsync(cancellationToken);

            // 3. Sync Product Variants with Natural FK Mapping
            var neonProdIdMap = await neonDb.Products.AsNoTracking().ToDictionaryAsync(p => p.Name.Trim().ToLowerInvariant(), p => p.Id, cancellationToken);
            var localProdIdMap = await localDb.Products.AsNoTracking().ToDictionaryAsync(p => p.Name.Trim().ToLowerInvariant(), p => p.Id, cancellationToken);

            var localVariants = await localDb.ProductVariants.Include(v => v.Product).AsNoTracking().ToListAsync(cancellationToken);
            var neonVariants = await neonDb.ProductVariants.Include(v => v.Product).AsNoTracking().ToListAsync(cancellationToken);

            foreach (var v in localVariants)
            {
                if (v.Product == null) continue;
                var prodNameKey = v.Product.Name.Trim().ToLowerInvariant();
                if (!neonProdIdMap.TryGetValue(prodNameKey, out var neonProductId)) continue;

                var varKey = v.Name.Trim().ToLowerInvariant();
                var existing = await neonDb.ProductVariants
                    .FirstOrDefaultAsync(nv => nv.ProductId == neonProductId && nv.Name.ToLower() == varKey, cancellationToken);

                if (existing == null)
                {
                    neonDb.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = neonProductId,
                        Name = v.Name,
                        Size = v.Size,
                        Color = v.Color,
                        CapType = v.CapType,
                        Material = v.Material,
                        ImagePath = v.ImagePath,
                        IsActive = v.IsActive,
                        CreatedAt = EnsureUtc(v.CreatedAt)
                    });
                }
                else
                {
                    existing.Size = v.Size;
                    existing.Color = v.Color;
                    existing.CapType = v.CapType;
                    existing.Material = v.Material;
                    existing.ImagePath = v.ImagePath;
                    existing.IsActive = v.IsActive;
                }
            }

            foreach (var v in neonVariants)
            {
                if (v.Product == null) continue;
                var prodNameKey = v.Product.Name.Trim().ToLowerInvariant();
                if (!localProdIdMap.TryGetValue(prodNameKey, out var localProductId)) continue;

                var varKey = v.Name.Trim().ToLowerInvariant();
                var existing = await localDb.ProductVariants
                    .FirstOrDefaultAsync(lv => lv.ProductId == localProductId && lv.Name.ToLower() == varKey, cancellationToken);

                if (existing == null)
                {
                    localDb.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = localProductId,
                        Name = v.Name,
                        Size = v.Size,
                        Color = v.Color,
                        CapType = v.CapType,
                        Material = v.Material,
                        ImagePath = v.ImagePath,
                        IsActive = v.IsActive,
                        CreatedAt = v.CreatedAt
                    });
                }
                else
                {
                    existing.Size = v.Size;
                    existing.Color = v.Color;
                    existing.CapType = v.CapType;
                    existing.Material = v.Material;
                    existing.ImagePath = v.ImagePath;
                    existing.IsActive = v.IsActive;
                }
            }

            await localDb.SaveChangesAsync(cancellationToken);
            await neonDb.SaveChangesAsync(cancellationToken);

            // 4. Sync Stock Transactions with Natural FK Mapping via ProductVariant & User
            var localTxs = await localDb.StockTransactions
                .Include(t => t.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(t => t.User)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var neonUserUsernameMap = await neonDb.Users.AsNoTracking().ToDictionaryAsync(u => u.Username.Trim().ToLowerInvariant(), u => u.Id, cancellationToken);
            var neonVarList = await neonDb.ProductVariants.Include(v => v.Product).AsNoTracking().ToListAsync(cancellationToken);

            foreach (var tx in localTxs)
            {
                if (tx.ProductVariant?.Product == null || tx.User == null) continue;

                var prodNameKey = tx.ProductVariant.Product.Name.Trim().ToLowerInvariant();
                var varNameKey = tx.ProductVariant.Name.Trim().ToLowerInvariant();
                var userKey = tx.User.Username.Trim().ToLowerInvariant();

                var targetNeonVariant = neonVarList.FirstOrDefault(v => v.Product?.Name.Trim().ToLowerInvariant() == prodNameKey && v.Name.Trim().ToLowerInvariant() == varNameKey);
                if (targetNeonVariant == null) continue;

                if (!neonUserUsernameMap.TryGetValue(userKey, out var neonUserId)) continue;

                var txUtcTime = EnsureUtc(tx.CreatedAt);
                var existing = await neonDb.StockTransactions
                    .FirstOrDefaultAsync(nt => nt.ProductVariantId == targetNeonVariant.Id && nt.CreatedAt == txUtcTime && nt.Quantity == tx.Quantity && nt.Type == tx.Type, cancellationToken);

                if (existing == null)
                {
                    neonDb.StockTransactions.Add(new StockTransaction
                    {
                        ProductVariantId = targetNeonVariant.Id,
                        UserId = neonUserId,
                        Type = tx.Type,
                        Quantity = tx.Quantity,
                        Notes = tx.Notes,
                        CreatedAt = txUtcTime
                    });
                }
            }

            await neonDb.SaveChangesAsync(cancellationToken);

            _connectionManager.UpdateLastSyncTime();
            _logger.LogInformation("Database sync cycle finished: Pushed {PushedUsers} users, {PushedProducts} products to Neon Cloud.", pushedUsers, pushedProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database background sync attempt encountered an error: {Message}", ex.Message);
        }
    }

    private static DateTime EnsureUtc(DateTime dt)
    {
        if (dt == DateTime.MinValue) return DateTime.UtcNow;
        return dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }
}

public static class DbConnectionManagerExtensions
{
    public static string ConvertUrlToConnectionString(string databaseUrl)
    {
        if (string.IsNullOrWhiteSpace(databaseUrl)) return "";
        var value = databaseUrl.Trim().Trim('"').Trim('\'');

        if (value.StartsWith("Host=", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        if (value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            value = value.Substring("postgresql://".Length);
        }
        else if (value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            value = value.Substring("postgres://".Length);
        }
        else
        {
            return value;
        }

        var queryIndex = value.IndexOf('?');
        if (queryIndex >= 0) value = value.Substring(0, queryIndex);

        var slashIndex = value.IndexOf('/');
        if (slashIndex < 0) return value;

        var authority = value.Substring(0, slashIndex);
        var database = value.Substring(slashIndex + 1);

        var atIndex = authority.LastIndexOf('@');
        if (atIndex < 0) return value;

        var userInfo = authority.Substring(0, atIndex);
        var hostPort = authority.Substring(atIndex + 1);

        var colonIndex = userInfo.IndexOf(':');
        if (colonIndex < 0) return value;

        var username = Uri.UnescapeDataString(userInfo.Substring(0, colonIndex));
        var password = Uri.UnescapeDataString(userInfo.Substring(colonIndex + 1));

        var host = hostPort;
        var port = 5432;
        var lastColon = hostPort.LastIndexOf(':');
        if (lastColon > 0 && int.TryParse(hostPort.Substring(lastColon + 1), out var parsedPort))
        {
            host = hostPort.Substring(0, lastColon);
            port = parsedPort;
        }

        database = Uri.UnescapeDataString(database);

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
    }
}
