using EsmatPlastic.API.DTOs.Auth;

namespace EsmatPlastic.API.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}
