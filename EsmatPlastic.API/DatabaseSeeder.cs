using EsmatPlastic.Domain.Entities;
using EsmatPlastic.Domain.Enums;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var configuration = scope.ServiceProvider
            .GetRequiredService<IConfiguration>();

        await SeedAsync(db, configuration);
    }

    public static async Task SeedAsync(
        AppDbContext db,
        IConfiguration configuration)
    {

        var passwordHasher = new PasswordHasher<User>();

        var adminUsername =
            configuration["InitialAdmin:Username"] ?? "admin";

        var adminPassword =
            configuration["InitialAdmin:Password"] ?? "Admin@12345";

        var adminFullName =
            configuration["InitialAdmin:FullName"] ?? "مدير النظام";

        var admin = await db.Users
            .FirstOrDefaultAsync(x => x.Username == adminUsername);

        if (admin is null)
        {
            admin = new User
            {
                Username = adminUsername,
                FullName = adminFullName,
                Role = UserRole.Admin,
                IsActive = true
            };

            admin.PasswordHash = passwordHasher.HashPassword(
                admin,
                adminPassword);

            db.Users.Add(admin);
        }
        else
        {
            admin.Role = UserRole.Admin;
            admin.IsActive = true;
        }

        var permissionNames = new[]
        {
            "Users.View",
            "Users.Create",
            "Users.Edit",
            "Users.Delete",
            "Permissions.Manage",
            "Products.View",
            "Products.Create",
            "Products.Edit",
            "Products.Delete",
            "Stock.View",
            "Stock.In",
            "Stock.Out",
            "Reports.View"
        };

        foreach (var permissionName in permissionNames)
        {
            var exists = await db.Permissions
                .AnyAsync(x => x.Name == permissionName);

            if (!exists)
            {
                db.Permissions.Add(new Permission
                {
                    Name = permissionName,
                    Description = permissionName,
                    IsActive = true
                });
            }
        }

        await db.SaveChangesAsync();

        var permissions = await db.Permissions
            .Where(x => permissionNames.Contains(x.Name))
            .ToListAsync();

        var existingUserPermissions = await db.UserPermissions
            .Where(x => x.UserId == admin.Id)
            .Select(x => x.PermissionId)
            .ToListAsync();

        foreach (var permission in permissions)
        {
            if (!existingUserPermissions.Contains(permission.Id))
            {
                db.UserPermissions.Add(new UserPermission
                {
                    UserId = admin.Id,
                    PermissionId = permission.Id
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
