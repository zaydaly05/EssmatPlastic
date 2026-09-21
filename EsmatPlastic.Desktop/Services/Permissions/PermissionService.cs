using EsmatPlastic.Desktop.Models.Permissions;

namespace EsmatPlastic.Desktop.Services.Permissions;

public class PermissionService
{
    private readonly ApiClient _apiClient;

    public PermissionService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<PermissionResponse>>
        GetAllAsync()
    {
        var result =
            await _apiClient.GetAsync
                <List<PermissionResponse>>(
                    "api/Permissions");

        return result ?? new List<PermissionResponse>();
    }
}
