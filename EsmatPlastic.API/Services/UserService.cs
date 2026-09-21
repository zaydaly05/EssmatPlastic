using EsmatPlastic.API.DTOs.Users;
using EsmatPlastic.Domain.Entities;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<User> _passwordHasher;

    public UserService(AppDbContext db)
    {
        _db = db;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<List<UserResponse>> GetAllAsync()
    {
        return await _db.Users
            .AsNoTracking()
            .Include(x => x.UserPermissions)
                .ThenInclude(x => x.Permission)
            .OrderBy(x => x.Username)
            .Select(x => new UserResponse
            {
                Id = x.Id,
                Username = x.Username,
                FullName = x.FullName,
                Role = x.Role,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                PermissionIds = x.UserPermissions
                    .Select(p => p.PermissionId)
                    .ToList(),
                Permissions = x.UserPermissions
                    .Where(p => p.Permission.IsActive)
                    .Select(p => p.Permission.Name)
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<UserResponse?> GetByIdAsync(int id)
    {
        return await _db.Users
            .AsNoTracking()
            .Include(x => x.UserPermissions)
                .ThenInclude(x => x.Permission)
            .Where(x => x.Id == id)
            .Select(x => new UserResponse
            {
                Id = x.Id,
                Username = x.Username,
                FullName = x.FullName,
                Role = x.Role,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                PermissionIds = x.UserPermissions
                    .Select(p => p.PermissionId)
                    .ToList(),
                Permissions = x.UserPermissions
                    .Where(p => p.Permission.IsActive)
                    .Select(p => p.Permission.Name)
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserResponse?> CreateAsync(
        CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new InvalidOperationException(
                "Username is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException(
                "Password is required.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new InvalidOperationException(
                "Full name is required.");
        }

        var username = request.Username.Trim();

        var usernameExists = await _db.Users
            .AnyAsync(x => x.Username == username);

        if (usernameExists)
        {
            throw new InvalidOperationException(
                "Username already exists.");
        }

        if (!Enum.IsDefined(request.Role))
        {
            throw new InvalidOperationException(
                "Invalid user role.");
        }

        var permissionIds = request.PermissionIds
            .Distinct()
            .ToList();

        var validPermissionIds = await _db.Permissions
            .Where(x =>
                x.IsActive &&
                permissionIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync();

        if (validPermissionIds.Count != permissionIds.Count)
        {
            throw new InvalidOperationException(
                "One or more selected permissions are invalid.");
        }

        var user = new User
        {
            Username = username,
            FullName = request.FullName.Trim(),
            Role = request.Role,
            IsActive = request.IsActive
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        _db.Users.Add(user);

        await _db.SaveChangesAsync();

        foreach (var permissionId in validPermissionIds)
        {
            _db.UserPermissions.Add(new UserPermission
            {
                UserId = user.Id,
                PermissionId = permissionId
            });
        }

        await _db.SaveChangesAsync();

        return await GetByIdAsync(user.Id);
    }

    public async Task<UserResponse?> UpdateAsync(
        int id,
        UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new InvalidOperationException(
                "Full name is required.");
        }

        if (!Enum.IsDefined(request.Role))
        {
            throw new InvalidOperationException(
                "Invalid user role.");
        }

        var user = await _db.Users
            .Include(x => x.UserPermissions)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user is null)
        {
            return null;
        }

        var permissionIds = request.PermissionIds
            .Distinct()
            .ToList();

        var validPermissionIds = await _db.Permissions
            .Where(x =>
                x.IsActive &&
                permissionIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync();

        if (validPermissionIds.Count != permissionIds.Count)
        {
            throw new InvalidOperationException(
                "One or more selected permissions are invalid.");
        }

        user.FullName = request.FullName.Trim();
        user.Role = request.Role;
        user.IsActive = request.IsActive;

        _db.UserPermissions.RemoveRange(
            user.UserPermissions);

        foreach (var permissionId in validPermissionIds)
        {
            _db.UserPermissions.Add(new UserPermission
            {
                UserId = user.Id,
                PermissionId = permissionId
            });
        }

        await _db.SaveChangesAsync();

        return await GetByIdAsync(user.Id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user is null)
        {
            return false;
        }

        var hasTransactions = await _db.StockTransactions
            .AnyAsync(x => x.UserId == id);

        if (hasTransactions)
        {
            throw new InvalidOperationException(
                "This user has stock transactions and cannot be deleted. Deactivate the user instead.");
        }

        _db.Users.Remove(user);

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ChangePasswordAsync(
        int id,
        string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            throw new InvalidOperationException(
                "Password is required.");
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user is null)
        {
            return false;
        }

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            newPassword);

        await _db.SaveChangesAsync();

        return true;
    }
}
