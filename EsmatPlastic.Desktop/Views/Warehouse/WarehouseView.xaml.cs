using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Models.Stock;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Stock;
using EsmatPlastic.Desktop.ViewModels.Warehouse;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.Warehouse;

public partial class WarehouseView : UserControl
{
    private readonly WarehouseViewModel _viewModel;

    private readonly AppSession _appSession;

    public WarehouseView()
    {
        InitializeComponent();

        var provider =
            App.ServiceProvider;

        var stockService =
            provider
                .GetRequiredService<StockService>();

        _appSession =
            provider
                .GetRequiredService<AppSession>();

        _viewModel =
            new WarehouseViewModel(
                stockService);

        DataContext =
            _viewModel;

        ApplyPermissions();

        Loaded += WarehouseView_Loaded;
    }

    private async void WarehouseView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        Loaded -= WarehouseView_Loaded;

        await _viewModel.LoadAsync();
    }

    private void ApplyPermissions()
    {
        InButton.Visibility =
            _appSession.HasPermission("Stock.In")
                ? Visibility.Visible
                : Visibility.Collapsed;

        OutButton.Visibility =
            _appSession.HasPermission("Stock.Out")
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    private async void RefreshButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        RefreshButton.IsEnabled = false;
        RefreshButton.Content = "جاري التحديث...";

        try
        {
            await _viewModel.LoadAsync();
        }
        finally
        {
            RefreshButton.IsEnabled = true;
            RefreshButton.Content = "⟳  تحديث";
        }
    }

    private async void InButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        await OpenTransactionWindow(
            allowIn: true,
            allowOut: false);
    }

    private async void OutButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        await OpenTransactionWindow(
            allowIn: false,
            allowOut: true);
    }

    private async Task OpenTransactionWindow(
        bool allowIn,
        bool allowOut)
    {
        if (_viewModel.SelectedStock is null)
        {
            MessageBox.Show(
                "يرجى اختيار صنف أولاً.",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        var stock =
            _viewModel.SelectedStock;

        var window =
            new StockTransactionWindow(
                App.ServiceProvider
                    .GetRequiredService<StockService>(),
                stock,
                allowIn,
                allowOut);

        window.Owner =
            Window.GetWindow(this);

        var result =
            window.ShowDialog();

        if (result == true)
        {
            await _viewModel.LoadAsync();
        }
    }

    private async void HistoryButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_viewModel.SelectedStock is null)
        {
            MessageBox.Show(
                "يرجى اختيار صنف أولاً.",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        var history =
            new StockHistoryWindow(
                App.ServiceProvider
                    .GetRequiredService<StockService>(),
                _viewModel.SelectedStock);

        history.Owner =
            Window.GetWindow(this);

        history.ShowDialog();

        await _viewModel.LoadAsync();
    }
}
