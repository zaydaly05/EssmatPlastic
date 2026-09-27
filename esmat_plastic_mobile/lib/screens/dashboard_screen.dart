import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../models/auth_models.dart';
import '../providers/auth_provider.dart';
import '../services/firestore_client.dart';
import '../theme/app_theme.dart';
import '../widgets/shared_widgets.dart';
import 'login_screen.dart';
import 'workspace_screen.dart';

/// Port of DashboardPage.xaml + DashboardPage.xaml.cs + DashboardViewModel —
/// Operations dashboard with stat cards, sidebar navigation, and permission gating.
class DashboardScreen extends StatefulWidget {
  final LoginResponse user;

  const DashboardScreen({super.key, required this.user});

  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  final GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>();

  // Dashboard stats
  int _productCount = 0;
  int _variantCount = 0;
  double _currentStock = 0;
  double _totalIn = 0;
  double _totalOut = 0;
  String _databaseStatus = 'جاري الاتصال...';
  String _statusMessage = '';
  bool _isLoading = false;
  bool _hasLoaded = false;

  bool get _canViewProducts => widget.user.hasPermission('Products.View');
  bool get _canViewStock => widget.user.hasPermission('Stock.View');
  bool get _canViewReports => widget.user.hasPermission('Reports.View');
  bool get _canAccessWorkspace =>
      _canViewProducts ||
      _canViewStock ||
      _canViewReports ||
      widget.user.hasPermission('Users.View') ||
      widget.user.hasPermission('Permissions.Manage') ||
      widget.user.role == 'Admin' ||
      widget.user.role == 'Secretary';
  bool get _hasNoDashboardPermission => !_canViewProducts && !_canViewStock;

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  FirebaseFirestoreClient get _firestore =>
      context.read<AuthProvider>().firestoreClient;

  Future<void> _loadData({bool forceRefresh = false}) async {
    if (_isLoading || (_hasLoaded && !forceRefresh)) return;

    setState(() {
      _isLoading = true;
      _statusMessage = '';
      _databaseStatus = 'جاري تحميل البيانات...';
    });

    try {
      final firestore = _firestore;

      // Load products count
      if (_canViewProducts) {
        final products = await firestore.getCollection(
          'products',
          (json) => json['isActive'] as bool? ?? true,
        );
        _productCount = products.where((p) => p.data == true).length;
      }

      // Load variants and transactions for stock data
      if (_canViewStock) {
        final variants = await firestore.getCollection(
          'productVariants',
          (json) => json['isActive'] as bool? ?? true,
        );
        _variantCount = variants.where((v) => v.data == true).length;

        final transactions = await firestore.getCollection(
          'stockTransactions',
          (json) => {
            'type': json['type'] as int? ?? 0,
            'quantity': (json['quantity'] as num?)?.toDouble() ?? 0.0,
          },
        );

        double totalIn = 0, totalOut = 0;
        for (final tx in transactions) {
          final type = (tx.data as Map)['type'] as int;
          final quantity = (tx.data as Map)['quantity'] as double;
          if (type == 1) {
            totalIn += quantity;
          } else if (type == 2) {
            totalOut += quantity;
          }
        }
        _currentStock = totalIn - totalOut;

        if (_canViewReports) {
          _totalIn = totalIn;
          _totalOut = totalOut;
        }
      }

      _databaseStatus = 'متصل - Firestore';
      if (_hasNoDashboardPermission) {
        _statusMessage = 'لا توجد صلاحيات لعرض بيانات لوحة التحكم.';
      }
      _hasLoaded = true;
    } catch (e) {
      // Fallback to EsmatPlastic API if Firestore is unconfigured or unavailable
      try {
        final apiClient = context.read<AuthProvider>().apiClient;

        if (_canViewProducts) {
          final products = await apiClient.getListAsync(
            'api/Products',
            (json) => json['isActive'] as bool? ?? true,
          );
          _productCount = products.where((active) => active).length;
        }

        if (_canViewStock) {
          final variants = await apiClient.getListAsync(
            'api/ProductVariants',
            (json) => json['isActive'] as bool? ?? true,
          );
          _variantCount = variants.where((active) => active).length;

          final transactions = await apiClient.getListAsync(
            'api/Stock/transactions',
            (json) => {
              'type': json['type'] as int? ?? 0,
              'quantity': (json['quantity'] as num?)?.toDouble() ?? 0.0,
            },
          );

          double totalIn = 0, totalOut = 0;
          for (final tx in transactions) {
            final type = tx['type'] as int;
            final quantity = tx['quantity'] as double;
            if (type == 1) {
              totalIn += quantity;
            } else if (type == 2) {
              totalOut += quantity;
            }
          }
          _currentStock = totalIn - totalOut;

          if (_canViewReports) {
            _totalIn = totalIn;
            _totalOut = totalOut;
          }
        }

        _databaseStatus = 'متصل - API الخادم المحلي';
        if (_hasNoDashboardPermission) {
          _statusMessage = 'لا توجد صلاحيات لعرض بيانات لوحة التحكم.';
        } else {
          _statusMessage = '';
        }
        _hasLoaded = true;
      } catch (apiErr) {
        _databaseStatus = 'تعذر تحميل البيانات';
        _statusMessage = 'تعذر تحميل بيانات لوحة التحكم من Firestore أو API الخادم.';
      }
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  void _logout() {
    final auth = context.read<AuthProvider>();
    auth.logout();
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const LoginScreen()),
      (route) => false,
    );
  }

