import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Port of `EsmatPlastic.Mobile.Services.LocalizationService` —
/// Arabic/English localization with persistent language preference.
class LocalizationService extends ChangeNotifier {
  static final LocalizationService _instance = LocalizationService._();
  static LocalizationService get instance => _instance;

  factory LocalizationService() => _instance;

  String _language = 'ar';

  LocalizationService._();

  Future<void> init() async {
    final prefs = await SharedPreferences.getInstance();
    _language = prefs.getString('language') ?? 'ar';
    notifyListeners();
  }

  String get language => _language;
  bool get isArabic => _language.toLowerCase() == 'ar';

  Future<void> setLanguage(String lang) async {
    if (_language == lang) return;
    _language = lang;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('language', lang);
    notifyListeners();
  }

  String operator [](String key) {
    final dict = isArabic ? _ar : _en;
    return dict[key] ?? key;
  }

  String tr(String key) => this[key];

  static const Map<String, String> _ar = {
    'AppName': 'إسمت بلاستيك',
    'Dashboard': 'لوحة التحكم',
    'Warehouse': 'المستودع',
    'Products': 'المنتجات',
    'Reports': 'التقارير',
    'Users': 'المستخدمون',
    'Permissions': 'الصلاحيات',
    'Settings': 'الإعدادات',
    'Logout': 'تسجيل الخروج',
    'Refresh': 'تحديث',
    'Save': 'حفظ',
    'Cancel': 'إلغاء',
    'Edit': 'تعديل',
    'Delete': 'حذف',
    'Search': 'بحث',
    'Incoming': 'وارد',
    'Outgoing': 'صادر',
    'Stock': 'المخزون',
    'CurrentStock': 'المخزون الحالي',
    'TotalIn': 'إجمالي الوارد',
    'TotalOut': 'إجمالي الصادر',
    'Active': 'نشط',
    'Inactive': 'غير نشط',
    'AppTagline': 'نظام إدارة المخزون',
    'Welcome': 'مرحباً',
    'HaveANiceDay': 'نتمنى لك يوماً سعيداً',
    'AdminRole': 'مدير النظام',
    'WarehouseRole': 'أمين المستودع',
    'AccountantRole': 'محاسب',
    'OrderRequests': 'طلبات الحجز',
    'StockIn': 'وارد مخزني',
    'StockOut': 'صادر مخزني',
    'Adjust': 'تعديل',
    'Quantity': 'الكمية',
    'Notes': 'ملاحظات',
    'InsufficientStock': 'المخزون غير كافٍ',
    'DuplicateProduct': 'منتج مكرر',
    'ChangePassword': 'تغيير كلمة المرور',
    'Login': 'تسجيل الدخول',
    'LoginButton': 'دخول',
    'UsernamePlaceholder': 'اسم المستخدم',
    'PasswordPlaceholder': 'كلمة المرور',
    'EnterCredentials': 'يرجى إدخال اسم المستخدم وكلمة المرور',
    'LoginFailed': 'فشل تسجيل الدخول. يرجى التحقق من البيانات',
    'ConnectionError': 'خطأ في الاتصال بالخادم',
    'Loading': 'جاري التحميل...',
    'NoPermission': 'لا توجد صلاحيات لعرض بيانات لوحة التحكم.',
    'RefreshData': 'تحديث البيانات',
    'ViewProducts': 'عرض المنتجات',
    'Variants': 'الأصناف',
    'SignOut': 'تسجيل الخروج',
    'NoProducts': 'لا توجد منتجات.',
    'NoMatchingProducts': 'لا توجد منتجات مطابقة.',
    'SearchProduct': 'ابحث عن منتج',
    'LoadError': 'تعذر تحميل المنتجات من Firestore. تحقق من الاتصال والصلاحيات.',
    'NoProductPermission': 'ليس لديك صلاحية لعرض المنتجات.',
    'ConnectedFirestore': 'متصل - Firestore',
    'LoadingData': 'جاري تحميل البيانات...',
    'LoadFailed': 'تعذر تحميل البيانات',
    'DashboardLoadFailed': 'تعذر تحميل بيانات لوحة التحكم من Firestore.',
    'LocalDatabase': 'قاعدة البيانات المحلية',
  };

  static const Map<String, String> _en = {
    'AppName': 'Esmat Plastic',
    'Dashboard': 'Dashboard',
    'Warehouse': 'Warehouse',
    'Products': 'Products',
    'Reports': 'Reports',
    'Users': 'Users',
    'Permissions': 'Permissions',
    'Settings': 'Settings',
    'Logout': 'Logout',
    'Refresh': 'Refresh',
    'Save': 'Save',
    'Cancel': 'Cancel',
    'Edit': 'Edit',
    'Delete': 'Delete',
    'Search': 'Search',
    'Incoming': 'Incoming',
    'Outgoing': 'Outgoing',
    'Stock': 'Stock',
    'CurrentStock': 'Current Stock',
    'TotalIn': 'Total In',
    'TotalOut': 'Total Out',
    'Active': 'Active',
    'Inactive': 'Inactive',
    'AppTagline': 'Inventory Management System',
    'Welcome': 'Welcome',
    'HaveANiceDay': 'Have a nice day',
    'AdminRole': 'Administrator',
    'WarehouseRole': 'Warehouse Keeper',
    'AccountantRole': 'Accountant',
    'OrderRequests': 'Order Requests',
    'StockIn': 'Stock In',
    'StockOut': 'Stock Out',
    'Adjust': 'Adjust',
    'Quantity': 'Quantity',
    'Notes': 'Notes',
    'InsufficientStock': 'Insufficient stock',
    'DuplicateProduct': 'Duplicate product',
    'ChangePassword': 'Change password',
    'Login': 'Login',
    'LoginButton': 'Sign In',
    'UsernamePlaceholder': 'Username',
    'PasswordPlaceholder': 'Password',
    'EnterCredentials': 'Please enter your username and password',
    'LoginFailed': 'Login failed. Please check your credentials',
    'ConnectionError': 'Connection error',
    'Loading': 'Loading...',
    'NoPermission': 'No permissions to view dashboard data.',
    'RefreshData': 'Refresh Data',
    'ViewProducts': 'View Products',
    'Variants': 'Variants',
    'SignOut': 'Sign Out',
    'NoProducts': 'No products.',
    'NoMatchingProducts': 'No matching products.',
    'SearchProduct': 'Search product',
    'LoadError':
        'Could not load products from Firestore. Check connection and permissions.',
    'NoProductPermission': 'You do not have permission to view products.',
    'ConnectedFirestore': 'Connected - Firestore',
    'LoadingData': 'Loading data...',
    'LoadFailed': 'Failed to load data',
    'DashboardLoadFailed': 'Could not load dashboard data from Firestore.',
    'LocalDatabase': 'Local Database',
  };
}
