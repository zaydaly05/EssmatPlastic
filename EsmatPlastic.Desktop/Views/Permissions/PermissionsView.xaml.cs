using System.Windows;
using System.Windows.Controls;
using EsmatPlastic.Desktop.Models.Permissions;
using EsmatPlastic.Desktop.Services.Localization;
using EsmatPlastic.Desktop.Services.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace EsmatPlastic.Desktop.Views.Permissions;

public partial class PermissionsView : UserControl
{
    private readonly PermissionService _service;
    private readonly LocalizationService _loc;

    private List<PermissionItemDisplay> _allDisplays = new();
    private bool _isInitializing = true;

    public PermissionsView()
    {
        InitializeComponent();

        _service = App.ServiceProvider.GetRequiredService<PermissionService>();
        _loc = App.ServiceProvider.GetRequiredService<LocalizationService>();

        ApplyLocalization();

        Loaded += PermissionsView_Loaded;
    }

    private void ApplyLocalization()
    {
        PageTitleText.Text = _loc["الصلاحيات"];
        PageSubtitleText.Text = _loc["عرض وتصفية جميع صلاحيات النظام وأقسامها"];

        TotalLabel.Text = _loc["إجمالي الصلاحيات"];
        TotalSubtext.Text = _loc["جميع صلاحيات النظام"];

        ActiveLabel.Text = _loc["الصلاحيات النشطة"];
        ActiveSubtext.Text = _loc["مفعلة ومتاحة للاستخدام"];

        CategoriesLabel.Text = _loc["أقسام الصلاحيات"];
        CategoriesSubtext.Text = _loc["وحدات وأقسام النظام"];

        HeaderId.Text = _loc["#"];
        HeaderName.Text = _loc["اسم الصلاحية"];
        HeaderCategory.Text = _loc["القسم"];
        HeaderDescription.Text = _loc["الوصف"];
        HeaderStatus.Text = _loc["الحالة"];

        RefreshButton.Content = _loc["تحديث"];
        EmptyStateText.Text = _loc["لم يتم العثور على صلاحيات مطابقة لنتائج البحث"];
    }

    private async void PermissionsView_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        RefreshButton.IsEnabled = false;
        StatusText.Text = _loc["جاري تحميل الصلاحيات..."];

        try
        {
            var rawPermissions = await _service.GetAllAsync();

            _allDisplays = rawPermissions.Select(p =>
            {
                var category = DeriveCategory(p.Name);
                return new PermissionItemDisplay
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = category,
                    Description = _loc[p.Description ?? p.Name],
                    IsActive = p.IsActive,
                    StatusText = p.IsActive ? _loc["نشط"] : _loc["معطل"],
                    StatusBg = p.IsActive ? "#D1FAE5" : "#F1F5F9",
                    StatusFg = p.IsActive ? "#059669" : "#64748B"
                };
            }).ToList();

            UpdateStatCards(rawPermissions);
            PopulateCategories();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"{_loc["تعذر تحميل الصلاحيات"]}: {ex.Message}";
        }
        finally
        {
            RefreshButton.IsEnabled = true;
        }
    }

    private void UpdateStatCards(List<PermissionResponse> raw)
    {
        TotalCountText.Text = raw.Count.ToString();
        ActiveCountText.Text = raw.Count(p => p.IsActive).ToString();

        var categories = _allDisplays.Select(d => d.Category).Distinct().ToList();
        CategoriesCountText.Text = categories.Count.ToString();
    }

    private void PopulateCategories()
    {
        _isInitializing = true;

        var allOption = _loc["جميع الأقسام"];
        var categories = new List<string> { allOption };
        categories.AddRange(_allDisplays.Select(d => d.Category).Distinct().OrderBy(c => c));

        CategoryFilter.ItemsSource = categories;
        CategoryFilter.SelectedIndex = 0;

        _isInitializing = false;
    }

    private string DeriveCategory(string permissionName)
    {
        if (string.IsNullOrWhiteSpace(permissionName))
            return _loc["عام"];

        string prefix = permissionName.Contains('.')
            ? permissionName.Split('.')[0]
            : permissionName.Split('_')[0];

        return prefix.ToLowerInvariant() switch
        {
            "users" or "user" => _loc["المستخدمون"],
            "products" or "product" => _loc["المنتجات"],
            "stock" or "warehouse" or "inventory" => _loc["المخزون"],
            "reports" or "report" => _loc["التقارير"],
            "settings" or "setting" => _loc["الإعدادات"],
            "permissions" or "permission" => _loc["الصلاحيات"],
            _ => _loc[prefix]
        };
    }

    private void ApplyFilter()
    {
        if (_isInitializing)
            return;

        var query = SearchInput.Text?.Trim().ToLowerInvariant() ?? string.Empty;
        var selectedCategory = CategoryFilter.SelectedItem as string;
        var allOption = _loc["جميع الأقسام"];

        var filtered = _allDisplays.Where(p =>
        {
            bool matchesSearch = string.IsNullOrEmpty(query) ||
                                 p.Name.ToLowerInvariant().Contains(query) ||
                                 p.Description.ToLowerInvariant().Contains(query) ||
                                 p.Category.ToLowerInvariant().Contains(query) ||
                                 p.Id.ToString().Contains(query);

            bool matchesCategory = string.IsNullOrEmpty(selectedCategory) ||
                                   selectedCategory == allOption ||
                                   p.Category.Equals(selectedCategory, StringComparison.OrdinalIgnoreCase);

            return matchesSearch && matchesCategory;
        }).ToList();

        PermissionsList.ItemsSource = filtered;

        EmptyStatePanel.Visibility = filtered.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        StatusText.Text = _loc.IsArabic
            ? $"يعرض {filtered.Count} من أصل {_allDisplays.Count} صلاحية"
            : $"Showing {filtered.Count} of {_allDisplays.Count} permissions";
    }

    private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilter();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadAsync();
    }
}

public class PermissionItemDisplay
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public string StatusBg { get; set; } = string.Empty;
    public string StatusFg { get; set; } = string.Empty;
}
