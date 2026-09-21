using EsmatPlastic.API.DTOs.Reports;
using EsmatPlastic.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EsmatPlastic.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("stock")]
    [Authorize(Policy = "Reports.View")]
    public async Task<ActionResult<List<StockReportResponse>>>
        GetStockReport()
    {
        var report =
            await _reportService.GetStockReportAsync();

        return Ok(report);
    }
}
