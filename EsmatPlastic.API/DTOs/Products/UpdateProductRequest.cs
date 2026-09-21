namespace EsmatPlastic.API.DTOs.Products;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImagePath { get; set; }

    public bool IsActive { get; set; } = true;
}
