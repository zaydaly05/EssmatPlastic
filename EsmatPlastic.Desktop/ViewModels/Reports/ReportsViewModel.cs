using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EsmatPlastic.Desktop.Models.Reports;
using EsmatPlastic.Desktop.Services.Reports;

namespace EsmatPlastic.Desktop.ViewModels.Reports;

public class ReportsViewModel : INotifyPropertyChanged
{
    private readonly ReportService _reportService;

    private bool _isLoading;

    private string _searchText = string.Empty;

    private string _statusMessage = string.Empty;

    public ObservableCollection<StockReportResponse> Items
    {
        get;
    } = new();

    public ObservableCollection<StockReportResponse> FilteredItems
    {
        get;
    } = new();

    public decimal TotalIn =>
        Items.Sum(x => x.TotalIn);

    public decimal TotalOut =>
        Items.Sum(x => x.TotalOut);

    public decimal CurrentStock =>
        Items.Sum(x => x.CurrentQuantity);

    public int VariantCount =>
        Items.Count;

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

    public bool IsLoading
    {
        get => _isLoading;

        private set
        {
            _isLoading = value;

            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;

        private set
        {
            _statusMessage = value;

            OnPropertyChanged();
        }
    }

    public ReportsViewModel(
        ReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;

        try
        {
            var result =
                await _reportService
                    .GetStockReportAsync();

            Items.Clear();

            foreach (var item in result)
            {
                Items.Add(item);
            }

            ApplyFilter();

            OnPropertyChanged(nameof(TotalIn));
            OnPropertyChanged(nameof(TotalOut));
            OnPropertyChanged(nameof(CurrentStock));
            OnPropertyChanged(nameof(VariantCount));

            StatusMessage =
                $"تم تحميل التقارير: {Items.Count} سجل.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"تعذر تحميل التقارير: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilter()
    {
        FilteredItems.Clear();

        var search =
            SearchText.Trim();

        foreach (var item in Items)
        {
            if (string.IsNullOrWhiteSpace(search) ||
                item.ProductName.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase) ||
                item.VariantName.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase) ||
                (item.Size?.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase)
                    ?? false))
            {
                FilteredItems.Add(item);
            }
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
