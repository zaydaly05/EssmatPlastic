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
        Title = _loc.T("إضافة منتج");
        ModalTitle.Text = _loc.T("إضافة منتج جديد");
        ModalSubtitle.Text = _loc.T("أدخل معلومات المنتج الأساسية والصورة للحفظ في النظام.");

        PhotoLabel.Text = _loc.T("صورة المنتج");
        SelectPhotoButton.Content = _loc.T("🖼️  اختيار صورة للمنتج");
        PhotoPathText.Text = _loc.T("لم يتم اختيار صورة بعد");

        NameLabel.Text = _loc.T("اسم المنتج");
        DescLabel.Text = _loc.T("الوصف");
        CancelButton.Content = _loc.T("إلغاء");
        SaveButton.Content = _loc.T("حفظ");
    }

    private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = _loc.T("اختيار صورة للمنتج"),
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
                _loc.T("يرجى إدخال اسم المنتج."),
                _loc.T("تنبيه"),
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
                    _loc.T("تعذر حفظ المنتج."),
                    _loc.T("خطأ"),
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
                _loc.T("خطأ"),
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
