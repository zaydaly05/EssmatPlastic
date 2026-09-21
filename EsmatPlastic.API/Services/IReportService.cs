using EsmatPlastic.API.DTOs.Reports;

namespace EsmatPlastic.API.Services;

public interface IReportService
{
    Task<List<StockReportResponse>> GetStockReportAsync();
}
