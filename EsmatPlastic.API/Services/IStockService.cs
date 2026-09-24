using EsmatPlastic.API.DTOs.Stock;

namespace EsmatPlastic.API.Services;

public interface IStockService
{
    Task<StockTransactionResponse?> CreateTransactionAsync(
        int userId,
        CreateStockTransactionRequest request);

    Task<List<StockTransactionResponse>> GetTransactionsAsync(
        int? productVariantId = null,
        int? take = null);

    Task<List<StockBalanceResponse>> GetCurrentStockAsync();

    Task<StockBalanceResponse?> GetVariantStockAsync(
        int productVariantId);
}
