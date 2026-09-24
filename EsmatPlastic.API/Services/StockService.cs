using EsmatPlastic.API.DTOs.Stock;
using EsmatPlastic.Domain.Entities;
using EsmatPlastic.Domain.Enums;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API.Services;

public class StockService : IStockService
{
    private readonly IResilientDbExecutor _executor;
    private readonly IDbSyncTrigger _syncTrigger;

    public StockService(IResilientDbExecutor executor, IDbSyncTrigger syncTrigger)
    {
        _executor = executor;
        _syncTrigger = syncTrigger;
    }

    public async Task<StockTransactionResponse?> CreateTransactionAsync(
        int userId,
        CreateStockTransactionRequest request)
    {
        if (request.ProductVariantId <= 0)
        {
            throw new InvalidOperationException("A valid product variant is required.");
        }

        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than zero.");
        }

        if (request.Type != StockTransactionType.In && request.Type != StockTransactionType.Out)
        {
            throw new InvalidOperationException("Invalid stock transaction type.");
        }

        return await _executor.ExecuteAsync(async db =>
        {
            var variant = await db.ProductVariants
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x =>
                    x.Id == request.ProductVariantId &&
                    x.IsActive &&
                    x.Product.IsActive);

            if (variant is null)
            {
                throw new InvalidOperationException("The selected product variant does not exist or is inactive.");
            }

            var userExists = await db.Users
                .AnyAsync(x => x.Id == userId && x.IsActive);

            if (!userExists)
            {
                throw new InvalidOperationException("The current user does not exist or is inactive.");
            }

            if (request.Type == StockTransactionType.Out)
            {
                var totalIn = await db.StockTransactions
                    .AsNoTracking()
                    .Where(x => x.ProductVariantId == request.ProductVariantId && x.Type == StockTransactionType.In)
                    .SumAsync(x => (decimal?)x.Quantity) ?? 0;

                var totalOut = await db.StockTransactions
                    .AsNoTracking()
                    .Where(x => x.ProductVariantId == request.ProductVariantId && x.Type == StockTransactionType.Out)
                    .SumAsync(x => (decimal?)x.Quantity) ?? 0;

                var currentStock = totalIn - totalOut;

                if (request.Quantity > currentStock)
                {
                    throw new InvalidOperationException($"Insufficient stock. Current quantity: {currentStock}.");
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

            db.StockTransactions.Add(transaction);
            await db.SaveChangesAsync();
            _syncTrigger.TriggerSync();

            return await db.StockTransactions
                .AsNoTracking()
                .Where(x => x.Id == transaction.Id)
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
        });
    }

    public async Task<List<StockTransactionResponse>> GetTransactionsAsync(
        int? productVariantId = null,
        int? take = null)
    {
        return await _executor.ExecuteAsync(async db =>
        {
            var query = db.StockTransactions.AsNoTracking().AsQueryable();

            if (productVariantId.HasValue)
            {
                query = query.Where(x => x.ProductVariantId == productVariantId.Value);
            }

            IQueryable<StockTransaction> orderedQuery =
                query.OrderByDescending(x => x.CreatedAt);
            if (take.HasValue)
            {
                orderedQuery = orderedQuery.Take(Math.Clamp(take.Value, 1, 500));
            }

            return await orderedQuery
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
        });
    }

    public async Task<List<StockBalanceResponse>> GetCurrentStockAsync()
    {
        return await _executor.ExecuteAsync(async db =>
        {
            var stockTotals = await db.StockTransactions
                .AsNoTracking()
                .GroupBy(t => new { t.ProductVariantId, t.Type })
                .Select(g => new
                {
                    g.Key.ProductVariantId,
                    g.Key.Type,
                    Sum = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            var inDict = stockTotals
                .Where(x => x.Type == StockTransactionType.In)
                .ToDictionary(x => x.ProductVariantId, x => x.Sum);

            var outDict = stockTotals
                .Where(x => x.Type == StockTransactionType.Out)
                .ToDictionary(x => x.ProductVariantId, x => x.Sum);

            var variants = await db.ProductVariants
                .AsNoTracking()
                .Where(x => x.IsActive && x.Product.IsActive)
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
                    ImagePath = x.ImagePath
                })
                .ToListAsync();

            foreach (var item in variants)
            {
                item.TotalIn = inDict.TryGetValue(item.ProductVariantId, out var totalIn) ? totalIn : 0;
                item.TotalOut = outDict.TryGetValue(item.ProductVariantId, out var totalOut) ? totalOut : 0;
                item.CurrentQuantity = item.TotalIn - item.TotalOut;
            }

            return variants;
        });
    }

    public async Task<StockBalanceResponse?> GetVariantStockAsync(int productVariantId)
    {
        return await _executor.ExecuteAsync(async db =>
        {
            var variant = await db.ProductVariants
                .AsNoTracking()
                .Where(x => x.Id == productVariantId && x.IsActive && x.Product.IsActive)
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
                    TotalIn = db.StockTransactions
                        .Where(t => t.ProductVariantId == x.Id && t.Type == StockTransactionType.In)
                        .Sum(t => (decimal?)t.Quantity) ?? 0,
                    TotalOut = db.StockTransactions
                        .Where(t => t.ProductVariantId == x.Id && t.Type == StockTransactionType.Out)
                        .Sum(t => (decimal?)t.Quantity) ?? 0
                })
                .FirstOrDefaultAsync();

            if (variant is null) return null;

            variant.CurrentQuantity = variant.TotalIn - variant.TotalOut;
            return variant;
        });
    }
}
