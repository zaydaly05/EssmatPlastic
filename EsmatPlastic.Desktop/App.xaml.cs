using System.Windows;
using System.Windows.Controls;
using System.IO;
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

        var services = new ServiceCollection();

        ConfigureServices(services);

        ServiceProvider =
            services.BuildServiceProvider();

        var settingsService =
            ServiceProvider.GetRequiredService<SettingsService>();

        var localizationService =
            ServiceProvider.GetRequiredService<LocalizationService>();

        localizationService.SetLanguage(
            settingsService.Current.RememberLanguage
                ? settingsService.Current.Language
                : "ar");

        var apiBaseUrl = await LocalApiLauncher.EnsureApiRunningAsync(
            settingsService.Current.ApiBaseUrl);
        var apiClient = ServiceProvider.GetRequiredService<ApiClient>();
        apiClient.UpdateBaseUrl(apiBaseUrl);

        // Only check released packages; developer builds can keep using the source tree.
        if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EsmatPlastic.API.exe")))
        {
            var update = await apiClient.GetLatestUpdateAsync();
            var updateService = new AppUpdateService();
            if (update is not null && updateService.IsNewerVersion(update.Version))
            {
                var message = localizationService.IsArabic
                    ? $"يتوفر تحديث جديد ({update.Version}). هل تريد تنزيله وتثبيته الآن؟"
                    : $"Version {update.Version} is available. Download and install it now?";
                var title = localizationService.IsArabic ? "تحديث التطبيق" : "Application update";
                var choice = MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (choice == MessageBoxResult.Yes &&
                    await updateService.DownloadAndApplyAsync(update))
                {
                    Shutdown();
                    return;
                }
            }
        }

        EventManager.RegisterClassHandler(
            typeof(Window),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler(Window_Loaded));

        EventManager.RegisterClassHandler(
            typeof(UserControl),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler(UserControl_Loaded));

        var loginWindow =
            ServiceProvider
                .GetRequiredService<LoginWindow>();

        MainWindow = loginWindow;

        loginWindow.Show();
    }

    private static void Window_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is Window window &&
            e.OriginalSource is Window)
        {
            ServiceProvider
                .GetRequiredService<LocalizationService>()
                .ApplyTo(window);
        }
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

    private static void UserControl_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is UserControl view &&
            ReferenceEquals(e.OriginalSource, view))
        {
            ServiceProvider
                .GetRequiredService<LocalizationService>()
                .ApplyTo(view);
        }
    }

    public static void ChangeLanguage(string language)
    {
        var localizationService =
            ServiceProvider.GetRequiredService<LocalizationService>();
        var settingsService =
            ServiceProvider.GetRequiredService<SettingsService>();

        localizationService.SetLanguage(language);

        if (settingsService.Current.RememberLanguage)
        {
            settingsService.Current.Language = localizationService.Language;
            settingsService.Save();
        }

        ApplyLanguage();
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
        services.AddSingleton<UserService>();
        services.AddSingleton<PermissionService>();
        services.AddSingleton<SettingsService>();
        services.AddSingleton<LocalizationService>();

        services.AddTransient<LoginWindow>();

        services.AddSingleton<EsmatPlastic.Desktop.Views.MainWindow>();

        services.AddTransient<AddProductWindow>();
    }

    protected override void OnExit(
        ExitEventArgs e)
    {
        LocalApiLauncher.Stop();

        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnExit(e);
    }
}



