using EsmatPlastic.Desktop.Models.Stock;

namespace EsmatPlastic.Desktop.Models.Dashboard;

public class DashboardSummary
{
    public int ProductCount { get; set; }
    public int VariantCount { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal TotalIn { get; set; }
    public decimal TotalOut { get; set; }
    public int LowStockCount { get; set; }
    public List<StockTransactionResponse> RecentTransactions { get; set; } = new();
    public List<StockBalanceResponse> LowStockItems { get; set; } = new();
}
