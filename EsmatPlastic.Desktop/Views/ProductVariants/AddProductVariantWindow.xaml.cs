using System.Windows;
using EsmatPlastic.Desktop.Models.ProductVariants;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.ProductVariants;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.ProductVariants;

public partial class AddProductVariantWindow : Window
{
    private readonly ProductVariantService _variantService;
    private readonly int _productId;
    private readonly LocalizationService _loc;

    public ProductVariantResponse? CreatedVariant { get; private set; }

    public AddProductVariantWindow(
        ProductVariantService variantService,
        int productId)
    {
        InitializeComponent();

        _variantService = variantService;
        _productId = productId;
        _loc = App.ServiceProvider.GetRequiredService<LocalizationService>();

        ApplyLocalization();

        Loaded += (_, _) => NameInput.Focus();
    }

    private void ApplyLocalization()
    {
        Title = _loc["إضافة صنف / عبوة"];
        HeaderTitle.Text = _loc["إضافة صنف / عبوة جديدة"];
        HeaderSubtitle.Text = _loc["أدخل تفاصيل ومواصفات العبوة أو الصنف للمنتج."];
        NameLabel.Text = _loc["اسم الصنف"];
        SizeLabel.Text = _loc["المقاس"];
        ColorLabel.Text = _loc["اللون"];
        CapTypeLabel.Text = _loc["نوع الغطاء"];
        MaterialLabel.Text = _loc["المادة"];
        CancelButton.Content = _loc["إلغاء"];
        SaveButton.Content = _loc["حفظ"];
    }

    private async void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameInput.Text))
        {
            MessageBox.Show(
                _loc["يرجى إدخال اسم الصنف."],
                _loc["تنبيه"],
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            NameInput.Focus();
            return;
        }

        SaveButton.IsEnabled = false;

        try
        {
            var request = new CreateProductVariantRequest
            {
                ProductId = _productId,
                Name = NameInput.Text.Trim(),
                Size = EmptyToNull(SizeInput.Text),
                Color = EmptyToNull(ColorInput.Text),
                CapType = EmptyToNull(CapTypeInput.Text),
                Material = EmptyToNull(MaterialInput.Text)
            };

            CreatedVariant = await _variantService.CreateAsync(request);

            if (CreatedVariant is null)
            {
                MessageBox.Show(
                    _loc["تعذر حفظ الصنف."],
                    _loc["خطأ"],
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
                _loc["خطأ في حفظ الصنف"],
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private static string? EmptyToNull(string value)
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

