using EsmatPlastic.API.Services;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace EsmatPlastic.API.Controllers;

[ApiController]
[Route("api/updates")]
public sealed class UpdatesController : ControllerBase
{
    private readonly IDbConnectionManager _connectionManager;

    public UpdatesController(IDbConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest(CancellationToken cancellationToken)
    {
        var connectionString = _connectionManager.NeonConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return NoContent();
        }

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand(
                "SELECT \"Version\", \"DownloadUrl\", \"PublishedAt\" FROM \"AppUpdates\" WHERE \"Id\" = 1",
                connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
                return NoContent();

            return Ok(new LatestUpdateResponse(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetFieldValue<DateTime>(2)));
        }
        catch (PostgresException ex) when (ex.SqlState == "42P01")
        {
            // The updates table is created by the release publisher when the first build is published.
            return NoContent();
        }
        catch (NpgsqlException)
        {
            return NoContent();
        }
    }
}

public sealed record LatestUpdateResponse(string Version, string DownloadUrl, DateTime PublishedAt);
