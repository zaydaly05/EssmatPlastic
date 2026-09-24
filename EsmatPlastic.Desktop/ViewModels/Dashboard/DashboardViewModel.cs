using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EsmatPlastic.Desktop.Models.Dashboard;
using EsmatPlastic.Desktop.Models.Stock;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.Products;
using EsmatPlastic.Desktop.Services.Stock;

namespace EsmatPlastic.Desktop.ViewModels.Dashboard;

public class DashboardViewModel : INotifyPropertyChanged
{
    private readonly ProductService _productService;
    private readonly StockService _stockService;
    private readonly AppSession _appSession;
    private readonly LocalizationService _loc;

    private int _productCount;
    private int _variantCount;
    private decimal _currentStock;
    private decimal _totalIn;
    private decimal _totalOut;
    private int _lowStockCount;
    private bool _isLoading;

    public ObservableCollection<DashboardActivityItem> RecentTransactions { get; } = new();
    public ObservableCollection<StockBalanceResponse> LowStockItems { get; } = new();

    public int ProductCount
    {
        get => _productCount;
        private set
        {
            _productCount = value;
            OnPropertyChanged();
        }
    }

    public int VariantCount
    {
        get => _variantCount;
        private set
        {
            _variantCount = value;
            OnPropertyChanged();
        }
    }

    public decimal CurrentStock
    {
        get => _currentStock;
        private set
        {
            _currentStock = value;
            OnPropertyChanged();
        }
    }

    public decimal TotalIn
    {
        get => _totalIn;
        private set
        {
            _totalIn = value;
            OnPropertyChanged();
        }
    }

    public decimal TotalOut
    {
        get => _totalOut;
        private set
        {
            _totalOut = value;
            OnPropertyChanged();
        }
    }

    public int LowStockCount
    {
        get => _lowStockCount;
        private set
        {
            _lowStockCount = value;
            OnPropertyChanged();
        }
    }

    public bool HasLowStockAlerts => LowStockCount > 0;
    public bool HasRecentTransactions => RecentTransactions.Count > 0;

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public bool CanViewProducts => _appSession.HasPermission("Products.View");
    public bool CanViewStock => _appSession.HasPermission("Stock.View");
    public bool CanViewReports => _appSession.HasPermission("Reports.View");

    public DashboardViewModel(
        ProductService productService,
        StockService stockService,
        AppSession appSession,
        LocalizationService loc)
    {
        _productService = productService;
        _stockService = stockService;
        _appSession = appSession;
        _loc = loc;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;

        try
        {
            if (CanViewProducts)
            {
                var products = await _productService.GetAllAsync();
                ProductCount = products.Count;
            }

            if (CanViewStock)
            {
                var stock = await _stockService.GetCurrentStockAsync();
                VariantCount = stock.Count;
                CurrentStock = stock.Sum(x => x.CurrentQuantity);

                // Low stock items threshold: quantity <= 10
                var lowItems = stock.Where(x => x.CurrentQuantity <= 10).OrderBy(x => x.CurrentQuantity).Take(6).ToList();
                LowStockCount = stock.Count(x => x.CurrentQuantity <= 10);

                LowStockItems.Clear();
                foreach (var item in lowItems)
                {
                    LowStockItems.Add(item);
                }

                if (CanViewReports)
                {
                    TotalIn = stock.Sum(x => x.TotalIn);
                    TotalOut = stock.Sum(x => x.TotalOut);

                    var transactions = await _stockService.GetTransactionsAsync(take: 8);
                    var recent = transactions.OrderByDescending(t => t.CreatedAt).Take(8).ToList();

                    RecentTransactions.Clear();
                    foreach (var tx in recent)
                    {
                        bool isIn = tx.Type == StockTransactionType.In;
                        RecentTransactions.Add(new DashboardActivityItem
                        {
                            Id = tx.Id,
                            ProductName = tx.ProductName,
                            VariantName = tx.VariantName,
                            TransactionTypeText = isIn ? _loc.T("وارد") : _loc.T("صادر"),
                            TransactionTypeColor = isIn ? "#059669" : "#EF4444",
                            TransactionTypeBg = isIn ? "#D1FAE5" : "#FEE2E2",
                            Quantity = tx.Quantity,
                            UserFullName = string.IsNullOrWhiteSpace(tx.FullName) ? tx.Username : tx.FullName,
                            CreatedAt = tx.CreatedAt,
                            Notes = tx.Notes
                        });
                    }
                }

                OnPropertyChanged(nameof(HasLowStockAlerts));
                OnPropertyChanged(nameof(HasRecentTransactions));
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
