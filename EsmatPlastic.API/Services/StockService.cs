using EsmatPlastic.API.DTOs.Stock;
using EsmatPlastic.Domain.Entities;
using EsmatPlastic.Domain.Enums;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API.Services;

public class StockService : IStockService
{
    private readonly AppDbContext _db;

    public StockService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<StockTransactionResponse?> CreateTransactionAsync(
        int userId,
        CreateStockTransactionRequest request)
    {
        if (request.ProductVariantId <= 0)
        {
            throw new InvalidOperationException(
                "A valid product variant is required.");
        }

        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException(
                "Quantity must be greater than zero.");
        }

        if (request.Type != StockTransactionType.In &&
            request.Type != StockTransactionType.Out)
        {
            throw new InvalidOperationException(
                "Invalid stock transaction type.");
        }

        var variant = await _db.ProductVariants
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.Id == request.ProductVariantId &&
                x.IsActive &&
                x.Product.IsActive);

        if (variant is null)
        {
            throw new InvalidOperationException(
                "The selected product variant does not exist or is inactive.");
        }

        var userExists = await _db.Users
            .AnyAsync(x =>
                x.Id == userId &&
                x.IsActive);

        if (!userExists)
        {
            throw new InvalidOperationException(
                "The current user does not exist or is inactive.");
        }

        if (request.Type == StockTransactionType.Out)
        {
            var currentStock = await CalculateStockAsync(
                request.ProductVariantId);

            if (request.Quantity > currentStock)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock. Current quantity: {currentStock}.");
            }
        }

        var transaction = new StockTransaction
        {
            ProductVariantId = request.ProductVariantId,
            Type = request.Type,
            Quantity = request.Quantity,
            UserId = userId,
            Notes = request.Notes?.Trim()
        };

        _db.StockTransactions.Add(transaction);

        await _db.SaveChangesAsync();

        return await GetTransactionByIdAsync(transaction.Id);
    }

    public async Task<List<StockTransactionResponse>> GetTransactionsAsync(
        int? productVariantId = null)
    {
        var query = _db.StockTransactions
            .AsNoTracking()
            .AsQueryable();

        if (productVariantId.HasValue)
        {
            query = query.Where(x =>
                x.ProductVariantId == productVariantId.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new StockTransactionResponse
            {
                Id = x.Id,
                ProductVariantId = x.ProductVariantId,
                ProductName = x.ProductVariant.Product.Name,
                VariantName = x.ProductVariant.Name,
                Type = x.Type,
                Quantity = x.Quantity,
                UserId = x.UserId,
                Username = x.User.Username,
                FullName = x.User.FullName,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<StockBalanceResponse>> GetCurrentStockAsync()
    {
        var variants = await _db.ProductVariants
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.Product.IsActive)
            .Select(x => new StockBalanceResponse
            {
                ProductVariantId = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                VariantName = x.Name,
                Size = x.Size,
                Color = x.Color,
                CapType = x.CapType,
                Material = x.Material,
                ImagePath = x.ImagePath,
                TotalIn = _db.StockTransactions
                    .Where(t =>
                        t.ProductVariantId == x.Id &&
                        t.Type == StockTransactionType.In)
                    .Sum(t => (decimal?)t.Quantity) ?? 0,

                TotalOut = _db.StockTransactions
                    .Where(t =>
                        t.ProductVariantId == x.Id &&
                        t.Type == StockTransactionType.Out)
                    .Sum(t => (decimal?)t.Quantity) ?? 0
            })
            .ToListAsync();

        foreach (var item in variants)
        {
            item.CurrentQuantity =
                item.TotalIn - item.TotalOut;
        }

        return variants;
    }

    public async Task<StockBalanceResponse?> GetVariantStockAsync(
        int productVariantId)
    {
        var variant = await _db.ProductVariants
            .AsNoTracking()
            .Where(x =>
                x.Id == productVariantId &&
                x.IsActive &&
                x.Product.IsActive)
            .Select(x => new StockBalanceResponse
            {
                ProductVariantId = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                VariantName = x.Name,
                Size = x.Size,
                Color = x.Color,
                CapType = x.CapType,
                Material = x.Material,
                ImagePath = x.ImagePath,
                TotalIn = _db.StockTransactions
                    .Where(t =>
                        t.ProductVariantId == x.Id &&
                        t.Type == StockTransactionType.In)
                    .Sum(t => (decimal?)t.Quantity) ?? 0,

                TotalOut = _db.StockTransactions
                    .Where(t =>
                        t.ProductVariantId == x.Id &&
                        t.Type == StockTransactionType.Out)
                    .Sum(t => (decimal?)t.Quantity) ?? 0
            })
            .FirstOrDefaultAsync();

        if (variant is null)
        {
            return null;
        }

        variant.CurrentQuantity =
            variant.TotalIn - variant.TotalOut;

        return variant;
    }

    private async Task<decimal> CalculateStockAsync(
        int productVariantId)
    {
        var totalIn = await _db.StockTransactions
            .Where(x =>
                x.ProductVariantId == productVariantId &&
                x.Type == StockTransactionType.In)
            .SumAsync(x => (decimal?)x.Quantity) ?? 0;

        var totalOut = await _db.StockTransactions
            .Where(x =>
                x.ProductVariantId == productVariantId &&
                x.Type == StockTransactionType.Out)
            .SumAsync(x => (decimal?)x.Quantity) ?? 0;

        return totalIn - totalOut;
    }

    private async Task<StockTransactionResponse?> GetTransactionByIdAsync(
        int id)
    {
        return await _db.StockTransactions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new StockTransactionResponse
            {
                Id = x.Id,
                ProductVariantId = x.ProductVariantId,
                ProductName = x.ProductVariant.Product.Name,
                VariantName = x.ProductVariant.Name,
                Type = x.Type,
                Quantity = x.Quantity,
                UserId = x.UserId,
                Username = x.User.Username,
                FullName = x.User.FullName,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();
    }
}
