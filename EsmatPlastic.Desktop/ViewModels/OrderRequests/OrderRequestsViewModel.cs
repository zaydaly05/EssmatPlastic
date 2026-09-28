using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EsmatPlastic.Desktop.Models.OrderRequests;
using EsmatPlastic.Desktop.Models.Stock;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.OrderRequests;
using EsmatPlastic.Desktop.Services.Stock;

namespace EsmatPlastic.Desktop.ViewModels.OrderRequests;

public class OrderRequestsViewModel : INotifyPropertyChanged
{
    private readonly OrderRequestService _orderRequestService;
    private readonly StockService _stockService;
    private readonly AppSession _appSession;
    private readonly LocalizationService _loc;

    private bool _isLoading;
    private string _statusMessage = string.Empty;
    private string _customerName = string.Empty;
    private string _customerPhone = string.Empty;
    private StockBalanceResponse? _selectedVariant;
    private decimal _quantity = 1;
    private string _searchText = string.Empty;
    private string _selectedStatusFilter = "All";

    public ObservableCollection<OrderRequestResponseDto> OrderRequests { get; } = new();
    public ObservableCollection<OrderRequestResponseDto> FilteredRequests { get; } = new();
    public ObservableCollection<StockBalanceResponse> AvailableVariants { get; } = new();
    public ObservableCollection<OrderItemDto> CurrentItems { get; } = new();

    public bool IsAdmin => _appSession.IsAdmin();
    public bool CanManage => _appSession.Role == "Admin" || _appSession.Role == "Secretary";

    public string CustomerName
    {
        get => _customerName;
        set { if (_customerName != value) { _customerName = value; OnPropertyChanged(); } }
    }

    public string CustomerPhone
    {
        get => _customerPhone;
        set { if (_customerPhone != value) { _customerPhone = value; OnPropertyChanged(); } }
    }

    public StockBalanceResponse? SelectedVariant
    {
        get => _selectedVariant;
        set { if (_selectedVariant != value) { _selectedVariant = value; OnPropertyChanged(); } }
    }

    public decimal Quantity
    {
        get => _quantity;
        set { if (_quantity != value) { _quantity = value; OnPropertyChanged(); } }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }
    }

    public string SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            if (_selectedStatusFilter != value)
            {
                _selectedStatusFilter = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); }
    }

    // Stats
    public int TotalCount => OrderRequests.Count;
    public int PendingCount => OrderRequests.Count(x => x.Status == "Pending");
    public int ApprovedCount => OrderRequests.Count(x => x.Status == "Approved" || x.Status == "Processing");
    public int CompletedCount => OrderRequests.Count(x => x.Status == "Completed");
    public int CancelledCount => OrderRequests.Count(x => x.Status == "Cancelled");

    public OrderRequestsViewModel(
        OrderRequestService orderRequestService,
        StockService stockService,
        AppSession appSession,
        LocalizationService loc)
    {
        _orderRequestService = orderRequestService;
        _stockService = stockService;
        _appSession = appSession;
        _loc = loc;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var requests = await _orderRequestService.GetAllAsync();
            OrderRequests.Clear();
            foreach (var r in requests.OrderByDescending(x => x.RequestedAt))
            {
                OrderRequests.Add(r);
            }

            var stock = await _stockService.GetCurrentStockAsync();
            AvailableVariants.Clear();
            foreach (var s in stock)
            {
                AvailableVariants.Add(s);
            }

            ApplyFilter();
            UpdateStats();

            StatusMessage = _loc.IsArabic
                ? $"تم تحميل {OrderRequests.Count} طلب حجز"
                : $"Loaded {OrderRequests.Count} order requests";
        }
        catch (Exception ex)
        {
            StatusMessage = _loc.IsArabic
                ? $"خطأ في التحميل: {ex.Message}"
                : $"Load error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void AddCurrentItem()
    {
        if (SelectedVariant == null) return;
        if (Quantity <= 0) return;

        var existing = CurrentItems.FirstOrDefault(x => x.ProductVariantId == SelectedVariant.ProductVariantId);
        if (existing != null)
        {
            existing.Quantity += Quantity;
            // Refresh collection item display
            int idx = CurrentItems.IndexOf(existing);
            CurrentItems.RemoveAt(idx);
            CurrentItems.Insert(idx, existing);
        }
        else
        {
            CurrentItems.Add(new OrderItemDto
            {
                ProductVariantId = SelectedVariant.ProductVariantId,
                Quantity = Quantity,
                VariantName = $"{SelectedVariant.ProductName} - {SelectedVariant.VariantName} ({SelectedVariant.Size})"
            });
        }

        Quantity = 1;
    }

    public void RemoveItem(OrderItemDto item)
    {
        CurrentItems.Remove(item);
    }

    public async Task<bool> SubmitRequestAsync()
    {
        if (string.IsNullOrWhiteSpace(CustomerName) || CurrentItems.Count == 0)
        {
            return false;
        }

        IsLoading = true;
        try
        {
            var dto = new CreateOrderRequestDto
            {
                CustomerName = CustomerName.Trim(),
                CustomerPhone = string.IsNullOrWhiteSpace(CustomerPhone) ? null : CustomerPhone.Trim(),
                Items = CurrentItems.ToList()
            };

            await _orderRequestService.CreateAsync(dto);

            CustomerName = string.Empty;
            CustomerPhone = string.Empty;
            CurrentItems.Clear();

            await LoadAsync();
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = _loc.IsArabic
                ? $"خطأ أثناء إرسال الطلب: {ex.Message}"
                : $"Failed to submit order: {ex.Message}";
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> CancelRequestAsync(int id)
    {
        IsLoading = true;
        try
        {
            await _orderRequestService.CancelAsync(id);
            await LoadAsync();
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = _loc.IsArabic
                ? $"خطأ أثناء إلغاء الطلب: {ex.Message}"
                : $"Failed to cancel order: {ex.Message}";
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        IsLoading = true;
        try
        {
            await _orderRequestService.UpdateStatusAsync(id, status);
            await LoadAsync();
            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = _loc.IsArabic
                ? $"خطأ أثناء تحديث حالة الطلب: {ex.Message}"
                : $"Failed to update status: {ex.Message}";
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilter()
    {
        FilteredRequests.Clear();

        var query = SearchText.Trim();
        var statusFilter = SelectedStatusFilter;

        foreach (var req in OrderRequests)
        {
            bool matchesSearch = string.IsNullOrWhiteSpace(query) ||
                req.CustomerName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (req.CustomerPhone?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                req.Id.ToString().Contains(query) ||
                req.ItemsSummary.Contains(query, StringComparison.OrdinalIgnoreCase);

            bool matchesStatus = statusFilter == "All" || req.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase);

            if (matchesSearch && matchesStatus)
            {
                FilteredRequests.Add(req);
            }
        }
    }

    private void UpdateStats()
    {
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(PendingCount));
        OnPropertyChanged(nameof(ApprovedCount));
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(CancelledCount));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