  void _openSection(String section) {
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => WorkspaceScreen(
          user: widget.user,
          section: section,
          onLogout: _logout,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Directionality(
      textDirection: TextDirection.rtl,
      child: Scaffold(
        key: _scaffoldKey,
        drawer: _buildSidebarDrawer(),
        body: Column(
          children: [
            // Custom app bar
            Container(
              color: Colors.white,
              padding: EdgeInsets.only(
                top: MediaQuery.of(context).padding.top + 14,
                left: 20,
                right: 20,
                bottom: 14,
              ),
              child: Row(
                children: [
                  IconButton(
                    icon: const Text('☰',
                        style: TextStyle(
                            fontSize: 24, color: AppTheme.primaryDark)),
                    onPressed: () =>
                        _scaffoldKey.currentState?.openDrawer(),
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'Esmat Plastic',
                          style: TextStyle(
                            fontSize: 21,
                            fontWeight: FontWeight.bold,
                            color: AppTheme.ink,
                          ),
                        ),
                        Text(
                          'Operations dashboard',
                          style: TextStyle(
                            fontSize: 13,
                            color: AppTheme.muted,
                          ),
                        ),
                      ],
                    ),
                  ),
                  TextButton(
                    onPressed: _logout,
                    style: TextButton.styleFrom(
                      backgroundColor: AppTheme.errorBg,
                      foregroundColor: AppTheme.errorDark,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(8),
                      ),
                      padding: const EdgeInsets.symmetric(
                          horizontal: 12, vertical: 8),
                    ),
                    child: const Text('Sign out'),
                  ),
                ],
              ),
            ),

            // Body
            Expanded(
              child: RefreshIndicator(
                onRefresh: () => _loadData(forceRefresh: true),
                color: AppTheme.primary,
                child: SingleChildScrollView(
                  physics: const AlwaysScrollableScrollPhysics(),
                  padding: const EdgeInsets.all(18),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      // Database status card
                      PanelCard(
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          children: [
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                const Text('قاعدة البيانات المحلية',
                                    style: TextStyle(
                                        fontSize: 14, color: AppTheme.muted)),
                                Text(_databaseStatus,
                                    style: const TextStyle(
                                        fontWeight: FontWeight.bold,
                                        color: AppTheme.primaryDark)),
                              ],
                            ),
                            if (_statusMessage.isNotEmpty) ...[
                              const SizedBox(height: 8),
                              Text(_statusMessage,
                                  style: const TextStyle(
                                      color: AppTheme.errorDark)),
                            ],
                          ],
                        ),
                      ),
                      const SizedBox(height: 14),

                      // View Products button
                      if (_canAccessWorkspace)
                        ElevatedButton(
                          onPressed: () => _openSection('Products'),
                          style: ElevatedButton.styleFrom(
                            backgroundColor: AppTheme.primaryDark,
                            minimumSize: const Size(double.infinity, 48),
                          ),
                          child: const Text('عرض المنتجات',
                              style: TextStyle(color: Colors.white)),
                        ),
                      const SizedBox(height: 14),

                      if (_isLoading)
                        const Center(
                          child: CircularProgressIndicator(
                              color: AppTheme.primary),
                        ),

                      // System summary title
                      const Text('ملخص النظام',
                          style: TextStyle(
                              fontSize: 19,
                              fontWeight: FontWeight.bold,
                              color: AppTheme.ink)),
                      const SizedBox(height: 14),

                      // Stats grid
                      Row(
                        children: [
                          if (_canViewProducts)
                            Expanded(
                              child: StatCard(
                                title: 'المنتجات',
                                value: '$_productCount',
                                backgroundColor: AppTheme.productCardBg,
                                borderColor: AppTheme.productCardBorder,
                                onTap: () => _openSection('Products'),
                              ),
                            ),
                          if (_canViewProducts && _canViewStock)
                            const SizedBox(width: 12),
                          if (_canViewStock)
                            Expanded(
                              child: StatCard(
                                title: 'الأصناف',
                                value: '$_variantCount',
                                backgroundColor: AppTheme.variantCardBg,
                                borderColor: AppTheme.variantCardBorder,
                                onTap: () => _openSection('Products'),
                              ),
                            ),
                        ],
                      ),
                      if (_canViewStock) ...[
                        const SizedBox(height: 12),
                        StatCard(
                          title: 'المخزون الحالي',
                          value: _currentStock.toStringAsFixed(0),
                          backgroundColor: AppTheme.stockCardBg,
                          borderColor: AppTheme.stockCardBorder,
                          onTap: () => _openSection('Warehouse'),
                        ),
                      ],
                      if (_canViewReports) ...[
                        const SizedBox(height: 12),
                        Row(
                          children: [
                            Expanded(
                              child: StatCard(
                                title: 'إجمالي الوارد',
                                value: _totalIn.toStringAsFixed(0),
                                backgroundColor: AppTheme.inCardBg,
                                borderColor: AppTheme.inCardBorder,
                                onTap: () => _openSection('Reports'),
                              ),
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: StatCard(
                                title: 'إجمالي الصادر',
                                value: _totalOut.toStringAsFixed(0),
                                backgroundColor: AppTheme.outCardBg,
                                borderColor: AppTheme.outCardBorder,
                                onTap: () => _openSection('Reports'),
                              ),
                            ),
                          ],
                        ),
                      ],

                      if (_hasNoDashboardPermission) ...[
                        const SizedBox(height: 14),
                        Container(
                          padding: const EdgeInsets.all(14),
                          color: AppTheme.outCardBg,
                          child: const Text(
                            'لا توجد صلاحيات لعرض بيانات لوحة التحكم.',
                            style: TextStyle(color: Color(0xFF8A5A00)),
                          ),
                        ),
                      ],

                      const SizedBox(height: 14),
                      ElevatedButton(
                        onPressed: () => _loadData(forceRefresh: true),
                        style: ElevatedButton.styleFrom(
                          backgroundColor: AppTheme.primary,
                          minimumSize: const Size(double.infinity, 48),
                        ),
                        child: const Text('تحديث البيانات',
                            style: TextStyle(color: Colors.white)),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSidebarDrawer() {
    final user = widget.user;
    return Drawer(
      backgroundColor: AppTheme.sidebarBg,
      child: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 26),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text('Esmat Plastic',
                  style: TextStyle(
                      fontSize: 22,
                      fontWeight: FontWeight.bold,
                      color: Colors.white)),
              const SizedBox(height: 2),
              Text(user.fullName,
                  style: const TextStyle(
                      fontSize: 14, color: AppTheme.sidebarMuted)),
              const SizedBox(height: 24),
              _drawerItem('⌂   Dashboard', null, () {
                Navigator.of(context).pop();
              }),
              if (user.hasPermission('Products.View'))
                _drawerItem(
                    '▦   Products & variants', 'Products.View', () {
                  Navigator.of(context).pop();
                  _openSection('Products');
                }),
              if (user.hasPermission('Stock.View'))
                _drawerItem('▤   Warehouse', 'Stock.View', () {
                  Navigator.of(context).pop();
                  _openSection('Warehouse');
                }),
              if (user.hasPermission('Reports.View'))
                _drawerItem('▥   Reports', 'Reports.View', () {
                  Navigator.of(context).pop();
                  _openSection('Reports');
                }),
              if (user.role == 'Admin' || user.role == 'Secretary')
                _drawerItem('▧   Order requests', null, () {
                  Navigator.of(context).pop();
                  _openSection('Orders');
                }),
              if (user.hasPermission('Users.View'))
                _drawerItem('♙   Users', 'Users.View', () {
                  Navigator.of(context).pop();
                  _openSection('Users');
                }),
              if (user.hasPermission('Permissions.Manage'))
                _drawerItem('⚙   Permissions', 'Permissions.Manage', () {
                  Navigator.of(context).pop();
                  _openSection('Permissions');
                }),
              _drawerItem('⚙   Settings', null, () {
                Navigator.of(context).pop();
                _openSection('Settings');
              }),
              const SizedBox(height: 22),
              SizedBox(
                width: double.infinity,
                child: ElevatedButton(
                  onPressed: () {
                    Navigator.of(context).pop();
                    _logout();
                  },
                  style: ElevatedButton.styleFrom(
                    backgroundColor: const Color(0xFF263044),
                    foregroundColor: const Color(0xFFFCA5A5),
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(10)),
                    minimumSize: const Size(double.infinity, 48),
                  ),
                  child: const Text('Sign out'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _drawerItem(String title, String? permission, VoidCallback onTap) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 2),
      child: SizedBox(
        width: double.infinity,
        child: TextButton(
          onPressed: onTap,
          style: TextButton.styleFrom(
            foregroundColor: AppTheme.sidebarText,
            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 11),
            shape:
                RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
            alignment: Alignment.centerLeft,
          ),
          child: Text(title, style: const TextStyle(fontSize: 15)),
        ),
      ),
    );
  }
}
