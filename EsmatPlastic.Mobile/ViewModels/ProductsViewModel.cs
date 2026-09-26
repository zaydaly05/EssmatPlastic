using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EsmatPlastic.Shared.Models.Auth;
using EsmatPlastic.Shared.Services;

namespace EsmatPlastic.Mobile.ViewModels;

public sealed class ProductsViewModel : INotifyPropertyChanged
{
    private readonly FirebaseFirestoreClient _firestoreClient;
    private readonly LoginResponse _user;
    private readonly List<ProductItemViewModel> _allProducts = new();
    private string _searchText = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isLoading;
    private bool _isRefreshing;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ProductItemViewModel> Products { get; } = new();

    public bool CanViewProducts => _user.Permissions.Any(
        permission => permission.Equals("Products.View", StringComparison.OrdinalIgnoreCase));

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

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            if (_isRefreshing == value)
                return;

            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public ICommand RefreshCommand { get; }

    public ProductsViewModel(FirebaseFirestoreClient firestoreClient, LoginResponse user)
    {
        _firestoreClient = firestoreClient;
        _user = user;
        RefreshCommand = new AsyncCommand(LoadAsync);
    }

    public async Task LoadAsync()
    {
        if (IsLoading)
            return;

        if (!CanViewProducts)
        {
            StatusMessage = "ليس لديك صلاحية لعرض المنتجات.";
            IsRefreshing = false;
            return;
        }

        IsLoading = true;
        StatusMessage = string.Empty;

        try
        {
            var documents = await _firestoreClient.GetCollectionAsync<ProductRecord>("products");
            _allProducts.Clear();
            _allProducts.AddRange(documents.Select(document => new ProductItemViewModel
            {
                Name = document.Data.Name,
                Description = document.Data.Description,
                StatusText = document.Data.IsActive ? "نشط" : "غير نشط",
                IsActive = document.Data.IsActive
            }));
            ApplyFilter();

            if (_allProducts.Count == 0)
                StatusMessage = "لا توجد منتجات.";
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or System.Text.Json.JsonException)
        {
            StatusMessage = "تعذر تحميل المنتجات من Firestore. تحقق من الاتصال والصلاحيات.";
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
        }
    }

    private void ApplyFilter()
    {
        var search = SearchText.Trim();
        var filtered = _allProducts.Where(product =>
            string.IsNullOrWhiteSpace(search) ||
            product.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
            (product.Description?.Contains(search, StringComparison.CurrentCultureIgnoreCase) ?? false));

        Products.Clear();
        foreach (var product in filtered)
            Products.Add(product);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private sealed class ProductRecord
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    private sealed class AsyncCommand : ICommand
    {
        private readonly Func<Task> _execute;

        public event EventHandler? CanExecuteChanged;

        public AsyncCommand(Func<Task> execute) => _execute = execute;

        public bool CanExecute(object? parameter) => true;

        public async void Execute(object? parameter) => await _execute();
    }
}

public sealed class ProductItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}