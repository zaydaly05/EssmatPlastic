using EsmatPlastic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Net.Sockets;

namespace EsmatPlastic.API.Services;

public interface IResilientDbExecutor
{
    Task<T> ExecuteAsync<T>(Func<AppDbContext, Task<T>> operation);
    Task ExecuteAsync(Func<AppDbContext, Task> operation);
}

public class ResilientDbExecutor : IResilientDbExecutor
{
    private readonly IDbConnectionManager _connectionManager;
    private readonly ILogger<ResilientDbExecutor> _logger;

    public ResilientDbExecutor(
        IDbConnectionManager connectionManager,
        ILogger<ResilientDbExecutor> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(Func<AppDbContext, Task<T>> operation)
    {
        await _connectionManager.EvaluateConnectionAsync();

        try
        {
            await using var db = CreateDbContext(_connectionManager.ActiveConnectionString);
            return await operation(db);
        }
        catch (Exception ex) when (_connectionManager.CurrentMode == DatabaseProviderMode.CloudNeon && IsNetworkOrConnectionException(ex))
        {
            _logger.LogWarning(ex, "Neon request failed; retrying against local SQLite.");
            await _connectionManager.EvaluateConnectionAsync();
            await using var localDb = CreateDbContext(_connectionManager.ActiveConnectionString);
            return await operation(localDb);
        }
    }

    public async Task ExecuteAsync(Func<AppDbContext, Task> operation)
    {
        await _connectionManager.EvaluateConnectionAsync();

        try
        {
            await using var db = CreateDbContext(_connectionManager.ActiveConnectionString);
            await operation(db);
        }
        catch (Exception ex) when (_connectionManager.CurrentMode == DatabaseProviderMode.CloudNeon && IsNetworkOrConnectionException(ex))
        {
            _logger.LogWarning(ex, "Neon request failed; retrying against local SQLite.");
            await _connectionManager.EvaluateConnectionAsync();
            await using var localDb = CreateDbContext(_connectionManager.ActiveConnectionString);
            await operation(localDb);
        }
    }

    private static AppDbContext CreateDbContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
        else
        {
            optionsBuilder.UseSqlite(connectionString);
        }

        return new AppDbContext(optionsBuilder.Options);
    }

    private static bool IsNetworkOrConnectionException(Exception ex)
    {
        if (ex is NpgsqlException ||
            ex is SocketException ||
            ex is TimeoutException ||
            ex is IOException ||
            ex is DbUpdateException)
        {
            return true;
        }

        if (ex.InnerException != null)
        {
            return IsNetworkOrConnectionException(ex.InnerException);
        }

        return false;
    }
}
