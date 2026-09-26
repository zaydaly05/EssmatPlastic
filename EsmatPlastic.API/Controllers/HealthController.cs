using EsmatPlastic.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EsmatPlastic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IDbConnectionManager _dbConnectionManager;
    private readonly FirestoreDbSyncBackgroundService _syncService;

    public HealthController(
        IDbConnectionManager dbConnectionManager,
        FirestoreDbSyncBackgroundService syncService)
    {
        _dbConnectionManager = dbConnectionManager;
        _syncService = syncService;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var mode = await _dbConnectionManager.EvaluateConnectionAsync();

        return Ok(new
        {
            status = "Healthy",
            architecture = "Local SQLite with bidirectional Firestore synchronization",
            mode = mode.ToString(),
            isOnline = true,
            isFirestoreOnline = _syncService.IsOnline,
            isNeonBackupOnline = false,
            primaryDatabase = "Local SQLite",
            lastSyncUtc = _syncService.LastSyncTimeUtc,
            timestampUtc = DateTime.UtcNow
        });
    }

    [HttpPost("sync")]
    public async Task<IActionResult> TriggerSync()
    {
        if (!_syncService.IsOnline)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "Firestore is offline or not configured; local SQLite is still available." });
        }

        await _syncService.PerformDatabaseSyncAsync();

        return Ok(new
        {
            message = "Database synchronization triggered successfully.",
            lastSyncUtc = _syncService.LastSyncTimeUtc
        });
    }
}
