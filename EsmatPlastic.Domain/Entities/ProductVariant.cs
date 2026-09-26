namespace EsmatPlastic.Domain.Entities;

public class ProductVariant : ISyncTimestamped
{
    public int Id { get; set; }

    public Guid SyncId { get; set; }

    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Size { get; set; }

    public string? Color { get; set; }

    public string? CapType { get; set; }

    public string? Material { get; set; }

    public string? ImagePath { get; set; }

    public bool IsActive { get; set; } = true;

    public decimal ReservedQuantity { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
}   