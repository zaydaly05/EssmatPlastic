using EsmatPlastic.Desktop.Models.OrderRequests;
using EsmatPlastic.Desktop.Models.Stock;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.OrderRequests;
using EsmatPlastic.Desktop.Services.Settings;
using EsmatPlastic.Desktop.Services.Stock;
using EsmatPlastic.Desktop.ViewModels.OrderRequests;
using Xunit;

namespace EsmatPlastic.Tests;

public class OrderRequestsViewModelTests
{
    private (OrderRequestsViewModel vm, AppSession session) CreateViewModel()
    {
        var settingsService = new SettingsService();
        var apiClient = new ApiClient(settingsService);
        var orderRequestService = new OrderRequestService(apiClient);
        var stockService = new StockService(apiClient);
        var session = new AppSession();
        var loc = new LocalizationService();

        var vm = new OrderRequestsViewModel(orderRequestService, stockService, session, loc);
        return (vm, session);
    }

    [Fact]
    public void AddCurrentItem_ShouldAddItemToCurrentItemsCollection()
    {
        // Arrange
        var (vm, _) = CreateViewModel();
        var variant = new StockBalanceResponse
        {
            ProductVariantId = 10,
            ProductName = "Test Product",
            VariantName = "500ml",
            Size = "Large"
        };
        vm.SelectedVariant = variant;
        vm.Quantity = 5;

        // Act
        vm.AddCurrentItem();

        // Assert
        Assert.Single(vm.CurrentItems);
        Assert.Equal(10, vm.CurrentItems[0].ProductVariantId);
        Assert.Equal(5, vm.CurrentItems[0].Quantity);
        Assert.Equal(1, vm.Quantity); // Reset to default 1
    }

    [Fact]
    public void AddCurrentItem_WhenVariantAlreadyExists_ShouldAccumulateQuantity()
    {
        // Arrange
        var (vm, _) = CreateViewModel();
        var variant = new StockBalanceResponse
        {
            ProductVariantId = 10,
            ProductName = "Test Product",
            VariantName = "500ml",
            Size = "Large"
        };
        vm.SelectedVariant = variant;
        vm.Quantity = 3;
        vm.AddCurrentItem();

        // Act
        vm.SelectedVariant = variant;
        vm.Quantity = 4;
        vm.AddCurrentItem();

        // Assert
        Assert.Single(vm.CurrentItems);
        Assert.Equal(7, vm.CurrentItems[0].Quantity);
    }

    [Fact]
    public void RemoveItem_ShouldRemoveSpecifiedItem()
    {
        // Arrange
        var (vm, _) = CreateViewModel();
        var item = new OrderItemDto { ProductVariantId = 1, Quantity = 2, VariantName = "Item 1" };
        vm.CurrentItems.Add(item);

        // Act
        vm.RemoveItem(item);

        // Assert
        Assert.Empty(vm.CurrentItems);
    }

    [Fact]
    public async Task SubmitRequestAsync_WithEmptyCustomerNameOrItems_ShouldReturnFalse()
    {
        // Arrange
        var (vm, _) = CreateViewModel();

        // Act & Assert 1: Empty CustomerName, Empty Items
        var result1 = await vm.SubmitRequestAsync();
        Assert.False(result1);

        // Act & Assert 2: Has CustomerName but Empty Items
        vm.CustomerName = "John Customer";
        var result2 = await vm.SubmitRequestAsync();
        Assert.False(result2);

        // Act & Assert 3: Empty CustomerName but Has Items
        vm.CustomerName = "";
        vm.CurrentItems.Add(new OrderItemDto { ProductVariantId = 1, Quantity = 10 });
        var result3 = await vm.SubmitRequestAsync();
        Assert.False(result3);
    }

    [Fact]
    public void CanManage_ShouldBeTrueOnlyForAdminOrSecretary()
    {
        // Arrange
        var (vm, session) = CreateViewModel();

        // Admin
        session.Start(new EsmatPlastic.Desktop.Models.LoginResponse { Role = "Admin", Token = "tok" });
        Assert.True(vm.CanManage);

        // Secretary
        session.Start(new EsmatPlastic.Desktop.Models.LoginResponse { Role = "Secretary", Token = "tok" });
        Assert.True(vm.CanManage);

        // Warehouse
        session.Start(new EsmatPlastic.Desktop.Models.LoginResponse { Role = "Warehouse", Token = "tok" });
        Assert.False(vm.CanManage);
    }
}
