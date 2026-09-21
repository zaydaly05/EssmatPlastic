namespace EsmatPlastic.API.DTOs.ProductVariants;

public class CreateProductVariantRequest
{
    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Size { get; set; }

    public string? Color { get; set; }

    public string? CapType { get; set; }

    public string? Material { get; set; }

    public string? ImagePath { get; set; }
}
