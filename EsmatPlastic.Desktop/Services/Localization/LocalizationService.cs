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
        ["AppName"] = "عصمت للبلاستيك",
        ["AppTagline"] = "نظام إدارة المخزون",
        ["Dashboard"] = "لوحة التحكم",
        ["Warehouse"] = "المستودع",
        ["Products"] = "المنتجات",
        ["ProductVariants"] = "أصناف المنتج",
        ["Reports"] = "التقارير",
        ["Users"] = "المستخدمون",
        ["Permissions"] = "الصلاحيات",
        ["Settings"] = "الإعدادات",
        ["Logout"] = "تسجيل الخروج",
        ["Login"] = "تسجيل الدخول",
        ["Refresh"] = "تحديث",
        ["Refreshing"] = "جاري التحديث...",
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
        ["AdminRole"] = "مدير النظام",
        ["WarehouseRole"] = "أمين المستودع",
        ["AccountantRole"] = "محاسب",
        ["Welcome"] = "مرحباً",
        ["HaveANiceDay"] = "نتمنى لك يوماً سعيداً",
        ["Language"] = "اللغة",
        ["Arabic"] = "العربية",
        ["English"] = "English"
    };

    private readonly Dictionary<string, string> _en = new()
    {
        ["AppName"] = "Esmat Plastic",
        ["AppTagline"] = "Inventory Management System",
        ["Dashboard"] = "Dashboard",
        ["Warehouse"] = "Warehouse",
        ["Products"] = "Products",
        ["ProductVariants"] = "Product Variants",
        ["Reports"] = "Reports",
        ["Users"] = "Users",
        ["Permissions"] = "Permissions",
        ["Settings"] = "Settings",
        ["Logout"] = "Log out",
        ["Login"] = "Sign in",
        ["Refresh"] = "Refresh",
        ["Refreshing"] = "Updating...",
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
        ["AdminRole"] = "Administrator",
        ["WarehouseRole"] = "Warehouse Keeper",
        ["AccountantRole"] = "Accountant",
        ["Welcome"] = "Welcome",
        ["HaveANiceDay"] = "Have a productive day",
        ["Language"] = "Language",
        ["Arabic"] = "العربية",
        ["English"] = "English"
    };

    private readonly Dictionary<string, string> _extraArToEn = new()
    {
        ["عصمت للبلاستيك"] = "Esmat Plastic",
        ["عصمت بلاستيك"] = "Esmat Plastic",
        ["إسمت بلاستيك"] = "Esmat Plastic",
        ["نظام إدارة المخزون"] = "Inventory Management System",
        ["مرحباً"] = "Welcome",
        ["المستخدم"] = "User",
        ["المستخدمون"] = "Users",
        ["إدارة المستخدمين والصلاحيات"] = "Manage users and permissions",
        ["إدارة حسابات المستخدمين وصلاحياتهم"] = "Manage user accounts and permissions",
        ["نتمنى لك يوماً سعيداً"] = "Have a productive day",
        ["الأصناف"] = "Variants",
        ["أصناف المنتج"] = "Product Variants",
        ["ملخص النظام"] = "System Summary",
        ["نظرة عامة على بيانات النظام"] = "A simple overview of inventory and product activity",
        ["نظرة عامة ومباشرة على بيانات المنتجات والحركات المخزنية"] = "Live overview of products and stock movements",
        ["إدارة المنتجات والأصناف"] = "Manage products and packaging variants",
        ["إدارة المخزون وحركات المنتجات"] = "Track stock balances and movements",
        ["إدارة صلاحيات النظام"] = "Review system permissions",
        ["تقارير حركة المخزون"] = "Stock movement reports",
        ["تقارير شاملة لحركة ورصيد المخزون"] = "Comprehensive stock movement and balance reports",
        ["إضافة منتج"] = "Add Product",
        ["+  إضافة منتج"] = "+  Add Product",
        ["+ إضافة منتج"] = "+ Add Product",
        ["إضافة منتج جديد"] = "Add New Product",
        ["تعديل المنتج"] = "Edit Product",
        ["إضافة صنف"] = "Add Variant",
        ["+  إضافة صنف"] = "+  Add Variant",
        ["+ إضافة صنف"] = "+ Add Variant",
        ["إضافة صنف / عبوة"] = "Add Variant / Package",
        ["إضافة صنف / عبوة جديدة"] = "Add New Variant / Package",
        ["تعديل الصنف"] = "Edit Variant",
        ["بيانات المستخدم"] = "User Details",
        ["إضافة مستخدم"] = "Add User",
        ["+  إضافة مستخدم"] = "+  Add User",
        ["+ إضافة مستخدم"] = "+ Add User",
        ["تغيير كلمة المرور"] = "Change Password",
        ["حركة مخزون"] = "Stock Transaction",
        ["إضافة حركة مخزون"] = "Add Stock Transaction",
        ["سجل حركات المخزون"] = "Stock Transaction History",
        ["العربية"] = "العربية",
        ["لغة التطبيق"] = "Application Language",
        ["عنوان واجهة API"] = "API Base URL",
        ["معلومات التطبيق"] = "Application Information",
        ["اسم المستخدم"] = "Username",
        ["الاسم الكامل"] = "Full Name",
        ["كلمة المرور"] = "Password",
        ["كلمة المرور الجديدة"] = "New Password",
        ["تأكيد كلمة المرور"] = "Confirm Password",
        ["الوصف"] = "Description",
        ["رقم المنتج"] = "Product",
        ["رقم الصنف"] = "Variant",
        ["اسم المنتج"] = "Product Name",
        ["اسم الصنف"] = "Variant Name",
        ["المقاس"] = "Size",
        ["اللون"] = "Color",
        ["نوع الغطاء"] = "Cap Type",
        ["نوع الحركة"] = "Transaction Type",
        ["الغطاء"] = "Cap",
        ["المادة"] = "Material",
        ["الحالة"] = "Status",
        ["نشط حالياً"] = "Currently Active",
        ["غير نشط"] = "Inactive",
        ["نشط"] = "Active",
        ["المتاح"] = "Available",
        ["الوارد"] = "Incoming",
        ["الصادر"] = "Outgoing",
        ["الرصيد الحالي"] = "Current Balance",
        ["إجمالي الوارد"] = "Total Incoming",
        ["إجمالي الصادر"] = "Total Outgoing",
        ["النوع"] = "Type",
        ["الكمية"] = "Quantity",
        ["التاريخ"] = "Date",
        ["تاريخ الإنشاء"] = "Creation Date",
        ["الملاحظات"] = "Notes",
        ["المنتج"] = "Product",
        ["الصنف"] = "Variant",
        ["وارد"] = "Incoming",
        ["صادر"] = "Outgoing",
        ["تم الحفظ"] = "Saved",
        ["حفظ الحركة"] = "Save Transaction",
        ["سجل الحركات"] = "History",
        ["لا توجد بيانات لعرضها حالياً."] = "There is no data to display.",
        ["إلغاء"] = "Cancel",
        ["حفظ"] = "Save",
        ["حفظ الإعدادات"] = "Save Settings",
        ["تعديل"] = "Edit",
        ["حذف"] = "Delete",
        ["بحث"] = "Search",
        ["تحديث"] = "Refresh",
        ["⟳  تحديث"] = "⟳  Refresh",
        ["مدير النظام"] = "Administrator",
        ["أمين المستودع"] = "Warehouse Keeper",
        ["محاسب"] = "Accountant",
        ["تسجيل الدخول"] = "Sign in",
        ["الدور"] = "Role",
        ["تذكر اللغة عند تشغيل التطبيق"] = "Remember language when the application starts",
        ["تذكر اللغة المحددة عند فتح التطبيق دائماً"] = "Always remember selected language on application startup",
        ["لا توجد صلاحيات مرتبطة بهذا المستخدم"] = "This user has no assigned permissions",
        ["يرجى تسجيل الخروج والتواصل مع مدير النظام لمنح الصلاحيات المناسبة."] = "Please log out and contact the administrator to assign the required permissions.",
        ["تعديل المستخدم"] = "Edit User",
        ["جاري التحديث..."] = "Updating...",
        ["تنبيه"] = "Warning",
        ["خطأ"] = "Error",
        ["تأكيد"] = "Confirm",
        ["تأكيد الحذف"] = "Confirm Deletion",
        ["تم"] = "Done",
        ["عدد الأصناف"] = "Variant Count",
        ["الإجراءات"] = "Actions",
        ["الاسم"] = "Name",
        ["عرض وتصفية جميع صلاحيات النظام وأقسامها"] = "View and filter all system permissions and modules",
        ["إجمالي الصلاحيات"] = "Total Permissions",
        ["جميع صلاحيات النظام"] = "All System Permissions",
        ["الصلاحيات النشطة"] = "Active Permissions",
        ["مفعلة ومتاحة للاستخدام"] = "Active and available for use",
        ["أقسام الصلاحيات"] = "Permission Modules",
        ["وحدات وأقسام النظام"] = "System modules and sections",
        ["اسم الصلاحية"] = "Permission Name",
        ["القسم"] = "Category",
        ["الوصف التفصيلي"] = "Detailed Description",
        ["إجمالي المنتجات"] = "Total Products",
        ["الأصناف والمواصفات"] = "Variants & Specifications",
        ["إجمالي القطع المخزنة"] = "Total Stored Items",
        ["الواردات الإجمالية"] = "Total Incoming",
        ["الصادرات الإجمالية"] = "Total Outgoing",
        ["آخر الحركات المخزنية"] = "Recent Stock Activity",
        ["أحدث العمليات التي تم تسجيلها"] = "Latest registered operations",
        ["تنبيهات انخفاض المخزون"] = "Low Stock Alerts",
        ["ملخص وعمليات النظام"] = "System Insights & Operations",
        ["حالة الاتصال بالواجهة:"] = "Server Connection Status:",
        ["متصل بنجاح"] = "Connected successfully",
        ["آخر تحديث:"] = "Last Updated:",
        ["الآن"] = "Just now",
        ["تحديد الكل"] = "Select All",
        ["إلغاء الكل"] = "Clear All",
        ["صلاحيات المستخدم"] = "User Permissions",
        ["📥  تصدير CSV"] = "📥  Export CSV",
        ["🖨️  طباعة التقارير"] = "🖨️  Print Report",
        ["المنتج والصنف"] = "Product & Variant",
        ["الوارد الإجمالي"] = "Total Incoming",
        ["الصادر الإجمالي"] = "Total Outgoing",
        ["المخزون المتاح"] = "Available Stock",
        ["⚙️  الإعدادات العامة"] = "⚙️  General Settings",
        ["تفضيلات لغة التطبيق وتكوينات اتصال السيرفر"] = "Language preferences and server connection settings",
        ["🌐  لغة الواجهة والتفضيلات"] = "🌐  Language & Preferences",
        ["🔌  اتصال خادم واجهة البيانات (API)"] = "🔌  API Server Connection",
        ["عنوان السيرفر (Server API Base URL)"] = "Server API Base URL",
        ["فحص الاتصال"] = "Test Connection"
    };

    public string Language
    {
        get => _language;
        private set
        {
            if (_language == value)
                return;

            _language = value;
            OnPropertyChanged(null);
        }
    }

    public bool IsArabic => Language.Equals("ar", StringComparison.OrdinalIgnoreCase);

    public FlowDirection FlowDirection => IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    public string this[string key]
    {
        get
        {
            var dictionary = IsArabic ? _ar : _en;
            return dictionary.TryGetValue(key, out var value) ? value : key;
        }
    }

    public string T(string value)
    {
        return Translate(value);
    }

    public void SetLanguage(string language)
    {
        Language = language.Equals("en", StringComparison.OrdinalIgnoreCase) ? "en" : "ar";
    }

    public void ApplyTo(DependencyObject root)
    {
        if (root is FrameworkElement element)
        {
            element.FlowDirection = FlowDirection;
        }

        if (root is Window window)
        {
            window.Title = Translate(window.Title);
            window.FontFamily = new FontFamily("Segoe UI, Segoe UI Arabic, Tahoma");
        }

        TranslateElement(root, new HashSet<DependencyObject>());
    }

    private void TranslateElement(DependencyObject element, HashSet<DependencyObject> visited)
    {
        if (!visited.Add(element))
            return;

        if (element is FrameworkElement fe)
        {
            fe.FlowDirection = FlowDirection;
        }

        if (element is TextBlock textBlock &&
            !BindingOperations.IsDataBound(textBlock, TextBlock.TextProperty) &&
            textBlock.Tag as string != "NoTranslate")
        {
            textBlock.Text = Translate(textBlock.Text);
        }
        else if (element is Button button && button.Content is string buttonContent)
        {
            button.Content = Translate(buttonContent);
        }
        else if (element is CheckBox checkBox && checkBox.Content is string checkContent)
        {
            checkBox.Content = Translate(checkContent);
        }
        else if (element is ComboBoxItem comboBoxItem && comboBoxItem.Content is string itemContent)
        {
            comboBoxItem.Content = Translate(itemContent);
        }
        else if (element is ContentControl contentControl &&
                 contentControl.Content is string content &&
                 !(contentControl is Button) &&
                 !(contentControl is CheckBox))
        {
            contentControl.Content = Translate(content);
        }

        if (element is ItemsControl itemsControl && itemsControl.ItemsSource is not null)
        {
            var source = itemsControl.ItemsSource;
            itemsControl.ItemsSource = null;
            itemsControl.ItemsSource = source;
        }

        foreach (var child in LogicalTreeHelper.GetChildren(element).OfType<DependencyObject>())
        {
            TranslateElement(child, visited);
        }

        if (element is not Visual && element is not System.Windows.Media.Media3D.Visual3D)
        {
            return;
        }

        var visualCount = VisualTreeHelper.GetChildrenCount(element);
        for (var index = 0; index < visualCount; index++)
        {
            TranslateElement(VisualTreeHelper.GetChild(element, index), visited);
        }
    }

    private string Translate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        if (IsArabic)
        {
            if (_en.ContainsValue(value))
            {
                var key = _en.FirstOrDefault(pair => pair.Value == value).Key;
                return _ar[key];
            }

            return _extraArToEn.FirstOrDefault(pair => pair.Value == value).Key is { Length: > 0 } arabic
                ? arabic
                : value;
        }

        if (_ar.ContainsValue(value))
        {
            var key = _ar.FirstOrDefault(pair => pair.Value == value).Key;
            return key.Length > 0 ? _en[key] : value;
        }

        return _extraArToEn.TryGetValue(value, out var translation)
            ? translation
            : value;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
