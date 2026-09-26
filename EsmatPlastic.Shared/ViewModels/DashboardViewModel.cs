using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using EsmatPlastic.Shared.Models.Auth;
using EsmatPlastic.Shared.Models.Dashboard;
using EsmatPlastic.Shared.Services;

namespace EsmatPlastic.Shared.ViewModels;

public sealed class DashboardViewModel : INotifyPropertyChanged
{
    private readonly FirebaseFirestoreClient _firestoreClient;
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

    public DashboardViewModel(FirebaseFirestoreClient firestoreClient, LoginResponse user)
    {
        _firestoreClient = firestoreClient;
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
            DatabaseStatus = "متصل - Firestore";

            if (CanViewProducts)
            {
                var products = await _firestoreClient.GetCollectionAsync<DashboardProductRecord>("products");
                ProductCount = products.Count(item => item.Data.IsActive);
            }

            if (CanViewStock)
            {
                var variants = await _firestoreClient.GetCollectionAsync<DashboardVariantRecord>("productVariants");
                var transactions = await _firestoreClient.GetCollectionAsync<DashboardTransactionRecord>("stockTransactions");
                VariantCount = variants.Count(item => item.Data.IsActive);
                var totalIn = transactions
                    .Where(item => item.Data.Type == 1)
                    .Sum(item => item.Data.Quantity);
                var totalOut = transactions
                    .Where(item => item.Data.Type == 2)
                    .Sum(item => item.Data.Quantity);
                CurrentStock = totalIn - totalOut;

                if (CanViewReports)
                {
                    TotalIn = totalIn;
                    TotalOut = totalOut;
                }
            }

            if (HasNoDashboardPermission)
                StatusMessage = "لا توجد صلاحيات لعرض بيانات لوحة التحكم.";
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or InvalidOperationException)
        {
            DatabaseStatus = "تعذر تحميل البيانات";
            StatusMessage = "تعذر تحميل بيانات لوحة التحكم من Firestore.";
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

    private sealed class DashboardProductRecord
    {
        public bool IsActive { get; set; } = true;
    }

    private sealed class DashboardVariantRecord
    {
        public bool IsActive { get; set; } = true;
    }

    private sealed class DashboardTransactionRecord
    {
        public int Type { get; set; }
        public decimal Quantity { get; set; }
    }
}