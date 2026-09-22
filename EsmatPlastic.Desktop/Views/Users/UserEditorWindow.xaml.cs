using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Models.Users;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.Permissions;
using EsmatPlastic.Desktop.Services.Users;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.Users;

public partial class UserEditorWindow : Window
{
    private readonly UserService _userService;
    private readonly PermissionService _permissionService;
    private readonly UserResponse? _existingUser;
    private readonly LocalizationService _loc;

    public ObservableCollection<PermissionSelectionItem> Permissions { get; } = new();
    public UserResponse? SavedUser { get; private set; }

    public UserEditorWindow(
        UserService userService,
        UserResponse? existingUser = null)
    {
        InitializeComponent();

        _userService = userService;
        _permissionService = App.ServiceProvider.GetRequiredService<PermissionService>();
        _existingUser = existingUser;
        _loc = App.ServiceProvider.GetRequiredService<LocalizationService>();

        RoleInput.ItemsSource = Enum.GetValues<UserRole>();
        PermissionsItemsControl.ItemsSource = Permissions;
        RoleInput.SelectionChanged += RoleInput_SelectionChanged;

        ApplyLocalization();

        if (existingUser is not null)
        {
            Title = _loc.T("تعديل المستخدم");
            HeaderTitle.Text = _loc.T("تعديل المستخدم");

            UsernameInput.Text = existingUser.Username;
            UsernameInput.IsEnabled = false;

            FullNameInput.Text = existingUser.FullName;
            RoleInput.SelectedItem = existingUser.Role;
            ActiveInput.IsChecked = existingUser.IsActive;

            PasswordPanel.Visibility = Visibility.Collapsed;
            Loaded += (_, _) => FullNameInput.Focus();
        }
        else
        {
            Title = _loc.T("إضافة مستخدم");
            HeaderTitle.Text = _loc.T("بيانات المستخدم");
            RoleInput.SelectedItem = UserRole.Warehouse;
            Loaded += (_, _) => UsernameInput.Focus();
        }

        Loaded += Window_Loaded;
    }

    private void ApplyLocalization()
    {
        HeaderSubtitle.Text = _loc.T("أدخل بيانات حساب المستخدم والدور والصلاحيات.");
        UsernameLabel.Text = _loc.T("اسم المستخدم");
        FullNameLabel.Text = _loc.T("الاسم الكامل");
        PasswordLabel.Text = _loc.T("كلمة المرور");
        RoleLabel.Text = _loc.T("الدور");
        ActiveInput.Content = _loc.T("نشط حالياً");
        PermissionsLabel.Text = _loc.T("صلاحيات المستخدم");
        CancelButton.Content = _loc.T("إلغاء");
        SaveButton.Content = _loc.T("حفظ");
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadPermissionsAsync();
    }

    private void RoleInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_existingUser is not null) return;
        if (RoleInput.SelectedItem is UserRole selectedRole)
        {
            AutoCheckPermissionsForRole(selectedRole);
        }
    }

    private void AutoCheckPermissionsForRole(UserRole role)
    {
        if (Permissions.Count == 0) return;

        foreach (var item in Permissions)
        {
            item.IsGranted = ShouldGrantPermissionForRole(role, item.Name);
        }
    }

    private static bool ShouldGrantPermissionForRole(UserRole role, string permissionName)
    {
        return role switch
        {
            UserRole.Admin => true,
            UserRole.Warehouse => permissionName is "Products.View" or "Stock.View" or "Stock.In" or "Stock.Out",
            UserRole.Accountant => permissionName is "Products.View" or "Stock.View" or "Reports.View",
            _ => false
        };
    }

    private async Task LoadPermissionsAsync()
    {
        try
        {
            var systemPermissions = await _permissionService.GetAllAsync();
            var grantedIds = _existingUser != null ? new HashSet<int>(_existingUser.PermissionIds) : new HashSet<int>();

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

            if (_existingUser is null && RoleInput.SelectedItem is UserRole role)
            {
                AutoCheckPermissionsForRole(role);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                _loc.T("تعذر تحميل الصلاحيات"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void Save_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_existingUser is null)
        {
            if (string.IsNullOrWhiteSpace(UsernameInput.Text))
            {
                MessageBox.Show(
                    _loc.T("يرجى إدخال اسم المستخدم."),
                    _loc.T("تنبيه"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                UsernameInput.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordInput.Password))
            {
                MessageBox.Show(
                    _loc.T("يرجى إدخال كلمة المرور."),
                    _loc.T("تنبيه"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                PasswordInput.Focus();
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(FullNameInput.Text))
        {
            MessageBox.Show(
                _loc.T("يرجى إدخال الاسم الكامل."),
                _loc.T("تنبيه"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            FullNameInput.Focus();
            return;
        }

        SaveButton.IsEnabled = false;

        try
        {
            var selectedPermissionIds = Permissions
                .Where(p => p.IsGranted)
                .Select(p => p.Id)
                .ToList();

            if (_existingUser is null)
            {
                var result = await _userService.CreateAsync(
                    new CreateUserRequest
                    {
                        Username = UsernameInput.Text.Trim(),
                        Password = PasswordInput.Password,
                        FullName = FullNameInput.Text.Trim(),
                        Role = (UserRole)RoleInput.SelectedItem!,
                        IsActive = ActiveInput.IsChecked == true,
                        PermissionIds = selectedPermissionIds
                    });

                SavedUser = result;
            }
            else
            {
                var result = await _userService.UpdateAsync(
                    _existingUser.Id,
                    new UpdateUserRequest
                    {
                        FullName = FullNameInput.Text.Trim(),
                        Role = (UserRole)RoleInput.SelectedItem!,
                        IsActive = ActiveInput.IsChecked == true,
                        PermissionIds = selectedPermissionIds
                    });

                SavedUser = result;
            }

            if (SavedUser is not null)
            {
                DialogResult = true;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                _loc.T("تعذر حفظ المستخدم"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private void Cancel_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}




