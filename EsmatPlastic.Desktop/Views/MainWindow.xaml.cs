using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Models.Products;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.Settings;
using EsmatPlastic.Desktop.Views.Permissions;
using EsmatPlastic.Desktop.Views.ProductVariants;
using EsmatPlastic.Desktop.Views.Products;
using EsmatPlastic.Desktop.Views.Reports;
using EsmatPlastic.Desktop.Views.Settings;
using EsmatPlastic.Desktop.Views.Users;
using EsmatPlastic.Desktop.Views.Warehouse;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly AppSession _appSession;
    private readonly ApiClient _apiClient;
    private Button? _selectedNav;

    public MainWindow(
        AppSession appSession,
        ApiClient apiClient)
    {
        InitializeComponent();

        _appSession = appSession;
        _apiClient = apiClient;

        Loaded += async (_, _) =>
        {
            LoadUserInformation();
            ApplyLocalization();
            UpdateLanguageButtons();
            await CheckHealthAsync();
            StartHealthTimer();
        };
    }

    private void StartHealthTimer()
    {
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10)
        };
        timer.Tick += async (_, _) => await CheckHealthAsync();
        timer.Start();
    }

    private async Task CheckHealthAsync()
    {
        var loc = App.ServiceProvider.GetRequiredService<LocalizationService>();
        try
        {
            var health = await _apiClient.GetHealthStatusAsync();
            if (health != null && health.IsOnline)
            {
                if (health.IsNeonBackupOnline)
                {
                    ConnectionStatusText.Text = loc.IsArabic
                        ? "🟢 LAN المحلي + نسخ سحابي"
                        : "🟢 Local LAN + Cloud Backup";
                    ConnectionStatusDot.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 142, 62));
                    ConnectionStatusBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 244, 234));
                    ConnectionStatusBorder.ToolTip = loc.IsArabic
                        ? "شبكة مصنع محلية + نسخة احتياطية سحابية متصلة (Neon PostgreSQL)"
                        : "Local factory LAN + cloud backup connected (Neon PostgreSQL)";
                }
                else
                {
                    ConnectionStatusText.Text = loc.IsArabic
                        ? "🔵 LAN محلي (بدون سحاب)"
                        : "🔵 Local LAN (Offline Backup)";
                    ConnectionStatusDot.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(26, 115, 232));
                    ConnectionStatusBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(232, 240, 254));
                    ConnectionStatusBorder.ToolTip = loc.IsArabic
                        ? "شبكة مصنع محلية تعمل بشكل مباشر (النسخ السحابي غير متصل حالياً)"
                        : "Local factory LAN running directly (cloud backup currently offline)";
                }
            }
            else
            {
                ConnectionStatusText.Text = loc.IsArabic
                    ? "🟡 API غير متصل"
                    : "🟡 API Offline / Disconnected";
                ConnectionStatusDot.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(181, 129, 5));
                ConnectionStatusBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(254, 247, 224));
                ConnectionStatusBorder.ToolTip = loc.IsArabic
                    ? "الخادم الرئيسي غير متصل"
                    : "Main server is offline";
            }
        }
        catch
        {
            ConnectionStatusText.Text = loc.IsArabic
                ? "🟡 API غير متصل"
                : "🟡 API Offline / Disconnected";
            ConnectionStatusDot.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(181, 129, 5));
            ConnectionStatusBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(254, 247, 224));
        }
    }

    private void ApplyLocalization()
    {
        var loc = App.ServiceProvider.GetRequiredService<LocalizationService>();

        AppNameText.Text = loc["AppName"];
        AppTaglineText.Text = loc["AppTagline"];
        WelcomeText.Text = loc["Welcome"];
        HaveANiceDayText.Text = loc["HaveANiceDay"];

        DashboardButton.Content = loc["Dashboard"];
        WarehouseButton.Content = loc["Warehouse"];
        ProductsButton.Content = loc["Products"];
        ReportsButton.Content = loc["Reports"];
        PermissionsButton.Content = loc["Permissions"];
        UsersButton.Content = loc["Users"];
        SettingsButton.Content = loc["Settings"];
        LogoutButton.Content = loc["Logout"];
    }

    private void LoadUserInformation()
    {
        FullNameText.Text = _appSession.FullName;
        RoleText.Text = LocalizedRole(_appSession.Role);
    }

    private static string LocalizedRole(string role)
    {
        var loc = App.ServiceProvider.GetRequiredService<LocalizationService>();

        return role switch
        {
            "Admin" => loc["AdminRole"],
            "Warehouse" => loc["WarehouseRole"],
            "Accountant" => loc["AccountantRole"],
            _ => role
        };
    }

    private void ApplyPermissions()
    {
        WarehouseButton.Visibility =
            _appSession.HasPermission("Stock.View") ? Visibility.Visible : Visibility.Collapsed;

        ProductsButton.Visibility =
            _appSession.HasPermission("Products.View") ? Visibility.Visible : Visibility.Collapsed;

        ReportsButton.Visibility =
            _appSession.HasPermission("Reports.View") ? Visibility.Visible : Visibility.Collapsed;

        PermissionsButton.Visibility =
            _appSession.HasPermission("Permissions.Manage") ? Visibility.Visible : Visibility.Collapsed;

        UsersButton.Visibility =
            _appSession.HasPermission("Users.View") ? Visibility.Visible : Visibility.Collapsed;
    }

    public void RefreshForCurrentUser()
    {
        LoadUserInformation();
        ApplyLocalization();
        ApplyPermissions();
        ShowDashboard();
        UpdateLanguageButtons();
    }

    private void SelectNav(Button button)
    {
        foreach (var nav in new[]
                 {
                     DashboardButton,
                     WarehouseButton,
                     ProductsButton,
                     ReportsButton,
                     PermissionsButton,
                     UsersButton,
                     SettingsButton
                 })
        {
            nav.Tag = null;
        }

        button.Tag = "Selected";
        _selectedNav = button;
    }

    private void ShowContent(UserControl view, Button nav)
    {
        SelectNav(nav);
        MainContent.Content = view;
    }

    private void ShowDashboard()
    {
        ShowContent(new DashboardView(), DashboardButton);
    }

    public void ShowProductVariants(ProductResponse product)
    {
        ShowContent(
            new ProductVariantsView(product.Id, product.Name),
            ProductsButton);
    }

    private void DashboardButton_Click(object sender, RoutedEventArgs e) =>
        ShowDashboard();

    private void WarehouseButton_Click(object sender, RoutedEventArgs e) =>
        ShowContent(new WarehouseView(), WarehouseButton);

    private void ProductsButton_Click(object sender, RoutedEventArgs e) =>
        ShowContent(new ProductsView(), ProductsButton);

    private void ReportsButton_Click(object sender, RoutedEventArgs e) =>
        ShowContent(new ReportsView(), ReportsButton);

    private void UsersButton_Click(object sender, RoutedEventArgs e) =>
        ShowContent(new UsersView(), UsersButton);

    private void PermissionsButton_Click(object sender, RoutedEventArgs e) =>
        ShowContent(new PermissionsView(), PermissionsButton);

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var window =
            new SettingsWindow(
                App.ServiceProvider.GetRequiredService<SettingsService>());

        window.Owner = this;
        window.ShowDialog();

        LoadUserInformation();
        UpdateLanguageButtons();
    }

    private void ArabicLangButton_Click(object sender, RoutedEventArgs e)
    {
        App.ChangeLanguage("ar");
        AfterLanguageChange();
    }

    private void EnglishLangButton_Click(object sender, RoutedEventArgs e)
    {
        App.ChangeLanguage("en");
        AfterLanguageChange();
    }

    private void AfterLanguageChange()
    {
        LoadUserInformation();
        ApplyLocalization();
        UpdateLanguageButtons();
        _ = CheckHealthAsync();

        if (_selectedNav == WarehouseButton)
            ShowContent(new WarehouseView(), WarehouseButton);
        else if (_selectedNav == ProductsButton)
            ShowContent(new ProductsView(), ProductsButton);
        else if (_selectedNav == ReportsButton)
            ShowContent(new ReportsView(), ReportsButton);
        else if (_selectedNav == UsersButton)
            ShowContent(new UsersView(), UsersButton);
        else if (_selectedNav == PermissionsButton)
            ShowContent(new PermissionsView(), PermissionsButton);
        else
            ShowDashboard();
    }

    private void UpdateLanguageButtons()
    {
        var isArabic = App.ServiceProvider
            .GetRequiredService<LocalizationService>()
            .IsArabic;

        ArabicLangButton.Tag = isArabic ? "Selected" : null;
        EnglishLangButton.Tag = isArabic ? null : "Selected";
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _appSession.Clear();
        _apiClient.ClearToken();

        var loginWindow =
            App.ServiceProvider.GetRequiredService<LoginWindow>();

        loginWindow.Show();
        Hide();
    }
}
