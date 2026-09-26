using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EsmatPlastic.Mobile.Services;

public class LocalizationService : INotifyPropertyChanged
{
    private static readonly Lazy<LocalizationService> _instance = new(() => new LocalizationService());
    public static LocalizationService Instance => _instance.Value;

    private string _language = Preferences.Get("language", "ar");

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
        ["OrderRequests"] = "طلبات الحجز",
        ["StockIn"] = "وارد مخزني",
        ["StockOut"] = "صادر مخزني",
        ["Adjust"] = "تعديل",
        ["Quantity"] = "الكمية",
        ["Notes"] = "ملاحظات",
        ["InsufficientStock"] = "المخزون غير كافٍ",
        ["DuplicateProduct"] = "منتج مكرر",
        ["ChangePassword"] = "تغيير كلمة المرور"
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
        ["OrderRequests"] = "Order Requests",
        ["StockIn"] = "Stock In",
        ["StockOut"] = "Stock Out",
        ["Adjust"] = "Adjust",
        ["Quantity"] = "Quantity",
        ["Notes"] = "Notes",
        ["InsufficientStock"] = "Insufficient stock",
        ["DuplicateProduct"] = "Duplicate product",
        ["ChangePassword"] = "Change password"
    };

    public string Language
    {
        get => _language;
        set
        {
            if (_language == value) return;
            _language = value;
            Preferences.Set("language", value);
            OnPropertyChanged(string.Empty);
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

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
