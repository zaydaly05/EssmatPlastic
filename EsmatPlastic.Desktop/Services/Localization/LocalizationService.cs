using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace EsmatPlastic.Desktop.Services.Localization;

public class LocalizationService : INotifyPropertyChanged
{
    private string _language = "ar";

    private readonly Dictionary<string, string> _ar = new()
    {
        ["AppName"] = "إسمت بلاستيك",
        ["Dashboard"] = "لوحة التحكم",
        ["Warehouse"] = "المستودع",
        ["Products"] = "المنتجات",
        ["Reports"] = "التقارير",
        ["Users"] = "المستخدمون",
        ["Permissions"] = "الصلاحيات",
        ["Settings"] = "الإعدادات",
        ["Logout"] = "تسجيل الخروج",
        ["Refresh"] = "تحديث",
        ["Save"] = "حفظ",
        ["Cancel"] = "إلغاء",
        ["Edit"] = "تعديل",
        ["Delete"] = "حذف",
        ["Search"] = "بحث",
        ["Incoming"] = "وارد",
        ["Outgoing"] = "صادر",
        ["Stock"] = "المخزون",
        ["CurrentStock"] = "المخزون الحالي",
        ["TotalIn"] = "إجمالي الوارد",
        ["TotalOut"] = "إجمالي الصادر",
        ["Active"] = "نشط",
        ["Inactive"] = "غير نشط",
        ["AppTagline"] = "نظام إدارة المخزون",
        ["Welcome"] = "مرحباً",
        ["HaveANiceDay"] = "نتمنى لك يوماً سعيداً",
        ["AdminRole"] = "مدير النظام",
        ["WarehouseRole"] = "أمين المستودع",
        ["AccountantRole"] = "محاسب",
        ["OrderRequests"] = "طلبات الحجز"
    };

    private readonly Dictionary<string, string> _en = new()
    {
        ["AppName"] = "Esmat Plastic",
        ["Dashboard"] = "Dashboard",
        ["Warehouse"] = "Warehouse",
        ["Products"] = "Products",
        ["Reports"] = "Reports",
        ["Users"] = "Users",
        ["Permissions"] = "Permissions",
        ["Settings"] = "Settings",
        ["Logout"] = "Logout",
        ["Refresh"] = "Refresh",
        ["Save"] = "Save",
        ["Cancel"] = "Cancel",
        ["Edit"] = "Edit",
        ["Delete"] = "Delete",
        ["Search"] = "Search",
        ["Incoming"] = "Incoming",
        ["Outgoing"] = "Outgoing",
        ["Stock"] = "Stock",
        ["CurrentStock"] = "Current Stock",
        ["TotalIn"] = "Total In",
        ["TotalOut"] = "Total Out",
        ["Active"] = "Active",
        ["Inactive"] = "Inactive",
        ["AppTagline"] = "Inventory Management System",
        ["Welcome"] = "Welcome",
        ["HaveANiceDay"] = "Have a nice day",
        ["AdminRole"] = "Administrator",
        ["WarehouseRole"] = "Warehouse Keeper",
        ["AccountantRole"] = "Accountant",
        ["OrderRequests"] = "Order Requests"
    };

    public string Language
    {
        get => _language;
        private set
        {
            if (_language == value) return;
            _language = value;
            OnPropertyChanged(string.Empty); // Notify all properties changed
        }
    }

    public bool IsArabic => Language.Equals("ar", StringComparison.OrdinalIgnoreCase);

    public string this[string key]
    {
        get
        {
            var dictionary = IsArabic ? _ar : _en;
            return dictionary.TryGetValue(key, out var value) ? value : key;
        }
    }

    public void SetLanguage(string language)
    {
        Language = language.Equals("en", StringComparison.OrdinalIgnoreCase) ? "en" : "ar";
    }

    public void ApplyTo(Window window)
    {
        window.Title = this[window.Title];
        window.FlowDirection = IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        TranslateElement(window);
    }

    private void TranslateElement(DependencyObject element)
    {
        if (element is TextBlock textBlock && !BindingOperations.IsDataBound(textBlock, TextBlock.TextProperty))
        {
            textBlock.Text = this[textBlock.Text];
        }

        if (element is Button button && button.Content is string content)
        {
            button.Content = this[content];
        }

        if (element is ComboBoxItem comboBoxItem && comboBoxItem.Content is string itemContent)
        {
            comboBoxItem.Content = this[itemContent];
        }

        if (element is Window win)
        {
            win.FlowDirection = IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        int childrenCount = VisualTreeHelper.GetChildrenCount(element);
        for (int i = 0; i < childrenCount; i++)
        {
            TranslateElement(VisualTreeHelper.GetChild(element, i));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
