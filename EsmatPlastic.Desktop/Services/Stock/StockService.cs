using EsmatPlastic.Desktop.Models.Stock;

namespace EsmatPlastic.Desktop.Services.Stock;

public class StockService
{
    private readonly ApiClient _apiClient;

    public StockService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<StockBalanceResponse>>
        GetCurrentStockAsync()
    {
        var result =
            await _apiClient.GetAsync
                <List<StockBalanceResponse>>(
                    "api/Stock/current");

        return result ?? new List<StockBalanceResponse>();
    }

    public async Task<StockBalanceResponse?>
        GetVariantStockAsync(int productVariantId)
    {
        return await _apiClient.GetAsync
            <StockBalanceResponse>(
                $"api/Stock/current/{productVariantId}");
    }

    public async Task<List<StockTransactionResponse>>
        GetTransactionsAsync(
            int? productVariantId = null)
    {
        var endpoint =
            "api/Stock/transactions";

        if (productVariantId.HasValue)
        {
            endpoint +=
                $"?productVariantId={productVariantId.Value}";
        }

        var result =
            await _apiClient.GetAsync
                <List<StockTransactionResponse>>(
                    endpoint);

        return result ?? new List<StockTransactionResponse>();
    }

    public async Task<StockTransactionResponse?>
        CreateTransactionAsync(
            CreateStockTransactionRequest request)
    {
        return await _apiClient.PostAsync
            <CreateStockTransactionRequest,
             StockTransactionResponse>(
                "api/Stock/transaction",
                request);
    }
}
