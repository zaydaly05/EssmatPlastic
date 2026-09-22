using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.Reports;
using EsmatPlastic.Desktop.ViewModels.Reports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace EsmatPlastic.Desktop.Views.Reports;

public partial class ReportsView : UserControl
{
    private readonly ReportsViewModel _viewModel;
    private readonly ReportExportService _exportService;
    private readonly LocalizationService _loc;

    public ReportsView()
    {
        InitializeComponent();

        _loc = App.ServiceProvider.GetRequiredService<LocalizationService>();
        _exportService = App.ServiceProvider.GetRequiredService<ReportExportService>();
        _viewModel = new ReportsViewModel(App.ServiceProvider.GetRequiredService<ReportService>());

        DataContext = _viewModel;
        ApplyLocalization();

        Loaded += ReportsView_Loaded;
    }

    private void ApplyLocalization()
    {
        PageTitleText.Text = _loc.T("التقارير");
        PageSubtitleText.Text = _loc.T("تقارير شاملة لحركة ورصيد المخزون");

        ExportButton.Content = _loc.T("📥  تصدير CSV");
        PrintButton.Content = _loc.T("🖨️  طباعة التقارير");
        RefreshButton.Content = _loc.T("⟳  تحديث");

        StatTotalInLabel.Text = _loc.T("إجمالي الوارد");
        StatTotalOutLabel.Text = _loc.T("إجمالي الصادر");
        StatStockLabel.Text = _loc.T("الرصيد الحالي");
        StatVariantsLabel.Text = _loc.T("الأصناف");

        HeaderProduct.Text = _loc.T("المنتج والصنف");
        HeaderSize.Text = _loc.T("المقاس");
        HeaderTotalIn.Text = _loc.T("الوارد الإجمالي");
        HeaderTotalOut.Text = _loc.T("الصادر الإجمالي");
        HeaderCurrentStock.Text = _loc.T("المخزون المتاح");
    }

    private async void ReportsView_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= ReportsView_Loaded;
        await _viewModel.LoadAsync();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshButton.IsEnabled = false;
        RefreshButton.Content = _loc.T("جاري التحديث...");

        try
        {
            await _viewModel.LoadAsync();
        }
        finally
        {
            RefreshButton.IsEnabled = true;
            RefreshButton.Content = _loc.T("⟳  تحديث");
        }
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.FilteredItems.Count == 0)
        {
            MessageBox.Show(
                _loc.T("لا توجد بيانات تقارير متاحة للتصدير."),
                _loc.T("تنبيه"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var saveFileDialog = new SaveFileDialog
        {
            Title = _loc.T("تصدير تقرير المخزون"),
            Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            FileName = $"Stock_Report_{DateTime.Now:yyyyMMdd_HHmm}.csv"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                _exportService.ExportToCsv(_viewModel.FilteredItems, saveFileDialog.FileName);

                MessageBox.Show(
                    _loc.IsArabic ? "تم تصدير التقرير بنجاح إلى ملف CSV." : "Report exported successfully to CSV.",
                    _loc.T("تم التصدير"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    _loc.T("تعذر تصدير التقرير"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void PrintButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.FilteredItems.Count == 0)
        {
            MessageBox.Show(
                _loc.T("لا توجد بيانات تقارير متاحة للطباعة."),
                _loc.T("تنبيه"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            _exportService.PrintReport(
                _viewModel.FilteredItems,
                _viewModel.TotalIn,
                _viewModel.TotalOut,
                _viewModel.CurrentStock,
                _viewModel.VariantCount);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                _loc.T("تعذر طباعة التقرير"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
