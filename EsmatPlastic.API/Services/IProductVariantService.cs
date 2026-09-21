using EsmatPlastic.API.DTOs.ProductVariants;

namespace EsmatPlastic.API.Services;

public interface IProductVariantService
{
    Task<List<ProductVariantResponse>> GetByProductIdAsync(int productId);

    Task<ProductVariantResponse?> GetByIdAsync(int id);

    Task<ProductVariantResponse?> CreateAsync(
        CreateProductVariantRequest request);

    Task<ProductVariantResponse?> UpdateAsync(
        int id,
        UpdateProductVariantRequest request);

    Task<bool> DeleteAsync(int id);
}
