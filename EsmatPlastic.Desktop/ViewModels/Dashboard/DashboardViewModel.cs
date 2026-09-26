using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;
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
    public ObservableCollection<DashboardMovementDay> WeeklyMovement { get; } = new();

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
    public decimal StockHealthPercent => VariantCount == 0 ? 0 :
        Math.Round((decimal)(VariantCount - LowStockCount) / VariantCount * 100, 0);

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
            var productsTask = CanViewProducts ? _productService.GetAllAsync() : null;
            var stockTask = CanViewStock ? _stockService.GetCurrentStockAsync() : null;
            var transactionsTask = CanViewStock
                ? _stockService.GetTransactionsAsync(take: 500)
                : null;

            var pendingTasks = new List<Task>();
            if (productsTask is not null) pendingTasks.Add(productsTask);
            if (stockTask is not null) pendingTasks.Add(stockTask);
            if (transactionsTask is not null) pendingTasks.Add(transactionsTask);
            await Task.WhenAll(pendingTasks);

            if (productsTask is not null)
                ProductCount = (await productsTask).Count;

            if (stockTask is not null)
            {
                var stock = await stockTask;
                VariantCount = stock.Count;
                CurrentStock = stock.Sum(x => x.CurrentQuantity);
                TotalIn = stock.Sum(x => x.TotalIn);
                TotalOut = stock.Sum(x => x.TotalOut);

                // Low stock items threshold: quantity <= 10
                var lowItems = stock.Where(x => x.CurrentQuantity <= 10).OrderBy(x => x.CurrentQuantity).Take(6).ToList();
                LowStockCount = stock.Count(x => x.CurrentQuantity <= 10);

                LowStockItems.Clear();
                foreach (var item in lowItems)
                {
                    LowStockItems.Add(item);
                }

                if (transactionsTask is not null)
                {
                    var transactions = await transactionsTask;
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
                            TransactionTypeText = isIn ? _loc["وارد"] : _loc["صادر"],
                            TransactionTypeColor = isIn ? "#059669" : "#EF4444",
                            TransactionTypeBg = isIn ? "#D1FAE5" : "#FEE2E2",
                            Quantity = tx.Quantity,
                            UserFullName = string.IsNullOrWhiteSpace(tx.FullName) ? tx.Username : tx.FullName,
                            CreatedAt = tx.CreatedAt,
                            Notes = tx.Notes
                        });
                    }

                    BuildWeeklyMovement(transactions);
                }

                OnPropertyChanged(nameof(HasLowStockAlerts));
                OnPropertyChanged(nameof(HasRecentTransactions));
                OnPropertyChanged(nameof(StockHealthPercent));
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void BuildWeeklyMovement(IReadOnlyCollection<StockTransactionResponse> transactions)
    {
        var start = DateTime.Today.AddDays(-6);
        var days = Enumerable.Range(0, 7).Select(offset => start.AddDays(offset)).ToList();
        var totals = days.Select(day =>
        {
            var daily = transactions.Where(tx => tx.CreatedAt.ToLocalTime().Date == day.Date).ToList();
            return (Day: day, Incoming: daily.Where(tx => tx.Type == StockTransactionType.In).Sum(tx => tx.Quantity),
                Outgoing: daily.Where(tx => tx.Type == StockTransactionType.Out).Sum(tx => tx.Quantity));
        }).ToList();
        var maximum = totals.Max(x => Math.Max(x.Incoming, x.Outgoing));
        var culture = _loc.IsArabic ? CultureInfo.GetCultureInfo("ar-EG") : CultureInfo.CurrentCulture;

        WeeklyMovement.Clear();
        foreach (var day in totals)
        {
            WeeklyMovement.Add(new DashboardMovementDay
            {
                DayLabel = day.Day.ToString("ddd", culture),
                Incoming = day.Incoming,
                Outgoing = day.Outgoing,
                IncomingPercent = maximum == 0 ? 0 : (double)(day.Incoming / maximum * 100),
                OutgoingPercent = maximum == 0 ? 0 : (double)(day.Outgoing / maximum * 100)
            });
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
