using System.Windows;
using EsmatPlastic.Desktop.Models.ProductVariants;
using EsmatPlastic.Desktop.Services.ProductVariants;

namespace EsmatPlastic.Desktop.Views.ProductVariants;

public partial class EditProductVariantWindow : Window
{
    private readonly ProductVariantService _variantService;

    private readonly ProductVariantResponse _variant;

    public ProductVariantResponse? UpdatedVariant
    {
        get;
        private set;
    }

    public EditProductVariantWindow(
        ProductVariantService variantService,
        ProductVariantResponse variant)
    {
        InitializeComponent();

        _variantService = variantService;
        _variant = variant;

        NameInput.Text =
            variant.Name;

        SizeInput.Text =
            variant.Size ?? string.Empty;

        ColorInput.Text =
            variant.Color ?? string.Empty;

        CapTypeInput.Text =
            variant.CapType ?? string.Empty;

        MaterialInput.Text =
            variant.Material ?? string.Empty;

        ActiveInput.IsChecked =
            variant.IsActive;
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
                new UpdateProductVariantRequest
                {
                    Name =
                        NameInput.Text.Trim(),

                    Size =
                        EmptyToNull(SizeInput.Text),

                    Color =
                        EmptyToNull(ColorInput.Text),

                    CapType =
                        EmptyToNull(CapTypeInput.Text),

                    Material =
                        EmptyToNull(MaterialInput.Text),

                    ImagePath =
                        _variant.ImagePath,

                    IsActive =
                        ActiveInput.IsChecked == true
                };

            UpdatedVariant =
                await _variantService
                    .UpdateAsync(
                        _variant.Id,
                        request);

            if (UpdatedVariant is null)
            {
                MessageBox.Show(
                    "تعذر تحديث الصنف.",
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
                "خطأ في تحديث الصنف",
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
