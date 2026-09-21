using EsmatPlastic.Desktop.Services.Products;
using EsmatPlastic.Desktop.Services.Stock;
using EsmatPlastic.Desktop.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EsmatPlastic.Desktop.ViewModels.Dashboard;

public class DashboardViewModel : INotifyPropertyChanged
{
    private readonly ProductService _productService;

    private readonly StockService _stockService;

    private readonly AppSession _appSession;

    private int _productCount;

    private int _variantCount;

    private decimal _currentStock;

    private decimal _totalIn;

    private decimal _totalOut;

    private bool _isLoading;

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

    public bool IsLoading
    {
        get => _isLoading;

        private set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public bool CanViewProducts =>
        _appSession.HasPermission("Products.View");

    public bool CanViewStock =>
        _appSession.HasPermission("Stock.View");

    public bool CanViewReports =>
        _appSession.HasPermission("Reports.View");

    public DashboardViewModel(
        ProductService productService,
        StockService stockService,
        AppSession appSession)
    {
        _productService = productService;
        _stockService = stockService;
        _appSession = appSession;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;

        try
        {
            if (CanViewProducts)
            {
                var products =
                    await _productService
                        .GetAllAsync();

                ProductCount =
                    products.Count;
            }

            if (CanViewStock)
            {
                var stock =
                    await _stockService
                        .GetCurrentStockAsync();

                VariantCount =
                    stock.Count;

                CurrentStock =
                    stock.Sum(x =>
                        x.CurrentQuantity);

                if (CanViewReports)
                {
                    TotalIn =
                        stock.Sum(x =>
                            x.TotalIn);

                    TotalOut =
                        stock.Sum(x =>
                            x.TotalOut);
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler?
        PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
