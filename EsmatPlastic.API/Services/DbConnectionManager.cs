using System.Data.Common;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EsmatPlastic.API.Services;

public enum DatabaseProviderMode
{
    LocalSqlite,
    CloudNeon
}

public interface IDbConnectionManager
{
    DatabaseProviderMode CurrentMode { get; }
    bool IsOnline { get; }
    bool IsNeonBackupAvailable { get; }
    DateTime? LastSyncTimeUtc { get; }
    string ActiveConnectionString { get; }
    string LocalConnectionString { get; }
    string NeonConnectionString { get; }
    Task<DatabaseProviderMode> EvaluateConnectionAsync(CancellationToken cancellationToken = default);
    void UpdateLastSyncTime();
}

public class DbConnectionManager : IDbConnectionManager
{
    private readonly string _neonConnectionString;
    private readonly string _localConnectionString;
    private readonly ILogger<DbConnectionManager> _logger;

    private DatabaseProviderMode _currentMode = DatabaseProviderMode.LocalSqlite;
    private bool _isOnline = false;
    private DateTime? _lastSyncTimeUtc;
    private readonly object _lock = new();

    public DatabaseProviderMode CurrentMode
    {
        get { lock (_lock) return _currentMode; }
        private set { lock (_lock) _currentMode = value; }
    }

    public bool IsOnline
    {
        get { lock (_lock) return _isOnline; }
        private set { lock (_lock) _isOnline = value; }
    }

    public bool IsNeonBackupAvailable => IsOnline;

    public DateTime? LastSyncTimeUtc
    {
        get { lock (_lock) return _lastSyncTimeUtc; }
    }

    public string ActiveConnectionString
    {
        get
        {
            lock (_lock)
            {
                return _currentMode == DatabaseProviderMode.CloudNeon
                    ? _neonConnectionString
                    : _localConnectionString;
            }
        }
    }

    public string LocalConnectionString => _localConnectionString;
    public string NeonConnectionString => _neonConnectionString;

    public DbConnectionManager(
        string neonConnectionString,
        string localConnectionString,
        ILogger<DbConnectionManager> logger)
    {
        _neonConnectionString = neonConnectionString;
        _localConnectionString = localConnectionString;
        _logger = logger;
    }

    public void UpdateLastSyncTime()
    {
        lock (_lock)
        {
            _lastSyncTimeUtc = DateTime.UtcNow;
        }
    }

    public async Task<DatabaseProviderMode> EvaluateConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_neonConnectionString))
        {
            IsOnline = false;
            CurrentMode = DatabaseProviderMode.LocalSqlite;
            _logger.LogInformation("Local SQLite database active. Neon is unconfigured.");
            return CurrentMode;
        }

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            await using var conn = new NpgsqlConnection(_neonConnectionString);
            await conn.OpenAsync(cts.Token);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1;";
            await cmd.ExecuteScalarAsync(cts.Token);

            IsOnline = true;
            CurrentMode = DatabaseProviderMode.CloudNeon;
            _logger.LogInformation("Neon database is online and is now the active read/write database.");
            return CurrentMode;
        }
        catch (Exception ex)
        {
            IsOnline = false;
            CurrentMode = DatabaseProviderMode.LocalSqlite;
            _logger.LogWarning(ex, "Neon is unavailable; using local SQLite for reads and writes.");
            return CurrentMode;
        }
    }
}
