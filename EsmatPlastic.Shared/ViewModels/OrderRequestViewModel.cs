using System.Collections.ObjectModel;
using EsmatPlastic.Shared.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace EsmatPlastic.Shared.ViewModels;

public class OrderRequestViewModel : INotifyPropertyChanged
{
    private readonly ApiClient _apiClient;
    private string _customerName;
    private string _customerPhone;
    private ObservableCollection<OrderItemDto> _selectedItems = new();
    private bool _isBusy;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string CustomerName
    {
        get => _customerName;
        set { _customerName = value; OnPropertyChanged(); }
    }

    public string CustomerPhone
    {
        get => _customerPhone;
        set { _customerPhone = value; OnPropertyChanged(); }
    }

    public ObservableCollection<OrderItemDto> SelectedItems => _selectedItems;

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public OrderRequestViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        _customerName = string.Empty;
        _customerPhone = string.Empty;
    }

    public async Task<bool> SubmitRequestAsync()
    {
        if (string.IsNullOrWhiteSpace(CustomerName) || SelectedItems.Count == 0)
            return false;

        IsBusy = true;
        try
        {
            var request = new
            {
                CustomerName = CustomerName,
                CustomerPhone = CustomerPhone,
                Items = SelectedItems.Select(i => new { i.ProductVariantId, i.Quantity }).ToList()
            };

            var success = await _apiClient.PostAsync<object, bool>("api/OrderRequests", request);
            return success;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void AddItem(int variantId, string name, decimal qty)
    {
        _selectedItems.Add(new OrderItemDto { ProductVariantId = variantId, VariantName = name, Quantity = qty });
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class OrderItemDto
{
    public int ProductVariantId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}
