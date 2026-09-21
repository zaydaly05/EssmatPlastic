using EsmatPlastic.API.DTOs.Users;

namespace EsmatPlastic.API.Services;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync();

    Task<UserResponse?> GetByIdAsync(int id);

    Task<UserResponse?> CreateAsync(CreateUserRequest request);

    Task<UserResponse?> UpdateAsync(
        int id,
        UpdateUserRequest request);

    Task<bool> DeleteAsync(int id);

    Task<bool> ChangePasswordAsync(
        int id,
        string newPassword);
}
