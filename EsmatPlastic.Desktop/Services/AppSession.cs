using EsmatPlastic.Desktop.Models;

namespace EsmatPlastic.Desktop.Services;

public class AppSession
{
    public LoginResponse? CurrentUser { get; private set; }

    public bool IsLoggedIn =>
        CurrentUser is not null &&
        !string.IsNullOrWhiteSpace(CurrentUser.Token);

    public int UserId =>
        CurrentUser?.UserId ?? 0;

    public string Username =>
        CurrentUser?.Username ?? string.Empty;

    public string FullName =>
        CurrentUser?.FullName ?? string.Empty;

    public string Role =>
        CurrentUser?.Role ?? string.Empty;

    public List<string> Permissions =>
        CurrentUser?.Permissions ?? new List<string>();

    public void Start(LoginResponse user)
    {
        CurrentUser = user;
    }

    public void Clear()
    {
        CurrentUser = null;
    }

    public bool HasPermission(string permission)
    {
        return Permissions.Any(x =>
            x.Equals(
                permission,
                StringComparison.OrdinalIgnoreCase));
    }

    public bool IsAdmin()
    {
        return Role.Equals(
            "Admin",
            StringComparison.OrdinalIgnoreCase);
    }

    public bool IsWarehouse()
    {
        return Role.Equals(
            "Warehouse",
            StringComparison.OrdinalIgnoreCase);
    }

    public bool IsAccountant()
    {
        return Role.Equals(
            "Accountant",
            StringComparison.OrdinalIgnoreCase);
    }
}
