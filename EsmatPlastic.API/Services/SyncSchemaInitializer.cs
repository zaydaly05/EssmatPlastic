using System.Data.Common;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API.Services;

public static class SyncSchemaInitializer
{
    private const string Epoch = "1970-01-01 00:00:00";

    private static readonly (string Table, string? TimestampColumn)[] Tables =
    [
        ("Users", "CreatedAt"),
        ("Products", "CreatedAt"),
        ("ProductVariants", "CreatedAt"),
        ("StockTransactions", "CreatedAt"),
        ("Permissions", null),
        ("UserPermissions", null),
        ("OrderRequests", "RequestedAt"),
        ("OrderRequestItems", null)
    ];

    public static async Task EnsureAsync(
        AppDbContext db,
        CancellationToken cancellationToken = default)
    {
        var provider = db.Database.ProviderName ?? string.Empty;
        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            foreach (var (table, timestampColumn) in Tables)
            {
                if (provider.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
                {
                    await EnsurePostgresColumnsAsync(
                        connection,
                        table,
                        timestampColumn,
                        cancellationToken);
                }
                else
                {
                    await EnsureSqliteColumnsAsync(
                        connection,
                        table,
                        timestampColumn,
                        cancellationToken);
                }
            }
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    private static async Task EnsureSqliteColumnsAsync(
        DbConnection connection,
        string table,
        string? timestampColumn,
        CancellationToken cancellationToken)
    {
        if (!await HasSqliteColumnAsync(connection, table, "UpdatedAt", cancellationToken))
        {
            await ExecuteAsync(
                connection,
                $"ALTER TABLE \"{table}\" ADD COLUMN \"UpdatedAt\" TEXT NOT NULL DEFAULT '{Epoch}';",
                cancellationToken);

            if (timestampColumn is not null)
            {
                await ExecuteAsync(
                    connection,
                    $"UPDATE \"{table}\" SET \"UpdatedAt\" = \"{timestampColumn}\";",
                    cancellationToken);
            }
        }

        if (!await HasSqliteColumnAsync(connection, table, "SyncId", cancellationToken))
        {
            const string emptyGuid = "00000000-0000-0000-0000-000000000000";
            await ExecuteAsync(
                connection,
                $"ALTER TABLE \"{table}\" ADD COLUMN \"SyncId\" TEXT NOT NULL DEFAULT '{emptyGuid}';",
                cancellationToken);
            await ExecuteAsync(
                connection,
                $"UPDATE \"{table}\" SET \"SyncId\" = lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || substr(lower(hex(randomblob(2))), 2) || '-a' || substr(lower(hex(randomblob(2))), 2) || '-' || lower(hex(randomblob(6))) WHERE \"SyncId\" = '{emptyGuid}';",
                cancellationToken);
        }

        await ExecuteAsync(
            connection,
            $"CREATE UNIQUE INDEX IF NOT EXISTS \"IX_{table}_SyncId\" ON \"{table}\" (\"SyncId\");",
            cancellationToken);
    }

    private static async Task EnsurePostgresColumnsAsync(
        DbConnection connection,
        string table,
        string? timestampColumn,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            connection,
            $"ALTER TABLE \"{table}\" ADD COLUMN IF NOT EXISTS \"UpdatedAt\" timestamp with time zone NOT NULL DEFAULT '{Epoch}+00'::timestamptz;",
            cancellationToken);

        if (timestampColumn is not null)
        {
            await ExecuteAsync(
                connection,
                $"UPDATE \"{table}\" SET \"UpdatedAt\" = \"{timestampColumn}\" WHERE \"UpdatedAt\" = '{Epoch}+00'::timestamptz;",
                cancellationToken);
        }

        await ExecuteAsync(
            connection,
            $"ALTER TABLE \"{table}\" ADD COLUMN IF NOT EXISTS \"SyncId\" uuid NOT NULL DEFAULT gen_random_uuid();",
            cancellationToken);
        await ExecuteAsync(
            connection,
            $"CREATE UNIQUE INDEX IF NOT EXISTS \"IX_{table}_SyncId\" ON \"{table}\" (\"SyncId\");",
            cancellationToken);
    }

    private static async Task<bool> HasSqliteColumnAsync(
        DbConnection connection,
        string table,
        string column,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info(\"{table}\");";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task ExecuteAsync(
        DbConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}