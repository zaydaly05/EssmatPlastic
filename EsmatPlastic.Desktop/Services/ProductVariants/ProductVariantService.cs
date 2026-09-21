using EsmatPlastic.Desktop.Models.ProductVariants;

namespace EsmatPlastic.Desktop.Services.ProductVariants;

public class ProductVariantService
{
    private readonly ApiClient _apiClient;

    public ProductVariantService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<ProductVariantResponse>>
        GetByProductIdAsync(int productId)
    {
        var result =
            await _apiClient.GetAsync
                <List<ProductVariantResponse>>(
                    $"api/ProductVariants/product/{productId}");

        return result ?? new List<ProductVariantResponse>();
    }

    public async Task<ProductVariantResponse?>
        GetByIdAsync(int id)
    {
        return await _apiClient.GetAsync
            <ProductVariantResponse>(
                $"api/ProductVariants/{id}");
    }

    public async Task<ProductVariantResponse?>
        CreateAsync(
            CreateProductVariantRequest request)
    {
        return await _apiClient.PostAsync
            <CreateProductVariantRequest, ProductVariantResponse>(
                "api/ProductVariants",
                request);
    }

    public async Task<ProductVariantResponse?>
        UpdateAsync(
            int id,
            UpdateProductVariantRequest request)
    {
        return await _apiClient.PutAsync
            <UpdateProductVariantRequest, ProductVariantResponse>(
                $"api/ProductVariants/{id}",
                request);
    }

    public async Task DeleteAsync(int id)
    {
        await _apiClient.DeleteAsync(
            $"api/ProductVariants/{id}");
    }
}
