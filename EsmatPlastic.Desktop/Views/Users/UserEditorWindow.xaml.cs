using EsmatPlastic.Desktop.Models.Users;
using System.Windows;
using EsmatPlastic.Desktop.Services.Users;

namespace EsmatPlastic.Desktop.Views.Users;

public partial class UserEditorWindow : Window
{
    private readonly UserService _userService;

    private readonly UserResponse? _existingUser;

    public UserResponse? SavedUser { get; private set; }

    public UserEditorWindow(
        UserService userService,
        UserResponse? existingUser = null)
    {
        InitializeComponent();

        _userService = userService;

        _existingUser = existingUser;

        RoleInput.ItemsSource =
            Enum.GetValues<UserRole>();

        if (existingUser is not null)
        {
            Title = "تعديل المستخدم";

            UsernameInput.Text =
                existingUser.Username;

            UsernameInput.IsEnabled = false;

            FullNameInput.Text =
                existingUser.FullName;

            RoleInput.SelectedItem =
                existingUser.Role;

            ActiveInput.IsChecked =
                existingUser.IsActive;

            PasswordInput.Visibility =
                Visibility.Collapsed;
        }
        else
        {
            RoleInput.SelectedItem =
                UserRole.Warehouse;
        }
    }

    private async void Save_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(
            FullNameInput.Text))
        {
            MessageBox.Show(
                "يرجى إدخال الاسم الكامل.",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        try
        {
            if (_existingUser is null)
            {
                if (string.IsNullOrWhiteSpace(
                    UsernameInput.Text))
                {
                    MessageBox.Show(
                        "يرجى إدخال اسم المستخدم.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                if (string.IsNullOrWhiteSpace(
                    PasswordInput.Password))
                {
                    MessageBox.Show(
                        "يرجى إدخال كلمة المرور.",
                        "تنبيه",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                var result =
                    await _userService.CreateAsync(
                        new CreateUserRequest
                        {
                            Username =
                                UsernameInput.Text.Trim(),

                            Password =
                                PasswordInput.Password,

                            FullName =
                                FullNameInput.Text.Trim(),

                            Role =
                                (UserRole)RoleInput.SelectedItem!,

                            IsActive =
                                ActiveInput.IsChecked == true
                        });

                SavedUser = result;
            }
            else
            {
                var result =
                    await _userService.UpdateAsync(
                        _existingUser.Id,
                        new UpdateUserRequest
                        {
                            FullName =
                                FullNameInput.Text.Trim(),

                            Role =
                                (UserRole)RoleInput.SelectedItem!,

                            IsActive =
                                ActiveInput.IsChecked == true,

                            PermissionIds =
                                _existingUser.PermissionIds
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
                "تعذر حفظ المستخدم",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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



