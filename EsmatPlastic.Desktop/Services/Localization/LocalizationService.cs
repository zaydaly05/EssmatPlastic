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
            ["Inactive"] = "غير نشط"
        };

    private readonly Dictionary<string, string> _en =
        new()
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
            ["Inactive"] = "Inactive"
        };

    private readonly Dictionary<string, string> _extraArToEn =
        new()
        {
            ["إسمت بلاستيك"] = "Esmat Plastic",
            ["نظام إدارة المخزون"] = "Inventory Management System",
            ["مرحباً"] = "Welcome",
            ["المستخدم"] = "User",
            ["نتمنى لك يوماً سعيداً"] = "Have a nice day",
            ["الأصناف"] = "Variants",
            ["ملخص النظام"] = "System Summary",
            ["نظرة عامة على بيانات النظام"] = "Overview of system data",
            ["إدارة المنتجات والأصناف"] = "Manage products and variants",
            ["إدارة المخزون وحركات المنتجات"] = "Manage stock and product transactions",
            ["إدارة المستخدمين والصلاحيات"] = "Manage users and permissions",
            ["إدارة صلاحيات النظام"] = "Manage system permissions",
            ["تقارير حركة المخزون"] = "Stock movement reports",
            ["إضافة منتج"] = "Add Product",
            ["إضافة منتج جديد"] = "Add New Product",
            ["تعديل المنتج"] = "Edit Product",
            ["إضافة صنف"] = "Add Variant",
            ["إضافة صنف / عبوة"] = "Add Variant / Package",
            ["إضافة صنف / عبوة جديدة"] = "Add New Variant / Package",
            ["تعديل الصنف"] = "Edit Variant",
            ["بيانات المستخدم"] = "User Details",
            ["إضافة مستخدم"] = "Add User",
            ["تغيير كلمة المرور"] = "Change Password",
            ["حركة مخزون"] = "Stock Transaction",
            ["إضافة حركة مخزون"] = "Add Stock Transaction",
            ["سجل حركات المخزون"] = "Stock Transaction History",
            ["العربية"] = "Arabic",
            ["لغة التطبيق"] = "Application Language",
            ["عنوان واجهة API"] = "API Base URL",
            ["معلومات التطبيق"] = "Application Information",
            ["اسم المستخدم"] = "Username",
            ["الاسم الكامل"] = "Full Name",
            ["كلمة المرور"] = "Password",
            ["كلمة المرور الجديدة"] = "New Password",
            ["تأكيد كلمة المرور"] = "Confirm Password",
            ["الوصف"] = "Description",
            ["رقم المنتج"] = "Product ID",
            ["رقم الصنف"] = "Variant ID",
            ["اسم المنتج"] = "Product Name",
            ["اسم الصنف"] = "Variant Name",
            ["المقاس"] = "Size",
            ["اللون"] = "Color",
            ["نوع الغطاء"] = "Cap Type",
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
            ["الملاحظات"] = "Notes",
            ["المستخدم"] = "User",
            ["المنتج"] = "Product",
            ["الصنف"] = "Variant",
            ["وارد"] = "Incoming",
            ["صادر"] = "Outgoing",
            ["تم الحفظ"] = "Saved",
            ["حفظ الحركة"] = "Save Transaction",
            ["سجل الحركات"] = "Transaction History",
            ["لا توجد بيانات لعرضها حالياً."] = "There is no data to display.",
            ["إلغاء"] = "Cancel",
            ["حفظ"] = "Save",
            ["تعديل"] = "Edit",
            ["حذف"] = "Delete",
            ["بحث"] = "Search",
            ["تحديث"] = "Refresh",
            ["مدير النظام"] = "Administrator",
            ["أمين المستودع"] = "Warehouse Keeper",
            ["محاسب"] = "Accountant",
            ["تسجيل الدخول"] = "Log in",
            ["الدور"] = "Role",
            ["تذكر اللغة عند تشغيل التطبيق"] = "Remember language when the application starts",
            ["يتم تحميل بيانات المنتجات والمخزون من واجهة API الخاصة بالنظام."] = "Product and inventory data is loaded from the system API.",
            ["يمكنك مراجعة الكميات والحركات من خلال الأقسام المتاحة."] = "Review quantities and movements through the available sections.",
            ["ستظهر الإحصاءات هنا بعد تحميل البيانات."] = "Statistics will appear here after the data loads.",
            ["لا توجد صلاحيات مرتبطة بهذا المستخدم"] = "No permissions are assigned to this user",
            ["يرجى تسجيل الخروج والتواصل مع مدير النظام لمنح الصلاحيات المناسبة."] = "Please log out and contact the administrator to assign the required permissions.",
            ["أرصدة المنتجات في المستودع"] = "Product inventory balances",
            ["تعديل المستخدم"] = "Edit user",
            ["جاري التحديث..."] = "Updating...",
            ["تنبيه"] = "Warning",
            ["خطأ"] = "Error",
            ["تأكيد"] = "Confirm",
            ["تأكيد الحذف"] = "Confirm deletion",
            ["تم"] = "Done",
            ["تم الحفظ"] = "Saved",
            ["يرجى إدخال اسم المنتج."] = "Please enter a product name.",
            ["يرجى إدخال اسم الصنف."] = "Please enter a variant name.",
            ["يرجى اختيار منتج أولاً."] = "Please select a product first.",
            ["يرجى اختيار صنف أولاً."] = "Please select a variant first.",
            ["يرجى اختيار نوع الحركة."] = "Please select a movement type.",
            ["يرجى إدخال كمية صحيحة أكبر من صفر."] = "Please enter a valid quantity greater than zero.",
            ["يرجى إدخال اسم المستخدم وكلمة المرور."] = "Please enter a username and password.",
            ["تعذر الاتصال بالخادم."] = "Unable to connect to the server.",
            ["حدث خطأ أثناء تسجيل الدخول."] = "An error occurred while signing in.",
            ["يرجى إدخال كلمة المرور الجديدة."] = "Please enter the new password.",
            ["يجب أن تتكون كلمة المرور من 6 أحرف على الأقل."] = "The password must be at least 6 characters.",
            ["كلمتا المرور غير متطابقتين."] = "The passwords do not match.",
            ["تم تغيير كلمة المرور بنجاح."] = "Password changed successfully.",
            ["تعذر تحميل بيانات لوحة التحكم"] = "Unable to load dashboard data",
            ["خطأ في الاتصال بالخادم"] = "Server connection error",
            ["حدث خطأ داخلي في الخادم. يرجى المحاولة مرة أخرى."] = "An internal server error occurred. Please try again."
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
        window.FlowDirection = IsArabic
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;

        TranslateElement(window);
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
