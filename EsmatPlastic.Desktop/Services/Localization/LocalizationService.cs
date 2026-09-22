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
        ["فحص الاتصال"] = "Test Connection",
        ["لوحة تحكم بسيطة لإدارة مخزون البلاستيك."] = "A simple dashboard to manage plastic inventory.",
        ["اختر اللغة ثم سجّل الدخول للمتابعة."] = "Choose language and sign in to continue.",
        ["إدارة حسابات المستخدمين والدور والصلاحيات"] = "Manage user accounts, roles, and permissions",
        ["إدارة المنتجات والأصناف والبطاقات"] = "Manage products, variants, and cards",
        ["أدخل معلومات المنتج الأساسية والصورة للحفظ في النظام."] = "Enter basic product information and image to save.",
        ["صورة المنتج"] = "Product Image",
        ["🖼️  اختيار صورة للمنتج"] = "🖼️  Select Product Image",
        ["🖼️  تغيير صورة المنتج"] = "🖼️  Change Product Image",
        ["اختيار صورة للمنتج"] = "Select Product Image",
        ["لم يتم اختيار صورة بعد"] = "No image selected yet",
        ["تعديل بيانات المنتج"] = "Edit Product Information",
        ["قم بتحديث معلومات المنتج أو صورته أو حالته."] = "Update product details, image, or status.",
        ["حذف المنتج"] = "Delete Product",
        ["عرض الأصناف"] = "View Variants",
        ["إضافة صنف جديد"] = "Add New Variant",
        ["أدخل مواصفات الصنف الفرعي كالمقاس واللون والغطاء."] = "Enter variant specifications like size, color, and cap.",
        ["تعديل بيانات الصنف"] = "Edit Variant Details",
        ["حذف الصنف"] = "Delete Variant",
        ["يرجى اختيار صنف أولاً."] = "Please select a variant first.",
        ["يرجى اختيار منتج أولاً."] = "Please select a product first.",
        ["أدخل بيانات حساب المستخدم والدور والصلاحيات."] = "Enter user account details, role, and permissions.",
        ["تعديل المستخدم"] = "Edit User",
        ["حذف المستخدم"] = "Delete User",
        ["تسجيل حركة مخزنية"] = "Record Stock Transaction",
        ["أدخل كمية الحركة الواردة أو الصادرة والملاحظات."] = "Enter incoming or outgoing quantity and notes.",
        ["أرصدة المنتجات في المستودع"] = "Product balances in warehouse",
        ["+ وارد"] = "+ Incoming",
        ["- صادر"] = "- Outgoing",
        ["معطل"] = "Disabled",
        ["تسجيل الخروج"] = "Log out",
        ["تنبيهات المخزون المنخفض"] = "Low Stock Alerts",
        ["💡 ملخص وعمليات النظام"] = "💡 System Insights & Operations",
        ["يتم تحديث جميع بيانات المنتجات والمخزون بشكل فوري ومباشر مع خادم النظام."] = "All product and stock data is updated live with the system server.",
        ["• حالة الاتصال بالواجهة:"] = "• API Connection Status:",
        ["• حالة الاتصالبالواجهة:"] = "• API Connection Status:",
        ["حالة الاتصالبالواجهة:"] = "API Connection Status:",
        ["متصل بنجاح"] = "Connected successfully",
        [" متصل بنجاح"] = " Connected successfully",
        ["• آخر تحديث:"] = "• Last Updated:",
        ["الآن"] = "Just now",
        [" الآن"] = " Just now",
        ["المخزون الحقيقي"] = "Available Stock",
        ["المتاح الحقيقي"] = "Available Stock",
        ["الصورة"] = "Image",
        ["المنتج والوصف"] = "Product & Description",
        ["جميع الأقسام"] = "All Categories",
        ["عام"] = "General",
        ["إجمالي المستخدمين"] = "Total Users",
        ["جميع الحسابات المنسجلة"] = "All Registered Accounts",
        ["الحسابات النشطة"] = "Active Accounts",
        ["حسابات مفعلة ومتاحة"] = "Active & Available Accounts",
        ["مديرو النظام"] = "Administrators",
        ["صلاحيات كاملة بالنظام"] = "Full System Permissions",
        ["يرجى إدخال اسم المستخدم وكلمة المرور."] = "Please enter username and password.",
        ["اسم المستخدم أو كلمة المرور غير صحيحة."] = "Invalid username or password.",
        ["تعذر الاتصال بالخادم."] = "Unable to connect to server.",
        ["حدث خطأ أثناء تسجيل الدخول."] = "An error occurred during sign in.",
        ["يرجى إدخال عنوان واجهة API."] = "Please enter API base URL.",
        ["يرجى إدخال عنوان السيرفر أولاً."] = "Please enter server URL first.",
        ["جاري التوصيل وفحص الخادم..."] = "Connecting and checking server...",
        ["🟢 الاتصال بالسيرفر يعمل بنجاح!"] = "🟢 Server connection successful!",
        ["🔴 تعذر الاتصال بالسيرفر. يرجى التحقق من العنوان أو حالة الخادم."] = "🔴 Unable to connect to server. Check URL or server status.",
        ["تم حفظ الإعدادات وتحديث الاتصال واللغة بنجاح."] = "Settings saved and connection updated successfully.",
        ["يرجى إدخال اسم المنتج."] = "Please enter product name.",
        ["تعذر حفظ المنتج."] = "Failed to save product.",
        ["تعذر تحديث المنتج."] = "Failed to update product.",
        ["خطأ في تحديث المنتج"] = "Error updating product",
        ["يرجى إدخال اسم الصنف."] = "Please enter variant name.",
        ["تعذر حفظ الصنف."] = "Failed to save variant.",
        ["تعذر تحديث الصنف."] = "Failed to update variant.",
        ["خطأ في حفظ الصنف"] = "Error saving variant",
        ["خطأ في تحديث الصنف"] = "Error updating variant",
        ["يرجى إدخال كمية صحيحة أكبر من صفر."] = "Please enter a valid quantity greater than zero.",
        ["يرجى اختيار نوع الحركة."] = "Please select transaction type.",
        ["تعذر حفظ حركة المخزون."] = "Failed to save stock transaction.",
        ["تمت إضافة الحركة الواردة بنجاح."] = "Incoming transaction added successfully.",
        ["تمت إضافة الحركة الصادرة بنجاح."] = "Outgoing transaction added successfully.",
        ["خطأ في الاتصال بالخادم"] = "Server connection error",
        ["تعذر تحميل سجل الحركات"] = "Failed to load transaction history",
        ["يرجى إدخال اسم المستخدم."] = "Please enter username.",
        ["يرجى إدخال كلمة المرور."] = "Please enter password.",
        ["يرجى إدخال الاسم الكامل."] = "Please enter full name.",
        ["تعذر حفظ المستخدم"] = "Failed to save user",
        ["تعذر حذف المستخدم"] = "Failed to delete user",
        ["تعذر تحميل الصلاحيات"] = "Failed to load permissions",
        ["جاري تحميل الصلاحيات..."] = "Loading permissions...",
        ["لم يتم العثور على صلاحيات مطابقة لنتائج البحث"] = "No matching permissions found",
        ["تعذر طباعة التقرير"] = "Failed to print report",
        ["تعذر تصدير التقرير"] = "Failed to export report",
        ["لا توجد بيانات تقارير متاحة للتصدير."] = "No report data available for export.",
        ["لا توجد بيانات تقارير متاحة للطباعة."] = "No report data available for printing.",
        ["تصدير تقرير المخزون"] = "Export Stock Report",
        ["تم تصدير التقرير بنجاح إلى ملف CSV."] = "Report exported successfully to CSV.",
        ["تم التصدير"] = "Exported",
        ["تقرير حركة ورصيد المخزون"] = "Stock Movement and Balance Report",
        ["تقرير حركة المخزون"] = "Stock Movement Report",
        ["تاريخ التصدير:"] = "Export Date:",
        ["شركة عصمت للبلاستيك"] = "Esmat Plastic Company",
        ["يرجى إدخال كلمة المرور الجديدة."] = "Please enter new password.",
        ["يجب أن تتكون كلمة المرور من 6 أحرف على الأقل."] = "Password must be at least 6 characters.",
        ["كلمتا المرور غير متطابقتين."] = "Passwords do not match.",
        ["تم تغيير كلمة المرور بنجاح."] = "Password changed successfully.",
        ["تعذر تحميل بيانات لوحة التحكم"] = "Failed to load dashboard data",
        ["تعذر تحميل المنتجات"] = "Failed to load products",
        ["تعذر تحميل الأصناف"] = "Failed to load variants",
        ["تعذر تحميل المخزون"] = "Failed to load stock",
        ["أدخل تفاصيل ومواصفات العبوة أو الصنف للمنتج."] = "Enter packaging or variant specifications for the product.",
        ["تعديل بيانات العبوة أو تعديل حالة التفعيل."] = "Update packaging details or toggle active status.",
        ["حدد الصلاحيات الممنوحة لهذا المستخدم في النظام."] = "Select permissions granted to this user in the system.",
        ["قائمة الصلاحيات"] = "Permissions List",
        ["إلغاء التحديد"] = "Clear Selection",
        ["حفظ الصلاحيات"] = "Save Permissions",
        ["تعذر حفظ صلاحيات المستخدم."] = "Failed to save user permissions.",
        ["أدخل كلمة المرور الجديدة للمستخدم."] = "Enter the new password for the user.",
        ["بحث باسم المنتج أو الصنف أو المقاس..."] = "Search by product, variant, or size...",
        ["بحث باسم المستخدم أو الاسم الكامل أو الدور..."] = "Search by username, full name, or role...",
        ["بحث باسم الصلاحية أو الوصف..."] = "Search by permission name or description...",
        ["هل تريد حذف المستخدم {0}؟"] = "Do you want to delete user {0}?",
        ["انقر لتعديل/رفع صورة للمنتج"] = "Click to edit / upload product image",
        ["عفواً، ميزة إضافة المنتجات ورفع الصور مقتصرة على مدير النظام فقط."] = "Sorry, adding products and uploading photos is restricted to the administrator.",
        ["عفواً، ميزة تعديل المنتجات ورفع الصور مقتصرة على مدير النظام فقط."] = "Sorry, editing products and uploading photos is restricted to the administrator.",
        ["عفواً، ميزة تعديل المنتجات مقتصرة على مدير النظام فقط."] = "Sorry, editing products is restricted to the administrator.",
        ["عفواً، ميزة حذف المنتجات مقتصرة على مدير النظام فقط."] = "Sorry, deleting products is restricted to the administrator.",
        ["تنبيه الصلاحيات"] = "Permission Notice",
        ["شركة عصمت للبلاستيك - نظام إدارة المخازن"] = "Esmat Plastic Company - Inventory Management System",
        ["إغلاق"] = "Close",
        ["سجّل حركة دخول (وارد) أو خروج (صادر) من المخزن."] = "Record an incoming or outgoing stock transaction.",
        ["إدارة صلاحيات المستخدم"] = "Manage User Permissions",
        ["المستخدم:"] = "User:",
        ["المنتج:"] = "Product:",
        ["الصنف:"] = "Variant:",
        ["النوع:"] = "Type:",
        ["الكمية:"] = "Quantity:",
        ["التاريخ:"] = "Date:",
        ["الملاحظات:"] = "Notes:",
        ["الصلاحيات"] = "Permissions"
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
            if (fe.ToolTip is string toolTipStr && !string.IsNullOrWhiteSpace(toolTipStr))
            {
                fe.ToolTip = Translate(toolTipStr);
            }
        }

        if (element is HeaderedContentControl headeredControl && headeredControl.Header is string headerStr)
        {
            headeredControl.Header = Translate(headerStr);
        }

        if (element is TextBlock textBlock)
        {
            if (!BindingOperations.IsDataBound(textBlock, TextBlock.TextProperty) &&
                textBlock.Tag as string != "NoTranslate")
            {
                textBlock.Text = Translate(textBlock.Text);
            }

            if (textBlock.Inlines.Count > 0)
            {
                foreach (var inline in textBlock.Inlines.Cast<System.Windows.Documents.Inline>().ToList())
                {
                    if (inline is System.Windows.Documents.Run run && !string.IsNullOrWhiteSpace(run.Text))
                    {
                        run.Text = Translate(run.Text);
                    }
                }
            }
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

        if (element is DataGrid dataGrid)
        {
            foreach (var column in dataGrid.Columns)
            {
                if (column.Header is string colHeader)
                {
                    column.Header = Translate(colHeader);
                }
            }
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
            if (_en.FirstOrDefault(pair => pair.Value == value) is { Key: { Length: > 0 } key } &&
                _ar.TryGetValue(key, out var arabicValue))
            {
                return arabicValue;
            }

            return _extraArToEn.FirstOrDefault(pair => pair.Value == value).Key is { Length: > 0 } arabic
                ? arabic
                : value;
        }

        if (_ar.ContainsValue(value))
        {
            var key = _ar.FirstOrDefault(pair => pair.Value == value).Key;
            return key.Length > 0 && _en.TryGetValue(key, out var englishValue)
                ? englishValue
                : value;
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
