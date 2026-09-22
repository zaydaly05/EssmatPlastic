using System.Data.Common;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EsmatPlastic.API.Services;

public enum DatabaseProviderMode
{
    LocalNetworkPrimary,
    CloudNeonBackup
}

public interface IDbConnectionManager
{
    DatabaseProviderMode CurrentMode { get; }
    bool IsOnline { get; }
    bool IsNeonBackupAvailable { get; }
    DateTime? LastSyncTimeUtc { get; }
    string ActiveConnectionString { get; }
    string NeonConnectionString { get; }
    Task<DatabaseProviderMode> EvaluateConnectionAsync(CancellationToken cancellationToken = default);
    void UpdateLastSyncTime();
}

public class DbConnectionManager : IDbConnectionManager
{
    private readonly string _neonConnectionString;
    private readonly string _localConnectionString;
    private readonly ILogger<DbConnectionManager> _logger;

    private DatabaseProviderMode _currentMode = DatabaseProviderMode.LocalNetworkPrimary;
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

    public string ActiveConnectionString => _localConnectionString;
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
        CurrentMode = DatabaseProviderMode.LocalNetworkPrimary;

        if (string.IsNullOrWhiteSpace(_neonConnectionString))
        {
            IsOnline = false;
            _logger.LogInformation("Option B Local LAN Primary Database active. Neon cloud backup is unconfigured.");
            return DatabaseProviderMode.LocalNetworkPrimary;
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
            _logger.LogInformation("Option B Local LAN Primary Database active. NEON CLOUD BACKUP IS ONLINE & REACHABLE.");
            return DatabaseProviderMode.LocalNetworkPrimary;
        }
        catch (Exception ex)
        {
            IsOnline = false;
            _logger.LogWarning(ex, "Option B Local LAN Primary Database active. Neon cloud backup is currently offline or unreachable.");
            return DatabaseProviderMode.LocalNetworkPrimary;
        }
    }
}
