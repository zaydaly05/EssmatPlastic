using EsmatPlastic.API.DTOs.Products;
using EsmatPlastic.Domain.Entities;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProductResponse>> GetAllAsync()
    {
        return await _db.Products
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
            .ToListAsync();
    }

    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        return await _db.Products
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
            .FirstOrDefaultAsync();
    }

    public async Task<ProductResponse?> CreateAsync(
        CreateProductRequest request)
    {
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var exists = await _db.Products
            .AnyAsync(x => x.Name == name);

        if (exists)
        {
            throw new InvalidOperationException(
                "A product with the same name already exists.");
        }

        var product = new Product
        {
            Name = name,
            Description = request.Description?.Trim(),
            ImagePath = request.ImagePath?.Trim(),
            IsActive = true
        };

        _db.Products.Add(product);

        await _db.SaveChangesAsync();

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
    }

    public async Task<ProductResponse?> UpdateAsync(
        int id,
        UpdateProductRequest request)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product is null)
        {
            return null;
        }

        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException(
                "Product name is required.");
        }

        var duplicate = await _db.Products
            .AnyAsync(x =>
                x.Id != id &&
                x.Name == name);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "A product with the same name already exists.");
        }

        product.Name = name;
        product.Description = request.Description?.Trim();
        product.ImagePath = request.ImagePath?.Trim();
        product.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _db.Products
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product is null)
        {
            return false;
        }

        if (product.Variants.Count > 0)
        {
            throw new InvalidOperationException(
                "Cannot delete a product that has variants.");
        }

        _db.Products.Remove(product);

        await _db.SaveChangesAsync();

        return true;
    }
}
