using System.Windows;
using EsmatPlastic.Desktop.Models.Products;
using EsmatPlastic.Desktop.Services.Products;

namespace EsmatPlastic.Desktop.Views.Products;

public partial class AddProductWindow : Window
{
    private readonly ProductService _productService;

    public ProductResponse? CreatedProduct { get; private set; }

    public AddProductWindow(ProductService productService)
    {
        InitializeComponent();

        _productService = productService;
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
            var request = new CreateProductRequest
            {
                Name = NameInput.Text.Trim(),
                Description =
                    string.IsNullOrWhiteSpace(DescriptionInput.Text)
                        ? null
                        : DescriptionInput.Text.Trim()
            };

            CreatedProduct =
                await _productService.CreateAsync(request);

            if (CreatedProduct is null)
            {
                MessageBox.Show(
                    "تعذر حفظ المنتج.",
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
                "خطأ في حفظ المنتج",
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
