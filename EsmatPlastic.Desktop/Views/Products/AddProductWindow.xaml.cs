using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using EsmatPlastic.Desktop.Models.Products;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.Products;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace EsmatPlastic.Desktop.Views.Products;

public partial class AddProductWindow : Window
{
    private readonly ProductService _productService;
    private readonly LocalizationService _loc;
    private string? _selectedImagePath;

    public ProductResponse? CreatedProduct { get; private set; }

    public AddProductWindow(ProductService productService)
    {
        InitializeComponent();

        _productService = productService;
        _loc = App.ServiceProvider.GetRequiredService<LocalizationService>();

        ApplyLocalization();
        Loaded += (_, _) => NameInput.Focus();
    }

    private void ApplyLocalization()
    {
        Title = _loc["إضافة منتج"];
        ModalTitle.Text = _loc["إضافة منتج جديد"];
        ModalSubtitle.Text = _loc["أدخل معلومات المنتج الأساسية والصورة للحفظ في النظام."];

        PhotoLabel.Text = _loc["صورة المنتج"];
        SelectPhotoButton.Content = _loc["🖼️  اختيار صورة للمنتج"];
        PhotoPathText.Text = _loc["لم يتم اختيار صورة بعد"];

        NameLabel.Text = _loc["اسم المنتج"];
        DescLabel.Text = _loc["الوصف"];
        CancelButton.Content = _loc["إلغاء"];
        SaveButton.Content = _loc["حفظ"];
    }

    private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = _loc["اختيار صورة للمنتج"],
            Filter = "Image Files (*.png;*.jpg;*.jpeg;*.webp)|*.png;*.jpg;*.jpeg;*.webp|All Files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            _selectedImagePath = dialog.FileName;
            PhotoPathText.Text = Path.GetFileName(_selectedImagePath);

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_selectedImagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                PhotoPreview.Source = bitmap;
                PhotoPreview.Visibility = Visibility.Visible;
                ImagePlaceholder.Visibility = Visibility.Collapsed;
            }
            catch
            {
                // Fallback if image fails to decode
            }
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameInput.Text))
        {
            MessageBox.Show(
                _loc["يرجى إدخال اسم المنتج."],
                _loc["تنبيه"],
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            NameInput.Focus();
            return;
        }

        SaveButton.IsEnabled = false;

        try
        {
            var request = new CreateProductRequest
            {
                Name = NameInput.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(DescriptionInput.Text)
                    ? null
                    : DescriptionInput.Text.Trim(),
                ImagePath = _selectedImagePath
            };

            CreatedProduct = await _productService.CreateAsync(request);

            if (CreatedProduct is null)
            {
                MessageBox.Show(
                    _loc["تعذر حفظ المنتج."],
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
                _loc["خطأ"],
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
