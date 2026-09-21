using EsmatPlastic.Domain.Entities;
using EsmatPlastic.Domain.Enums;
using EsmatPlastic.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EsmatPlastic.API;

public static class TestDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var passwordHasher = new PasswordHasher<User>();

        Console.WriteLine("");
        Console.WriteLine("==========================================");
        Console.WriteLine("   INSERTING ESMAT PLASTIC TEST DATA");
        Console.WriteLine("==========================================");

        // ====================================================
        // USERS
        // ====================================================

        var users = new[]
        {
            new
            {
                Username = "warehouse",
                FullName = "موظف المخزن - اختبار",
                Role = UserRole.Warehouse,
                Password = "Warehouse@123"
            },
            new
            {
                Username = "accountant",
                FullName = "المحاسب - اختبار",
                Role = UserRole.Accountant,
                Password = "Accountant@123"
            }
        };

        var createdUsers = new Dictionary<string, User>();

        foreach (var data in users)
        {
            var user = await db.Users
                .FirstOrDefaultAsync(x => x.Username == data.Username);

            if (user is null)
            {
                user = new User
                {
                    Username = data.Username,
                    FullName = data.FullName,
                    Role = data.Role,
                    IsActive = true
                };

                user.PasswordHash =
                    passwordHasher.HashPassword(
                        user,
                        data.Password);

                db.Users.Add(user);

                Console.WriteLine(
                    $"Created user: {data.Username}");
            }
            else
            {
                Console.WriteLine(
                    $"User already exists: {data.Username}");
            }

            createdUsers[data.Username] = user;
        }

        await db.SaveChangesAsync();

        var warehouseUser =
            createdUsers["warehouse"];

        var accountantUser =
            createdUsers["accountant"];

        // ====================================================
        // PERMISSIONS
        // ====================================================

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
            if (!await db.Permissions
                .AnyAsync(x => x.Name == permissionName))
            {
                db.Permissions.Add(new Permission
                {
                    Name = permissionName,
                    Description = permissionName,
                    IsActive = true
                });

                Console.WriteLine(
                    $"Created permission: {permissionName}");
            }
        }

        await db.SaveChangesAsync();

        var permissions =
            await db.Permissions
                .Where(x =>
                    permissionNames.Contains(x.Name))
                .ToListAsync();

        // Warehouse permissions
        var warehousePermissionNames = new[]
        {
            "Products.View",
            "Stock.View",
            "Stock.In",
            "Stock.Out"
        };

        foreach (var permission in permissions
            .Where(x =>
                warehousePermissionNames.Contains(x.Name)))
        {
            var exists =
                await db.UserPermissions.AnyAsync(x =>
                    x.UserId == warehouseUser.Id &&
                    x.PermissionId == permission.Id);

            if (!exists)
            {
                db.UserPermissions.Add(
                    new UserPermission
                    {
                        UserId = warehouseUser.Id,
                        PermissionId = permission.Id
                    });
            }
        }

        // Accountant permissions
        var accountantPermissionNames = new[]
        {
            "Products.View",
            "Stock.View",
            "Reports.View"
        };

        foreach (var permission in permissions
            .Where(x =>
                accountantPermissionNames.Contains(x.Name)))
        {
            var exists =
                await db.UserPermissions.AnyAsync(x =>
                    x.UserId == accountantUser.Id &&
                    x.PermissionId == permission.Id);

            if (!exists)
            {
                db.UserPermissions.Add(
                    new UserPermission
                    {
                        UserId = accountantUser.Id,
                        PermissionId = permission.Id
                    });
            }
        }

        await db.SaveChangesAsync();

        // ====================================================
        // PRODUCTS
        // ====================================================

        var productData = new[]
        {
            new
            {
                Name = "جراكن بلاستيك",
                Description = "جراكن بلاستيك متعددة الأحجام",
                Variants = new[]
                {
                    new
                    {
                        Name = "جركن 1 لتر أبيض",
                        Size = "1 لتر",
                        Color = "أبيض",
                        CapType = "غطاء عادي",
                        Material = "HDPE"
                    },
                    new
                    {
                        Name = "جركن 5 لتر أبيض",
                        Size = "5 لتر",
                        Color = "أبيض",
                        CapType = "غطاء عادي",
                        Material = "HDPE"
                    },
                    new
                    {
                        Name = "جركن 10 لتر أزرق",
                        Size = "10 لتر",
                        Color = "أزرق",
                        CapType = "غطاء أمان",
                        Material = "HDPE"
                    }
                }
            },

            new
            {
                Name = "برطمانات بلاستيك",
                Description = "برطمانات شفافة وغير شفافة",
                Variants = new[]
                {
                    new
                    {
                        Name = "برطمان 250 مل شفاف",
                        Size = "250 مل",
                        Color = "شفاف",
                        CapType = "غطاء لولبي",
                        Material = "PET"
                    },
                    new
                    {
                        Name = "برطمان 500 مل شفاف",
                        Size = "500 مل",
                        Color = "شفاف",
                        CapType = "غطاء لولبي",
                        Material = "PET"
                    }
                }
            },

            new
            {
                Name = "زجاجات عصير بلاستيك",
                Description = "زجاجات عصير شفافة بأحجام مختلفة",
                Variants = new[]
                {
                    new
                    {
                        Name = "زجاجة عصير 250 مل",
                        Size = "250 مل",
                        Color = "شفاف",
                        CapType = "غطاء لولبي",
                        Material = "PET"
                    },
                    new
                    {
                        Name = "زجاجة عصير 500 مل",
                        Size = "500 مل",
                        Color = "شفاف",
                        CapType = "غطاء لولبي",
                        Material = "PET"
                    },
                    new
                    {
                        Name = "زجاجة عصير 1 لتر",
                        Size = "1 لتر",
                        Color = "شفاف",
                        CapType = "غطاء لولبي",
                        Material = "PET"
                    }
                }
            },

            new
            {
                Name = "برطمانات توابل",
                Description = "عبوات توابل بأحجام مختلفة",
                Variants = new[]
                {
                    new
                    {
                        Name = "برطمان توابل 100 مل",
                        Size = "100 مل",
                        Color = "شفاف",
                        CapType = "غطاء لولبي",
                        Material = "PET"
                    },
                    new
                    {
                        Name = "برطمان توابل 200 مل",
                        Size = "200 مل",
                        Color = "شفاف",
                        CapType = "غطاء لولبي",
                        Material = "PET"
                    }
                }
            },

            new
            {
                Name = "منتجات مخصصة",
                Description = "منتجات بلاستيك حسب طلب العميل",
                Variants = new[]
                {
                    new
                    {
                        Name = "عبوة مخصصة 750 مل",
                        Size = "750 مل",
                        Color = "حسب الطلب",
                        CapType = "حسب الطلب",
                        Material = "PET"
                    }
                }
            }
        };

        var createdVariants =
            new List<ProductVariant>();

        foreach (var productDataItem in productData)
        {
            var product =
                await db.Products
                    .Include(x => x.Variants)
                    .FirstOrDefaultAsync(x =>
                        x.Name == productDataItem.Name);

            if (product is null)
            {
                product = new Product
                {
                    Name = productDataItem.Name,
                    Description = productDataItem.Description,
                    IsActive = true
                };

                db.Products.Add(product);

                await db.SaveChangesAsync();

                Console.WriteLine(
                    $"Created product: {product.Name}");
            }
            else
            {
                Console.WriteLine(
                    $"Product already exists: {product.Name}");
            }

            foreach (var variantData in productDataItem.Variants)
            {
                var variant =
                    product.Variants.FirstOrDefault(x =>
                        x.Name == variantData.Name);

                if (variant is null)
                {
                    variant = new ProductVariant
                    {
                        ProductId = product.Id,
                        Name = variantData.Name,
                        Size = variantData.Size,
                        Color = variantData.Color,
                        CapType = variantData.CapType,
                        Material = variantData.Material,
                        IsActive = true
                    };

                    db.ProductVariants.Add(variant);

                    await db.SaveChangesAsync();

                    Console.WriteLine(
                        $"  Created variant: {variant.Name}");
                }
                else
                {
                    Console.WriteLine(
                        $"  Variant already exists: {variant.Name}");
                }

                createdVariants.Add(variant);
            }
        }

        // ====================================================
        // STOCK TRANSACTIONS
        // ====================================================

        var stockTransactions = new[]
        {
            new
            {
                VariantName = "جركن 1 لتر أبيض",
                Quantity = 5000m,
                Type = StockTransactionType.In,
                UserId = warehouseUser.Id,
                Notes = "رصيد افتتاحي - اختبار"
            },

            new
            {
                VariantName = "جركن 1 لتر أبيض",
                Quantity = 750m,
                Type = StockTransactionType.Out,
                UserId = warehouseUser.Id,
                Notes = "صرف للعميل - اختبار"
            },

            new
            {
                VariantName = "جركن 5 لتر أبيض",
                Quantity = 3000m,
                Type = StockTransactionType.In,
                UserId = warehouseUser.Id,
                Notes = "إنتاج جديد - اختبار"
            },

            new
            {
                VariantName = "جركن 10 لتر أزرق",
                Quantity = 1500m,
                Type = StockTransactionType.In,
                UserId = warehouseUser.Id,
                Notes = "رصيد افتتاحي - اختبار"
            },

            new
            {
                VariantName = "برطمان 250 مل شفاف",
                Quantity = 8000m,
                Type = StockTransactionType.In,
                UserId = warehouseUser.Id,
                Notes = "إنتاج جديد - اختبار"
            },

            new
            {
                VariantName = "برطمان 250 مل شفاف",
                Quantity = 1200m,
                Type = StockTransactionType.Out,
                UserId = warehouseUser.Id,
                Notes = "توريد عميل - اختبار"
            },

            new
            {
                VariantName = "برطمان 500 مل شفاف",
                Quantity = 4500m,
                Type = StockTransactionType.In,
                UserId = warehouseUser.Id,
                Notes = "رصيد افتتاحي - اختبار"
            },

            new
            {
                VariantName = "زجاجة عصير 250 مل",
                Quantity = 10000m,
                Type = StockTransactionType.In,
                UserId = warehouseUser.Id,
                Notes = "إنتاج جديد - اختبار"
            },

            new
            {
                VariantName = "زجاجة عصير 250 مل",
                Quantity = 2500m,
                Type = StockTransactionType.Out,
                UserId = warehouseUser.Id,
                Notes = "طلب عميل - اختبار"
            },

            new
            {
                VariantName = "زجاجة عصير 500 مل",
                Quantity = 7500m,
                Type = StockTransactionType.In,
                UserId = warehouseUser.Id,
                Notes = "إنتاج جديد - اختبار"
            },

            new
            {
                VariantName = "زجاجة عصير 1 لتر",
                Quantity = 3000m,
                Type = StockTransactionType.In,
                UserId = warehouseUser.Id,
                Notes = "رصيد افتتاحي - اختبار"
            },

            new
            {
                VariantName = "برطمان توابل 100 مل",
                Quantity = 12000m,
                Type = StockTransactionType.In,
                UserId = warehouseUser.Id,
                Notes = "إنتاج جديد - اختبار"
            },

            new
            {
                VariantName = "برطمان توابل 100 مل",
                Quantity = 3500m,
                Type = StockTransactionType.Out,
                UserId = warehouseUser.Id,
                Notes = "صرف للعميل - اختبار"
            },

            new
            {
                VariantName = "برطمان توابل 200 مل",
                Quantity = 6500m,
                Type = StockTransactionType.In,
                UserId = warehouseUser.Id,
                Notes = "إنتاج جديد - اختبار"
            },

            new
            {
                VariantName = "عبوة مخصصة 750 مل",
                Quantity = 2000m,
                Type = StockTransactionType.In,
                UserId = warehouseUser.Id,
                Notes = "طلب تصنيع مخصص - اختبار"
            }
        };

        foreach (var stock in stockTransactions)
        {
            var variant =
                createdVariants.FirstOrDefault(x =>
                    x.Name == stock.VariantName);

            if (variant is null)
            {
                Console.WriteLine(
                    $"Variant not found: {stock.VariantName}");

                continue;
            }

            var alreadyExists =
                await db.StockTransactions.AnyAsync(x =>
                    x.ProductVariantId == variant.Id &&
                    x.Type == stock.Type &&
                    x.Quantity == stock.Quantity &&
                    x.Notes == stock.Notes);

            if (!alreadyExists)
            {
                db.StockTransactions.Add(
                    new StockTransaction
                    {
                        ProductVariantId = variant.Id,
                        Type = stock.Type,
                        Quantity = stock.Quantity,
                        UserId = stock.UserId,
                        Notes = stock.Notes
                    });

                Console.WriteLine(
                    $"Created stock transaction: " +
                    $"{stock.Type} {stock.Quantity} - " +
                    $"{stock.VariantName}");
            }
        }

        await db.SaveChangesAsync();

        Console.WriteLine("");
        Console.WriteLine("==========================================");
        Console.WriteLine("       TEST DATA COMPLETED");
        Console.WriteLine("==========================================");
        Console.WriteLine("");
        Console.WriteLine("TEST LOGIN ACCOUNTS:");
        Console.WriteLine("");
        Console.WriteLine("Admin:");
        Console.WriteLine("  Username: admin");
        Console.WriteLine("  Password: Admin@12345");
        Console.WriteLine("");
        Console.WriteLine("Warehouse:");
        Console.WriteLine("  Username: warehouse");
        Console.WriteLine("  Password: Warehouse@123");
        Console.WriteLine("");
        Console.WriteLine("Accountant:");
        Console.WriteLine("  Username: accountant");
        Console.WriteLine("  Password: Accountant@123");
        Console.WriteLine("");
    }
}
