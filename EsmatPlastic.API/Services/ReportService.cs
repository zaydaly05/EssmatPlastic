using EsmatPlastic.API.DTOs.Reports;
using EsmatPlastic.Domain.Enums;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<StockReportResponse>> GetStockReportAsync()
    {
        var variants = await _db.ProductVariants
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.Product.IsActive)
            .Select(x => new StockReportResponse
            {
                ProductVariantId = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                VariantName = x.Name,
                Size = x.Size,
                Color = x.Color,
                CapType = x.CapType,
                Material = x.Material,

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
            .OrderBy(x => x.ProductName)
            .ThenBy(x => x.VariantName)
            .ToListAsync();

        foreach (var item in variants)
        {
            item.CurrentQuantity =
                item.TotalIn - item.TotalOut;
        }

        return variants;
    }
}
