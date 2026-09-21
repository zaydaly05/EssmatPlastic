using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Settings;
using EsmatPlastic.Desktop.Views.Products;
using EsmatPlastic.Desktop.Views.Warehouse;
using EsmatPlastic.Desktop.Views.Reports;
using EsmatPlastic.Desktop.Views.Users;
using EsmatPlastic.Desktop.Views.Permissions;
using EsmatPlastic.Desktop.Views.Settings;

namespace EsmatPlastic.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly AppSession _appSession;

    private readonly ApiClient _apiClient;

    public MainWindow(
        AppSession appSession,
        ApiClient apiClient)
    {
        InitializeComponent();

        _appSession = appSession;

        _apiClient = apiClient;

        LoadUserInformation();
        ApplyPermissions();

        ShowDashboard();
    }

    private void LoadUserInformation()
    {
        FullNameText.Text =
            _appSession.FullName;

        RoleText.Text =
            GetArabicRole(_appSession.Role);
    }

    private void ApplyPermissions()
    {
        WarehouseButton.Visibility =
            _appSession.HasPermission("Stock.View")
                ? Visibility.Visible
                : Visibility.Collapsed;

        ProductsButton.Visibility =
            _appSession.HasPermission("Products.View")
                ? Visibility.Visible
                : Visibility.Collapsed;

        ReportsButton.Visibility =
            _appSession.HasPermission("Reports.View")
                ? Visibility.Visible
                : Visibility.Collapsed;

        PermissionsButton.Visibility =
            _appSession.HasPermission("Permissions.Manage")
                ? Visibility.Visible
                : Visibility.Collapsed;

        UsersButton.Visibility =
            _appSession.HasPermission("Users.View")
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    private static string GetArabicRole(string role)
    {
        return role switch
        {
            "Admin" => "مدير النظام",
            "Warehouse" => "أمين المستودع",
            "Accountant" => "محاسب",
            _ => role
        };
    }

    private void ShowDashboard()
    {
        MainContent.Content =
            new DashboardView();
    }

    public void RefreshForCurrentUser()
    {
        LoadUserInformation();
        ApplyPermissions();
        ShowDashboard();
    }

    private void DashboardButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowDashboard();
    }

    private void WarehouseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        MainContent.Content =
            new WarehouseView();
    }

    private void ProductsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        MainContent.Content =
            new ProductsView();
    }

    private void ReportsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        MainContent.Content =
            new ReportsView();
    }

    private void UsersButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        MainContent.Content =
            new UsersView();
    }

    private void PermissionsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        MainContent.Content =
            new PermissionsView();
    }

    private void SettingsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var window =
            new SettingsWindow(
                App.ServiceProvider
                    .GetRequiredService<SettingsService>());

        window.Owner = this;

        window.ShowDialog();
    }

    private void LogoutButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _appSession.Clear();

        _apiClient.ClearToken();

        var loginWindow =
            App.ServiceProvider
                .GetRequiredService<LoginWindow>();

        loginWindow.Show();

        Hide();
    }
}





