using EsmatPlastic.Domain.Enums;

namespace EsmatPlastic.API.DTOs.Users;

public class UserResponse
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<int> PermissionIds { get; set; } = new();

    public List<string> Permissions { get; set; } = new();
}
