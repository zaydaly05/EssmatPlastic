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

    private readonly Dictionary<string, string> _ar =
        new()
        {
            ["AppName"] = "إسمت بلاستيك",
            ["AppTagline"] = "نظام إدارة المخزون",
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
            ["Welcome"] = "مرحباً",
            ["SettingsSaved"] = "تم حفظ الإعدادات وتطبيق اللغة.",
            ["Done"] = "تم",
            ["HaveANiceDay"] = "نتمنى لك يوماً سعيداً",
            ["AdminRole"] = "مدير النظام",
            ["WarehouseRole"] = "أمين المستودع",
            ["AccountantRole"] = "محاسب",
            ["Updating"] = "جاري التحديث...",
            ["RefreshBtn"] = "⟳  تحديث",
            ["ExportCsv"] = "تصدير CSV",
            ["PrintReport"] = "طباعة التقرير",
            ["Close"] = "إغلاق",
            ["Disabled"] = "معطل"
        };

    private readonly Dictionary<string, string> _en =
        new()
        {
            ["AppName"] = "Esmat Plastic",
            ["AppTagline"] = "Inventory Management System",
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
            ["Welcome"] = "Welcome",
            ["SettingsSaved"] = "Settings saved and language applied.",
            ["Done"] = "Done",
            ["HaveANiceDay"] = "Have a nice day",
            ["AdminRole"] = "Administrator",
            ["WarehouseRole"] = "Warehouse Keeper",
            ["AccountantRole"] = "Accountant",
            ["Updating"] = "Updating...",
            ["RefreshBtn"] = "⟳  Refresh",
            ["ExportCsv"] = "Export CSV",
            ["PrintReport"] = "Print Report",
            ["Close"] = "Close",
            ["Disabled"] = "Disabled"
        };

    private readonly Dictionary<string, string> _extraArToEn =
        new()
        {
            // App & Navigation
            ["إسمت بلاستيك"] = "Esmat Plastic",
            ["نظام إدارة المخزون"] = "Inventory Management System",
            ["مرحباً"] = "Welcome",
            ["المستخدم"] = "User",
            ["نتمنى لك يوماً سعيداً"] = "Have a nice day",
            ["الأصناف"] = "Variants",
            ["لوحة تحكم بسيطة لإدارة مخزون البلاستيك."] = "A simple dashboard for managing plastic inventory.",
            ["اختر اللغة ثم سجّل الدخول للمتابعة."] = "Choose a language, then sign in to continue.",
            ["عصمت للبلاستيك"] = "Esmat Plastic",
            ["إسمت بلاستيك لسطح المكتب"] = "Esmat Plastic Desktop",
            ["نظرة مباشرة على المنتجات وحركات المخزون"] = "Live overview of products and stock movements",
            ["إجمالي المنتجات"] = "Total products",
            ["الأصناف والمواصفات"] = "Variants & specifications",
            ["إجمالي العناصر المخزنة"] = "Total stored items",
            ["يرجى التواصل مع مدير النظام للحصول على الصلاحيات المطلوبة."] = "Please contact your administrator to get the required access.",
            ["أحدث العمليات المسجلة"] = "Latest registered operations",
            ["تتم مزامنة بيانات المنتجات والمخزون مع واجهة API للنظام."] = "Product and inventory data is synchronized with the system API.",
            ["تقارير شاملة لحركة المخزون وأرصدته"] = "Comprehensive stock movement and balance reports",
            ["المنتج والصنف"] = "Product & Variant",
            ["دخول"] = "In",
            ["خروج"] = "Out",
            ["▣  تصدير CSV"] = "▣  Export CSV",
            ["▧  طباعة التقرير"] = "▧  Print Report",
            ["⟳  تحديث"] = "⟳  Refresh",

            // Dashboard
            ["ملخص النظام"] = "System Summary",
            ["نظرة عامة على بيانات النظام"] = "Overview of system data",
            ["ستظهر الإحصاءات هنا بعد تحميل البيانات."] = "Statistics will appear here after the data loads.",
            ["يتم تحميل بيانات المنتجات والمخزون من واجهة API الخاصة بالنظام."] = "Product and inventory data is loaded from the system API.",
            ["يمكنك مراجعة الكميات والحركات من خلال الأقسام المتاحة."] = "Review quantities and movements through the available sections.",
            ["لا توجد صلاحيات مرتبطة بهذا المستخدم"] = "No permissions are assigned to this user",
            ["يرجى تسجيل الخروج والتواصل مع مدير النظام لمنح الصلاحيات المناسبة."] = "Please log out and contact the administrator to assign the required permissions.",

            // Products
            ["إدارة المنتجات والأصناف"] = "Manage products and variants",
            ["إدارة المنتجات والأصناف والبطاقات"] = "Manage products, variants and cards",
            ["إضافة منتج"] = "Add Product",
            ["إضافة منتج جديد"] = "Add New Product",
            ["+  إضافة منتج"] = "+  Add Product",
            ["تعديل المنتج"] = "Edit Product",
            ["تعديل بيانات المنتج"] = "Edit Product Data",
            ["قم بتحديث معلومات المنتج أو صورته أو حالته."] = "Update product info, image, or status.",
            ["أدخل معلومات المنتج الأساسية والصورة للحفظ في النظام."] = "Enter basic product info and image to save in the system.",
            ["حذف المنتج"] = "Delete Product",
            ["عرض الأصناف"] = "View Variants",
            ["الصورة"] = "Photo",
            ["المنتج والوصف"] = "Product & Description",
            ["عدد الأصناف"] = "Variant Count",
            ["الإجراءات"] = "Actions",
            ["صورة المنتج"] = "Product Photo",
            ["🖼️  اختيار صورة للمنتج"] = "🖼️  Select Product Image",
            ["اختيار صورة للمنتج"] = "Select Product Image",
            ["لم يتم اختيار صورة بعد"] = "No image selected yet",
            ["🖼️  تغيير صورة المنتج"] = "🖼️  Change Product Image",
            ["صورة المنتجات الحالية"] = "Current product image",
            ["انقر لتعديل/رفع صورة للمنتج"] = "Click to edit/upload product image",

            // Product Variants
            ["أصناف المنتج"] = "Product Variants",
            ["إضافة صنف"] = "Add Variant",
            ["+  إضافة صنف"] = "+  Add Variant",
            ["إضافة صنف / عبوة"] = "Add Variant / Package",
            ["إضافة صنف / عبوة جديدة"] = "Add New Variant / Package",
            ["أدخل تفاصيل ومواصفات العبوة أو الصنف للمنتج."] = "Enter variant/package details and specifications.",
            ["تعديل الصنف"] = "Edit Variant",
            ["تعديل بيانات العبوة أو تعديل حالة التفعيل."] = "Edit package data or change activation status.",

            // Users
            ["بيانات المستخدم"] = "User Details",
            ["إضافة مستخدم"] = "Add User",
            ["+  إضافة مستخدم"] = "+  Add User",
            ["تعديل المستخدم"] = "Edit User",
            ["إدارة حسابات المستخدمين والدور والصلاحيات"] = "Manage user accounts, roles, and permissions",
            ["أدخل بيانات حساب المستخدم والدور والصلاحيات."] = "Enter user account data, role, and permissions.",
            ["إجمالي المستخدمين"] = "Total Users",
            ["جميع الحسابات المنسجلة"] = "All registered accounts",
            ["الحسابات النشطة"] = "Active Accounts",
            ["حسابات مفعلة ومتاحة"] = "Active and available accounts",
            ["مديرو النظام"] = "System Administrators",
            ["صلاحيات كاملة بالنظام"] = "Full system access",
            ["صلاحيات المستخدم"] = "User Permissions",
            ["بحث باسم المستخدم أو الاسم الكامل أو الدور..."] = "Search by username, full name, or role...",
            ["تاريخ الإنشاء"] = "Created Date",
            ["هل تريد حذف المستخدم {0}؟"] = "Delete user {0}?",
            ["تعذر حذف المستخدم"] = "Failed to delete user",

            // User Permissions Window
            ["إدارة صلاحيات المستخدم"] = "Manage User Permissions",
            ["حدد الصلاحيات الممنوحة لهذا المستخدم في النظام."] = "Select the permissions granted to this user in the system.",
            ["قائمة الصلاحيات"] = "Permissions List",
            ["تحديد الكل"] = "Select All",
            ["إلغاء التحديد"] = "Clear All",
            ["حفظ الصلاحيات"] = "Save Permissions",
            ["تعذر حفظ صلاحيات المستخدم."] = "Failed to save user permissions.",

            // Change Password
            ["تغيير كلمة المرور"] = "Change Password",
            ["أدخل كلمة المرور الجديدة للمستخدم."] = "Enter the new password for the user.",
            ["كلمة المرور الجديدة"] = "New Password",
            ["تأكيد كلمة المرور"] = "Confirm Password",

            // Permissions View
            ["عرض وتصفية جميع صلاحيات النظام وأقسامها"] = "View and filter all system permissions and categories",
            ["إجمالي الصلاحيات"] = "Total Permissions",
            ["جميع صلاحيات النظام"] = "All system permissions",
            ["الصلاحيات النشطة"] = "Active Permissions",
            ["مفعلة ومتاحة للاستخدام"] = "Active and available for use",
            ["أقسام الصلاحيات"] = "Permission Categories",
            ["وحدات وأقسام النظام"] = "System modules and categories",
            ["اسم الصلاحية"] = "Permission Name",
            ["القسم"] = "Category",
            ["جميع الأقسام"] = "All Categories",
            ["بحث باسم الصلاحية أو الوصف..."] = "Search by permission name or description...",
            ["لم يتم العثور على صلاحيات مطابقة لنتائج البحث"] = "No permissions found matching the search criteria",
            ["جاري تحميل الصلاحيات..."] = "Loading permissions...",
            ["تعذر تحميل الصلاحيات"] = "Failed to load permissions",
            ["نشط"] = "Active",
            ["معطل"] = "Disabled",
            ["عام"] = "General",

            // Warehouse
            ["إدارة المخزون وحركات المنتجات"] = "Manage stock and product transactions",
            ["أرصدة المنتجات في المستودع"] = "Product inventory balances",
            ["+ وارد"] = "+ Incoming",
            ["- صادر"] = "- Outgoing",
            ["سجل الحركات"] = "Transaction History",

            // Stock Transaction
            ["حركة مخزون"] = "Stock Transaction",
            ["إضافة حركة مخزون"] = "Add Stock Transaction",
            ["سجّل حركة دخول (وارد) أو خروج (صادر) من المخزن."] = "Record an incoming or outgoing stock transaction.",
            ["نوع الحركة"] = "Transaction Type",
            ["حفظ الحركة"] = "Save Transaction",
            ["تعذر حفظ حركة المخزون."] = "Failed to save stock transaction.",
            ["تمت إضافة الحركة الواردة بنجاح."] = "Incoming transaction added successfully.",
            ["تمت إضافة الحركة الصادرة بنجاح."] = "Outgoing transaction added successfully.",

            // Stock History
            ["سجل حركات المخزون"] = "Stock Transaction History",
            ["تعذر تحميل سجل الحركات"] = "Failed to load transaction history",

            // Reports
            ["إدارة صلاحيات النظام"] = "Manage system permissions",
            ["تقارير حركة المخزون"] = "Stock movement reports",
            ["تقرير حركة ورصيد المخزون"] = "Stock Movement & Balance Report",
            ["تقرير حركة المخزون"] = "Stock Movement Report",
            ["المنتج"] = "Product",
            ["الصنف"] = "Variant",
            ["المقاس"] = "Size",
            ["اللون"] = "Color",
            ["نوع الغطاء"] = "Cap Type",
            ["الغطاء"] = "Cap",
            ["المادة"] = "Material",
            ["الحالة"] = "Status",
            ["نشط حالياً"] = "Currently Active",
            ["غير نشط"] = "Inactive",
            ["المتاح"] = "Available",
            ["الوارد"] = "Incoming",
            ["الصادر"] = "Outgoing",
            ["الرصيد الحالي"] = "Current Balance",
            ["إجمالي الوارد"] = "Total incoming",
            ["إجمالي الصادر"] = "Total outgoing",
            ["النوع"] = "Type",
            ["الكمية"] = "Quantity",
            ["التاريخ"] = "Date",
            ["الملاحظات"] = "Notes",
            ["اسم المنتج"] = "Product Name",
            ["اسم الصنف"] = "Variant Name",
            ["المخزون المتاح"] = "Available Stock",

            // Common fields
            ["اسم المستخدم"] = "Username",
            ["الاسم الكامل"] = "Full Name",
            ["كلمة المرور"] = "Password",
            ["الوصف"] = "Description",
            ["رقم المنتج"] = "Product ID",
            ["رقم الصنف"] = "Variant ID",
            ["الدور"] = "Role",
            ["#"] = "#",

            // Common actions & status
            ["وارد"] = "Incoming",
            ["صادر"] = "Outgoing",
            ["تم الحفظ"] = "Saved",
            ["إلغاء"] = "Cancel",
            ["حفظ"] = "Save",
            ["تعديل"] = "Edit",
            ["حذف"] = "Delete",
            ["بحث"] = "Search",
            ["تحديث"] = "Refresh",
            ["إغلاق"] = "Close",
            ["مدير النظام"] = "Administrator",
            ["أمين المستودع"] = "Warehouse Keeper",
            ["محاسب"] = "Accountant",
            ["تسجيل الدخول"] = "Log in",
            ["جاري التحديث..."] = "Updating...",
            // Settings
            ["العربية"] = "Arabic",
            ["لغة التطبيق"] = "Application Language",
            ["عنوان واجهة API"] = "API Base URL",
            ["معلومات التطبيق"] = "Application Information",
            ["تذكر اللغة عند تشغيل التطبيق"] = "Remember language when the application starts",
            ["إعداد لغة التطبيق والاتصال."] = "Configure the application language and connection.",
            ["اللغة والتفضيلات"] = "Language and preferences",
            ["الاتصال"] = "Connection",
            ["أدخل عنوان خادم API"] = "Enter the API server address",
            ["يستخدم التطبيق هذا العنوان للاتصال بواجهة API."] = "This address is used by the desktop app to reach the API.",

            // Dialogs & Messages
            ["تنبيه"] = "Warning",
            ["خطأ"] = "Error",
            ["تأكيد"] = "Confirm",
            ["تأكيد الحذف"] = "Confirm Deletion",
            ["تم"] = "Done",
            ["يرجى إدخال اسم المنتج."] = "Please enter a product name.",
            ["يرجى إدخال اسم الصنف."] = "Please enter a variant name.",
            ["يرجى اختيار منتج أولاً."] = "Please select a product first.",
            ["يرجى اختيار صنف أولاً."] = "Please select a variant first.",
            ["يرجى اختيار نوع الحركة."] = "Please select a movement type.",
            ["يرجى إدخال كمية صحيحة أكبر من صفر."] = "Please enter a valid quantity greater than zero.",
            ["يرجى إدخال اسم المستخدم وكلمة المرور."] = "Please enter a username and password.",
            ["يرجى إدخال اسم المستخدم."] = "Please enter a username.",
            ["يرجى إدخال كلمة المرور."] = "Please enter a password.",
            ["يرجى إدخال الاسم الكامل."] = "Please enter the full name.",
            ["تعذر الاتصال بالخادم."] = "Unable to connect to the server.",
            ["حدث خطأ أثناء تسجيل الدخول."] = "An error occurred while signing in.",
            ["يرجى إدخال كلمة المرور الجديدة."] = "Please enter the new password.",
            ["يجب أن تتكون كلمة المرور من 6 أحرف على الأقل."] = "The password must be at least 6 characters.",
            ["كلمتا المرور غير متطابقتين."] = "The passwords do not match.",
            ["تم تغيير كلمة المرور بنجاح."] = "Password changed successfully.",
            ["تعذر تحميل بيانات لوحة التحكم"] = "Unable to load dashboard data",
            ["خطأ في الاتصال بالخادم"] = "Server connection error",
            ["حدث خطأ داخلي في الخادم. يرجى المحاولة مرة أخرى."] = "An internal server error occurred. Please try again.",
            ["تعذر حفظ المنتج."] = "Failed to save the product.",
            ["تعذر تحديث المنتج."] = "Failed to update the product.",
            ["خطأ في تحديث المنتج"] = "Error updating the product",
            ["تعذر حفظ الصنف."] = "Failed to save the variant.",
            ["خطأ في حفظ الصنف"] = "Error saving variant",
            ["تعذر تحديث الصنف."] = "Failed to update the variant.",
            ["خطأ في تحديث الصنف"] = "Error updating variant",
            ["تعذر حفظ المستخدم"] = "Failed to save user",
            ["لا توجد بيانات لعرضها حالياً."] = "There is no data to display.",

            // Permissions tooltips & messages
            ["تنبيه الصلاحيات"] = "Permissions Warning",
            ["عفواً، ميزة إضافة المنتجات ورفع الصور مقتصرة على مدير النظام فقط."] = "Sorry, adding products and uploading images is restricted to administrators only.",
            ["عفواً، ميزة تعديل المنتجات ورفع الصور مقتصرة على مدير النظام فقط."] = "Sorry, editing products and uploading images is restricted to administrators only.",
            ["عفواً، ميزة تعديل المنتجات مقتصرة على مدير النظام فقط."] = "Sorry, editing products is restricted to administrators only.",
            ["عفواً، ميزة حذف المنتجات مقتصرة على مدير النظام فقط."] = "Sorry, deleting products is restricted to administrators only.",

            // Permission descriptions
            ["عرض المنتجات"] = "View Products",
            ["إنشاء المنتجات"] = "Create Products",
            ["تعديل المنتجات"] = "Edit Products",
            ["حذف المنتجات"] = "Delete Products",
            ["عرض المخزون"] = "View Stock",
            ["إضافة وارد"] = "Add Incoming",
            ["إضافة صادر"] = "Add Outgoing",
            ["عرض التقارير"] = "View Reports",
            ["عرض المستخدمين"] = "View Users",
            ["إدارة المستخدمين"] = "Manage Users",
            ["إدارة الصلاحيات"] = "Manage Permissions",
            ["الصلاحيات"] = "Permissions",
            ["المنتجات"] = "Products",
            ["المستخدمون"] = "Users",
            ["التقارير"] = "Reports",
            ["الإعدادات"] = "Settings",
            ["المخزون"] = "Stock",

            // Reports view headers
            ["المستودع"] = "Warehouse",
            ["لوحة التحكم"] = "Dashboard",
            ["المخزون الحالي"] = "Current Stock",
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

    public bool IsArabic =>
        Language.Equals(
            "ar",
            StringComparison.OrdinalIgnoreCase);

    public FlowDirection FlowDirection => IsArabic
        ? System.Windows.FlowDirection.RightToLeft
        : System.Windows.FlowDirection.LeftToRight;

    public string T(string value) => Translate(value);

    public string this[string key]
    {
        get
        {
            var dictionary =
                IsArabic
                    ? _ar
                    : _en;

            return dictionary.TryGetValue(
                       key,
                       out var value)
                ? value
                : key;
        }
    }

    public void SetLanguage(
        string language)
    {
        Language =
            language.Equals(
                "en",
                StringComparison.OrdinalIgnoreCase)
                ? "en"
                : "ar";
    }

    public void ApplyTo(Window window)
    {
        window.Title = Translate(window.Title);
        ApplyTo((FrameworkElement)window);
    }

    public void ApplyTo(FrameworkElement element)
    {
        element.FlowDirection = FlowDirection;
        TranslateElement(element);
    }

    private void TranslateElement(DependencyObject element)
    {
        if (element is TextBlock textBlock &&
            !BindingOperations.IsDataBound(
                textBlock,
                TextBlock.TextProperty))
        {
            textBlock.Text = Translate(textBlock.Text);
        }
        else if (element is TextBlock boundTextBlock)
        {
            BindingOperations
                .GetBindingExpressionBase(
                    boundTextBlock,
                    TextBlock.TextProperty)
                ?.UpdateTarget();
        }

        if (element is Button button &&
            button.Content is string content)
        {
            button.Content = Translate(content);
        }

        if (element is ComboBoxItem comboBoxItem &&
            comboBoxItem.Content is string itemContent)
        {
            comboBoxItem.Content = Translate(itemContent);
        }

        if (element is CheckBox checkBox &&
            checkBox.Content is string checkContent &&
            !BindingOperations.IsDataBound(
                checkBox,
                ContentControl.ContentProperty))
        {
            checkBox.Content = Translate(checkContent);
        }

        if (element is Label label &&
            label.Content is string labelContent &&
            !BindingOperations.IsDataBound(
                label,
                ContentControl.ContentProperty))
        {
            label.Content = Translate(labelContent);
        }

        if (element is HeaderedContentControl headered &&
            headered.Header is string headerText &&
            !BindingOperations.IsDataBound(
                headered,
                HeaderedContentControl.HeaderProperty))
        {
            headered.Header = Translate(headerText);
        }

        if (element is FrameworkElement fe &&
            fe.ToolTip is string toolTipText)
        {
            fe.ToolTip = Translate(toolTipText);
        }

        if (element is DataGrid dataGrid)
        {
            foreach (var column in dataGrid.Columns)
            {
                if (column.Header is string header)
                    column.Header = Translate(header);
            }
        }

        for (var index = 0;
             index < VisualTreeHelper.GetChildrenCount(element);
             index++)
        {
            TranslateElement(
                VisualTreeHelper.GetChild(element, index));
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
                var key = _en
                    .FirstOrDefault(pair => pair.Value == value)
                    .Key;

                return _ar[key];
            }

            return _extraArToEn
                .FirstOrDefault(pair => pair.Value == value)
                .Key is { Length: > 0 } arabic
                ? arabic
                : value;
        }

        if (_ar.ContainsValue(value))
        {
            var key = _ar
                .FirstOrDefault(pair => pair.Value == value)
                .Key;

            return key.Length > 0
                ? _en[key]
                : value;
        }

        return _extraArToEn.TryGetValue(value, out var translation)
            ? translation
            : value;
    }

    public event PropertyChangedEventHandler?
        PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
