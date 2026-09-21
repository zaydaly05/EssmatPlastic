using EsmatPlastic.Domain.Enums;

namespace EsmatPlastic.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserPermission> UserPermissions { get; set; }
        = new List<UserPermission>();

    public ICollection<StockTransaction> StockTransactions { get; set; }
        = new List<StockTransaction>();
}
