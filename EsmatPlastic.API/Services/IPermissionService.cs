using EsmatPlastic.API.DTOs.Permissions;

namespace EsmatPlastic.API.Services;

public interface IPermissionService
{
    Task<List<PermissionResponse>> GetAllAsync();

    Task<PermissionResponse?> GetByIdAsync(int id);
}
