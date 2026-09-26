namespace EsmatPlastic.Shared.Models.Dashboard;

public sealed class DashboardProductSummary
{
    public int Id { get; set; }
    public int VariantCount { get; set; }
}

public sealed class DashboardStockBalance
{
    public decimal TotalIn { get; set; }
    public decimal TotalOut { get; set; }
    public decimal CurrentQuantity { get; set; }
}