using System.Threading.Channels;
using System.Globalization;
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
    private readonly Channel<bool> _syncChannel = Channel.CreateBounded<bool>(
        new BoundedChannelOptions(1)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropWrite
        });
    private readonly SemaphoreSlim _syncGate = new(1, 1);
    private bool _schemasEnsured;

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
            var localConnStr = _connectionManager.LocalConnectionString;

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
            var configuration = _serviceProvider.GetRequiredService<IConfiguration>();
            await DatabaseSeeder.SeedAsync(localDb, configuration);
            _logger.LogInformation("Local primary database schema verified & ready.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize local primary database schema.");
        }
    }

    public async Task PerformDatabaseSyncAsync(CancellationToken cancellationToken = default)
    {
        await _syncGate.WaitAsync(cancellationToken);
        try
        {
            var neonRaw = _connectionManager.NeonConnectionString;

            if (string.IsNullOrWhiteSpace(neonRaw))
            {
                return;
            }

            var neonOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(neonRaw)
                .Options;

            var localConnStr = _connectionManager.LocalConnectionString;
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

            // Schema checks are expensive against Neon; do them once per process.
            if (!_schemasEnsured)
            {
                await neonDb.Database.EnsureCreatedAsync(cancellationToken);
                await localDb.Database.EnsureCreatedAsync(cancellationToken);
                await EnsureDeletionTableAsync(neonDb, cancellationToken);
                await EnsureDeletionTableAsync(localDb, cancellationToken);
                _schemasEnsured = true;
            }

            await SyncDeletionTombstonesAsync(localDb, neonDb, cancellationToken);

            int pushedUsers = 0;
            int pulledUsers = 0;
            int pushedProducts = 0;

            // 1. Sync Users (Bi-directional by Username)
            var neonUsers = await neonDb.Users.ToListAsync(cancellationToken);
            var localUsers = await localDb.Users.ToListAsync(cancellationToken);
            var localUsersSnapshot = localUsers.ToArray();

            var localUserMap = localUsers
                .GroupBy(u => u.Username.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            var neonUserMap = neonUsers
                .GroupBy(u => u.Username.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var user in neonUsers)
            {
                var key = user.Username.Trim().ToLowerInvariant();
                if (!localUserMap.TryGetValue(key, out var existing))
                {
                    existing = new User
                    {
                        Username = user.Username,
                        PasswordHash = user.PasswordHash,
                        FullName = user.FullName,
                        Role = user.Role,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt
                    };
                    localDb.Users.Add(existing);
                    localUserMap[key] = existing;
                    pulledUsers++;
                }
                else if (EnsureUtc(user.CreatedAt) >= EnsureUtc(existing.CreatedAt))
                {
                    existing.FullName = user.FullName;
                    existing.PasswordHash = user.PasswordHash;
                    existing.Role = user.Role;
                    existing.IsActive = user.IsActive;
                    existing.CreatedAt = EnsureUtc(user.CreatedAt);
                }
            }

            foreach (var user in localUsersSnapshot)
            {
                var key = user.Username.Trim().ToLowerInvariant();
                if (!neonUserMap.TryGetValue(key, out var existing))
                {
                    existing = new User
                    {
                        Username = user.Username,
                        PasswordHash = user.PasswordHash,
                        FullName = user.FullName,
                        Role = user.Role,
                        IsActive = user.IsActive,
                        CreatedAt = EnsureUtc(user.CreatedAt)
                    };
                    neonDb.Users.Add(existing);
                    neonUserMap[key] = existing;
                    pushedUsers++;
                }
                else if (EnsureUtc(user.CreatedAt) > EnsureUtc(existing.CreatedAt))
                {
                    existing.FullName = user.FullName;
                    existing.PasswordHash = user.PasswordHash;
                    existing.Role = user.Role;
                    existing.IsActive = user.IsActive;
                    existing.CreatedAt = EnsureUtc(user.CreatedAt);
                }
            }

            await localDb.SaveChangesAsync(cancellationToken);
            await neonDb.SaveChangesAsync(cancellationToken);

            // 2. Sync Products (Bi-directional by Name)
            var neonProducts = await neonDb.Products.ToListAsync(cancellationToken);
            var localProducts = await localDb.Products.ToListAsync(cancellationToken);
            var localProductsSnapshot = localProducts.ToArray();

            var localProdMap = localProducts
                .GroupBy(p => p.Name.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            var neonProdMap = neonProducts
                .GroupBy(p => p.Name.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var prod in neonProducts)
            {
                var key = prod.Name.Trim().ToLowerInvariant();
                if (!localProdMap.TryGetValue(key, out var existing))
                {
                    existing = new Product
                    {
                        Name = prod.Name,
                        Description = prod.Description,
                        ImagePath = prod.ImagePath,
                        IsActive = prod.IsActive,
                        CreatedAt = prod.CreatedAt
                    };
                    localDb.Products.Add(existing);
                    localProdMap[key] = existing;
                }
                else if (EnsureUtc(prod.CreatedAt) >= EnsureUtc(existing.CreatedAt))
                {
                    existing.Description = prod.Description;
                    existing.ImagePath = prod.ImagePath;
                    existing.IsActive = prod.IsActive;
                    existing.CreatedAt = EnsureUtc(prod.CreatedAt);
                }
            }

            foreach (var prod in localProductsSnapshot)
            {
                var key = prod.Name.Trim().ToLowerInvariant();
                if (!neonProdMap.TryGetValue(key, out var existing))
                {
                    existing = new Product
                    {
                        Name = prod.Name,
                        Description = prod.Description,
                        ImagePath = prod.ImagePath,
                        IsActive = prod.IsActive,
                        CreatedAt = EnsureUtc(prod.CreatedAt)
                    };
                    neonDb.Products.Add(existing);
                    neonProdMap[key] = existing;
                    pushedProducts++;
                }
                else if (EnsureUtc(prod.CreatedAt) > EnsureUtc(existing.CreatedAt))
                {
                    existing.Description = prod.Description;
                    existing.ImagePath = prod.ImagePath;
                    existing.IsActive = prod.IsActive;
                    existing.CreatedAt = EnsureUtc(prod.CreatedAt);
                }
            }

            await localDb.SaveChangesAsync(cancellationToken);
            await neonDb.SaveChangesAsync(cancellationToken);

            // 3. Sync Product Variants with Natural FK Mapping
            var neonProdIdMap = await neonDb.Products.AsNoTracking().ToDictionaryAsync(p => p.Name.Trim().ToLowerInvariant(), p => p.Id, cancellationToken);
            var localProdIdMap = await localDb.Products.AsNoTracking().ToDictionaryAsync(p => p.Name.Trim().ToLowerInvariant(), p => p.Id, cancellationToken);

            var localVariants = await localDb.ProductVariants.Include(v => v.Product).ToListAsync(cancellationToken);
            var neonVariants = await neonDb.ProductVariants.Include(v => v.Product).ToListAsync(cancellationToken);
            var localVariantsSnapshot = localVariants.ToArray();
            var neonVariantsSnapshot = neonVariants.ToArray();
            var localVariantMap = localVariants
                .Where(v => v.Product != null)
                .GroupBy(v => VariantKey(v.Product.Name, v.Name))
                .ToDictionary(g => g.Key, g => g.First());
            var neonVariantMap = neonVariants
                .Where(v => v.Product != null)
                .GroupBy(v => VariantKey(v.Product.Name, v.Name))
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var v in localVariantsSnapshot)
            {
                if (v.Product == null) continue;
                var prodNameKey = v.Product.Name.Trim().ToLowerInvariant();
                if (!neonProdIdMap.TryGetValue(prodNameKey, out var neonProductId)) continue;

                var key = VariantKey(v.Product.Name, v.Name);

                if (!neonVariantMap.TryGetValue(key, out var existing))
                {
                    existing = new ProductVariant
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
                    };
                    neonDb.ProductVariants.Add(existing);
                    neonVariantMap[key] = existing;
                }
                else if (EnsureUtc(v.CreatedAt) > EnsureUtc(existing.CreatedAt))
                {
                    existing.Size = v.Size;
                    existing.Color = v.Color;
                    existing.CapType = v.CapType;
                    existing.Material = v.Material;
                    existing.ImagePath = v.ImagePath;
                    existing.IsActive = v.IsActive;
                    existing.CreatedAt = EnsureUtc(v.CreatedAt);
                }
            }

            foreach (var v in neonVariantsSnapshot)
            {
                if (v.Product == null) continue;
                var prodNameKey = v.Product.Name.Trim().ToLowerInvariant();
                if (!localProdIdMap.TryGetValue(prodNameKey, out var localProductId)) continue;

                var key = VariantKey(v.Product.Name, v.Name);

                if (!localVariantMap.TryGetValue(key, out var existing))
                {
                    existing = new ProductVariant
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
                    };
                    localDb.ProductVariants.Add(existing);
                    localVariantMap[key] = existing;
                }
                else if (EnsureUtc(v.CreatedAt) >= EnsureUtc(existing.CreatedAt))
                {
                    existing.Size = v.Size;
                    existing.Color = v.Color;
                    existing.CapType = v.CapType;
                    existing.Material = v.Material;
                    existing.ImagePath = v.ImagePath;
                    existing.IsActive = v.IsActive;
                    existing.CreatedAt = EnsureUtc(v.CreatedAt);
                }
            }

            await localDb.SaveChangesAsync(cancellationToken);
            await neonDb.SaveChangesAsync(cancellationToken);

            // 4. Sync permissions and user-permission assignments by stable names.
            var localPermissions = await localDb.Permissions
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var neonPermissions = await neonDb.Permissions
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var localPermissionMap = localPermissions
                .GroupBy(p => p.Name.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());
            var neonPermissionMap = neonPermissions
                .GroupBy(p => p.Name.Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var permission in neonPermissions)
            {
                var key = permission.Name.Trim().ToLowerInvariant();
                if (!localPermissionMap.ContainsKey(key))
                {
                    localDb.Permissions.Add(new Permission
                    {
                        Name = permission.Name,
                        Description = permission.Description,
                        IsActive = permission.IsActive
                    });
                }
            }

            foreach (var permission in localPermissions)
            {
                var key = permission.Name.Trim().ToLowerInvariant();
                if (!neonPermissionMap.ContainsKey(key))
                {
                    neonDb.Permissions.Add(new Permission
                    {
                        Name = permission.Name,
                        Description = permission.Description,
                        IsActive = permission.IsActive
                    });
                }
            }

            await localDb.SaveChangesAsync(cancellationToken);
            await neonDb.SaveChangesAsync(cancellationToken);

            localPermissions = await localDb.Permissions.AsNoTracking().ToListAsync(cancellationToken);
            neonPermissions = await neonDb.Permissions.AsNoTracking().ToListAsync(cancellationToken);

            var localUsersWithPermissions = await localDb.Users
                .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
                .ToListAsync(cancellationToken);
            var neonUsersWithPermissions = await neonDb.Users
                .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
                .ToListAsync(cancellationToken);

            var localUserMapByName = localUsersWithPermissions
                .ToDictionary(u => u.Username.Trim().ToLowerInvariant());
            var neonUserMapByName = neonUsersWithPermissions
                .ToDictionary(u => u.Username.Trim().ToLowerInvariant());
            var localPermissionMapByName = localPermissions
                .ToDictionary(p => p.Name.Trim().ToLowerInvariant());
            var neonPermissionMapByName = neonPermissions
                .ToDictionary(p => p.Name.Trim().ToLowerInvariant());

            // The newer user version owns its permission assignment set. This also
            // propagates removals instead of merging them back into both databases.
            foreach (var userKey in localUserMapByName.Keys.Intersect(neonUserMapByName.Keys))
            {
                var localUser = localUserMapByName[userKey];
                var neonUser = neonUserMapByName[userKey];
                var localIsNewer = EnsureUtc(localUser.CreatedAt) > EnsureUtc(neonUser.CreatedAt);
                var sourceUser = localIsNewer ? localUser : neonUser;
                var sourcePermissions = sourceUser.UserPermissions
                    .Select(up => up.Permission.Name.Trim().ToLowerInvariant())
                    .ToHashSet();
                var targetUserWithPermissions = localIsNewer ? neonUser : localUser;
                var targetPermissions = targetUserWithPermissions.UserPermissions
                    .Select(up => up.Permission.Name.Trim().ToLowerInvariant())
                    .ToHashSet();

                var targetDb = localIsNewer ? neonDb : localDb;
                if (EnsureUtc(targetUserWithPermissions.CreatedAt) != EnsureUtc(sourceUser.CreatedAt))
                {
                    targetUserWithPermissions.CreatedAt = EnsureUtc(sourceUser.CreatedAt);
                }

                if (!sourcePermissions.SetEquals(targetPermissions))
                {
                    var targetPermissionMap = localIsNewer
                        ? neonPermissionMapByName
                        : localPermissionMapByName;

                    var existingAssignments = targetUserWithPermissions.UserPermissions.ToList();
                    foreach (var assignment in existingAssignments)
                    {
                        var permissionKey = assignment.Permission.Name.Trim().ToLowerInvariant();
                        if (!sourcePermissions.Contains(permissionKey))
                            targetDb.UserPermissions.Remove(assignment);
                    }

                    foreach (var permissionKey in sourcePermissions)
                    {
                        if (!targetPermissions.Contains(permissionKey) &&
                            targetPermissionMap.TryGetValue(permissionKey, out var targetPermission))
                        {
                            targetDb.UserPermissions.Add(new UserPermission
                            {
                                UserId = targetUserWithPermissions.Id,
                                PermissionId = targetPermission.Id
                            });
                        }
                    }
                }
            }

            await localDb.SaveChangesAsync(cancellationToken);
            await neonDb.SaveChangesAsync(cancellationToken);

            // 5. Sync Stock Transactions with Natural FK Mapping via ProductVariant & User
            var localTxs = await localDb.StockTransactions
                .Include(t => t.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(t => t.User)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var neonTxs = await neonDb.StockTransactions
                .Include(t => t.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(t => t.User)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            var neonUserUsernameMap = neonUserMap.ToDictionary(kv => kv.Key, kv => kv.Value.Id);
            var localUserUsernameMap = localUserMap.ToDictionary(kv => kv.Key, kv => kv.Value.Id);
            var neonTransactionKeys = neonTxs
                .Select(TransactionKey)
                .Where(key => key.Length > 0)
                .ToHashSet(StringComparer.Ordinal);
            var localTransactionKeys = localTxs
                .Select(TransactionKey)
                .Where(key => key.Length > 0)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var tx in localTxs)
            {
                if (tx.ProductVariant?.Product == null || tx.User == null) continue;

                var userKey = tx.User.Username.Trim().ToLowerInvariant();
                var variantKey = VariantKey(tx.ProductVariant.Product.Name, tx.ProductVariant.Name);
                if (!neonVariantMap.TryGetValue(variantKey, out var targetNeonVariant)) continue;

                if (!neonUserUsernameMap.TryGetValue(userKey, out var neonUserId)) continue;

                var txUtcTime = EnsureUtc(tx.CreatedAt);
                var transactionKey = TransactionKey(tx);
                if (neonTransactionKeys.Add(transactionKey))
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

            foreach (var tx in neonTxs)
            {
                if (tx.ProductVariant?.Product == null || tx.User == null) continue;

                var userKey = tx.User.Username.Trim().ToLowerInvariant();
                var variantKey = VariantKey(tx.ProductVariant.Product.Name, tx.ProductVariant.Name);
                if (!localVariantMap.TryGetValue(variantKey, out var targetLocalVariant)) continue;

                if (!localUserUsernameMap.TryGetValue(userKey, out var localUserId)) continue;

                var txUtcTime = EnsureUtc(tx.CreatedAt);
                if (localTransactionKeys.Add(TransactionKey(tx)))
                {
                    localDb.StockTransactions.Add(new StockTransaction
                    {
                        ProductVariantId = targetLocalVariant.Id,
                        UserId = localUserId,
                        Type = tx.Type,
                        Quantity = tx.Quantity,
                        Notes = tx.Notes,
                        CreatedAt = txUtcTime
                    });
                }
            }

            await neonDb.SaveChangesAsync(cancellationToken);
            await localDb.SaveChangesAsync(cancellationToken);

            _connectionManager.UpdateLastSyncTime();
            _logger.LogInformation("Database sync cycle finished: Pushed {PushedUsers} users, {PushedProducts} products to Neon Cloud.", pushedUsers, pushedProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database background sync attempt encountered an error: {Message}", ex.Message);
        }
        finally
        {
            _syncGate.Release();
        }
    }

    private static async Task EnsureDeletionTableAsync(
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        if (db.Database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true)
        {
            await db.Database.ExecuteSqlRawAsync(
                "CREATE TABLE IF NOT EXISTS \"DeletedRecords\" (\"EntityType\" TEXT NOT NULL, \"RecordKey\" TEXT NOT NULL, \"DeletedAt\" timestamp with time zone NOT NULL, PRIMARY KEY (\"EntityType\", \"RecordKey\"));",
                cancellationToken);
        }
        else
        {
            await db.Database.ExecuteSqlRawAsync(
                "CREATE TABLE IF NOT EXISTS DeletedRecords (EntityType TEXT NOT NULL, RecordKey TEXT NOT NULL, DeletedAt TEXT NOT NULL, PRIMARY KEY (EntityType, RecordKey));",
                cancellationToken);
        }
    }

    private static async Task SyncDeletionTombstonesAsync(
        AppDbContext localDb,
        AppDbContext neonDb,
        CancellationToken cancellationToken)
    {
        var localTombstones = await localDb.DeletedRecords
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var neonTombstones = await neonDb.DeletedRecords
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var localKeys = localTombstones
            .Select(x => $"{x.EntityType}|{x.RecordKey}")
            .ToHashSet();
        var neonKeys = neonTombstones
            .Select(x => $"{x.EntityType}|{x.RecordKey}")
            .ToHashSet();

        foreach (var tombstone in neonTombstones.Where(x => !localKeys.Contains($"{x.EntityType}|{x.RecordKey}")))
        {
            localDb.DeletedRecords.Add(new DeletedRecord
            {
                EntityType = tombstone.EntityType,
                RecordKey = tombstone.RecordKey,
                DeletedAt = EnsureUtc(tombstone.DeletedAt)
            });
        }

        foreach (var tombstone in localTombstones.Where(x => !neonKeys.Contains($"{x.EntityType}|{x.RecordKey}")))
        {
            neonDb.DeletedRecords.Add(new DeletedRecord
            {
                EntityType = tombstone.EntityType,
                RecordKey = tombstone.RecordKey,
                DeletedAt = EnsureUtc(tombstone.DeletedAt)
            });
        }

        await localDb.SaveChangesAsync(cancellationToken);
        await neonDb.SaveChangesAsync(cancellationToken);

        var allTombstones = localTombstones
            .Concat(neonTombstones)
            .GroupBy(x => $"{x.EntityType}|{x.RecordKey}")
            .Select(x => x.OrderByDescending(t => t.DeletedAt).First())
            .ToList();

        var localUsers = await localDb.Users.ToListAsync(cancellationToken);
        var neonUsers = await neonDb.Users.ToListAsync(cancellationToken);
        var localProducts = await localDb.Products.ToListAsync(cancellationToken);
        var neonProducts = await neonDb.Products.ToListAsync(cancellationToken);
        var localVariants = await localDb.ProductVariants.ToListAsync(cancellationToken);
        var neonVariants = await neonDb.ProductVariants.ToListAsync(cancellationToken);

        foreach (var tombstone in allTombstones)
        {
            var deletedAt = EnsureUtc(tombstone.DeletedAt);
            switch (tombstone.EntityType)
            {
                case "User":
                    localDb.Users.RemoveRange(localUsers.Where(x =>
                        x.Username.Trim().ToLowerInvariant() == tombstone.RecordKey && x.CreatedAt <= deletedAt));
                    neonDb.Users.RemoveRange(neonUsers.Where(x =>
                        x.Username.Trim().ToLowerInvariant() == tombstone.RecordKey && x.CreatedAt <= deletedAt));
                    break;
                case "Product":
                    localDb.Products.RemoveRange(localProducts.Where(x =>
                        x.Name.Trim().ToLowerInvariant() == tombstone.RecordKey && x.CreatedAt <= deletedAt));
                    neonDb.Products.RemoveRange(neonProducts.Where(x =>
                        x.Name.Trim().ToLowerInvariant() == tombstone.RecordKey && x.CreatedAt <= deletedAt));
                    break;
                case "ProductVariant":
                    var separator = tombstone.RecordKey.IndexOf('|');
                    if (separator <= 0) break;
                    var productKey = tombstone.RecordKey[..separator];
                    var variantKey = tombstone.RecordKey[(separator + 1)..];
                    var localProductIds = localProducts
                        .Where(x => x.Name.Trim().ToLowerInvariant() == productKey)
                        .Select(x => x.Id);
                    var neonProductIds = neonProducts
                        .Where(x => x.Name.Trim().ToLowerInvariant() == productKey)
                        .Select(x => x.Id);
                    localDb.ProductVariants.RemoveRange(localVariants.Where(x =>
                        localProductIds.Contains(x.ProductId) &&
                        x.Name.Trim().ToLowerInvariant() == variantKey &&
                        x.CreatedAt <= deletedAt));
                    neonDb.ProductVariants.RemoveRange(neonVariants.Where(x =>
                        neonProductIds.Contains(x.ProductId) &&
                        x.Name.Trim().ToLowerInvariant() == variantKey &&
                        x.CreatedAt <= deletedAt));
                    break;
            }
        }

        await localDb.SaveChangesAsync(cancellationToken);
        await neonDb.SaveChangesAsync(cancellationToken);
    }

    private static DateTime EnsureUtc(DateTime dt)
    {
        if (dt == DateTime.MinValue) return DateTime.UtcNow;
        return dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }

    private static string VariantKey(string productName, string variantName) =>
        $"{productName.Trim().ToLowerInvariant()}|{variantName.Trim().ToLowerInvariant()}";

    private static string TransactionKey(StockTransaction transaction)
    {
        if (transaction.ProductVariant?.Product == null || transaction.User == null)
            return string.Empty;

        return string.Join('\u001f',
            VariantKey(transaction.ProductVariant.Product.Name, transaction.ProductVariant.Name),
            transaction.User.Username.Trim().ToLowerInvariant(),
            ((int)transaction.Type).ToString(CultureInfo.InvariantCulture),
            transaction.Quantity.ToString(CultureInfo.InvariantCulture),
            EnsureUtc(transaction.CreatedAt).Ticks.ToString(CultureInfo.InvariantCulture),
            transaction.Notes ?? string.Empty);
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
