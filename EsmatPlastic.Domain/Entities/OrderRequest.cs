namespace EsmatPlastic.Domain.Entities;

public class OrderRequest
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string? CustomerPhone { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public OrderRequestStatus Status { get; set; } = OrderRequestStatus.Pending;

    public int UserId { get; set; } // The Secretary who made the request

    public User User { get; set; } = null!;

    public List<OrderRequestItem> Items { get; set; } = new();
}

public class OrderRequestItem
{
    public int Id { get; set; }

    public int OrderRequestId { get; set; }
    public OrderRequest OrderRequest { get; set; } = null!;

    public int ProductVariantId { get; set; }

    public decimal Quantity { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;
}

public enum OrderRequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Completed = 4
}
