using EsmatPlastic.API.DTOs.Users;
using EsmatPlastic.Domain.Entities;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API.Services;

public class UserService : IUserService
{
    private readonly IResilientDbExecutor _executor;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IDbSyncTrigger _syncTrigger;

    public UserService(IResilientDbExecutor executor, IDbSyncTrigger syncTrigger)
    {
        _executor = executor;
        _syncTrigger = syncTrigger;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<List<UserResponse>> GetAllAsync()
    {
        return await _executor.ExecuteAsync(async db =>
            await db.Users
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
                    PermissionIds = x.UserPermissions.Select(p => p.PermissionId).ToList(),
                    Permissions = x.UserPermissions.Where(p => p.Permission.IsActive).Select(p => p.Permission.Name).ToList()
                })
                .ToListAsync());
    }

    public async Task<UserResponse?> GetByIdAsync(int id)
    {
        return await _executor.ExecuteAsync(async db =>
            await db.Users
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
                    PermissionIds = x.UserPermissions.Select(p => p.PermissionId).ToList(),
                    Permissions = x.UserPermissions.Where(p => p.Permission.IsActive).Select(p => p.Permission.Name).ToList()
                })
                .FirstOrDefaultAsync());
    }

    public async Task<UserResponse?> CreateAsync(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new InvalidOperationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }

        var username = request.Username.Trim();

        return await _executor.ExecuteAsync(async db =>
        {
            var usernameExists = await db.Users.AnyAsync(x => x.Username == username);
            if (usernameExists)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            if (!Enum.IsDefined(request.Role))
            {
                throw new InvalidOperationException("Invalid user role.");
            }

            var permissionIds = request.PermissionIds.Distinct().ToList();

            var validPermissionIds = await db.Permissions
                .Where(x => x.IsActive && permissionIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            if (validPermissionIds.Count != permissionIds.Count)
            {
                throw new InvalidOperationException("One or more selected permissions are invalid.");
            }

            var user = new User
            {
                Username = username,
                FullName = request.FullName.Trim(),
                Role = request.Role,
                IsActive = request.IsActive
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            db.Users.Add(user);
            await db.SaveChangesAsync();

            foreach (var permissionId in validPermissionIds)
            {
                db.UserPermissions.Add(new UserPermission
                {
                    UserId = user.Id,
                    PermissionId = permissionId
                });
            }

            await db.SaveChangesAsync();
            _syncTrigger.TriggerSync();

            return await db.Users
                .AsNoTracking()
                .Include(x => x.UserPermissions)
                    .ThenInclude(x => x.Permission)
                .Where(x => x.Id == user.Id)
                .Select(x => new UserResponse
                {
                    Id = x.Id,
                    Username = x.Username,
                    FullName = x.FullName,
                    Role = x.Role,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    PermissionIds = x.UserPermissions.Select(p => p.PermissionId).ToList(),
                    Permissions = x.UserPermissions.Where(p => p.Permission.IsActive).Select(p => p.Permission.Name).ToList()
                })
                .FirstOrDefaultAsync();
        });
    }

    public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }

        if (!Enum.IsDefined(request.Role))
        {
            throw new InvalidOperationException("Invalid user role.");
        }

        return await _executor.ExecuteAsync(async db =>
        {
            var user = await db.Users.Include(x => x.UserPermissions).FirstOrDefaultAsync(x => x.Id == id);
            if (user is null) return null;

            var permissionIds = request.PermissionIds.Distinct().ToList();

            var validPermissionIds = await db.Permissions
                .Where(x => x.IsActive && permissionIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            if (validPermissionIds.Count != permissionIds.Count)
            {
                throw new InvalidOperationException("One or more selected permissions are invalid.");
            }

            user.FullName = request.FullName.Trim();
            user.Role = request.Role;
            user.IsActive = request.IsActive;
            user.CreatedAt = DateTime.UtcNow;

            db.UserPermissions.RemoveRange(user.UserPermissions);

            foreach (var permissionId in validPermissionIds)
            {
                db.UserPermissions.Add(new UserPermission
                {
                    UserId = user.Id,
                    PermissionId = permissionId
                });
            }

            await db.SaveChangesAsync();
            _syncTrigger.TriggerSync();

            return await db.Users
                .AsNoTracking()
                .Include(x => x.UserPermissions)
                    .ThenInclude(x => x.Permission)
                .Where(x => x.Id == user.Id)
                .Select(x => new UserResponse
                {
                    Id = x.Id,
                    Username = x.Username,
                    FullName = x.FullName,
                    Role = x.Role,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    PermissionIds = x.UserPermissions.Select(p => p.PermissionId).ToList(),
                    Permissions = x.UserPermissions.Where(p => p.Permission.IsActive).Select(p => p.Permission.Name).ToList()
                })
                .FirstOrDefaultAsync();
        });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _executor.ExecuteAsync(async db =>
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user is null) return false;

            var hasTransactions = await db.StockTransactions.AnyAsync(x => x.UserId == id);
            if (hasTransactions)
            {
                throw new InvalidOperationException("This user has stock transactions and cannot be deleted. Deactivate the user instead.");
            }

            db.Users.Remove(user);
            db.DeletedRecords.Add(new DeletedRecord
            {
                EntityType = "User",
                RecordKey = user.Username.Trim().ToLowerInvariant(),
                DeletedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
            _syncTrigger.TriggerSync();
            return true;
        });
    }

    public async Task<bool> ChangePasswordAsync(int id, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            throw new InvalidOperationException("Password is required.");
        }

        return await _executor.ExecuteAsync(async db =>
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user is null) return false;

            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            user.CreatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            _syncTrigger.TriggerSync();
            return true;
        });
    }
}
