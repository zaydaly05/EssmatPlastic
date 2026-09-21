using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EsmatPlastic.Desktop.Models.ProductVariants;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.ProductVariants;

namespace EsmatPlastic.Desktop.ViewModels.ProductVariants;

public class ProductVariantsViewModel : INotifyPropertyChanged
{
    private readonly ProductVariantService _variantService;
    private readonly AppSession _appSession;

    private int _productId;
    private string _productName = string.Empty;

    private ProductVariantResponse? _selectedVariant;

    private bool _isLoading;
    private string _statusMessage = string.Empty;

    public ObservableCollection<ProductVariantResponse> Variants { get; }
        = new();

    public int ProductId
    {
        get => _productId;
        private set
        {
            if (_productId == value)
                return;

            _productId = value;
            OnPropertyChanged();
        }
    }

    public string ProductName
    {
        get => _productName;
        private set
        {
            if (_productName == value)
                return;

            _productName = value;
            OnPropertyChanged();
        }
    }

    public ProductVariantResponse? SelectedVariant
    {
        get => _selectedVariant;
        set
        {
            if (_selectedVariant == value)
                return;

            _selectedVariant = value;

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

    public ICommand DeleteCommand { get; }

    public event EventHandler? AddRequested;

    public event EventHandler<ProductVariantResponse>?
        EditRequested;

    public event PropertyChangedEventHandler?
        PropertyChanged;

    public ProductVariantsViewModel(
        ProductVariantService variantService,
        AppSession appSession)
    {
        _variantService = variantService;
        _appSession = appSession;

        DeleteCommand = new RelayCommand(
            async _ => await DeleteSelectedAsync(),
            _ =>
                CanDelete &&
                SelectedVariant is not null &&
                !IsLoading);
    }

    public void SetProduct(
        int productId,
        string productName)
    {
        ProductId = productId;
        ProductName = productName;
    }

    public async Task LoadAsync()
    {
        if (ProductId <= 0)
            return;

        IsLoading = true;
        StatusMessage = string.Empty;

        try
        {
            var variants =
                await _variantService
                    .GetByProductIdAsync(ProductId);

            Variants.Clear();

            foreach (var variant in variants)
            {
                Variants.Add(variant);
            }

            StatusMessage =
                $"تم تحميل الأصناف: {Variants.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"تعذر تحميل الأصناف: {ex.Message}";
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

        AddRequested?.Invoke(
            this,
            EventArgs.Empty);
    }

    public void RequestEdit()
    {
        if (!CanEdit ||
            SelectedVariant is null)
            return;

        EditRequested?.Invoke(
            this,
            SelectedVariant);
    }

    private async Task DeleteSelectedAsync()
    {
        if (SelectedVariant is null)
            return;

        try
        {
            IsLoading = true;

            var variant =
                SelectedVariant;

            await _variantService
                .DeleteAsync(variant.Id);

            Variants.Remove(variant);

            SelectedVariant = null;

            StatusMessage =
                "تم حذف الصنف بنجاح.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"تعذر حذف الصنف: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
