namespace EsmatPlastic.API.DTOs.OrderRequests;

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
}

public class OrderRequestResponseDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItemResponseDto> Items { get; set; } = new();
}

public class OrderItemResponseDto
{
    public int ProductVariantId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}
