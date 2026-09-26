namespace EsmatPlastic.Domain.Entities;

public class Product : ISyncTimestamped
{
    public int Id { get; set; }

    public Guid SyncId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImagePath { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProductVariant> Variants { get; set; }
        = new List<ProductVariant>();
}
