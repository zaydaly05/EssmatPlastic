using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Services.Products;
using EsmatPlastic.Desktop.Views.ProductVariants;
using EsmatPlastic.Desktop.ViewModels.Products;
using EsmatPlastic.Desktop.Models.Products;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.Products;

public partial class ProductsView : UserControl
{
    private readonly ProductsViewModel _viewModel;

    public ProductsView()
    {
        InitializeComponent();

        var serviceProvider =
            App.ServiceProvider;

        var productService =
            serviceProvider.GetRequiredService<ProductService>();

        var appSession =
            serviceProvider.GetRequiredService
                <EsmatPlastic.Desktop.Services.AppSession>();

        _viewModel =
            new ProductsViewModel(
                productService,
                appSession);

        DataContext = _viewModel;

        Loaded += ProductsView_Loaded;

        UpdatePermissionVisibility();
    }

    private async void ProductsView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        Loaded -= ProductsView_Loaded;

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
            App.ServiceProvider
                .GetRequiredService<AddProductWindow>();

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
        _viewModel.RequestEdit();

        if (_viewModel.SelectedProduct is null)
            return;

        var window =
            new EditProductWindow(
                App.ServiceProvider
                    .GetRequiredService<ProductService>(),
                _viewModel.SelectedProduct);

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
        if (_viewModel.SelectedProduct is null)
        {
            MessageBox.Show(
                "يرجى اختيار منتج أولاً.",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        var product =
            _viewModel.SelectedProduct;

        var result =
            MessageBox.Show(
                $"هل تريد فتح أصناف المنتج:\n\n{product.Name}؟",
                "تأكيد",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _viewModel.DeleteCommand.Execute(null);
        }
    }

    private void VariantsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.Tag is ProductResponse product)
        {
            MessageBox.Show(
                $"هل تريد حذف المنتج:\n\n{product.Name}\n\nسيتم حذف البيانات المرتبطة به.",
                "تأكيد الحذف",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}

