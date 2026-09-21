namespace EsmatPlastic.API.DTOs.ProductVariants;

public class ProductVariantResponse
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Size { get; set; }

    public string? Color { get; set; }

    public string? CapType { get; set; }

    public string? Material { get; set; }

    public string? ImagePath { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
