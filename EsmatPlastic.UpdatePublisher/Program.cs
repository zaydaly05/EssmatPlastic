using Npgsql;

if (args.Length != 2 || !Version.TryParse(args[0], out _) ||
    !Uri.TryCreate(args[1], UriKind.Absolute, out var downloadUri) ||
    downloadUri.Scheme != Uri.UriSchemeHttps)
{
    Console.Error.WriteLine("Usage: EsmatPlastic.UpdatePublisher <version> <https-download-url>");
    return 2;
}

var rawConnectionString = Environment.GetEnvironmentVariable("NEON_DATABASE_URL");
if (string.IsNullOrWhiteSpace(rawConnectionString))
{
    Console.Error.WriteLine("NEON_DATABASE_URL must be configured as a repository Actions secret.");
    return 3;
}

await using var connection = new NpgsqlConnection(ToConnectionString(rawConnectionString));
await connection.OpenAsync();

const string createTableSql = """
    CREATE TABLE IF NOT EXISTS "AppUpdates" (
        "Id" integer PRIMARY KEY CHECK ("Id" = 1),
        "Version" text NOT NULL,
        "DownloadUrl" text NOT NULL,
        "PublishedAt" timestamp with time zone NOT NULL
    );
    """;
await using (var createTable = new NpgsqlCommand(createTableSql, connection))
    await createTable.ExecuteNonQueryAsync();

const string upsertSql = """
    INSERT INTO "AppUpdates" ("Id", "Version", "DownloadUrl", "PublishedAt")
    VALUES (1, @version, @downloadUrl, @publishedAt)
    ON CONFLICT ("Id") DO UPDATE SET
        "Version" = EXCLUDED."Version",
        "DownloadUrl" = EXCLUDED."DownloadUrl",
        "PublishedAt" = EXCLUDED."PublishedAt";
    """;

await using var command = new NpgsqlCommand(upsertSql, connection);
command.Parameters.AddWithValue("version", args[0]);
command.Parameters.AddWithValue("downloadUrl", downloadUri.ToString());
command.Parameters.AddWithValue("publishedAt", DateTime.UtcNow);
await command.ExecuteNonQueryAsync();
Console.WriteLine($"Published update metadata for version {args[0]}.");
return 0;

static string ToConnectionString(string value)
{
    value = value.Trim().Trim('"').Trim('\'');
    if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
        !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        return value;
    }

    var uri = new Uri(value);
    var userInfo = uri.UserInfo.Split(':', 2);
    if (userInfo.Length != 2)
        throw new InvalidOperationException("NEON_DATABASE_URL does not contain database credentials.");

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort || uri.Port < 0 ? 5432 : uri.Port,
        Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = Uri.UnescapeDataString(userInfo[1]),
        SslMode = SslMode.Require
    };

    return builder.ConnectionString;
}
