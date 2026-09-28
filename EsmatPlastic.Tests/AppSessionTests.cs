using EsmatPlastic.Desktop.Models;
using EsmatPlastic.Desktop.Services;
using Xunit;

namespace EsmatPlastic.Tests;

public class AppSessionTests
{
    [Fact]
    public void Start_ShouldSetUserAndLoggedInState()
    {
        // Arrange
        var appSession = new AppSession();
        var user = new LoginResponse
        {
            UserId = 42,
            Username = "johndoe",
            FullName = "John Doe",
            Role = "Admin",
            Token = "valid-token",
            Permissions = new List<string> { "ManageOrders", "ViewReports" }
        };

        // Act
        appSession.Start(user);

        // Assert
        Assert.True(appSession.IsLoggedIn);
        Assert.Equal(42, appSession.UserId);
        Assert.Equal("johndoe", appSession.Username);
        Assert.Equal("John Doe", appSession.FullName);
        Assert.Equal("Admin", appSession.Role);
        Assert.True(appSession.IsAdmin());
        Assert.False(appSession.IsWarehouse());
        Assert.False(appSession.IsAccountant());
    }

    [Fact]
    public void Clear_ShouldResetSessionState()
    {
        // Arrange
        var appSession = new AppSession();
        appSession.Start(new LoginResponse { UserId = 1, Role = "Warehouse", Token = "tok" });

        // Act
        appSession.Clear();

        // Assert
        Assert.False(appSession.IsLoggedIn);
        Assert.Equal(0, appSession.UserId);
        Assert.Equal(string.Empty, appSession.Role);
        Assert.Null(appSession.CurrentUser);
    }

    [Theory]
    [InlineData("Admin", "ManageOrders", true)]
    [InlineData("Secretary", "ManageOrders", true)]
    [InlineData("Secretary", "DeleteDatabase", false)]
    public void HasPermission_ShouldReturnExpectedResult(string role, string permissionToCheck, bool expected)
    {
        // Arrange
        var appSession = new AppSession();
        var user = new LoginResponse
        {
            UserId = 10,
            Role = role,
            Token = "jwt",
            Permissions = role == "Admin" ? new List<string>() : new List<string> { "ManageOrders" }
        };
        appSession.Start(user);

        // Act
        var result = appSession.HasPermission(permissionToCheck);

        // Assert
        Assert.Equal(expected, result);
    }
}
