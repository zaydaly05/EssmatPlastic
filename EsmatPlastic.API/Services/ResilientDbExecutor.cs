using EsmatPlastic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API.Services;

public interface IResilientDbExecutor
{
    Task<T> ExecuteAsync<T>(Func<AppDbContext, Task<T>> operation);
    Task ExecuteAsync(Func<AppDbContext, Task> operation);
}

public class ResilientDbExecutor : IResilientDbExecutor
{
    private readonly IDbConnectionManager _connectionManager;

    public ResilientDbExecutor(IDbConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task<T> ExecuteAsync<T>(Func<AppDbContext, Task<T>> operation)
    {
        await using var db = CreateDbContext(_connectionManager.LocalConnectionString);
        return await operation(db);
    }

    public async Task ExecuteAsync(Func<AppDbContext, Task> operation)
    {
        await using var db = CreateDbContext(_connectionManager.LocalConnectionString);
        await operation(db);
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
}
