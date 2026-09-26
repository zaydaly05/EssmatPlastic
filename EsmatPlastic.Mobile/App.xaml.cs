using EsmatPlastic.Mobile.Views;
using EsmatPlastic.Shared.Models.Auth;
using EsmatPlastic.Shared.Services;
using EsmatPlastic.Shared.ViewModels;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.Linq;

namespace EsmatPlastic.Mobile;

public partial class App : Application
{
    public static ApiClient SharedApiClient { get; private set; } = null!;
    public static FirebaseFirestoreClient SharedFirestoreClient { get; } = new("essmat-plastic");
    private static readonly FirebaseAuthClient SharedFirebaseAuthClient = new();

    public App()
    {
        InitializeComponent();
        SharedApiClient = new ApiClient();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(new LoginPage(CreateLoginViewModel())));
    }

    private LoginViewModel CreateLoginViewModel()
    {
        var viewModel = new LoginViewModel(
            SharedApiClient,
            SharedFirebaseAuthClient,
            SharedFirestoreClient);
        viewModel.LoginSucceeded += ShowDashboard;
        return viewModel;
    }

    private void ShowDashboard(LoginResponse user)
    {
        var window = Windows.FirstOrDefault();
        if (window is not null)
            window.Page = new NavigationPage(
                new DashboardPage(SharedFirestoreClient, user, ShowLogin));
    }

    private void ShowLogin()
    {
        SharedApiClient.ClearToken();
        SharedFirestoreClient.ClearIdToken();

        var window = Windows.FirstOrDefault();
        if (window is not null)
            window.Page = new NavigationPage(new LoginPage(CreateLoginViewModel()));
    }
}
