using System.Windows;
using EsmatPlastic.Desktop.Models.Users;
using EsmatPlastic.Desktop.Services.Users;

namespace EsmatPlastic.Desktop.Views.Users;

public partial class ChangePasswordWindow : Window
{
    private readonly UserService _userService;
    private readonly UserResponse _user;

    public ChangePasswordWindow(
        UserService userService,
        UserResponse user)
    {
        InitializeComponent();

        _userService = userService;
        _user = user;

        UsernameText.Text = user.Username;
        FullNameText.Text = user.FullName;
    }

    private async void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(
                NewPasswordBox.Password))
        {
            ErrorText.Text =
                "يرجى إدخال كلمة المرور الجديدة.";

            return;
        }

        if (NewPasswordBox.Password.Length < 6)
        {
            ErrorText.Text =
                "يجب أن تتكون كلمة المرور من 6 أحرف على الأقل.";

            return;
        }

        if (NewPasswordBox.Password !=
            ConfirmPasswordBox.Password)
        {
            ErrorText.Text =
                "كلمتا المرور غير متطابقتين.";

            return;
        }

        try
        {
            await _userService.ChangePasswordAsync(
                _user.Id,
                NewPasswordBox.Password);

            MessageBox.Show(
                "تم تغيير كلمة المرور بنجاح.",
                "تم",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            ErrorText.Text = ex.Message;
        }
    }

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}

