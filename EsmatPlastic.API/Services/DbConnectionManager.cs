namespace EsmatPlastic.API.Services;

public enum DatabaseProviderMode
{
    LocalSqlite,
    CloudFirestore
}

public interface IDbConnectionManager
{
    DatabaseProviderMode CurrentMode { get; }
    bool IsOnline { get; }
    bool IsFirestoreAvailable { get; }
    DateTime? LastSyncTimeUtc { get; }
    string ActiveConnectionString { get; }
    string LocalConnectionString { get; }
    string NeonConnectionString { get; }
    Task<DatabaseProviderMode> EvaluateConnectionAsync(CancellationToken cancellationToken = default);
}

public class DbConnectionManager : IDbConnectionManager
{
    private readonly string _neonConnectionString;
    private readonly string _localConnectionString;
    private readonly FirestoreDbSyncBackgroundService _firestoreSync;
    private readonly ILogger<DbConnectionManager> _logger;

    public DatabaseProviderMode CurrentMode
    {
        get => IsFirestoreAvailable
            ? DatabaseProviderMode.CloudFirestore
            : DatabaseProviderMode.LocalSqlite;
    }

    public bool IsOnline => _firestoreSync.IsOnline;
    public bool IsFirestoreAvailable => _firestoreSync.IsOnline;
    public DateTime? LastSyncTimeUtc => _firestoreSync.LastSyncTimeUtc;
    public string ActiveConnectionString => _localConnectionString;

    public string LocalConnectionString => _localConnectionString;
    public string NeonConnectionString => _neonConnectionString;

    public DbConnectionManager(
        string neonConnectionString,
        string localConnectionString,
        FirestoreDbSyncBackgroundService firestoreSync,
        ILogger<DbConnectionManager> logger)
    {
        _neonConnectionString = neonConnectionString;
        _localConnectionString = localConnectionString;
        _firestoreSync = firestoreSync;
        _logger = logger;
    }

    public Task<DatabaseProviderMode> EvaluateConnectionAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var mode = CurrentMode;
        _logger.LogDebug("Database mode is {Mode}; EF Core operations remain on local SQLite.", mode);
        return Task.FromResult(mode);
    }
}
