using EsmatPlastic.Domain.Enums;

namespace EsmatPlastic.API.DTOs.Stock;

public class StockTransactionResponse
{
    public int Id { get; set; }

    public int ProductVariantId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string VariantName { get; set; } = string.Empty;

    public StockTransactionType Type { get; set; }

    public decimal Quantity { get; set; }

    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
