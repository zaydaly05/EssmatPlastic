using EsmatPlastic.Domain.Entities;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API.Services;

public interface IOrderRequestService
{
    Task<int> CreateRequestAsync(int userId, string customerName, string? customerPhone, List<(int VariantId, decimal Qty)> items);
    Task<List<OrderRequest>> GetAllRequestsAsync();
    Task<OrderRequest?> GetRequestByIdAsync(int id);
    Task<bool> CancelRequestAsync(int requestId);
    Task<bool> UpdateStatusAsync(int requestId, OrderRequestStatus newStatus);
}

public class OrderRequestService : IOrderRequestService
{
    private readonly AppDbContext _context;

    public OrderRequestService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateRequestAsync(int userId, string customerName, string? customerPhone, List<(int VariantId, decimal Qty)> items)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var request = new OrderRequest
            {
                UserId = userId,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                RequestedAt = DateTime.UtcNow,
                Status = OrderRequestStatus.Pending
            };

            _context.OrderRequests.Add(request);
            await _context.SaveChangesAsync();

            foreach (var item in items)
            {
                var variant = await _context.ProductVariants.FindAsync(item.VariantId);
                if (variant == null) throw new Exception($"Product variant {item.VariantId} not found.");

                // Logic: Increase ReservedQuantity to "hold" the items
                variant.ReservedQuantity += item.Qty;

                _context.OrderRequestItems.Add(new OrderRequestItem
                {
                    OrderRequestId = request.Id,
                    ProductVariantId = item.VariantId,
                    Quantity = item.Qty
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return request.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<OrderRequest>> GetAllRequestsAsync()
    {
        return await _context.OrderRequests
            .Include(x => x.Items)
            .ThenInclude(i => i.ProductVariant)
            .ToListAsync();
    }

    public async Task<OrderRequest?> GetRequestByIdAsync(int id)
    {
        return await _context.OrderRequests
            .Include(x => x.Items)
            .ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> CancelRequestAsync(int requestId)
    {
        var request = await _context.OrderRequests
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == requestId);

        if (request == null || request.Status != OrderRequestStatus.Pending) return false;

        foreach (var item in request.Items)
        {
            var variant = await _context.ProductVariants.FindAsync(item.ProductVariantId);
            if (variant != null)
            {
                variant.ReservedQuantity -= item.Quantity;
            }
        }

        _context.OrderRequests.Remove(request);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(int requestId, OrderRequestStatus newStatus)
    {
        var request = await _context.OrderRequests.FindAsync(requestId);
        if (request == null) return false;

        request.Status = newStatus;

        // If Approved/Completed, logic for actual stock reduction would go here
        // For now, we just update the status.

        await _context.SaveChangesAsync();
        return true;
    }
}
