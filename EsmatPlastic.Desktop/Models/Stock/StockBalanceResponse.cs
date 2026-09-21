namespace EsmatPlastic.Desktop.Models.Stock;

public class StockBalanceResponse
{
    public int ProductVariantId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string VariantName { get; set; } = string.Empty;

    public string? Size { get; set; }

    public string? Color { get; set; }

    public string? CapType { get; set; }

    public string? Material { get; set; }

    public string? ImagePath { get; set; }

    public decimal TotalIn { get; set; }

    public decimal TotalOut { get; set; }

    public decimal CurrentQuantity { get; set; }
}
