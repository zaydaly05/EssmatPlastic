using EsmatPlastic.Mobile.ViewModels;
using EsmatPlastic.Shared.Models.Auth;
using EsmatPlastic.Shared.Services;

namespace EsmatPlastic.Mobile.Views;

public partial class ProductsPage : ContentPage
{
    private readonly ProductsViewModel _viewModel;

    public ProductsPage(FirebaseFirestoreClient firestoreClient, LoginResponse user)
    {
        InitializeComponent();
        _viewModel = new ProductsViewModel(firestoreClient, user);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private void SearchButtonPressed(object? sender, EventArgs e) =>
        _viewModel.SearchText = ((SearchBar)sender!).Text ?? string.Empty;
}