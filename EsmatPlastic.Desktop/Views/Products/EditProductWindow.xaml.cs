using System.Windows;
using EsmatPlastic.Desktop.Models.Products;
using EsmatPlastic.Desktop.Services.Products;

namespace EsmatPlastic.Desktop.Views.Products;

public partial class EditProductWindow : Window
{
    private readonly ProductService _productService;
    private readonly ProductResponse _product;

    public ProductResponse? UpdatedProduct { get; private set; }

    public EditProductWindow(
        ProductService productService,
        ProductResponse product)
    {
        InitializeComponent();

        _productService = productService;
        _product = product;

        NameInput.Text = product.Name;
        DescriptionInput.Text =
            product.Description ?? string.Empty;

        ActiveInput.IsChecked =
            product.IsActive;
    }

    private async void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameInput.Text))
        {
            MessageBox.Show(
                "يرجى إدخال اسم المنتج.",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        try
        {
            var request = new UpdateProductRequest
            {
                Name = NameInput.Text.Trim(),
                Description =
                    string.IsNullOrWhiteSpace(DescriptionInput.Text)
                        ? null
                        : DescriptionInput.Text.Trim(),
                ImagePath = _product.ImagePath,
                IsActive = ActiveInput.IsChecked == true
            };

            UpdatedProduct =
                await _productService.UpdateAsync(
                    _product.Id,
                    request);

            if (UpdatedProduct is null)
            {
                MessageBox.Show(
                    "تعذر تحديث المنتج.",
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "خطأ في تحديث المنتج",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
