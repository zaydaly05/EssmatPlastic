using EsmatPlastic.API.DTOs.ProductVariants;
using EsmatPlastic.Domain.Entities;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API.Services;

public class ProductVariantService : IProductVariantService
{
    private readonly IResilientDbExecutor _executor;
    private readonly IDbSyncTrigger _syncTrigger;

    public ProductVariantService(IResilientDbExecutor executor, IDbSyncTrigger syncTrigger)
    {
        _executor = executor;
        _syncTrigger = syncTrigger;
    }

    public async Task<List<ProductVariantResponse>> GetByProductIdAsync(int productId)
    {
        return await _executor.ExecuteAsync(async db =>
            await db.ProductVariants
                .AsNoTracking()
                .Where(x => x.ProductId == productId)
                .OrderBy(x => x.Name)
                .Select(x => new ProductVariantResponse
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ProductName = x.Product.Name,
                    Name = x.Name,
                    Size = x.Size,
                    Color = x.Color,
                    CapType = x.CapType,
                    Material = x.Material,
                    ImagePath = x.ImagePath,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync());
    }

    public async Task<ProductVariantResponse?> GetByIdAsync(int id)
    {
        return await _executor.ExecuteAsync(async db =>
            await db.ProductVariants
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ProductVariantResponse
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ProductName = x.Product.Name,
                    Name = x.Name,
                    Size = x.Size,
                    Color = x.Color,
                    CapType = x.CapType,
                    Material = x.Material,
                    ImagePath = x.ImagePath,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt
                })
                .FirstOrDefaultAsync());
    }

    public async Task<ProductVariantResponse?> CreateAsync(CreateProductVariantRequest request)
    {
        if (request.ProductId <= 0)
        {
            throw new InvalidOperationException("A valid product is required.");
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return await _executor.ExecuteAsync(async db =>
        {
            var productExists = await db.Products.AnyAsync(x => x.Id == request.ProductId && x.IsActive);
            if (!productExists)
            {
                throw new InvalidOperationException("The selected product does not exist or is inactive.");
            }

            var duplicate = await db.ProductVariants.AnyAsync(x => x.ProductId == request.ProductId && x.Name == name);
            if (duplicate)
            {
                throw new InvalidOperationException("A variant with the same name already exists for this product.");
            }

            var variant = new ProductVariant
            {
                ProductId = request.ProductId,
                Name = name,
                Size = request.Size?.Trim(),
                Color = request.Color?.Trim(),
                CapType = request.CapType?.Trim(),
                Material = request.Material?.Trim(),
                ImagePath = request.ImagePath?.Trim(),
                IsActive = true
            };

            db.ProductVariants.Add(variant);
            await db.SaveChangesAsync();
            _syncTrigger.TriggerSync();

            return await db.ProductVariants
                .AsNoTracking()
                .Where(x => x.Id == variant.Id)
                .Select(x => new ProductVariantResponse
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ProductName = x.Product.Name,
                    Name = x.Name,
                    Size = x.Size,
                    Color = x.Color,
                    CapType = x.CapType,
                    Material = x.Material,
                    ImagePath = x.ImagePath,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt
                })
                .FirstOrDefaultAsync();
        });
    }

    public async Task<ProductVariantResponse?> UpdateAsync(int id, UpdateProductVariantRequest request)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Variant name is required.");
        }

        return await _executor.ExecuteAsync(async db =>
        {
            var variant = await db.ProductVariants.FirstOrDefaultAsync(x => x.Id == id);
            if (variant is null) return null;

            var duplicate = await db.ProductVariants.AnyAsync(x => x.Id != id && x.ProductId == variant.ProductId && x.Name == name);
            if (duplicate)
            {
                throw new InvalidOperationException("A variant with the same name already exists for this product.");
            }

            variant.Name = name;
            variant.Size = request.Size?.Trim();
            variant.Color = request.Color?.Trim();
            variant.CapType = request.CapType?.Trim();
            variant.Material = request.Material?.Trim();
            variant.ImagePath = request.ImagePath?.Trim();
            variant.IsActive = request.IsActive;
            variant.CreatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            _syncTrigger.TriggerSync();

            return await db.ProductVariants
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ProductVariantResponse
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ProductName = x.Product.Name,
                    Name = x.Name,
                    Size = x.Size,
                    Color = x.Color,
                    CapType = x.CapType,
                    Material = x.Material,
                    ImagePath = x.ImagePath,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt
                })
                .FirstOrDefaultAsync();
        });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _executor.ExecuteAsync(async db =>
        {
            var variant = await db.ProductVariants.FirstOrDefaultAsync(x => x.Id == id);
            if (variant is null) return false;

            var hasTransactions = await db.StockTransactions.AnyAsync(x => x.ProductVariantId == id);
            if (hasTransactions)
            {
                throw new InvalidOperationException("Cannot delete a variant that has stock transactions.");
            }

            var productName = await db.Products
                .Where(x => x.Id == variant.ProductId)
                .Select(x => x.Name)
                .FirstAsync();

            db.ProductVariants.Remove(variant);
            db.DeletedRecords.Add(new DeletedRecord
            {
                EntityType = "ProductVariant",
                RecordKey = $"{productName.Trim().ToLowerInvariant()}|{variant.Name.Trim().ToLowerInvariant()}",
                DeletedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
            _syncTrigger.TriggerSync();
            return true;
        });
    }
}
