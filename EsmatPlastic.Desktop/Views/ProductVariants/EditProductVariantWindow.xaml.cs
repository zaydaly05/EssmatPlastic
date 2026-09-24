using System.Windows;
using EsmatPlastic.Desktop.Models.ProductVariants;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.ProductVariants;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.ProductVariants;

public partial class EditProductVariantWindow : Window
{
    private readonly ProductVariantService _variantService;
    private readonly ProductVariantResponse _variant;
    private readonly LocalizationService _loc;

    public ProductVariantResponse? UpdatedVariant { get; private set; }

    public EditProductVariantWindow(
        ProductVariantService variantService,
        ProductVariantResponse variant)
    {
        InitializeComponent();

        _variantService = variantService;
        _variant = variant;
        _loc = App.ServiceProvider.GetRequiredService<LocalizationService>();

        NameInput.Text = variant.Name;
        SizeInput.Text = variant.Size ?? string.Empty;
        ColorInput.Text = variant.Color ?? string.Empty;
        CapTypeInput.Text = variant.CapType ?? string.Empty;
        MaterialInput.Text = variant.Material ?? string.Empty;
        ActiveInput.IsChecked = variant.IsActive;

        ApplyLocalization();

        Loaded += (_, _) => NameInput.Focus();
    }

    private void ApplyLocalization()
    {
        Title = _loc["تعديل الصنف"];
        HeaderTitle.Text = _loc["تعديل الصنف"];
        HeaderSubtitle.Text = _loc["تعديل بيانات العبوة أو تعديل حالة التفعيل."];
        NameLabel.Text = _loc["اسم الصنف"];
        SizeLabel.Text = _loc["المقاس"];
        ColorLabel.Text = _loc["اللون"];
        CapTypeLabel.Text = _loc["نوع الغطاء"];
        MaterialLabel.Text = _loc["المادة"];
        ActiveInput.Content = _loc["نشط حالياً"];
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
            var request = new UpdateProductVariantRequest
            {
                Name = NameInput.Text.Trim(),
                Size = EmptyToNull(SizeInput.Text),
                Color = EmptyToNull(ColorInput.Text),
                CapType = EmptyToNull(CapTypeInput.Text),
                Material = EmptyToNull(MaterialInput.Text),
                ImagePath = _variant.ImagePath,
                IsActive = ActiveInput.IsChecked == true
            };

            UpdatedVariant = await _variantService.UpdateAsync(_variant.Id, request);

            if (UpdatedVariant is null)
            {
                MessageBox.Show(
                    _loc["تعذر تحديث الصنف."],
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
                _loc["خطأ في تحديث الصنف"],
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

