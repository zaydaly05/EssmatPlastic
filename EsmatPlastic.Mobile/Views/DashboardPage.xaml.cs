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
        NavigationPage.SetHasNavigationBar(this, false);
        _firestoreClient = firestoreClient;
        _user = user;
        _viewModel = new DashboardViewModel(firestoreClient, user);
        _logout = logout;
        BindingContext = _viewModel;
        AddSidebar();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private async void Refresh_Clicked(object sender, EventArgs e)
    {
        await _viewModel.LoadAsync(forceRefresh: true);
    }

    private void Logout_Clicked(object sender, EventArgs e)
    {
        _logout();
    }

    private async void Products_Clicked(object sender, EventArgs e)
    {
        await OpenSectionAsync("Products");
    }

    private Task OpenSectionAsync(string section) =>
        Navigation.PushAsync(new MobileWorkspacePage(_user, _logout, section));

    private async void ProductsCard_Tapped(object sender, TappedEventArgs e) =>
        await OpenSectionAsync("Products");

    private async void VariantsCard_Tapped(object sender, TappedEventArgs e) =>
        await OpenSectionAsync("Products");

    private async void WarehouseCard_Tapped(object sender, TappedEventArgs e) =>
        await OpenSectionAsync("Warehouse");

    private async void ReportsCard_Tapped(object sender, TappedEventArgs e) =>
        await OpenSectionAsync("Reports");

    private void Menu_Clicked(object sender, EventArgs e)
    {
        if (RootLayout.Children.LastOrDefault() is Grid sidebar)
            sidebar.IsVisible = true;
    }

    private void AddSidebar()
    {
        var overlay = new Grid
        {
            IsVisible = false,
            ZIndex = 20,
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(new GridLength(292)) },
            BackgroundColor = Color.FromArgb("#660B1729")
        };
        var scrim = new BoxView { Color = Colors.Transparent };
        var closeTap = new TapGestureRecognizer();
        closeTap.Tapped += (_, _) => overlay.IsVisible = false;
        scrim.GestureRecognizers.Add(closeTap);
        overlay.Children.Add(scrim);

        var menu = new VerticalStackLayout { Padding = new Thickness(18, 26), Spacing = 9, BackgroundColor = Color.FromArgb("#0F172A") };
        menu.Children.Add(new Label { Text = "Esmat Plastic", FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, Margin = new Thickness(8, 2, 8, 0) });
        menu.Children.Add(new Label { Text = _user.FullName, FontSize = 14, TextColor = Color.FromArgb("#94A3B8"), Margin = new Thickness(8, 0, 8, 18) });

        AddMenuItem("⌂   Dashboard", null, async () => { overlay.IsVisible = false; await Navigation.PopToRootAsync(); });
        AddMenuItem("▦   Products & variants", "Products.View", async () => await Navigate("Products"));
        AddMenuItem("▤   Warehouse", "Stock.View", async () => await Navigate("Warehouse"));
        AddMenuItem("▥   Reports", "Reports.View", async () => await Navigate("Reports"));
        if (_user.Role is "Admin" or "Secretary")
            AddMenuItem("▧   Order requests", null, async () => await Navigate("Orders"));
        AddMenuItem("♙   Users", "Users.View", async () => await Navigate("Users"));
        AddMenuItem("⚙   Permissions", "Permissions.Manage", async () => await Navigate("Permissions"));
        AddMenuItem("⚙   Settings", null, async () => await Navigate("Settings"));

        var signOut = new Button { Text = "Sign out", TextColor = Color.FromArgb("#FCA5A5"), BackgroundColor = Color.FromArgb("#263044"), CornerRadius = 10, HeightRequest = 48, Margin = new Thickness(0, 22, 0, 0), HorizontalOptions = LayoutOptions.Fill };
        signOut.Clicked += (_, _) => { overlay.IsVisible = false; _logout(); };
        menu.Children.Add(signOut);

        var panel = new Border
        {
            Content = new ScrollView { Content = menu },
            BackgroundColor = Color.FromArgb("#0F172A"),
            StrokeThickness = 0,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        Grid.SetColumn(panel, 1);
        overlay.Children.Add(panel);
        RootLayout.Children.Add(overlay);

        void AddMenuItem(string title, string? permission, Func<Task> navigate)
        {
            if (permission is not null && !_user.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase))
                return;
            var item = new Button
            {
                Text = title,
                TextColor = Color.FromArgb("#E2E8F0"),
                BackgroundColor = Colors.Transparent,
                CornerRadius = 10,
                Padding = new Thickness(14, 11),
                HeightRequest = 48
            };
            item.Clicked += async (_, _) => { overlay.IsVisible = false; await navigate(); };
            menu.Children.Add(item);
        }

        async Task Navigate(string section) => await OpenSectionAsync(section);
    }
}
