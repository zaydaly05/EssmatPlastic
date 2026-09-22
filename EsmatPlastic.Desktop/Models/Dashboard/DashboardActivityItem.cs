namespace EsmatPlastic.Desktop.Models.Dashboard;

public class DashboardActivityItem
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string TransactionTypeText { get; set; } = string.Empty; // "وارد" or "صادر"
    public string TransactionTypeColor { get; set; } = "#10B981"; // Green for In, Red for Out
    public string TransactionTypeBg { get; set; } = "#D1FAE5";
    public decimal Quantity { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
}
