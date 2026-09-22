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
    private readonly AppDbContext _primaryDb;
    private readonly IDbConnectionManager _connectionManager;
    private readonly ILogger<ResilientDbExecutor> _logger;

    public ResilientDbExecutor(
        AppDbContext primaryDb,
        IDbConnectionManager connectionManager,
        ILogger<ResilientDbExecutor> logger)
    {
        _primaryDb = primaryDb;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(Func<AppDbContext, Task<T>> operation)
    {
        return await operation(_primaryDb);
    }

    public async Task ExecuteAsync(Func<AppDbContext, Task> operation)
    {
        await operation(_primaryDb);
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
