using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Models.ProductVariants;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.ProductVariants;
using EsmatPlastic.Desktop.ViewModels.ProductVariants;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.ProductVariants;

public partial class ProductVariantsView : UserControl
{
    private readonly ProductVariantsViewModel _viewModel;

    public ProductVariantsView(
        int productId,
        string productName)
    {
        InitializeComponent();

        var serviceProvider =
            App.ServiceProvider;

        var variantService =
            serviceProvider
                .GetRequiredService<ProductVariantService>();

        var appSession =
            serviceProvider
                .GetRequiredService<AppSession>();

        _viewModel =
            new ProductVariantsViewModel(
                variantService,
                appSession);

        _viewModel.SetProduct(
            productId,
            productName);

        DataContext =
            _viewModel;

        var loc = App.ServiceProvider.GetRequiredService<LocalizationService>();
        ProductNameText.Text = $"{loc.T("المنتج")}: {productName}";

        UpdatePermissionVisibility();

        Loaded += ProductVariantsView_Loaded;
    }

    private async void ProductVariantsView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        Loaded -= ProductVariantsView_Loaded;

        await _viewModel.LoadAsync();
    }

    private void UpdatePermissionVisibility()
    {
        AddButton.Visibility =
            _viewModel.CanCreate
                ? Visibility.Visible
                : Visibility.Collapsed;

        EditButton.Visibility =
            _viewModel.CanEdit
                ? Visibility.Visible
                : Visibility.Collapsed;

        DeleteButton.Visibility =
            _viewModel.CanDelete
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    private void AddButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var window =
            new AddProductVariantWindow(
                App.ServiceProvider
                    .GetRequiredService<ProductVariantService>(),
                _viewModel.ProductId);

        window.Owner =
            Window.GetWindow(this);

        var result =
            window.ShowDialog();

        if (result == true)
        {
            _ = _viewModel.LoadAsync();
        }
    }

    private void EditButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_viewModel.SelectedVariant is null)
        {
            var loc = App.ServiceProvider.GetRequiredService<LocalizationService>();
            MessageBox.Show(
                loc.T("يرجى اختيار صنف أولاً."),
                loc.T("تنبيه"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        var window =
            new EditProductVariantWindow(
                App.ServiceProvider
                    .GetRequiredService<ProductVariantService>(),
                _viewModel.SelectedVariant);

        window.Owner =
            Window.GetWindow(this);

        var result =
            window.ShowDialog();

        if (result == true)
        {
            _ = _viewModel.LoadAsync();
        }
    }

    private void DeleteButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_viewModel.SelectedVariant is null)
        {
            var loc = App.ServiceProvider.GetRequiredService<LocalizationService>();
            MessageBox.Show(
                loc.T("يرجى اختيار صنف أولاً."),
                loc.T("تنبيه"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        var locConfirm = App.ServiceProvider.GetRequiredService<LocalizationService>();
        var variant =
            _viewModel.SelectedVariant;

        var result =
            MessageBox.Show(
                locConfirm.IsArabic
                    ? $"هل تريد حذف الصنف:\n\n{variant.Name}؟"
                    : $"Delete this variant?\n\n{variant.Name}",
                locConfirm.T("تأكيد الحذف"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (result ==
            MessageBoxResult.Yes)
        {
            _viewModel.DeleteCommand.Execute(null);
        }
    }
}
