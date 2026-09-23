using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using EsmatPlastic.Desktop.Services.Localization;
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

        ApplyButtonLocalization();

        _viewModel =
            new ReportsViewModel(
                App.ServiceProvider
                    .GetRequiredService<ReportService>());

        DataContext = _viewModel;

        Loaded += ReportsView_Loaded;
    }

    private void ApplyButtonLocalization()
    {
        var loc = App.ServiceProvider.GetRequiredService<LocalizationService>();
        ExportCsvButton.Content = "▣  " + loc["ExportCsv"];
        PrintReportButton.Content = "▧  " + loc["PrintReport"];
        RefreshButton.Content = loc["RefreshBtn"];
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
        var loc = App.ServiceProvider.GetRequiredService<LocalizationService>();
        RefreshButton.IsEnabled = false;
        RefreshButton.Content = loc["Updating"];

        try
        {
            await _viewModel.LoadAsync();
        }
        finally
        {
            RefreshButton.IsEnabled = true;
            RefreshButton.Content = loc["RefreshBtn"];
        }
    }

    private void ExportCsv_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = ".csv",
            FileName = "EsmatPlastic-Stock-Report.csv",
            Filter = "CSV files (*.csv)|*.csv"
        };

        if (dialog.ShowDialog(Window.GetWindow(this)) != true)
        {
            return;
        }

        new ReportExportService().ExportToCsv(_viewModel.Items, dialog.FileName);
    }

    private void PrintReport_Click(object sender, RoutedEventArgs e)
    {
        new ReportExportService().PrintReport(
            _viewModel.Items,
            _viewModel.TotalIn,
            _viewModel.TotalOut,
            _viewModel.CurrentStock,
            _viewModel.VariantCount);
    }
}
