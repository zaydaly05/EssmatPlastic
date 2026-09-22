using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using EsmatPlastic.Desktop.Models.Users;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.Permissions;
using EsmatPlastic.Desktop.Services.Users;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.Users;

public partial class UserPermissionsWindow : Window
{
    private readonly UserService _userService;
    private readonly PermissionService _permissionService;
    private readonly UserResponse _user;
    private readonly LocalizationService _loc;

    public ObservableCollection<PermissionSelectionItem> Permissions { get; } = new();

    public UserPermissionsWindow(
        UserService userService,
        PermissionService permissionService,
        UserResponse user)
    {
        InitializeComponent();

        _userService = userService;
        _permissionService = permissionService;
        _user = user;
        _loc = App.ServiceProvider.GetRequiredService<LocalizationService>();

        FullNameText.Text = user.FullName;
        UsernameText.Text = user.Username;
        RoleText.Text = LocalizedRole(user.Role.ToString(), _loc);

        PermissionsItemsControl.ItemsSource = Permissions;

        Loaded += Window_Loaded;
    }

    private static string LocalizedRole(string role, LocalizationService loc)
    {
        return role switch
        {
            "Admin" => loc["AdminRole"],
            "Warehouse" => loc["WarehouseRole"],
            "Accountant" => loc["AccountantRole"],
            _ => role
        };
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadPermissionsAsync();
    }

    private async Task LoadPermissionsAsync()
    {
        try
        {
            var systemPermissions = await _permissionService.GetAllAsync();
            var grantedIds = new HashSet<int>(_user.PermissionIds);

            Permissions.Clear();
            foreach (var perm in systemPermissions)
            {
                Permissions.Add(new PermissionSelectionItem
                {
                    Id = perm.Id,
                    Name = perm.Name,
                    Description = _loc.T(perm.Description ?? perm.Name),
                    IsGranted = grantedIds.Contains(perm.Id)
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                _loc.T("خطأ"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SelectAllButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in Permissions)
        {
            item.IsGranted = true;
        }
    }

    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in Permissions)
        {
            item.IsGranted = false;
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveButton.IsEnabled = false;

        try
        {
            var selectedPermissionIds = Permissions
                .Where(p => p.IsGranted)
                .Select(p => p.Id)
                .ToList();

            var updateRequest = new UpdateUserRequest
            {
                FullName = _user.FullName,
                Role = _user.Role,
                IsActive = _user.IsActive,
                PermissionIds = selectedPermissionIds
            };

            var updatedUser = await _userService.UpdateAsync(_user.Id, updateRequest);

            if (updatedUser is null)
            {
                MessageBox.Show(
                    _loc.T("تعذر حفظ صلاحيات المستخدم."),
                    _loc.T("خطأ"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MessageBox.Show(
                _loc.IsArabic ? $"تم حفظ صلاحيات المستخدم {_user.FullName} بنجاح." : $"Permissions for {_user.FullName} updated successfully.",
                _loc.T("تم الحفظ"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                _loc.T("خطأ"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

public class PermissionSelectionItem : INotifyPropertyChanged
{
    private bool _isGranted;

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public bool IsGranted
    {
        get => _isGranted;
        set
        {
            if (_isGranted == value)
                return;

            _isGranted = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
