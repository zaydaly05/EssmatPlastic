using EsmatPlastic.Domain.Enums;

namespace EsmatPlastic.API.DTOs.Stock;

public class CreateStockTransactionRequest
{
    public int ProductVariantId { get; set; }

    public StockTransactionType Type { get; set; }

    public decimal Quantity { get; set; }

    public string? Notes { get; set; }
}
