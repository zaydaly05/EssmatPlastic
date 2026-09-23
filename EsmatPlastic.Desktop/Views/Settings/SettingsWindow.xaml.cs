using System.Windows;
using EsmatPlastic.Desktop.Services.Localization;
using Microsoft.Extensions.DependencyInjection;
using EsmatPlastic.Desktop.Services.Settings;

namespace EsmatPlastic.Desktop.Views.Settings;

public partial class SettingsWindow : Window
{
    private readonly SettingsService _settingsService;

    public SettingsWindow(
        SettingsService settingsService)
    {
        InitializeComponent();

        _settingsService =
            settingsService;

        ApiInput.Text =
            _settingsService.Current.ApiBaseUrl;

        LanguageInput.SelectedIndex =
            _settingsService.Current.Language
                .Equals(
                    "en",
                    StringComparison.OrdinalIgnoreCase)
                ? 1
                : 0;

        RememberLanguageInput.IsChecked =
            _settingsService.Current.RememberLanguage;
    }

    private void Save_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (LanguageInput.SelectedItem
            is System.Windows.Controls.ComboBoxItem language)
        {
            _settingsService.Current.Language =
                language.Tag?.ToString() ?? "ar";
        }

        _settingsService.Current.ApiBaseUrl =
            ApiInput.Text.Trim();

        _settingsService.Current.RememberLanguage =
            RememberLanguageInput.IsChecked == true;

        _settingsService.Save();

        App.ServiceProvider
            .GetRequiredService<LocalizationService>()
            .SetLanguage(_settingsService.Current.Language);

        App.ApplyLanguage();

        MessageBox.Show(
            _settingsService.Current.Language == "ar"
                ? "تم حفظ الإعدادات وتطبيق اللغة."
                : "Settings saved and language applied.",
            _settingsService.Current.Language == "ar"
                ? "تم"
                : "Done",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        DialogResult = true;

        Close();
    }

    private void Cancel_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;

        Close();
    }
}
