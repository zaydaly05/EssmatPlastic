using EsmatPlastic.Domain.Entities;
using Xunit;

namespace EsmatPlastic.Tests;

public class OrderRequestDomainTests
{
    [Fact]
    public void OrderRequest_Initialization_ShouldSetDefaultValues()
    {
        // Act
        var request = new OrderRequest();

        // Assert
        Assert.Equal(OrderRequestStatus.Pending, request.Status);
        Assert.NotNull(request.Items);
        Assert.Empty(request.Items);
        Assert.True(request.RequestedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void OrderRequest_AddingItems_ShouldMaintainRelation()
    {
        // Arrange
        var request = new OrderRequest
        {
            Id = 1,
            CustomerName = "Alice",
            CustomerPhone = "0123456789"
        };

        var item = new OrderRequestItem
        {
            Id = 10,
            OrderRequestId = request.Id,
            OrderRequest = request,
            ProductVariantId = 5,
            Quantity = 100
        };

        // Act
        request.Items.Add(item);

        // Assert
        Assert.Single(request.Items);
        Assert.Equal(100, request.Items[0].Quantity);
        Assert.Equal(request.Id, request.Items[0].OrderRequestId);
        Assert.Equal("Alice", request.Items[0].OrderRequest.CustomerName);
    }

    [Theory]
    [InlineData(OrderRequestStatus.Pending, 1)]
    [InlineData(OrderRequestStatus.Approved, 2)]
    [InlineData(OrderRequestStatus.Rejected, 3)]
    [InlineData(OrderRequestStatus.Completed, 4)]
    [InlineData(OrderRequestStatus.Cancelled, 5)]
    public void OrderRequestStatus_EnumValueMapping_MatchesExpectedInts(OrderRequestStatus status, int expectedValue)
    {
        Assert.Equal(expectedValue, (int)status);
    }
}
