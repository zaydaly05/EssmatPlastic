using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EsmatPlastic.Shared.Models.Auth;
using EsmatPlastic.Shared.Models.Dashboard;
using EsmatPlastic.Shared.Services;

namespace EsmatPlastic.Shared.ViewModels;

public sealed class DashboardViewModel : INotifyPropertyChanged
{
    private readonly ApiClient _apiClient;
    private readonly LoginResponse _user;
    private int _productCount;
    private int _variantCount;
    private decimal _currentStock;
    private decimal _totalIn;
    private decimal _totalOut;
    private string _databaseStatus = "جاري الاتصال...";
    private string _statusMessage = string.Empty;
    private bool _isLoading;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string UserName => string.IsNullOrWhiteSpace(_user.FullName)
        ? _user.Username
        : _user.FullName;

    public bool CanViewProducts => HasPermission("Products.View");
    public bool CanViewStock => HasPermission("Stock.View");
    public bool CanViewReports => HasPermission("Reports.View");
    public bool HasNoDashboardPermission => !CanViewProducts && !CanViewStock;

    public int ProductCount
    {
        get => _productCount;
        private set => SetProperty(ref _productCount, value);
    }

    public int VariantCount
    {
        get => _variantCount;
        private set => SetProperty(ref _variantCount, value);
    }

    public decimal CurrentStock
    {
        get => _currentStock;
        private set => SetProperty(ref _currentStock, value);
    }

    public decimal TotalIn
    {
        get => _totalIn;
        private set => SetProperty(ref _totalIn, value);
    }

    public decimal TotalOut
    {
        get => _totalOut;
        private set => SetProperty(ref _totalOut, value);
    }

    public string DatabaseStatus
    {
        get => _databaseStatus;
        private set => SetProperty(ref _databaseStatus, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public DashboardViewModel(ApiClient apiClient, LoginResponse user)
    {
        _apiClient = apiClient;
        _user = user;
    }

    public async Task LoadAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        StatusMessage = string.Empty;

        try
        {
            var health = await _apiClient.GetHealthStatusAsync();
            if (health is null || !health.IsOnline)
            {
                DatabaseStatus = "الخادم المحلي غير متصل";
                StatusMessage = "تعذر الاتصال بقاعدة البيانات المحلية.";
                return;
            }

            DatabaseStatus = $"متصل - {health.PrimaryDatabase}";

            if (CanViewProducts)
            {
                var products = await _apiClient.GetAsync<List<DashboardProductSummary>>(
                    "api/Products");
                ProductCount = products?.Count ?? 0;
            }

            if (CanViewStock)
            {
                var stock = await _apiClient.GetAsync<List<DashboardStockBalance>>(
                    "api/Stock/current") ?? new List<DashboardStockBalance>();

                VariantCount = stock.Count;
                CurrentStock = stock.Sum(item => item.CurrentQuantity);

                if (CanViewReports)
                {
                    TotalIn = stock.Sum(item => item.TotalIn);
                    TotalOut = stock.Sum(item => item.TotalOut);
                }
            }

            if (HasNoDashboardPermission)
                StatusMessage = "لا توجد صلاحيات لعرض بيانات لوحة التحكم.";
        }
        catch (HttpRequestException)
        {
            DatabaseStatus = "تعذر تحميل البيانات";
            StatusMessage = "تعذر تحميل بيانات لوحة التحكم من الخادم المحلي.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool HasPermission(string permission) => _user.Permissions.Any(
        item => item.Equals(permission, StringComparison.OrdinalIgnoreCase));

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}