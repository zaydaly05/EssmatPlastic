using EsmatPlastic.Desktop.Models.Products;

namespace EsmatPlastic.Desktop.Services.Products;

public class ProductService
{
    private readonly ApiClient _apiClient;

    public ProductService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<ProductResponse>> GetAllAsync()
    {
        var result =
            await _apiClient.GetAsync<List<ProductResponse>>(
                "api/Products");

        return result ?? new List<ProductResponse>();
    }

    public async Task<ProductResponse?> CreateAsync(
        CreateProductRequest request)
    {
        return await _apiClient.PostAsync
            <CreateProductRequest, ProductResponse>(
                "api/Products",
                request);
    }

    public async Task<ProductResponse?> UpdateAsync(
        int id,
        UpdateProductRequest request)
    {
        return await _apiClient.PutAsync
            <UpdateProductRequest, ProductResponse>(
                $"api/Products/{id}",
                request);
    }

    public async Task DeleteAsync(int id)
    {
        await _apiClient.DeleteAsync(
            $"api/Products/{id}");
    }
}
