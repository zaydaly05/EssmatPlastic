namespace EsmatPlastic.Desktop.Models.Users;

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public List<int> PermissionIds { get; set; } = new();
}
