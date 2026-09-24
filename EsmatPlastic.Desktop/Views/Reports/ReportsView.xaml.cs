using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Services.Reports;
using EsmatPlastic.Desktop.ViewModels.Reports;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.Reports;

public partial class ReportsView : UserControl
{
    private readonly ReportsViewModel _viewModel;

    public ReportsView()
    {
        InitializeComponent();

        _viewModel =
            new ReportsViewModel(
                App.ServiceProvider
                    .GetRequiredService<ReportService>());

        DataContext = _viewModel;

        Loaded += ReportsView_Loaded;
    }

    private async void ReportsView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        Loaded -= ReportsView_Loaded;

        await _viewModel.LoadAsync();
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
}
