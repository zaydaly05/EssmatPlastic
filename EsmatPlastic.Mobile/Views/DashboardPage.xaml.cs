using EsmatPlastic.Shared.Models.Auth;
using EsmatPlastic.Shared.Services;
using EsmatPlastic.Shared.ViewModels;
using Microsoft.Maui.Controls;

namespace EsmatPlastic.Mobile.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;
    private readonly Action _logout;

    public DashboardPage(ApiClient apiClient, LoginResponse user, Action logout)
    {
        InitializeComponent();
        _viewModel = new DashboardViewModel(apiClient, user);
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
}