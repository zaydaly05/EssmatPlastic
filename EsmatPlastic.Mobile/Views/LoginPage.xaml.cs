using Microsoft.Maui.Controls;
using EsmatPlastic.Shared.ViewModels;

namespace EsmatPlastic.Mobile.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}