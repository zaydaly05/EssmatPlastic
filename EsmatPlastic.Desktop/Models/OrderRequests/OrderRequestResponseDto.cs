namespace EsmatPlastic.Desktop.Models.OrderRequests;

public class OrderRequestResponseDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItemResponseDto> Items { get; set; } = new();

    public string RequestedAtFormatted => RequestedAt.ToLocalTime().ToString("yyyy/MM/dd HH:mm");

    public string ItemsSummary => Items != null && Items.Count > 0
        ? string.Join(", ", Items.Select(i => $"{i.VariantName} ({i.Quantity})"))
        : "لا يوجد أصناف";

    public string LocalizedStatus => Status switch
    {
        "Pending" => "قيد الانتظار",
        "Approved" => "تم القبول",
        "Processing" => "قيد المعالجة",
        "Completed" => "مكتمل",
        "Cancelled" => "ملغي",
        _ => Status
    };

    public string StatusBgColor => Status switch
    {
        "Pending" => "#FEF3C7",     // Yellow/Amber
        "Approved" => "#DBEAFE",    // Light Blue
        "Processing" => "#E0E7FF",  // Indigo
        "Completed" => "#D1FAE5",   // Light Green
        "Cancelled" => "#FEE2E2",   // Light Red
        _ => "#F3F4F6"
    };

    public string StatusFgColor => Status switch
    {
        "Pending" => "#D97706",
        "Approved" => "#2563EB",
        "Processing" => "#4F46E5",
        "Completed" => "#059669",
        "Cancelled" => "#DC2626",
        _ => "#4B5563"
    };
}

public class OrderItemResponseDto
{
    public int ProductVariantId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}
