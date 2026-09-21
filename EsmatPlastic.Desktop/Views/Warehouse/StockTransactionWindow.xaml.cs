using System.Net.Http;
using System.Windows;
using EsmatPlastic.Desktop.Models.Stock;
using EsmatPlastic.Desktop.Services.Stock;

namespace EsmatPlastic.Desktop.Views.Warehouse;

public partial class StockTransactionWindow : Window
{
    private readonly StockService _stockService;

    private readonly StockBalanceResponse _stock;

    private readonly bool _allowIn;

    private readonly bool _allowOut;

    public StockTransactionResponse?
        CreatedTransaction
    {
        get;
        private set;
    }

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

        ProductText.Text =
            $"المنتج: {stock.ProductName}";

        VariantText.Text =
            BuildVariantText(stock);

        ConfigureTransactionTypes();
    }

    private void ConfigureTransactionTypes()
    {
        TransactionTypeInput.Items.Clear();

        if (_allowIn)
        {
            TransactionTypeInput.Items.Add(
                new System.Windows.Controls.ComboBoxItem
                {
                    Content = "وارد",
                    Tag = StockTransactionType.In
                });
        }

        if (_allowOut)
        {
            TransactionTypeInput.Items.Add(
                new System.Windows.Controls.ComboBoxItem
                {
                    Content = "صادر",
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
        if (!decimal.TryParse(
            QuantityInput.Text.Trim(),
            out var quantity) ||
            quantity <= 0)
        {
            MessageBox.Show(
                "يرجى إدخال كمية صحيحة أكبر من صفر.",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (TransactionTypeInput.SelectedItem
            is not System.Windows.Controls.ComboBoxItem selectedItem ||
            selectedItem.Tag is not StockTransactionType type)
        {
            MessageBox.Show(
                "يرجى اختيار نوع الحركة.",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        try
        {
            var request =
                new CreateStockTransactionRequest
                {
                    ProductVariantId =
                        _stock.ProductVariantId,

                    Type = type,

                    Quantity = quantity,

                    Notes =
                        string.IsNullOrWhiteSpace(
                            NotesInput.Text)
                            ? null
                            : NotesInput.Text.Trim()
                };

            CreatedTransaction =
                await _stockService
                    .CreateTransactionAsync(request);

            if (CreatedTransaction is null)
            {
                MessageBox.Show(
                    "تعذر حفظ حركة المخزون.",
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            MessageBox.Show(
                type == StockTransactionType.In
                    ? "تمت إضافة الحركة الواردة بنجاح."
                    : "تمت إضافة الحركة الصادرة بنجاح.",
                "تم الحفظ",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;

            Close();
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show(
                ex.Message,
                "خطأ في الاتصال بالخادم",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "خطأ",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static string BuildVariantText(
        StockBalanceResponse stock)
    {
        var parts =
            new List<string>();

        if (!string.IsNullOrWhiteSpace(stock.VariantName))
            parts.Add(stock.VariantName);

        if (!string.IsNullOrWhiteSpace(stock.Size))
            parts.Add($"المقاس: {stock.Size}");

        if (!string.IsNullOrWhiteSpace(stock.Color))
            parts.Add($"اللون: {stock.Color}");

        if (!string.IsNullOrWhiteSpace(stock.CapType))
            parts.Add($"الغطاء: {stock.CapType}");

        if (!string.IsNullOrWhiteSpace(stock.Material))
            parts.Add($"المادة: {stock.Material}");

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

