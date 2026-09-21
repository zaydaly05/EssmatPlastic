using EsmatPlastic.Domain.Enums;

namespace EsmatPlastic.API.DTOs.Users;

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public List<int> PermissionIds { get; set; } = new();
}
