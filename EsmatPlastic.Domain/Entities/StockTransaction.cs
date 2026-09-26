using EsmatPlastic.Domain.Enums;

namespace EsmatPlastic.Domain.Entities;

public class StockTransaction : ISyncTimestamped
{
    public int Id { get; set; }

    public Guid SyncId { get; set; }

    public int ProductVariantId { get; set; }

    public StockTransactionType Type { get; set; }

    public decimal Quantity { get; set; }

    public int UserId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ProductVariant ProductVariant { get; set; } = null!;

    public User User { get; set; } = null!;
}

