using EsmatPlastic.API.DTOs.Products;
using EsmatPlastic.Domain.Entities;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API.Services;

public class ProductService : IProductService
{
    private readonly IResilientDbExecutor _executor;
    private readonly IDbSyncTrigger _syncTrigger;

    public ProductService(IResilientDbExecutor executor, IDbSyncTrigger syncTrigger)
    {
        _executor = executor;
        _syncTrigger = syncTrigger;
    }

    public async Task<List<ProductResponse>> GetAllAsync()
    {
        return await _executor.ExecuteAsync(async db =>
            await db.Products
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new ProductResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ImagePath = x.ImagePath,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    VariantCount = x.Variants.Count
                })
                .ToListAsync());
    }

    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        return await _executor.ExecuteAsync(async db =>
            await db.Products
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ProductResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    ImagePath = x.ImagePath,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    VariantCount = x.Variants.Count
                })
                .FirstOrDefaultAsync());
    }

    public async Task<ProductResponse?> CreateAsync(CreateProductRequest request)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return await _executor.ExecuteAsync(async db =>
        {
            var exists = await db.Products.AnyAsync(x => x.Name == name);
            if (exists)
            {
                throw new InvalidOperationException("A product with the same name already exists.");
            }

            var product = new Product
            {
                Name = name,
                Description = request.Description?.Trim(),
                ImagePath = request.ImagePath?.Trim(),
                IsActive = true
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();
            _syncTrigger.TriggerSync();

            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ImagePath = product.ImagePath,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                VariantCount = 0
            };
        });
    }

    public async Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Product name is required.");
        }

        return await _executor.ExecuteAsync(async db =>
        {
            var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product is null) return null;

            var duplicate = await db.Products.AnyAsync(x => x.Id != id && x.Name == name);
            if (duplicate)
            {
                throw new InvalidOperationException("A product with the same name already exists.");
            }

            product.Name = name;
            product.Description = request.Description?.Trim();
            product.ImagePath = request.ImagePath?.Trim();
            product.IsActive = request.IsActive;
            product.CreatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            _syncTrigger.TriggerSync();

            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ImagePath = product.ImagePath,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                VariantCount = product.Variants.Count
            };
        });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _executor.ExecuteAsync(async db =>
        {
            var product = await db.Products
                .Include(x => x.Variants)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product is null) return false;

            if (product.Variants.Count > 0)
            {
                throw new InvalidOperationException("Cannot delete a product that has variants.");
            }

            db.Products.Remove(product);
            db.DeletedRecords.Add(new DeletedRecord
            {
                EntityType = "Product",
                RecordKey = product.Name.Trim().ToLowerInvariant(),
                DeletedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
            _syncTrigger.TriggerSync();
            return true;
        });
    }
}
