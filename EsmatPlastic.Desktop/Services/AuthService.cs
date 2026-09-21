using EsmatPlastic.Desktop.Models;

namespace EsmatPlastic.Desktop.Services;

public class AuthService
{
    private readonly ApiClient _apiClient;

    public AuthService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<LoginResponse?> LoginAsync(
        string username,
        string password)
    {
        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        var response =
            await _apiClient.PostAsync<
                LoginRequest,
                LoginResponse>(
                    "api/Auth/login",
                    request);

        if (response is not null)
        {
            _apiClient.SetToken(response.Token);
        }

        return response;
    }

    public void Logout()
    {
        _apiClient.ClearToken();
    }
}
