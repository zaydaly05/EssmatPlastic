using System.Windows;
using EsmatPlastic.Desktop.Models.Users;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.Users;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.Users;

public partial class ChangePasswordWindow : Window
{
    private readonly UserService _userService;
    private readonly UserResponse _user;
    private readonly LocalizationService _loc;

    public ChangePasswordWindow(
        UserService userService,
        UserResponse user)
    {
        InitializeComponent();

        _userService = userService;
        _user = user;
        _loc = App.ServiceProvider.GetRequiredService<LocalizationService>();

        UsernameText.Text = user.Username;
        FullNameText.Text = user.FullName;

        ApplyLocalization();

        Loaded += (_, _) => NewPasswordBox.Focus();
    }

    private void ApplyLocalization()
    {
        Title = _loc.T("تغيير كلمة المرور");
        HeaderTitle.Text = _loc.T("تغيير كلمة المرور");
        HeaderSubtitle.Text = _loc.T("أدخل كلمة المرور الجديدة للمستخدم.");
        UsernameLabel.Text = _loc.T("اسم المستخدم");
        FullNameLabel.Text = _loc.T("الاسم الكامل");
        NewPasswordLabel.Text = _loc.T("كلمة المرور الجديدة");
        ConfirmPasswordLabel.Text = _loc.T("تأكيد كلمة المرور");
        CancelButton.Content = _loc.T("إلغاء");
        SaveButton.Content = _loc.T("حفظ");
    }

    private async void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(NewPasswordBox.Password))
        {
            ErrorText.Text = _loc.T("يرجى إدخال كلمة المرور الجديدة.");
            NewPasswordBox.Focus();
            return;
        }

        if (NewPasswordBox.Password.Length < 6)
        {
            ErrorText.Text = _loc.T("يجب أن تتكون كلمة المرور من 6 أحرف على الأقل.");
            NewPasswordBox.Focus();
            return;
        }

        if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
        {
            ErrorText.Text = _loc.T("كلمتا المرور غير متطابقتين.");
            ConfirmPasswordBox.Focus();
            return;
        }

        SaveButton.IsEnabled = false;

        try
        {
            await _userService.ChangePasswordAsync(
                _user.Id,
                NewPasswordBox.Password);

            MessageBox.Show(
                _loc.T("تم تغيير كلمة المرور بنجاح."),
                _loc.T("تم"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            ErrorText.Text = ex.Message;
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}


