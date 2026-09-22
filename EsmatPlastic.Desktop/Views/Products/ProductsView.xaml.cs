using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EsmatPlastic.Desktop.Models.Products;
using EsmatPlastic.Desktop.Services;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.Products;
using EsmatPlastic.Desktop.ViewModels.Products;
using EsmatPlastic.Desktop.Views.ProductVariants;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.Products;

public partial class ProductsView : UserControl
{
    private readonly ProductsViewModel _viewModel;
    private readonly AppSession _appSession;
    private readonly LocalizationService _loc;

    public ProductsView()
    {
        InitializeComponent();

        var serviceProvider = App.ServiceProvider;
        _loc = serviceProvider.GetRequiredService<LocalizationService>();
        _appSession = serviceProvider.GetRequiredService<AppSession>();

        _viewModel = new ProductsViewModel(
            serviceProvider.GetRequiredService<ProductService>(),
            _appSession);

        DataContext = _viewModel;
        ApplyLocalization();

        Loaded += ProductsView_Loaded;
        UpdatePermissionVisibility();
    }

    private void ApplyLocalization()
    {
        PageTitleText.Text = _loc.T("المنتجات");
        PageSubtitleText.Text = _loc.T("إدارة المنتجات والأصناف والبطاقات");

        AddButton.Content = _loc.T("+  إضافة منتج");
        HeaderPhoto.Text = _loc.T("الصورة");
        HeaderProduct.Text = _loc.T("المنتج والوصف");
        HeaderVariants.Text = _loc.T("عدد الأصناف");
        HeaderStatus.Text = _loc.T("الحالة");
        HeaderActions.Text = _loc.T("الإجراءات");

        EditButton.Content = _loc.T("تعديل المنتج");
        DeleteButton.Content = _loc.T("حذف المنتج");
    }

    private async void ProductsView_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= ProductsView_Loaded;
        await _viewModel.LoadAsync();
        UpdatePermissionVisibility();
    }

    private void UpdatePermissionVisibility()
    {
        // Admin or Products.Manage permissions required to add/edit/delete/upload photos
        bool isAdminOrManage = _appSession.IsAdmin() || _appSession.HasPermission("Products.Manage");

        AddButton.Visibility = (isAdminOrManage || _viewModel.CanCreate) ? Visibility.Visible : Visibility.Collapsed;
        EditButton.Visibility = (isAdminOrManage || _viewModel.CanEdit) ? Visibility.Visible : Visibility.Collapsed;
        DeleteButton.Visibility = (isAdminOrManage || _viewModel.CanDelete) ? Visibility.Visible : Visibility.Collapsed;
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_appSession.IsAdmin() && !_viewModel.CanCreate)
        {
            MessageBox.Show(
                _loc.T("عفواً، ميزة إضافة المنتجات ورفع الصور مقتصرة على مدير النظام فقط."),
                _loc.T("تنبيه الصلاحيات"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var window = App.ServiceProvider.GetRequiredService<AddProductWindow>();
        window.Owner = Window.GetWindow(this);

        if (window.ShowDialog() == true)
        {
            _ = _viewModel.LoadAsync();
        }
    }

    private void PhotoThumbnail_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is ProductResponse product)
        {
            if (!_appSession.IsAdmin() && !_viewModel.CanEdit)
            {
                MessageBox.Show(
                    _loc.T("عفواً، ميزة تعديل المنتجات ورفع الصور مقتصرة على مدير النظام فقط."),
                    _loc.T("تنبيه الصلاحيات"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var window = new EditProductWindow(
                App.ServiceProvider.GetRequiredService<ProductService>(),
                product);

            window.Owner = Window.GetWindow(this);

            if (window.ShowDialog() == true)
            {
                _ = _viewModel.LoadAsync();
            }
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_appSession.IsAdmin() && !_viewModel.CanEdit)
        {
            MessageBox.Show(
                _loc.T("عفواً، ميزة تعديل المنتجات مقتصرة على مدير النظام فقط."),
                _loc.T("تنبيه الصلاحيات"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        _viewModel.RequestEdit();

        if (_viewModel.SelectedProduct is null)
        {
            MessageBox.Show(
                _loc.T("يرجى اختيار منتج أولاً."),
                _loc.T("تنبيه"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var window = new EditProductWindow(
            App.ServiceProvider.GetRequiredService<ProductService>(),
            _viewModel.SelectedProduct);

        window.Owner = Window.GetWindow(this);

        if (window.ShowDialog() == true)
        {
            _ = _viewModel.LoadAsync();
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_appSession.IsAdmin() && !_viewModel.CanDelete)
        {
            MessageBox.Show(
                _loc.T("عفواً، ميزة حذف المنتجات مقتصرة على مدير النظام فقط."),
                _loc.T("تنبيه الصلاحيات"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (_viewModel.SelectedProduct is null)
        {
            MessageBox.Show(
                _loc.T("يرجى اختيار منتج أولاً."),
                _loc.T("تنبيه"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var product = _viewModel.SelectedProduct;

        var result = MessageBox.Show(
            _loc.IsArabic
                ? $"هل تريد حذف المنتج:\n\n{product.Name}\n\nسيتم حذف البيانات المرتبطة به."
                : $"Delete this product?\n\n{product.Name}\n\nRelated data will also be removed.",
            _loc.T("تأكيد الحذف"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _viewModel.DeleteCommand.Execute(null);
        }
    }

    private void VariantsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.Tag is ProductResponse product &&
            Window.GetWindow(this) is MainWindow mainWindow)
        {
            mainWindow.ShowProductVariants(product);
        }
    }
}
