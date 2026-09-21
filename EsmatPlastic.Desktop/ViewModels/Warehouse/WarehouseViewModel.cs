using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EsmatPlastic.Desktop.Models.Stock;
using EsmatPlastic.Desktop.Services.Stock;

namespace EsmatPlastic.Desktop.ViewModels.Warehouse;

public class WarehouseViewModel : INotifyPropertyChanged
{
    private readonly StockService _stockService;

    private StockBalanceResponse? _selectedStock;

    private bool _isLoading;

    private string _searchText = string.Empty;

    private string _statusMessage = string.Empty;

    public ObservableCollection<StockBalanceResponse> StockItems
    {
        get;
    } = new();

    public ObservableCollection<StockBalanceResponse> FilteredStockItems
    {
        get;
    } = new();

    public StockBalanceResponse? SelectedStock
    {
        get => _selectedStock;

        set
        {
            if (_selectedStock == value)
                return;

            _selectedStock = value;

            OnPropertyChanged();

            OnPropertyChanged(
                nameof(HasSelectedStock));
        }
    }

    public bool HasSelectedStock =>
        SelectedStock is not null;

    public bool IsLoading
    {
        get => _isLoading;

        private set
        {
            if (_isLoading == value)
                return;

            _isLoading = value;

            OnPropertyChanged();
        }
    }

    public string SearchText
    {
        get => _searchText;

        set
        {
            if (_searchText == value)
                return;

            _searchText = value;

            OnPropertyChanged();

            ApplyFilter();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;

        private set
        {
            if (_statusMessage == value)
                return;

            _statusMessage = value;

            OnPropertyChanged();
        }
    }

    public WarehouseViewModel(
        StockService stockService)
    {
        _stockService = stockService;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;

        StatusMessage = string.Empty;

        try
        {
            var items =
                await _stockService
                    .GetCurrentStockAsync();

            StockItems.Clear();

            foreach (var item in items)
            {
                StockItems.Add(item);
            }

            ApplyFilter();

            StatusMessage =
                $"تم تحميل المخزون: {StockItems.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"تعذر تحميل المخزون: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilter()
    {
        FilteredStockItems.Clear();

        var text =
            SearchText.Trim();

        foreach (var item in StockItems)
        {
            if (string.IsNullOrWhiteSpace(text) ||
                item.ProductName.Contains(
                    text,
                    StringComparison.OrdinalIgnoreCase) ||
                item.VariantName.Contains(
                    text,
                    StringComparison.OrdinalIgnoreCase) ||
                (item.Size?.Contains(
                    text,
                    StringComparison.OrdinalIgnoreCase)
                    ?? false))
            {
                FilteredStockItems.Add(item);
            }
        }
    }

    public event PropertyChangedEventHandler?
        PropertyChanged;

    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(
                propertyName));
    }
}
