using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EsmatPlastic.Desktop.Models.Products;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Products;

namespace EsmatPlastic.Desktop.ViewModels.Products;

public class ProductsViewModel : INotifyPropertyChanged
{
    private readonly ProductService _productService;
    private readonly AppSession _appSession;

    private bool _isLoading;
    private string _statusMessage = string.Empty;
    private ProductResponse? _selectedProduct;

    public ObservableCollection<ProductResponse> Products { get; }
        = new();

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
        AppSession appSession)
    {
        _productService = productService;
        _appSession = appSession;

        LoadCommand = new RelayCommand(
            async _ => await LoadAsync());

        DeleteCommand = new RelayCommand(
            async _ => await DeleteSelectedAsync(),
            _ => CanDelete && SelectedProduct is not null && !IsLoading);
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        StatusMessage = string.Empty;

        try
        {
            var products =
                await _productService.GetAllAsync();

            Products.Clear();

            foreach (var product in products)
                Products.Add(product);

            StatusMessage =
                $"تم تحميل المنتجات: {Products.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"تعذر تحميل المنتجات: {ex.Message}";
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

            StatusMessage = "تم حذف المنتج بنجاح.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"تعذر حذف المنتج: {ex.Message}";
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

    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
