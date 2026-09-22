using System.Windows;
using System.Windows.Media;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.Settings;

public partial class SettingsWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly ApiClient _apiClient;
    private readonly LocalizationService _loc;

    public SettingsWindow(SettingsService settingsService)
    {
        InitializeComponent();

        _settingsService = settingsService;
        _apiClient = App.ServiceProvider.GetRequiredService<ApiClient>();
        _loc = App.ServiceProvider.GetRequiredService<LocalizationService>();

        ApiInput.Text = _settingsService.Current.ApiBaseUrl;

        LanguageInput.SelectedIndex = _settingsService.Current.Language.Equals("en", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        RememberLanguageInput.IsChecked = _settingsService.Current.RememberLanguage;

        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        Title = _loc.T("الإعدادات");
        HeaderTitle.Text = _loc.T("⚙️  الإعدادات العامة");
        HeaderSubtitle.Text = _loc.T("تفضيلات لغة التطبيق وتكوينات اتصال السيرفر");

        LangSectionTitle.Text = _loc.T("🌐  لغة الواجهة والتفضيلات");
        LangLabel.Text = _loc.T("لغة التطبيق");
        RememberLanguageInput.Content = _loc.T("تذكر اللغة المحددة عند فتح التطبيق دائماً");

        ApiSectionTitle.Text = _loc.T("🔌  اتصال خادم واجهة البيانات (API)");
        ApiLabel.Text = _loc.T("عنوان السيرفر (Server API Base URL)");
        TestApiButton.Content = _loc.T("فحص الاتصال");

        CancelButton.Content = _loc.T("إلغاء");
        SaveButton.Content = _loc.T("حفظ الإعدادات");
    }

    private async void TestApiButton_Click(object sender, RoutedEventArgs e)
    {
        string url = ApiInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            ApiStatusText.Text = _loc.T("يرجى إدخال عنوان السيرفر أولاً.");
            ApiStatusText.Foreground = Brushes.OrangeRed;
            return;
        }

        TestApiButton.IsEnabled = false;
        ApiStatusText.Text = _loc.T("جاري التوصيل وفحص الخادم...");
        ApiStatusText.Foreground = (Brush)FindResource("MutedBrush");

        bool isConnected = await _apiClient.TestConnectionAsync(url);

        TestApiButton.IsEnabled = true;

        if (isConnected)
        {
            ApiStatusText.Text = _loc.IsArabic ? "🟢 الاتصال بالسيرفر يعمل بنجاح!" : "🟢 Server connection successful!";
            ApiStatusText.Foreground = (Brush)FindResource("SuccessBrush");
        }
        else
        {
            ApiStatusText.Text = _loc.IsArabic ? "🔴 تعذر الاتصال بالسيرفر. يرجى التحقق من العنوان أو حالة الخادم." : "🔴 Unable to connect to server. Check URL or server status.";
            ApiStatusText.Foreground = (Brush)FindResource("DangerBrush");
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (LanguageInput.SelectedItem is System.Windows.Controls.ComboBoxItem language)
        {
            _settingsService.Current.Language = language.Tag?.ToString() ?? "ar";
        }

        string newApiUrl = ApiInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(newApiUrl))
        {
            MessageBox.Show(
                _loc.T("يرجى إدخال عنوان واجهة API."),
                _loc.T("تنبيه"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        _settingsService.Current.ApiBaseUrl = newApiUrl;
        _settingsService.Current.RememberLanguage = RememberLanguageInput.IsChecked == true;
        _settingsService.Save();

        // Update active API client base URL
        _apiClient.UpdateBaseUrl(newApiUrl);

        // Apply language immediately
        _loc.SetLanguage(_settingsService.Current.Language);
        App.ApplyLanguage();

        MessageBox.Show(
            _settingsService.Current.Language == "ar"
                ? "تم حفظ الإعدادات وتحديث الاتصال واللغة بنجاح."
                : "Settings saved and connection updated successfully.",
            _loc.T("تم الحفظ"),
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
