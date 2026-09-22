using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using EsmatPlastic.Desktop.Models.Products;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.Products;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace EsmatPlastic.Desktop.Views.Products;

public partial class EditProductWindow : Window
{
    private readonly ProductService _productService;
    private readonly ProductResponse _product;
    private readonly LocalizationService _loc;
    private string? _selectedImagePath;

    public ProductResponse? UpdatedProduct { get; private set; }

    public EditProductWindow(ProductService productService, ProductResponse product)
    {
        InitializeComponent();

        _productService = productService;
        _product = product;
        _loc = App.ServiceProvider.GetRequiredService<LocalizationService>();
        _selectedImagePath = product.ImagePath;

        NameInput.Text = product.Name;
        DescriptionInput.Text = product.Description ?? string.Empty;
        ActiveInput.IsChecked = product.IsActive;

        if (!string.IsNullOrWhiteSpace(product.ImagePath))
        {
            PhotoPathText.Text = Path.GetFileName(product.ImagePath);
            TryLoadPreview(product.ImagePath);
        }

        ApplyLocalization();
        Loaded += (_, _) => NameInput.Focus();
    }

    private void ApplyLocalization()
    {
        Title = _loc.T("تعديل المنتج");
        ModalTitle.Text = _loc.T("تعديل بيانات المنتج");
        ModalSubtitle.Text = _loc.T("قم بتحديث معلومات المنتج أو صورته أو حالته.");

        PhotoLabel.Text = _loc.T("صورة المنتج");
        SelectPhotoButton.Content = _loc.T("🖼️  تغيير صورة المنتج");
        if (string.IsNullOrWhiteSpace(_selectedImagePath))
        {
            PhotoPathText.Text = _loc.T("لم يتم اختيار صورة بعد");
        }

        NameLabel.Text = _loc.T("اسم المنتج");
        DescLabel.Text = _loc.T("الوصف");
        ActiveInput.Content = _loc.T("نشط حالياً");

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
            TryLoadPreview(_selectedImagePath);
        }
    }

    private void TryLoadPreview(string path)
    {
        try
        {
            if (File.Exists(path) || Uri.IsWellFormedUriString(path, UriKind.Absolute))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                PhotoPreview.Source = bitmap;
                PhotoPreview.Visibility = Visibility.Visible;
                ImagePlaceholder.Visibility = Visibility.Collapsed;
            }
        }
        catch
        {
            // Ignore preview errors
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
            var request = new UpdateProductRequest
            {
                Name = NameInput.Text.Trim(),
                Description = string.IsNullOrWhiteSpace(DescriptionInput.Text)
                    ? null
                    : DescriptionInput.Text.Trim(),
                ImagePath = _selectedImagePath,
                IsActive = ActiveInput.IsChecked == true
            };

            UpdatedProduct = await _productService.UpdateAsync(_product.Id, request);

            if (UpdatedProduct is null)
            {
                MessageBox.Show(
                    _loc.T("تعذر تحديث المنتج."),
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
                _loc.T("خطأ في تحديث المنتج"),
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
