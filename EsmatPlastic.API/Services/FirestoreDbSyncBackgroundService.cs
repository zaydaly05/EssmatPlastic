using System.Data.Common;
using System.Text.Json;
using System.Threading.Channels;
using EsmatPlastic.Domain.Entities;
using EsmatPlastic.Infrastructure.Data;
using Google.Cloud.Firestore;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API.Services;

[FirestoreData]
public sealed class FirestoreSyncRecord
{
    [FirestoreProperty("syncId")]
    public string SyncId { get; set; } = string.Empty;

    [FirestoreProperty("updatedAt")]
    public Timestamp UpdatedAt { get; set; }

    [FirestoreProperty("payloadJson")]
    public string PayloadJson { get; set; } = string.Empty;

    [FirestoreProperty("references")]
    public Dictionary<string, string> References { get; set; } = new();
}

[FirestoreData]
public sealed class FirestoreCredentialRecord
{
    [FirestoreProperty("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [FirestoreProperty("updatedAt")]
    public Timestamp UpdatedAt { get; set; }
}

[FirestoreData]
public sealed class FirestoreTombstoneRecord
{
    [FirestoreProperty("entityType")]
    public string EntityType { get; set; } = string.Empty;

    [FirestoreProperty("recordKey")]
    public string RecordKey { get; set; } = string.Empty;

    [FirestoreProperty("deletedAt")]
    public Timestamp DeletedAt { get; set; }
}

public sealed class FirestoreDbSyncBackgroundService : BackgroundService, IDbSyncTrigger
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IConfiguration _configuration;
    private readonly ILogger<FirestoreDbSyncBackgroundService> _logger;
    private readonly Channel<bool> _syncChannel = Channel.CreateBounded<bool>(1);
    private readonly SemaphoreSlim _syncGate = new(1, 1);
    private FirestoreDb? _firestore;
    private Exception? _firestoreInitializationError;
    private bool _hasLoggedOfflineError;
    private DateTime _lastSyncUtc = DateTime.UnixEpoch;
    private bool _initialSyncCompleted;

    public bool IsOnline { get; private set; }
    public DateTime? LastSyncTimeUtc { get; private set; }

    public FirestoreDbSyncBackgroundService(
        IConfiguration configuration,
        ILogger<FirestoreDbSyncBackgroundService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void TriggerSync() => _syncChannel.Writer.TryWrite(true);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await EnsureLocalSchemaCreatedAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformDatabaseSyncAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                IsOnline = false;
                if (!_hasLoggedOfflineError)
                {
                    _logger.LogWarning(ex, "Firestore is unavailable; local SQLite remains available.");
                    _hasLoggedOfflineError = true;
                }
                else
                {
                    _logger.LogDebug("Firestore sync is still unavailable: {Message}", ex.Message);
                }
            }

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                cts.CancelAfter(TimeSpan.FromSeconds(3));
                await _syncChannel.Reader.ReadAsync(cts.Token);
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public async Task PerformDatabaseSyncAsync(CancellationToken cancellationToken = default)
    {
        await _syncGate.WaitAsync(cancellationToken);
        try
        {
            var firestore = GetFirestore();
            await using var localDb = CreateLocalDbContext();
            await localDb.Database.EnsureCreatedAsync(cancellationToken);
            await SyncSchemaInitializer.EnsureAsync(localDb, cancellationToken);

            await SyncDeletedRecordsAsync(localDb, firestore, cancellationToken);
            await SyncEntitiesAsync(
                localDb, "users", localDb.Users, "userCredentials",
                OnGetReferencesAsync, OnApplyReferencesAsync,
                omitPasswordHash: true,
                OnPullUserAsync, OnPushUserAsync, cancellationToken);
            await SyncEntitiesAsync(
                localDb, "products", localDb.Products, null,
                OnGetReferencesAsync, OnApplyReferencesAsync,
                cancellationToken: cancellationToken);
            await SyncEntitiesAsync(
                localDb, "permissions", localDb.Permissions, null,
                OnGetReferencesAsync, OnApplyReferencesAsync,
                cancellationToken: cancellationToken);
            await SyncEntitiesAsync(
                localDb, "productVariants", localDb.ProductVariants, null,
                OnGetReferencesAsync, OnApplyReferencesAsync,
                cancellationToken: cancellationToken);
            await SyncEntitiesAsync(
                localDb, "userPermissions", localDb.UserPermissions, null,
                OnGetReferencesAsync, OnApplyReferencesAsync,
                cancellationToken: cancellationToken);
            await SyncEntitiesAsync(
                localDb, "orderRequests", localDb.OrderRequests, null,
                OnGetReferencesAsync, OnApplyReferencesAsync,
                cancellationToken: cancellationToken);
            await SyncEntitiesAsync(
                localDb, "orderRequestItems", localDb.OrderRequestItems, null,
                OnGetReferencesAsync, OnApplyReferencesAsync,
                cancellationToken: cancellationToken);
            await SyncEntitiesAsync(
                localDb, "stockTransactions", localDb.StockTransactions, null,
                OnGetReferencesAsync, OnApplyReferencesAsync,
                cancellationToken: cancellationToken);

            _lastSyncUtc = DateTime.UtcNow;
            _initialSyncCompleted = true;
            IsOnline = true;
            LastSyncTimeUtc = _lastSyncUtc;
            _hasLoggedOfflineError = false;
        }
        catch
        {
            IsOnline = false;
            throw;
        }
        finally
        {
            _syncGate.Release();
        }
    }

    private FirestoreDb GetFirestore()
    {
        if (_firestore is not null)
        {
            return _firestore;
        }

        if (_firestoreInitializationError is not null)
        {
            throw new InvalidOperationException("Firestore initialization failed; restart after configuring credentials.",
                _firestoreInitializationError);
        }

        var projectId = _configuration["Firebase:ProjectId"];
        if (string.IsNullOrWhiteSpace(projectId))
        {
            throw new InvalidOperationException("Firebase:ProjectId is not configured.");
        }

        try
        {
            _firestore = FirestoreDb.Create(projectId);
            return _firestore;
        }
        catch (Exception ex)
        {
            _firestoreInitializationError = ex;
            throw;
        }
    }

    private AppDbContext CreateLocalDbContext()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
        var options = new DbContextOptionsBuilder<AppDbContext>();

        if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
        {
            options.UseNpgsql(connectionString);
        }
        else
        {
            options.UseSqlite(connectionString);
        }

        return new AppDbContext(options.Options);
    }

    private async Task EnsureLocalSchemaCreatedAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var db = CreateLocalDbContext();
            await db.Database.EnsureCreatedAsync(cancellationToken);
            await SyncSchemaInitializer.EnsureAsync(db, cancellationToken);
            var configuration = _configuration;
            await DatabaseSeeder.SeedAsync(db, configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize local SQLite database.");
        }
    }

    private async Task SyncEntitiesAsync<TEntity>(
        AppDbContext db,
        string collectionName,
        DbSet<TEntity> set,
        string? protectedCollection,
        Func<TEntity, AppDbContext, Task<Dictionary<string, string>>> getReferences,
        Func<TEntity, Dictionary<string, string>, AppDbContext, Task> applyReferences,
        bool omitPasswordHash = false,
        Func<TEntity, FirestoreSyncRecord, FirestoreDb, CancellationToken, Task>? onPull = null,
        Func<TEntity, FirestoreDb, CancellationToken, Task>? onPush = null,
        CancellationToken cancellationToken = default)
        where TEntity : class, ISyncTimestamped, new()
    {
        var collection = GetFirestore().Collection(collectionName);
        var cloudRecords = await LoadCloudRecordsAsync(collection, cancellationToken);
        var localRecords = await set.ToListAsync(cancellationToken);
        var localById = localRecords.ToDictionary(record => record.SyncId);

        foreach (var cloudRecord in cloudRecords.Values)
        {
            if (!Guid.TryParse(cloudRecord.SyncId, out var syncId))
            {
                continue;
            }

            localById.TryGetValue(syncId, out var existing);
            if (existing is not null && cloudRecord.UpdatedAt.ToDateTime() < EnsureUtc(existing.UpdatedAt))
            {
                continue;
            }

            var incoming = DeserializeEntity<TEntity>(cloudRecord.PayloadJson);
            incoming.SyncId = syncId;
            incoming.UpdatedAt = EnsureUtc(cloudRecord.UpdatedAt.ToDateTime());
            await applyReferences(incoming, cloudRecord.References, db);

            if (existing is null)
            {
                set.Add(incoming);
                localById[syncId] = incoming;
            }
            else
            {
                ApplyCloudValues(db, existing, incoming, set);
            }

            if (onPull is not null)
            {
                await onPull(incoming, cloudRecord, GetFirestore(), cancellationToken);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        localRecords = await set.ToListAsync(cancellationToken);

        foreach (var localRecord in localRecords.Where(record => EnsureUtc(record.UpdatedAt) >= _lastSyncUtc))
        {
            var documentId = localRecord.SyncId.ToString("D");
            if (!cloudRecords.TryGetValue(documentId, out var cloudRecord))
            {
                var snapshot = await collection.Document(documentId).GetSnapshotAsync(cancellationToken);
                if (snapshot.Exists)
                {
                    cloudRecord = snapshot.ConvertTo<FirestoreSyncRecord>();
                }
            }

            if (cloudRecord is not null && cloudRecord.UpdatedAt.ToDateTime() >= EnsureUtc(localRecord.UpdatedAt))
            {
                if (cloudRecords.ContainsKey(documentId))
                {
                    continue;
                }

                var incoming = DeserializeEntity<TEntity>(cloudRecord.PayloadJson);
                incoming.SyncId = localRecord.SyncId;
                incoming.UpdatedAt = EnsureUtc(cloudRecord.UpdatedAt.ToDateTime());
                await applyReferences(incoming, cloudRecord.References, db);
                ApplyCloudValues(db, localRecord, incoming, set);
                if (onPull is not null)
                {
                    await onPull(incoming, cloudRecord, GetFirestore(), cancellationToken);
                }

                continue;
            }

            var references = await getReferences(localRecord, db);
            var payload = SerializeEntity(db, localRecord, omitPasswordHash);
            var record = new FirestoreSyncRecord
            {
                SyncId = documentId,
                UpdatedAt = Timestamp.FromDateTime(EnsureUtc(localRecord.UpdatedAt)),
                PayloadJson = payload,
                References = references
            };

            await collection.Document(documentId).SetAsync(record, cancellationToken: cancellationToken);
            if (onPush is not null)
            {
                await onPush(localRecord, GetFirestore(), cancellationToken);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Dictionary<string, FirestoreSyncRecord>> LoadCloudRecordsAsync(
        CollectionReference collection,
        CancellationToken cancellationToken)
    {
        Query query = collection;
        if (_initialSyncCompleted)
        {
            query = query.WhereGreaterThanOrEqualTo(
                "updatedAt",
                Timestamp.FromDateTime(EnsureUtc(_lastSyncUtc)));
        }

        var snapshot = await query.GetSnapshotAsync(cancellationToken);
        return snapshot.Documents
            .Where(document => document.Exists)
            .Select(document => (document.Id, Record: document.ConvertTo<FirestoreSyncRecord>()))
            .ToDictionary(item => item.Id, item => item.Record, StringComparer.OrdinalIgnoreCase);
    }

    private static string SerializeEntity<TEntity>(AppDbContext db, TEntity entity, bool omitPasswordHash)
        where TEntity : class
    {
        var values = db.Entry(entity).Properties
            .Where(property =>
                property.Metadata.Name != "Id" &&
                property.Metadata.Name != nameof(ISyncTimestamped.SyncId) &&
                !property.Metadata.Name.EndsWith("Id", StringComparison.Ordinal) &&
                !(omitPasswordHash && property.Metadata.Name == nameof(User.PasswordHash)))
            .ToDictionary(property => property.Metadata.Name, property => property.CurrentValue);

        return JsonSerializer.Serialize(values, JsonOptions);
    }

    private static TEntity DeserializeEntity<TEntity>(string payloadJson)
        where TEntity : class, new()
    {
        var entity = new TEntity();
        using var document = JsonDocument.Parse(payloadJson);
        foreach (var field in document.RootElement.EnumerateObject())
        {
            var property = typeof(TEntity).GetProperty(field.Name);
            if (property?.CanWrite == true && property.Name != "Id" && property.Name != nameof(ISyncTimestamped.SyncId))
            {
                property.SetValue(entity, field.Value.Deserialize(property.PropertyType, JsonOptions));
            }
        }

        return entity;
    }

    private static void ApplyCloudValues<TEntity>(
        AppDbContext db,
        TEntity existing,
        TEntity incoming,
        DbSet<TEntity> set)
        where TEntity : class
    {
        var entityType = db.Model.FindEntityType(typeof(TEntity))!;
        var keyProperties = entityType.FindPrimaryKey()!.Properties;
        var existingEntry = db.Entry(existing);
        var incomingEntry = db.Entry(incoming);
        var keyChanged = keyProperties.Any(property =>
            !Equals(existingEntry.Property(property.Name).CurrentValue,
                incomingEntry.Property(property.Name).CurrentValue));

        if (keyChanged)
        {
            foreach (var keyProperty in keyProperties.Where(property => property.Name == "Id"))
            {
                incomingEntry.Property(keyProperty.Name).CurrentValue =
                    existingEntry.Property(keyProperty.Name).CurrentValue;
            }

            set.Remove(existing);
            set.Add(incoming);
            return;
        }

        foreach (var property in entityType.GetProperties().Where(property => !property.IsPrimaryKey()))
        {
            existingEntry.Property(property.Name).CurrentValue = incomingEntry.Property(property.Name).CurrentValue;
        }
    }

    private async Task<Dictionary<string, string>> OnGetReferencesAsync<TEntity>(
        TEntity entity,
        AppDbContext db)
        where TEntity : class
    {
        return entity switch
        {
            ProductVariant variant => new Dictionary<string, string>
            {
                ["product"] = (await db.Products.SingleAsync(item => item.Id == variant.ProductId)).SyncId.ToString("D")
            },
            UserPermission userPermission => new Dictionary<string, string>
            {
                ["user"] = (await db.Users.SingleAsync(item => item.Id == userPermission.UserId)).SyncId.ToString("D"),
                ["permission"] = (await db.Permissions.SingleAsync(item => item.Id == userPermission.PermissionId)).SyncId.ToString("D")
            },
            OrderRequest request => new Dictionary<string, string>
            {
                ["user"] = (await db.Users.SingleAsync(item => item.Id == request.UserId)).SyncId.ToString("D")
            },
            OrderRequestItem item => new Dictionary<string, string>
            {
                ["orderRequest"] = (await db.OrderRequests.SingleAsync(order => order.Id == item.OrderRequestId)).SyncId.ToString("D"),
                ["productVariant"] = (await db.ProductVariants.SingleAsync(variant => variant.Id == item.ProductVariantId)).SyncId.ToString("D")
            },
            StockTransaction transaction => new Dictionary<string, string>
            {
                ["productVariant"] = (await db.ProductVariants.SingleAsync(variant => variant.Id == transaction.ProductVariantId)).SyncId.ToString("D"),
                ["user"] = (await db.Users.SingleAsync(user => user.Id == transaction.UserId)).SyncId.ToString("D")
            },
            _ => new Dictionary<string, string>()
        };
    }

    private async Task OnApplyReferencesAsync<TEntity>(
        TEntity entity,
        Dictionary<string, string> references,
        AppDbContext db)
        where TEntity : class
    {
        switch (entity)
        {
            case ProductVariant variant:
                variant.ProductId = await ResolveIdAsync(db.Products, references, "product");
                break;
            case UserPermission userPermission:
                userPermission.UserId = await ResolveIdAsync(db.Users, references, "user");
                userPermission.PermissionId = await ResolveIdAsync(db.Permissions, references, "permission");
                break;
            case OrderRequest request:
                request.UserId = await ResolveIdAsync(db.Users, references, "user");
                break;
            case OrderRequestItem item:
                item.OrderRequestId = await ResolveIdAsync(db.OrderRequests, references, "orderRequest");
                item.ProductVariantId = await ResolveIdAsync(db.ProductVariants, references, "productVariant");
                break;
            case StockTransaction transaction:
                transaction.ProductVariantId = await ResolveIdAsync(db.ProductVariants, references, "productVariant");
                transaction.UserId = await ResolveIdAsync(db.Users, references, "user");
                break;
        }
    }

    private static async Task<int> ResolveIdAsync<TEntity>(
        DbSet<TEntity> set,
        IReadOnlyDictionary<string, string> references,
        string name)
        where TEntity : class, ISyncTimestamped
    {
        if (!references.TryGetValue(name, out var value) || !Guid.TryParse(value, out var syncId))
        {
            throw new InvalidOperationException($"Firestore record is missing the '{name}' sync reference.");
        }

        var entity = await set.SingleOrDefaultAsync(item => item.SyncId == syncId);
        return entity?.GetType().GetProperty("Id")?.GetValue(entity) as int?
            ?? throw new InvalidOperationException($"Referenced '{name}' record {syncId} was not found locally.");
    }

    private async Task OnPullUserAsync(
        User user,
        FirestoreSyncRecord record,
        FirestoreDb firestore,
        CancellationToken cancellationToken)
    {
        var snapshot = await firestore.Collection("userCredentials")
            .Document(record.SyncId)
            .GetSnapshotAsync(cancellationToken);
        if (snapshot.Exists)
        {
            var credentials = snapshot.ConvertTo<FirestoreCredentialRecord>();
            user.PasswordHash = credentials.PasswordHash;
        }
    }

    private async Task OnPushUserAsync(
        User user,
        FirestoreDb firestore,
        CancellationToken cancellationToken)
    {
        var credentials = new FirestoreCredentialRecord
        {
            PasswordHash = user.PasswordHash,
            UpdatedAt = Timestamp.FromDateTime(EnsureUtc(user.UpdatedAt))
        };
        await firestore.Collection("userCredentials")
            .Document(user.SyncId.ToString("D"))
            .SetAsync(credentials, cancellationToken: cancellationToken);
    }

    private async Task SyncDeletedRecordsAsync(
        AppDbContext db,
        FirestoreDb firestore,
        CancellationToken cancellationToken)
    {
        var collection = firestore.Collection("deletedRecords");
        var query = _initialSyncCompleted
            ? collection.WhereGreaterThanOrEqualTo("deletedAt", Timestamp.FromDateTime(EnsureUtc(_lastSyncUtc)))
            : (Query)collection;
        var cloudSnapshot = await query.GetSnapshotAsync(cancellationToken);
        var local = await db.DeletedRecords.ToListAsync(cancellationToken);
        var localByKey = local.ToDictionary(item => $"{item.EntityType}|{item.RecordKey}");
        var cloudByKey = new Dictionary<string, (FirestoreTombstoneRecord Record, string DocumentId)>();

        foreach (var document in cloudSnapshot.Documents)
        {
            var record = document.ConvertTo<FirestoreTombstoneRecord>();
            var key = $"{record.EntityType}|{record.RecordKey}";
            cloudByKey[key] = (record, document.Id);
            if (!localByKey.TryGetValue(key, out var existing))
            {
                var tombstone = new DeletedRecord
                {
                    EntityType = record.EntityType,
                    RecordKey = record.RecordKey,
                    DeletedAt = EnsureUtc(record.DeletedAt.ToDateTime())
                };
                db.DeletedRecords.Add(tombstone);
                localByKey[key] = tombstone;
            }
            else if (record.DeletedAt.ToDateTime() > EnsureUtc(existing.DeletedAt))
            {
                existing.DeletedAt = EnsureUtc(record.DeletedAt.ToDateTime());
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        foreach (var tombstone in localByKey.Values)
        {
            var key = $"{tombstone.EntityType}|{tombstone.RecordKey}";
            if (cloudByKey.TryGetValue(key, out var cloud) &&
                cloud.Record.DeletedAt.ToDateTime() >= EnsureUtc(tombstone.DeletedAt))
            {
                continue;
            }

            var record = new FirestoreTombstoneRecord
            {
                EntityType = tombstone.EntityType,
                RecordKey = tombstone.RecordKey,
                DeletedAt = Timestamp.FromDateTime(EnsureUtc(tombstone.DeletedAt))
            };
            await collection.Document(CreateTombstoneDocumentId(key))
                .SetAsync(record, cancellationToken: cancellationToken);
        }

        await ApplyTombstonesToLocalAsync(db, localByKey.Values, cancellationToken);
    }

    private static async Task ApplyTombstonesToLocalAsync(
        AppDbContext db,
        IEnumerable<DeletedRecord> tombstones,
        CancellationToken cancellationToken)
    {
        var users = await db.Users.ToListAsync(cancellationToken);
        var products = await db.Products.ToListAsync(cancellationToken);
        var variants = await db.ProductVariants.ToListAsync(cancellationToken);

        foreach (var tombstone in tombstones)
        {
            var key = tombstone.RecordKey;
            var deletedAt = EnsureUtc(tombstone.DeletedAt);
            switch (tombstone.EntityType)
            {
                case "User":
                    db.Users.RemoveRange(users.Where(user =>
                        user.Username.Trim().ToLowerInvariant() == key &&
                        EnsureUtc(user.UpdatedAt) <= deletedAt));
                    break;
                case "Product":
                    db.Products.RemoveRange(products.Where(product =>
                        product.Name.Trim().ToLowerInvariant() == key &&
                        EnsureUtc(product.UpdatedAt) <= deletedAt));
                    break;
                case "ProductVariant":
                    var separator = key.IndexOf('|');
                    if (separator <= 0)
                    {
                        break;
                    }

                    var productKey = key[..separator];
                    var variantKey = key[(separator + 1)..];
                    var productIds = products
                        .Where(product => product.Name.Trim().ToLowerInvariant() == productKey)
                        .Select(product => product.Id)
                        .ToHashSet();
                    db.ProductVariants.RemoveRange(variants.Where(variant =>
                        productIds.Contains(variant.ProductId) &&
                        variant.Name.Trim().ToLowerInvariant() == variantKey &&
                        EnsureUtc(variant.UpdatedAt) <= deletedAt));
                    break;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string CreateTombstoneDocumentId(string key) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(key)))
            .ToLowerInvariant();

    private static DateTime EnsureUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}