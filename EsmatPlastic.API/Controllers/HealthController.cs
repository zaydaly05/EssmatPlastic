using EsmatPlastic.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EsmatPlastic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IDbConnectionManager _dbConnectionManager;

    public HealthController(IDbConnectionManager dbConnectionManager)
    {
        _dbConnectionManager = dbConnectionManager;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var mode = await _dbConnectionManager.EvaluateConnectionAsync();

        return Ok(new
        {
            status = "Healthy",
            architecture = "Neon primary with automatic local SQLite fallback",
            mode = mode.ToString(),
            isOnline = true,
            isNeonBackupOnline = _dbConnectionManager.IsNeonBackupAvailable,
            primaryDatabase = _dbConnectionManager.ActiveConnectionString.Contains("Host=") ? "Neon PostgreSQL" : "Local SQLite",
            lastSyncUtc = _dbConnectionManager.LastSyncTimeUtc,
            timestampUtc = DateTime.UtcNow
        });
    }

    [HttpPost("sync")]
    public async Task<IActionResult> TriggerSync([FromServices] DbSyncBackgroundService syncService)
    {
        if (!_dbConnectionManager.IsNeonBackupAvailable)
        {
            return BadRequest(new { message = "Cannot perform sync while Neon cloud backup is offline or unreachable." });
        }

        await syncService.PerformDatabaseSyncAsync();

        return Ok(new
        {
            message = "Database synchronization triggered successfully.",
            lastSyncUtc = _dbConnectionManager.LastSyncTimeUtc
        });
    }
}
