using EsmatPlastic.API.DTOs.Permissions;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API.Services;

public class PermissionService : IPermissionService
{
    private readonly IResilientDbExecutor _executor;

    public PermissionService(IResilientDbExecutor executor)
    {
        _executor = executor;
    }

    public async Task<List<PermissionResponse>> GetAllAsync()
    {
        return await _executor.ExecuteAsync(async db =>
            await db.Permissions
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => new PermissionResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .ToListAsync());
    }

    public async Task<PermissionResponse?> GetByIdAsync(int id)
    {
        return await _executor.ExecuteAsync(async db =>
            await db.Permissions
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new PermissionResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync());
    }
}
