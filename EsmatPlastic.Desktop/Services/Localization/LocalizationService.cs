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
        ["SecretaryRole"] = "سكرتير",
        ["OrderRequests"] = "طلبات الحجز",
        ["StatusPending"] = "معلق",
        ["StatusApproved"] = "مقبول",
        ["StatusProcessing"] = "قيد المعالجة",
        ["StatusCompleted"] = "مكتمل",
        ["StatusCancelled"] = "ملغى",
        ["StatusRejected"] = "مرفوض",
        ["All"] = "الكل",
        ["CustomerName"] = "اسم الزبون",
        ["CustomerPhone"] = "رقم الهاتف",
        ["Quantity"] = "الكمية",
        ["Variant"] = "الصنف",
        ["Actions"] = "الإجراءات",
        ["Add"] = "إضافة",
        ["Remove"] = "إزالة",
        ["Submit"] = "إرسال",
        ["Loading"] = "جاري التحميل...",
        ["Login"] = "تسجيل الدخول",
        ["Username"] = "اسم المستخدم",
        ["Password"] = "كلمة المرور",
        ["Language"] = "اللغة",
        ["Arabic"] = "العربية",
        ["English"] = "الإنجليزية"
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
        ["SecretaryRole"] = "Secretary",
        ["OrderRequests"] = "Order Requests",
        ["StatusPending"] = "Pending",
        ["StatusApproved"] = "Approved",
        ["StatusProcessing"] = "Processing",
        ["StatusCompleted"] = "Completed",
        ["StatusCancelled"] = "Cancelled",
        ["StatusRejected"] = "Rejected",
        ["All"] = "All",
        ["CustomerName"] = "Customer Name",
        ["CustomerPhone"] = "Customer Phone",
        ["Quantity"] = "Quantity",
        ["Variant"] = "Variant",
        ["Actions"] = "Actions",
        ["Add"] = "Add",
        ["Remove"] = "Remove",
        ["Submit"] = "Submit",
        ["Loading"] = "Loading...",
        ["Login"] = "Login",
        ["Username"] = "Username",
        ["Password"] = "Password",
        ["Language"] = "Language",
        ["Arabic"] = "Arabic",
        ["English"] = "English"
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
        if (element == null) return;

        if (element is TextBlock textBlock && !BindingOperations.IsDataBound(textBlock, TextBlock.TextProperty))
        {
            if (!string.IsNullOrEmpty(textBlock.Text))
            {
                textBlock.Text = this[textBlock.Text];
            }
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

        try
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childrenCount; i++)
            {
                TranslateElement(VisualTreeHelper.GetChild(element, i));
            }
        }
        catch (Exception)
        {
            // Skip elements that cannot be traversed
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
