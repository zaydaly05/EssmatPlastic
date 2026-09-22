using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.Products;
using EsmatPlastic.Desktop.Services.Stock;
using EsmatPlastic.Desktop.ViewModels.Dashboard;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views;

public partial class DashboardView : UserControl
{
    private readonly DashboardViewModel _viewModel;
    private readonly LocalizationService _localization;

    public DashboardView()
    {
        InitializeComponent();

        _localization = App.ServiceProvider.GetRequiredService<LocalizationService>();

        _viewModel = new DashboardViewModel(
            App.ServiceProvider.GetRequiredService<ProductService>(),
            App.ServiceProvider.GetRequiredService<StockService>(),
            App.ServiceProvider.GetRequiredService<AppSession>(),
            _localization);

        DataContext = _viewModel;
        ApplyPermissions();
        ApplyLocalization();

        Loaded += DashboardView_Loaded;
    }

    private void ApplyLocalization()
    {
        TitleText.Text = _localization.T("لوحة التحكم");
        SubtitleText.Text = _localization.T("نظرة عامة ومباشرة على بيانات المنتجات والحركات المخزنية");
        RefreshButton.Content = _localization.T("⟳  تحديث");
    }

    private async void DashboardView_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= DashboardView_Loaded;
        ApplyPermissions();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            await _viewModel.LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                _localization.T("تعذر تحميل بيانات لوحة التحكم"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ApplyPermissions()
    {
        var hasDashboardPermission =
            _viewModel.CanViewProducts ||
            _viewModel.CanViewStock ||
            _viewModel.CanViewReports;

        NoPermissionsPanel.Visibility = hasDashboardPermission
            ? Visibility.Collapsed
            : Visibility.Visible;

        ProductsCard.Visibility = _viewModel.CanViewProducts
            ? Visibility.Visible
            : Visibility.Collapsed;

        VariantsCard.Visibility = _viewModel.CanViewStock
            ? Visibility.Visible
            : Visibility.Collapsed;

        StockCard.Visibility = _viewModel.CanViewStock
            ? Visibility.Visible
            : Visibility.Collapsed;

        TotalInCard.Visibility = _viewModel.CanViewReports
            ? Visibility.Visible
            : Visibility.Collapsed;

        TotalOutCard.Visibility = _viewModel.CanViewReports
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        RefreshButton.IsEnabled = false;
        RefreshButton.Content = _localization.T("جاري التحديث...");

        try
        {
            await LoadAsync();
        }
        finally
        {
            RefreshButton.IsEnabled = true;
            RefreshButton.Content = _localization.T("⟳  تحديث");
        }
    }
}
