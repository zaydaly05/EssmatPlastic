using EsmatPlastic.Desktop.Models.Users;

namespace EsmatPlastic.Desktop.Services.Users;

public class UserService
{
    private readonly ApiClient _apiClient;

    public UserService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<UserResponse>>
        GetAllAsync()
    {
        var result =
            await _apiClient.GetAsync
                <List<UserResponse>>(
                    "api/Users");

        return result ?? new List<UserResponse>();
    }

    public async Task<UserResponse?>
        CreateAsync(CreateUserRequest request)
    {
        return await _apiClient.PostAsync
            <CreateUserRequest, UserResponse>(
                "api/Users",
                request);
    }

    public async Task<UserResponse?>
        UpdateAsync(
            int id,
            UpdateUserRequest request)
    {
        return await _apiClient.PutAsync
            <UpdateUserRequest, UserResponse>(
                $"api/Users/{id}",
                request);
    }

    public async Task DeleteAsync(int id)
    {
        await _apiClient.DeleteAsync(
            $"api/Users/{id}");
    }

    public async Task ChangePasswordAsync(
        int id,
        string password)
    {
        await _apiClient.PutAsync
            <ChangePasswordRequest, object>(
                $"api/Users/{id}/password",
                new ChangePasswordRequest
                {
                    NewPassword = password
                });
    }
}
