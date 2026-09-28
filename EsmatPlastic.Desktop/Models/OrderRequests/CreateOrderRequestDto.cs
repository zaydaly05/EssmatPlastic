namespace EsmatPlastic.Desktop.Models.OrderRequests;

public class CreateOrderRequestDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public string VariantName { get; set; } = string.Empty;
}
