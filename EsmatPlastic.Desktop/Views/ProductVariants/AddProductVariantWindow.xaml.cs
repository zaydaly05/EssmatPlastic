using System.Windows;
using EsmatPlastic.Desktop.Models.ProductVariants;
using EsmatPlastic.Desktop.Services.ProductVariants;

namespace EsmatPlastic.Desktop.Views.ProductVariants;

public partial class AddProductVariantWindow : Window
{
    private readonly ProductVariantService _variantService;

    private readonly int _productId;

    public ProductVariantResponse? CreatedVariant
    {
        get;
        private set;
    }

    public AddProductVariantWindow(
        ProductVariantService variantService,
        int productId)
    {
        InitializeComponent();

        _variantService = variantService;
        _productId = productId;
    }

    private async void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(
            NameInput.Text))
        {
            MessageBox.Show(
                "يرجى إدخال اسم الصنف.",
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        try
        {
            var request =
                new CreateProductVariantRequest
                {
                    ProductId = _productId,

                    Name =
                        NameInput.Text.Trim(),

                    Size =
                        EmptyToNull(SizeInput.Text),

                    Color =
                        EmptyToNull(ColorInput.Text),

                    CapType =
                        EmptyToNull(CapTypeInput.Text),

                    Material =
                        EmptyToNull(MaterialInput.Text)
                };

            CreatedVariant =
                await _variantService
                    .CreateAsync(request);

            if (CreatedVariant is null)
            {
                MessageBox.Show(
                    "تعذر حفظ الصنف.",
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
                "خطأ في حفظ الصنف",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static string? EmptyToNull(
        string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
