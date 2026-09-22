using System.Net.Http;
using System.Windows;
using EsmatPlastic.Desktop.Models.Stock;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.Stock;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.Warehouse;

public partial class StockTransactionWindow : Window
{
    private readonly StockService _stockService;
    private readonly StockBalanceResponse _stock;
    private readonly bool _allowIn;
    private readonly bool _allowOut;
    private readonly LocalizationService _loc;

    public StockTransactionResponse? CreatedTransaction { get; private set; }

    public StockTransactionWindow(
        StockService stockService,
        StockBalanceResponse stock,
        bool allowIn,
        bool allowOut)
    {
        InitializeComponent();

        _stockService = stockService;
        _stock = stock;
        _allowIn = allowIn;
        _allowOut = allowOut;
        _loc = App.ServiceProvider.GetRequiredService<LocalizationService>();

        ProductText.Text = $"{_loc.T("المنتج")}: {stock.ProductName}";
        VariantText.Text = BuildVariantText(stock, _loc);

        ApplyLocalization();
        ConfigureTransactionTypes();
        Loaded += (_, _) => QuantityInput.Focus();
    }

    private void ApplyLocalization()
    {
        Title = _loc.T("حركة مخزون");
        ModalHeaderText.Text = _loc.T("إضافة حركة مخزون");
        ModalSubtitleText.Text = _loc.IsArabic
            ? "سجّل حركة دخول (وارد) أو خروج (صادر) من المخزن."
            : "Record an incoming or outgoing stock transaction.";
        TransactionTypeLabel.Text = _loc.T("نوع الحركة");
        QuantityLabel.Text = _loc.T("الكمية");
        NotesLabel.Text = _loc.T("الملاحظات");
        CancelBtn.Content = _loc.T("إلغاء");
        SaveButton.Content = _loc.T("حفظ الحركة");
    }

    private void ConfigureTransactionTypes()
    {
        TransactionTypeInput.Items.Clear();

        if (_allowIn)
        {
            TransactionTypeInput.Items.Add(
                new System.Windows.Controls.ComboBoxItem
                {
                    Content = _loc.T("وارد"),
                    Tag = StockTransactionType.In
                });
        }

        if (_allowOut)
        {
            TransactionTypeInput.Items.Add(
                new System.Windows.Controls.ComboBoxItem
                {
                    Content = _loc.T("صادر"),
                    Tag = StockTransactionType.Out
                });
        }

        if (TransactionTypeInput.Items.Count > 0)
        {
            TransactionTypeInput.SelectedIndex = 0;
        }
    }

    private async void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (!decimal.TryParse(QuantityInput.Text.Trim(), out var quantity) || quantity <= 0)
        {
            MessageBox.Show(
                _loc.T("يرجى إدخال كمية صحيحة أكبر من صفر."),
                _loc.T("تنبيه"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            QuantityInput.Focus();
            return;
        }

        if (TransactionTypeInput.SelectedItem is not System.Windows.Controls.ComboBoxItem selectedItem ||
            selectedItem.Tag is not StockTransactionType type)
        {
            MessageBox.Show(
                _loc.T("يرجى اختيار نوع الحركة."),
                _loc.T("تنبيه"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        SaveButton.IsEnabled = false;

        try
        {
            var request = new CreateStockTransactionRequest
            {
                ProductVariantId = _stock.ProductVariantId,
                Type = type,
                Quantity = quantity,
                Notes = string.IsNullOrWhiteSpace(NotesInput.Text)
                    ? null
                    : NotesInput.Text.Trim()
            };

            CreatedTransaction = await _stockService.CreateTransactionAsync(request);

            if (CreatedTransaction is null)
            {
                MessageBox.Show(
                    _loc.T("تعذر حفظ حركة المخزون."),
                    _loc.T("خطأ"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            MessageBox.Show(
                type == StockTransactionType.In
                    ? _loc.T("تمت إضافة الحركة الواردة بنجاح.")
                    : _loc.T("تمت إضافة الحركة الصادرة بنجاح."),
                _loc.T("تم الحفظ"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show(
                ex.Message,
                _loc.T("خطأ في الاتصال بالخادم"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                _loc.T("خطأ"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private static string BuildVariantText(
        StockBalanceResponse stock,
        LocalizationService loc)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(stock.VariantName))
            parts.Add(stock.VariantName);

        if (!string.IsNullOrWhiteSpace(stock.Size))
            parts.Add($"{loc.T("المقاس")}: {stock.Size}");

        if (!string.IsNullOrWhiteSpace(stock.Color))
            parts.Add($"{loc.T("اللون")}: {stock.Color}");

        if (!string.IsNullOrWhiteSpace(stock.CapType))
            parts.Add($"{loc.T("نوع الغطاء")}: {stock.CapType}");

        if (!string.IsNullOrWhiteSpace(stock.Material))
            parts.Add($"{loc.T("المادة")}: {stock.Material}");

        return string.Join(" | ", parts);
    }

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}


