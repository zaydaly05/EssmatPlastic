using System.Windows;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;
    private readonly MainWindow _mainWindow;

    public LoginWindow(
        AuthService authService,
        AppSession appSession,
        MainWindow mainWindow)
    {
        InitializeComponent();

        _mainWindow = mainWindow;

        _viewModel = new LoginViewModel(
            authService,
            appSession,
            App.ServiceProvider.GetRequiredService<LocalizationService>());

        DataContext = _viewModel;
        _viewModel.LoginSucceeded += ViewModel_LoginSucceeded;

        Loaded += (_, _) =>
        {
            UpdateLanguageButtons();
            UsernameInput.Focus();
        };
    }

    private void PasswordInput_OnPasswordChanged(
        object sender,
        RoutedEventArgs e)
    {
        _viewModel.Password = PasswordInput.Password;
    }

    private void ArabicLangButton_Click(object sender, RoutedEventArgs e)
    {
        App.ChangeLanguage("ar");
        UpdateLanguageButtons();
    }

    private void EnglishLangButton_Click(object sender, RoutedEventArgs e)
    {
        App.ChangeLanguage("en");
        UpdateLanguageButtons();
    }

    private void UpdateLanguageButtons()
    {
        var loc = App.ServiceProvider.GetRequiredService<LocalizationService>();
        var isArabic = loc.IsArabic;

        ArabicLangButton.Tag = isArabic ? "Selected" : null;
        EnglishLangButton.Tag = isArabic ? null : "Selected";

        ApplyLocalization(loc);
    }

    private void ApplyLocalization(LocalizationService loc)
    {
        TitleText.Text = loc.T("تسجيل الدخول");
        SubtitleText.Text = loc.T("اختر اللغة ثم سجّل الدخول للمتابعة.");
        UsernameLabel.Text = loc.T("اسم المستخدم");
        PasswordLabel.Text = loc.T("كلمة المرور");
        LoginButton.Content = loc.T("تسجيل الدخول");
        BrandTitle.Text = loc.T("إسمت بلاستيك");
        BrandSubtitle.Text = loc.T("نظام إدارة المخزون");
        BrandDesc.Text = loc.T("لوحة تحكم بسيطة لإدارة مخزون البلاستيك.");
    }

    private void ViewModel_LoginSucceeded(object? sender, EventArgs e)
    {
        Hide();
        _mainWindow.RefreshForCurrentUser();
        _mainWindow.Show();
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.LoginSucceeded -= ViewModel_LoginSucceeded;
        base.OnClosed(e);
    }
}
