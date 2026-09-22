using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Products;
using EsmatPlastic.Desktop.Services.ProductVariants;
using EsmatPlastic.Desktop.Services.Stock;
using EsmatPlastic.Desktop.Services.Reports;
using EsmatPlastic.Desktop.Services.Users;
using EsmatPlastic.Desktop.Services.Permissions;
using EsmatPlastic.Desktop.Services.Settings;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Views;
using EsmatPlastic.Desktop.Views.Products;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ensure local API backend server is active (starts background process if not already running)
        await LocalApiLauncher.EnsureApiRunningAsync();

        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        var settingsService =
            ServiceProvider.GetRequiredService<SettingsService>();

        var localizationService =
            ServiceProvider.GetRequiredService<LocalizationService>();

        localizationService.SetLanguage(
            settingsService.Current.RememberLanguage
                ? settingsService.Current.Language
                : "ar");

        EventManager.RegisterClassHandler(
            typeof(Window),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler(Localizable_Loaded));

        EventManager.RegisterClassHandler(
            typeof(UserControl),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler(Localizable_Loaded));

        var loginWindow =
            ServiceProvider.GetRequiredService<LoginWindow>();

        MainWindow = loginWindow;
        loginWindow.Show();
    }

    private static void Localizable_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element ||
            e.OriginalSource != sender)
        {
            return;
        }

        ServiceProvider
            .GetRequiredService<LocalizationService>()
            .ApplyTo(element);
    }

    public static void ChangeLanguage(string language)
    {
        var localizationService =
            ServiceProvider.GetRequiredService<LocalizationService>();

        var settingsService =
            ServiceProvider.GetRequiredService<SettingsService>();

        localizationService.SetLanguage(language);
        settingsService.Current.Language = localizationService.Language;

        if (settingsService.Current.RememberLanguage)
        {
            settingsService.Save();
        }

        ApplyLanguage();
    }

    public static void ApplyLanguage()
    {
        var localizationService =
            ServiceProvider.GetRequiredService<LocalizationService>();

        foreach (Window window in Current.Windows)
        {
            localizationService.ApplyTo(window);
        }
    }

    private static void ConfigureServices(
        IServiceCollection services)
    {
        services.AddSingleton<ApiClient>();
        services.AddSingleton<AppSession>();
        services.AddSingleton<AuthService>();
        services.AddSingleton<ProductService>();
        services.AddSingleton<ProductVariantService>();
        services.AddSingleton<StockService>();
        services.AddSingleton<ReportService>();
        services.AddSingleton<ReportExportService>();
        services.AddSingleton<UserService>();
        services.AddSingleton<PermissionService>();
        services.AddSingleton<SettingsService>();
        services.AddSingleton<LocalizationService>();
        services.AddTransient<LoginWindow>();
        services.AddSingleton<EsmatPlastic.Desktop.Views.MainWindow>();
        services.AddTransient<AddProductWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnExit(e);
    }
}
