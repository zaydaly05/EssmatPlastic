using System.Collections.ObjectModel;
using System.Windows;
using EsmatPlastic.Desktop.Models.Stock;
using EsmatPlastic.Desktop.Services.Stock;

namespace EsmatPlastic.Desktop.Views.Warehouse;

public partial class StockHistoryWindow : Window
{
    private readonly StockService _stockService;

    private readonly StockBalanceResponse _stock;

    public ObservableCollection<StockTransactionResponse>
        Transactions { get; } = new();

    public StockHistoryWindow(
        StockService stockService,
        StockBalanceResponse stock)
    {
        InitializeComponent();

        _stockService = stockService;

        _stock = stock;

        DataContext = this;

        VariantText.Text =
            $"{stock.ProductName} - {stock.VariantName}";
    }

    private async void Window_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var result =
                await _stockService
                    .GetTransactionsAsync(
                        _stock.ProductVariantId);

            Transactions.Clear();

            foreach (var transaction in result)
            {
                Transactions.Add(transaction);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "تعذر تحميل سجل الحركات",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
