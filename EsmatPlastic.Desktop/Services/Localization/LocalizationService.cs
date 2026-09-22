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

    private readonly Dictionary<string, string> _extraArToEn = new(StringComparer.OrdinalIgnoreCase)
    {
        // App Core & Navigation
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
        ["إدارة المنتجات والأصناف والبطاقات"] = "Manage products, variants, and cards",
        ["إدارة المخزون وحركات المنتجات"] = "Track stock balances and movements",
        ["إدارة صلاحيات النظام"] = "Review system permissions",
        ["تقارير حركة المخزون"] = "Stock movement reports",
        ["تقارير شاملة لحركة ورصيد المخزون"] = "Comprehensive stock movement and balance reports",

        // Actions & Buttons
        ["إضافة منتج"] = "Add Product",
        ["+  إضافة منتج"] = "+  Add Product",
        ["+ إضافة منتج"] = "+ Add Product",
        ["إضافة منتج جديد"] = "Add New Product",
        ["تعديل المنتج"] = "Edit Product",
        ["حذف المنتج"] = "Delete Product",
        ["إضافة صنف"] = "Add Variant",
        ["+  إضافة صنف"] = "+  Add Variant",
        ["+ إضافة صنف"] = "+ Add Variant",
        ["إضافة صنف / عبوة"] = "Add Variant / Package",
        ["إضافة صنف / عبوة جديدة"] = "Add New Variant / Package",
        ["تعديل الصنف"] = "Edit Variant",
        ["عرض الأصناف"] = "View Variants",
        ["بيانات المستخدم"] = "User Details",
        ["إضافة مستخدم"] = "Add User",
        ["+  إضافة مستخدم"] = "+  Add User",
        ["+ إضافة مستخدم"] = "+ Add User",
        ["تعديل المستخدم"] = "Edit User",
        ["تغيير كلمة المرور"] = "Change Password",
        ["حركة مخزون"] = "Stock Transaction",
        ["إضافة حركة مخزون"] = "Add Stock Transaction",
        ["سجل حركات المخزون"] = "Stock Transaction History",
        ["سجل الحركات"] = "History",
        ["+ وارد"] = "+ Incoming",
        ["- صادر"] = "- Outgoing",
        ["📥  تصدير CSV"] = "📥  Export CSV",
        ["🖨️  طباعة التقارير"] = "🖨️  Print Reports",
        ["⟳  تحديث"] = "⟳  Refresh",
        ["فحص الاتصال"] = "Test Connection",
        ["تسجيل الدخول"] = "Sign in",
        ["تسجيل الخروج"] = "Log out",
        ["حفظ الإعدادات"] = "Save Settings",
        ["حفظ التعديلات"] = "Save Changes",
        ["حفظ الحركة"] = "Save Transaction",
        ["إلغاء"] = "Cancel",
        ["حفظ"] = "Save",
        ["تعديل"] = "Edit",
        ["حذف"] = "Delete",
        ["بحث"] = "Search",
        ["تحديث"] = "Refresh",
        ["الصلاحيات"] = "Permissions",

        // User Accounts & Roles
        ["مدير النظام"] = "System Administrator",
        ["أمين المستودع"] = "Warehouse Keeper",
        ["موظف المخزن - اختبار"] = "Warehouse Employee - Test",
        ["المحاسب - اختبار"] = "Accountant - Test",
        ["محاسب"] = "Accountant",
        ["مدير"] = "Administrator",

        // Product Names
        ["جراكن بلاستيك"] = "Plastic Jerrycans",
        ["برطمانات بلاستيك"] = "Plastic Jars",
        ["زجاجات عصير بلاستيك"] = "Plastic Juice Bottles",
        ["برطمانات توابل"] = "Spice Jars",
        ["منتجات مخصصة"] = "Custom Products",

        // Product Descriptions
        ["جراكن بلاستيك متعددة الأحجام"] = "Multi-size plastic jerrycans",
        ["برطمانات شفافة وغير شفافة"] = "Clear and opaque jars",
        ["زجاجات عصير شفافة بأحجام مختلفة"] = "Clear juice bottles in various sizes",
        ["عبوات توابل بأحجام مختلفة"] = "Spice containers in various sizes",
        ["منتجات بلاستيك حسب طلب العميل"] = "Custom plastic products on demand",

        // Variant Names
        ["جركن 1 لتر أبيض"] = "1L White Jerrycan",
        ["جركن 5 لتر أبيض"] = "5L White Jerrycan",
        ["جركن 10 لتر أزرق"] = "10L Blue Jerrycan",
        ["برطمان 250 مل شفاف"] = "250ml Clear Jar",
        ["برطمان 500 مل شفاف"] = "500ml Clear Jar",
        ["زجاجة عصير 250 مل"] = "250ml Juice Bottle",
        ["زجاجة عصير 500 مل"] = "500ml Juice Bottle",
        ["زجاجة عصير 1 لتر"] = "1L Juice Bottle",
        ["برطمان توابل 100 مل"] = "100ml Spice Jar",
        ["برطمان توابل 200 مل"] = "200ml Spice Jar",
        ["عبوة مخصصة 750 مل"] = "750ml Custom Container",

        // Specs & Properties
        ["1 لتر"] = "1 Liter",
        ["5 لتر"] = "5 Liters",
        ["10 لتر"] = "10 Liters",
        ["250 مل"] = "250 ml",
        ["500 مل"] = "500 ml",
        ["100 مل"] = "100 ml",
        ["200 مل"] = "200 ml",
        ["750 مل"] = "750 ml",
        ["أبيض"] = "White",
        ["أزرق"] = "Blue",
        ["شفاف"] = "Clear",
        ["حسب الطلب"] = "On Demand",
        ["غطاء عادي"] = "Standard Cap",
        ["غطاء أمان"] = "Safety Cap",
        ["غطاء لولبي"] = "Screw Cap",
        ["HDPE"] = "HDPE",
        ["PET"] = "PET",

        // Notes & Transactions
        ["رصيد افتتاحي - اختبار"] = "Opening Balance - Test",
        ["صرف للعميل - اختبار"] = "Customer Dispatch - Test",
        ["إنتاج جديد - اختبار"] = "New Production - Test",
        ["توريد عميل - اختبار"] = "Customer Delivery - Test",
        ["طلب عميل - اختبار"] = "Customer Order - Test",
        ["طلب تصنيع مخصص - اختبار"] = "Custom Manufacturing Order - Test",
        ["وارد"] = "Incoming",
        ["صادر"] = "Outgoing",

        // Labels & Headers
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
        ["تم الحفظ"] = "Saved",
        ["لا توجد بيانات لعرضها حالياً."] = "There is no data to display.",
        ["الدور"] = "Role",
        ["تذكر اللغة عند تشغيل التطبيق"] = "Remember language when the application starts",
        ["تذكر اللغة المحددة عند فتح التطبيق دائماً"] = "Always remember selected language on application startup",
        ["لا توجد صلاحيات مرتبطة بهذا المستخدم"] = "This user has no assigned permissions",
        ["جاري التحديث..."] = "Updating...",
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
        ["المنتج والصنف"] = "Product & Variant",
        ["الوارد الإجمالي"] = "Total Incoming",
        ["الصادر الإجمالي"] = "Total Outgoing",
        ["المخزون المتاح"] = "Available Stock",
        ["⚙️  الإعدادات العامة"] = "⚙️  General Settings",
        ["تفضيلات لغة التطبيق وتكوينات اتصال السيرفر"] = "Language preferences and server connection settings",
        ["🌐  لغة الواجهة والتفضيلات"] = "🌐  Language & Preferences",
        ["🔌  اتصال خادم واجهة البيانات (API)"] = "🔌  API Server Connection",
        ["عنوان السيرفر (Server API Base URL)"] = "Server API Base URL",

        // Permission System Names
        ["Users.View"] = "View Users",
        ["Users.Create"] = "Create Users",
        ["Users.Edit"] = "Edit Users",
        ["Users.Delete"] = "Delete Users",
        ["Permissions.Manage"] = "Manage Permissions",
        ["Products.View"] = "View Products",
        ["Products.Create"] = "Create Products",
        ["Products.Edit"] = "Edit Products",
        ["Products.Delete"] = "Delete Products",
        ["Stock.View"] = "View Stock",
        ["Stock.In"] = "Stock In",
        ["Stock.Out"] = "Stock Out",
        ["Reports.View"] = "View Reports"
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
            return dictionary.TryGetValue(key, out var value) ? value : Translate(key);
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

        if (element is TextBlock textBlock && textBlock.Tag as string != "NoTranslate")
        {
            textBlock.Text = Translate(textBlock.Text);
        }
        else if (element is Button button)
        {
            if (button.Content is string buttonContent)
            {
                button.Content = Translate(buttonContent);
            }
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

    public string Translate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var trimmed = value.Trim();

        if (IsArabic)
        {
            // If target is Arabic, return Arabic translation if input is English
            var revMatch = _extraArToEn.FirstOrDefault(p => p.Value.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(revMatch.Key))
                return revMatch.Key;

            if (_en.ContainsValue(trimmed))
            {
                var key = _en.FirstOrDefault(pair => pair.Value.Equals(trimmed, StringComparison.OrdinalIgnoreCase)).Key;
                if (!string.IsNullOrEmpty(key) && _ar.TryGetValue(key, out var arVal))
                    return arVal;
            }

            return value;
        }
        else
        {
            // Target is English, return English translation if input is Arabic or key
            if (_extraArToEn.TryGetValue(trimmed, out var enMatch))
                return enMatch;

            if (_ar.ContainsValue(trimmed))
            {
                var key = _ar.FirstOrDefault(pair => pair.Value.Equals(trimmed, StringComparison.OrdinalIgnoreCase)).Key;
                if (!string.IsNullOrEmpty(key) && _en.TryGetValue(key, out var enVal))
                    return enVal;
            }

            if (_ar.TryGetValue(trimmed, out var dictVal) && _en.TryGetValue(trimmed, out var enValDirect))
                return enValDirect;

            return value;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
