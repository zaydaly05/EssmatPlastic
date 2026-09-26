using EsmatPlastic.Shared.Models.Auth;
using EsmatPlastic.Shared.Services;
using EsmatPlastic.Shared.ViewModels;
using Microsoft.Maui.Controls;

namespace EsmatPlastic.Mobile.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;
    private readonly Action _logout;
    private readonly FirebaseFirestoreClient _firestoreClient;
    private readonly LoginResponse _user;

    public DashboardPage(FirebaseFirestoreClient firestoreClient, LoginResponse user, Action logout)
    {
        InitializeComponent();
        _firestoreClient = firestoreClient;
        _user = user;
        _viewModel = new DashboardViewModel(firestoreClient, user);
        _logout = logout;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private async void Refresh_Clicked(object sender, EventArgs e)
    {
        await _viewModel.LoadAsync();
    }

    private void Logout_Clicked(object sender, EventArgs e)
    {
        _logout();
    }

    private async void Products_Clicked(object sender, EventArgs e)
    {
        if (_user.Permissions.Any(permission =>
                permission.Equals("Products.View", StringComparison.OrdinalIgnoreCase)))
        {
            await Navigation.PushAsync(new ProductsPage(_firestoreClient, _user));
        }
    }
}