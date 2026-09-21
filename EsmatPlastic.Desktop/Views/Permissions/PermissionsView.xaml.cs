using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Models.Permissions;
using EsmatPlastic.Desktop.Services.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.Permissions;

public partial class PermissionsView : UserControl
{
    private readonly PermissionService _service;

    public PermissionsView()
    {
        InitializeComponent();

        _service =
            App.ServiceProvider
                .GetRequiredService<PermissionService>();

        Loaded += PermissionsView_Loaded;
    }

    private async void PermissionsView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        Loaded -= PermissionsView_Loaded;

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var permissions =
                await _service.GetAllAsync();

            PermissionsList.ItemsSource =
                permissions;

            StatusText.Text =
                $"تم تحميل الصلاحيات: {permissions.Count}";
        }
        catch (Exception ex)
        {
            StatusText.Text =
                $"تعذر تحميل الصلاحيات: {ex.Message}";
        }
    }
}
