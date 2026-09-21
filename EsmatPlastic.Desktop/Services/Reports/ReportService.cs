using EsmatPlastic.Desktop.Models.Reports;

namespace EsmatPlastic.Desktop.Services.Reports;

public class ReportService
{
    private readonly ApiClient _apiClient;

    public ReportService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<StockReportResponse>>
        GetStockReportAsync()
    {
        var result =
            await _apiClient.GetAsync
                <List<StockReportResponse>>(
                    "api/Reports/stock");

        return result ?? new List<StockReportResponse>();
    }
}
