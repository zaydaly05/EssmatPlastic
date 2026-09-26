using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using EsmatPlastic.Desktop.Models.Products;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.Products;

namespace EsmatPlastic.Desktop.ViewModels.Products;

public class ProductsViewModel : INotifyPropertyChanged
{
    private readonly ProductService _productService;
    private readonly AppSession _appSession;
    private readonly LocalizationService _loc;

    private bool _isLoading;
    private string _statusMessage = string.Empty;
    private string _searchText = string.Empty;
    private ProductResponse? _selectedProduct;

    public ObservableCollection<ProductResponse> Products { get; }
        = new();

    public ICollectionView FilteredProducts { get; }

    public ProductResponse? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (_selectedProduct == value)
                return;

            _selectedProduct = value;
            OnPropertyChanged();
            CommandManager.InvalidateRequerySuggested();
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
            CommandManager.InvalidateRequerySuggested();
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

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
                return;

            _searchText = value;
            OnPropertyChanged();
            FilteredProducts.Refresh();

            if (SelectedProduct is not null && !MatchesSearch(SelectedProduct))
                SelectedProduct = null;

            UpdateResultsMessage();
        }
    }

    public bool CanCreate =>
        _appSession.HasPermission("Products.Create");

    public bool CanEdit =>
        _appSession.HasPermission("Products.Edit");

    public bool CanDelete =>
        _appSession.HasPermission("Products.Delete");

    public ICommand LoadCommand { get; }
    public ICommand DeleteCommand { get; }

    public event EventHandler? AddRequested;
    public event EventHandler<ProductResponse>? EditRequested;
    public event EventHandler<ProductResponse>? VariantsRequested;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ProductsViewModel(
        ProductService productService,
        AppSession appSession,
        LocalizationService loc)
    {
        _productService = productService;
        _appSession = appSession;
        _loc = loc;
        FilteredProducts = CollectionViewSource.GetDefaultView(Products);
        FilteredProducts.Filter = item => item is ProductResponse product && MatchesSearch(product);

        LoadCommand = new RelayCommand(
            async _ => await LoadAsync());

        DeleteCommand = new RelayCommand(
            async _ => await DeleteSelectedAsync(),
            _ => CanDelete && SelectedProduct is not null && !IsLoading);
    }

    public async Task LoadAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        StatusMessage = _loc.IsArabic ? "جاري تحميل المنتجات..." : "Loading products...";

        try
        {
            var products =
                await _productService.GetAllAsync();

            Products.Clear();

            foreach (var product in products)
                Products.Add(product);

            FilteredProducts.Refresh();
            UpdateResultsMessage();
        }
        catch (Exception ex)
        {
            StatusMessage = _loc.IsArabic ? $"تعذر تحميل المنتجات: {ex.Message}" : $"Failed to load products: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void RequestAdd()
    {
        if (!CanCreate)
            return;

        AddRequested?.Invoke(this, EventArgs.Empty);
    }

    public void RequestEdit()
    {
        if (!CanEdit || SelectedProduct is null)
            return;

        EditRequested?.Invoke(
            this,
            SelectedProduct);
    }

    public void RequestVariants()
    {
        if (SelectedProduct is null)
            return;

        VariantsRequested?.Invoke(
            this,
            SelectedProduct);
    }

    private async Task DeleteSelectedAsync()
    {
        if (SelectedProduct is null)
            return;

        try
        {
            IsLoading = true;

            var id = SelectedProduct.Id;

            await _productService.DeleteAsync(id);

            Products.Remove(SelectedProduct);

            SelectedProduct = null;
            UpdateResultsMessage();
            StatusMessage = _loc.IsArabic ? "تم حذف المنتج بنجاح." : "Product deleted successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = _loc.IsArabic ? $"تعذر حذف المنتج: {ex.Message}" : $"Failed to delete product: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void RefreshPermissions()
    {
        OnPropertyChanged(nameof(CanCreate));
        OnPropertyChanged(nameof(CanEdit));
        OnPropertyChanged(nameof(CanDelete));

        CommandManager.InvalidateRequerySuggested();
    }

    private bool MatchesSearch(ProductResponse product)
    {
        var search = SearchText.Trim();
        return string.IsNullOrWhiteSpace(search) ||
            product.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
            (product.Description?.Contains(search, StringComparison.CurrentCultureIgnoreCase) ?? false);
    }

    private void UpdateResultsMessage()
    {
        var visibleCount = FilteredProducts.Cast<ProductResponse>().Count();
        StatusMessage = string.IsNullOrWhiteSpace(SearchText)
            ? (_loc.IsArabic ? $"إجمالي المنتجات: {Products.Count}" : $"Total products: {Products.Count}")
            : (_loc.IsArabic
                ? $"يعرض {visibleCount} من أصل {Products.Count} منتج"
                : $"Showing {visibleCount} of {Products.Count} products");
    }

    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
