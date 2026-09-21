using System.Windows;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.ViewModels;

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
            appSession);

        DataContext = _viewModel;

        _viewModel.LoginSucceeded +=
            ViewModel_LoginSucceeded;
    }

    private void PasswordInput_OnPasswordChanged(
        object sender,
        RoutedEventArgs e)
    {
        _viewModel.Password =
            PasswordInput.Password;
    }

    private void ViewModel_LoginSucceeded(
        object? sender,
        EventArgs e)
    {
        Hide();

        _mainWindow.RefreshForCurrentUser();
        _mainWindow.Show();
    }

    protected override void OnClosed(
        EventArgs e)
    {
        _viewModel.LoginSucceeded -=
            ViewModel_LoginSucceeded;

        base.OnClosed(e);
    }
}
