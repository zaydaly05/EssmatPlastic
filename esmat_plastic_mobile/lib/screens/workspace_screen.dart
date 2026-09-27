import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:share_plus/share_plus.dart';
import '../models/auth_models.dart';
import '../models/firestore_document.dart';
import '../models/records.dart';
import '../providers/auth_provider.dart';
import '../services/firestore_client.dart';
import '../theme/app_theme.dart';
import '../widgets/shared_widgets.dart';

/// Port of `MobileWorkspacePage.cs` — multi-section screen backing Products,
/// Warehouse, Reports, Orders, Users, Permissions, and Settings directly
/// via Firestore collections.
class WorkspaceScreen extends StatefulWidget {
  final LoginResponse user;
  final String section;
  final VoidCallback onLogout;

  const WorkspaceScreen({
    super.key,
    required this.user,
    required this.section,
    required this.onLogout,
  });

  @override
  State<WorkspaceScreen> createState() => _WorkspaceScreenState();
}

class _WorkspaceScreenState extends State<WorkspaceScreen> {
  late String _currentSection;
  bool _isLoading = false;
  String _message = '';

  // Products state
  List<FirestoreDataDocument<ProductRecord>> _products = [];
  List<FirestoreDataDocument<VariantRecord>> _variants = [];
  String _productSearch = '';

  // Warehouse state
  List<FirestoreDataDocument<TransactionRecord>> _transactions = [];

  // Orders state
  List<FirestoreDataDocument<OrderRecord>> _orders = [];

  // Users & Permissions state
  List<FirestoreDataDocument<UserRecord>> _users = [];
  List<FirestoreDataDocument<PermissionRecord>> _permissions = [];
  String _userSearch = '';

  @override
  void initState() {
    super.initState();
    _currentSection = widget.section;
    _loadSection();
  }

  FirebaseFirestoreClient get _firestore =>
      context.read<AuthProvider>().firestoreClient;

  bool _has(String permission) => widget.user.hasPermission(permission);
  bool get _canManageOrders =>
      widget.user.role == 'Admin' || widget.user.role == 'Secretary';

  String get _sectionTitle {
    switch (_currentSection) {
      case 'Products':
        return 'المنتجات والأصناف';
      case 'Warehouse':
        return 'المستودع والمخزون';
      case 'Reports':
        return 'التقارير';
      case 'Orders':
        return 'طلبات الحجز';
      case 'Users':
        return 'المستخدمين';
      case 'Permissions':
        return 'الصلاحيات';
      case 'Settings':
        return 'الإعدادات';
      default:
        return 'عصمت بلاستيك';
    }
  }

  Future<void> _loadSection() async {
    setState(() {
      _isLoading = true;
      _message = 'جاري التحميل من Firebase...';
    });

    try {
      final fs = _firestore;
      switch (_currentSection) {
        case 'Products':
          if (_has('Products.View')) {
            _products = await fs.getCollection(
              'products',
              (j) => ProductRecord.fromJson(j),
            );
            _variants = await fs.getCollection(
              'productVariants',
              (j) => VariantRecord.fromJson(j),
            );
          }
          break;

        case 'Warehouse':
          if (_has('Stock.View')) {
            _variants = await fs.getCollection(
              'productVariants',
              (j) => VariantRecord.fromJson(j),
            );
            _products = await fs.getCollection(
              'products',
              (j) => ProductRecord.fromJson(j),
            );
            _transactions = await fs.getCollection(
              'stockTransactions',
              (j) => TransactionRecord.fromJson(j),
            );
          }
          break;

        case 'Reports':
          if (_has('Reports.View')) {
            _variants = await fs.getCollection(
              'productVariants',
              (j) => VariantRecord.fromJson(j),
            );
            _products = await fs.getCollection(
              'products',
              (j) => ProductRecord.fromJson(j),
            );
            _transactions = await fs.getCollection(
              'stockTransactions',
              (j) => TransactionRecord.fromJson(j),
            );
          }
          break;

        case 'Orders':
          if (_canManageOrders) {
            _orders = await fs.getCollection(
              'orderRequests',
              (j) => OrderRecord.fromJson(j),
            );
            _variants = await fs.getCollection(
              'productVariants',
              (j) => VariantRecord.fromJson(j),
            );
          }
          break;

        case 'Users':
          if (_has('Users.View')) {
            _users = await fs.getCollection(
              'users',
              (j) => UserRecord.fromJson(j),
            );
            _permissions = await fs.getCollection(
              'permissions',
              (j) => PermissionRecord.fromJson(j),
            );
          }
          break;

        case 'Permissions':
          if (_has('Permissions.Manage')) {
            _permissions = await fs.getCollection(
              'permissions',
              (j) => PermissionRecord.fromJson(j),
            );
          }
          break;

        case 'Settings':
          break;
      }
      _message = '';
    } catch (e) {
      _message = 'فشل تحميل البيانات: ${e.toString()}';
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  // --- CRUD ACTIONS ---

  Future<void> _createProduct() async {
    final nameCtrl = TextEditingController();
    final descCtrl = TextEditingController();
    final imageCtrl = TextEditingController();

    final result = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('منتج جديد'),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                controller: nameCtrl,
                decoration: const InputDecoration(labelText: 'اسم المنتج *'),
              ),
              TextField(
                controller: descCtrl,
                decoration: const InputDecoration(labelText: 'الوصف (اختياري)'),
              ),
              TextField(
                controller: imageCtrl,
                decoration:
                    const InputDecoration(labelText: 'رابط الصورة (اختياري)'),
              ),
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('إلغاء'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('حفظ'),
          ),
        ],
      ),
    );

    if (result == true && nameCtrl.text.trim().isNotEmpty) {
      final name = nameCtrl.text.trim();
      final dup = _products.any((p) => p.data.name.toLowerCase() == name.toLowerCase());
      if (dup) {
        _showError('يوجد منتج بنفس هذا الاسم بالفعل.');
        return;
      }
      final now = DateTime.now().toUtc();
      final docId = DateTime.now().millisecondsSinceEpoch.toString();
      await _firestore.writeDocument(
        'products',
        docId,
        ProductRecord(
          name: name,
          description: descCtrl.text.trim(),
          imagePath: imageCtrl.text.trim(),
          isActive: true,
          createdAt: now,
          updatedAt: now,
        ).toJson(),
      );
      _loadSection();
    }
  }

  Future<void> _editProduct(FirestoreDataDocument<ProductRecord> product) async {
    final nameCtrl = TextEditingController(text: product.data.name);
    final descCtrl = TextEditingController(text: product.data.description ?? '');
    final imageCtrl = TextEditingController(text: product.data.imagePath ?? '');
    bool isActive = product.data.isActive;

    final result = await showDialog<bool>(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (context, setSt) => AlertDialog(
          title: const Text('تعديل المنتج'),
          content: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(
                  controller: nameCtrl,
                  decoration: const InputDecoration(labelText: 'اسم المنتج'),
                ),
                TextField(
                  controller: descCtrl,
                  decoration: const InputDecoration(labelText: 'الوصف'),
                ),
                TextField(
                  controller: imageCtrl,
                  decoration: const InputDecoration(labelText: 'رابط الصورة'),
                ),
                SwitchListTile(
                  title: const Text('نشط'),
                  value: isActive,
                  onChanged: (val) => setSt(() => isActive = val),
                ),
              ],
            ),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(ctx, false),
              child: const Text('إلغاء'),
            ),
            ElevatedButton(
              onPressed: () => Navigator.pop(ctx, true),
              child: const Text('حفظ'),
            ),
          ],
        ),
      ),
    );

    if (result == true && nameCtrl.text.trim().isNotEmpty) {
      product.data.name = nameCtrl.text.trim();
      product.data.description = descCtrl.text.trim();
      product.data.imagePath = imageCtrl.text.trim();
      product.data.isActive = isActive;
      product.data.updatedAt = DateTime.now().toUtc();
      await _firestore.writeDocument(
        'products',
        product.syncId,
        product.data.toJson(),
        product.references,
      );
      _loadSection();
    }
  }

  Future<void> _deleteProduct(FirestoreDataDocument<ProductRecord> product) async {
    final productVariants = _variants
        .where((v) => v.references['product'] == product.syncId)
        .map((v) => v.syncId)
        .toSet();

    final hasTx = _transactions.any(
        (t) => productVariants.contains(t.references['productVariant'] ?? ''));
    if (hasTx) {
      _showError('هذا المنتج مرتبطة به معاملات مخزنية. يمكنك تعطيله بدلاً من حذفه.');
      return;
    }

    final confirm = await _showConfirm('حذف المنتج', 'هل أنت تأكد من حذف ${product.data.name} وكافة أصنافه؟');
    if (confirm) {
      await _firestore.writeTombstone('Product', product.data.name.trim().toLowerCase());
      _loadSection();
    }
  }

  Future<void> _createVariant(FirestoreDataDocument<ProductRecord> product) async {
    final nameCtrl = TextEditingController();
    final sizeCtrl = TextEditingController();
    final colorCtrl = TextEditingController();
    final matCtrl = TextEditingController();

    final result = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text('إضافة صنف لـ ${product.data.name}'),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                controller: nameCtrl,
                decoration: const InputDecoration(labelText: 'اسم الصنف *'),
              ),
              TextField(
                controller: sizeCtrl,
                decoration: const InputDecoration(labelText: 'الحجم'),
              ),
              TextField(
                controller: colorCtrl,
                decoration: const InputDecoration(labelText: 'اللون'),
              ),
              TextField(
                controller: matCtrl,
                decoration: const InputDecoration(labelText: 'المادة'),
              ),
            ],
          ),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('إلغاء'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('حفظ'),
          ),
        ],
      ),
    );

    if (result == true && nameCtrl.text.trim().isNotEmpty) {
      final now = DateTime.now().toUtc();
      final docId = DateTime.now().millisecondsSinceEpoch.toString();
      await _firestore.writeDocument(
        'productVariants',
        docId,
        VariantRecord(
          name: nameCtrl.text.trim(),
          size: sizeCtrl.text.trim(),
          color: colorCtrl.text.trim(),
          material: matCtrl.text.trim(),
          isActive: true,
          createdAt: now,
          updatedAt: now,
        ).toJson(),
        {'product': product.syncId},
      );
      _loadSection();
    }
  }

  // Stock Transactions
  Future<void> _createTransaction(
    FirestoreDataDocument<VariantRecord> variant,
    int type,
    double available,
  ) async {
    final qtyCtrl = TextEditingController();
    final notesCtrl = TextEditingController();

    final title = type == 1 ? 'إدخال مخزون (Stock In)' : 'إخراج مخزون (Stock Out)';
    final result = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text('$title - ${variant.data.name}'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            if (type == 2)
              Text('المتاح الحالي: ${available.toStringAsFixed(0)}',
                  style: const TextStyle(fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            TextField(
              controller: qtyCtrl,
              keyboardType: TextInputType.number,
              decoration: const InputDecoration(labelText: 'الكمية *'),
            ),
            TextField(
              controller: notesCtrl,
              decoration: const InputDecoration(labelText: 'ملاحظات (اختياري)'),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('إلغاء'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('حفظ المعاملة'),
          ),
        ],
      ),
    );

    if (result == true) {
      final qty = double.tryParse(qtyCtrl.text.trim()) ?? 0;
      if (qty <= 0) {
        _showError('الرجاء إدخال كمية موجبة صحيحة.');
        return;
      }
      if (type == 2 && qty > available) {
        _showError('المخزون المتاح لا يكفي (${available.toStringAsFixed(0)} فقط متوفر).');
        return;
      }

      final now = DateTime.now().toUtc();
      final userId = _firestore.authenticatedUserId ?? '';
      final docId = DateTime.now().millisecondsSinceEpoch.toString();

      await _firestore.writeDocument(
        'stockTransactions',
        docId,
        TransactionRecord(
          type: type,
          quantity: qty,
          notes: notesCtrl.text.trim(),
          createdAt: now,
          updatedAt: now,
        ).toJson(),
        {
          'productVariant': variant.syncId,
          'user': userId,
        },
      );
      _loadSection();
    }
  }

  // Export Reports
  Future<void> _exportReports() async {
    final buffer = StringBuffer();
    buffer.writeln('Product,Variant,Inbound,Outbound,Current');

    for (final v in _variants) {
      final txs = _transactions
          .where((t) => t.references['productVariant'] == v.syncId)
          .toList();
      final pName = _products
              .firstWhere(
                (p) => p.syncId == v.references['product'],
                orElse: () => FirestoreDataDocument(
                  data: ProductRecord(name: 'المنتج'),
                  references: {},
                  syncId: '',
                ),
              )
              .data
              .name;
      final inQty = txs
          .where((t) => t.data.type == 1)
          .fold<double>(0, (s, t) => s + t.data.quantity);
      final outQty = txs
          .where((t) => t.data.type == 2)
          .fold<double>(0, (s, t) => s + t.data.quantity);
      final current = inQty - outQty;

      buffer.writeln(
        '"$pName","${v.data.name}",${inQty.toStringAsFixed(0)},${outQty.toStringAsFixed(0)},${current.toStringAsFixed(0)}',
      );
    }

    await Share.share(buffer.toString(), subject: 'تقرير المخزون - عصمت بلاستيك');
  }

  // --- HELPERS ---
  void _showError(String msg) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('تنبيه'),
        content: Text(msg),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('موافق'),
          ),
        ],
      ),
    );
  }

  Future<bool> _showConfirm(String title, String msg) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(title),
        content: Text(msg),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: const Text('إلغاء'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: AppTheme.danger),
            onPressed: () => Navigator.pop(ctx, true),
            child: const Text('تأكيد'),
          ),
        ],
      ),
    );
    return result ?? false;
  }

  // --- BUILD UI ---
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(_sectionTitle),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: _loadSection,
            tooltip: 'تحديث',
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _loadSection,
        child: _isLoading
            ? const Center(child: CircularProgressIndicator())
            : Column(
                children: [
                  if (_message.isNotEmpty)
                    Container(
                      padding: const EdgeInsets.all(12),
                      color: AppTheme.danger.withValues(alpha: 0.1),
                      child: Text(_message,
                          style: const TextStyle(color: AppTheme.danger)),
                    ),
                  Expanded(child: _buildSectionContent()),
                ],
              ),
      ),
    );
  }

  Widget _buildSectionContent() {
    switch (_currentSection) {
      case 'Products':
        return _buildProductsSection();
      case 'Warehouse':
        return _buildWarehouseSection();
      case 'Reports':
        return _buildReportsSection();
      case 'Orders':
        return _buildOrdersSection();
      case 'Users':
        return _buildUsersSection();
      case 'Permissions':
        return _buildPermissionsSection();
      case 'Settings':
        return _buildSettingsSection();
      default:
        return const Center(child: Text('قسم غير معروف'));
    }
  }

  // SECTION 1: PRODUCTS
  Widget _buildProductsSection() {
    if (!_has('Products.View')) {
      return const Center(child: Text('ليس لديك صلاحية لعرض المنتجات.'));
    }

    final filtered = _products.where((p) {
      if (_productSearch.isEmpty) return true;
      return p.data.name
              .toLowerCase()
              .contains(_productSearch.toLowerCase()) ||
          (p.data.description
                  ?.toLowerCase()
                  .contains(_productSearch.toLowerCase()) ??
              false);
    }).toList();

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        if (_has('Products.Create')) ...[
          PrimaryActionButton(
            text: '＋ إضافة منتج جديد',
            onPressed: _createProduct,
          ),
          const SizedBox(height: 12),
        ],
        TextField(
          decoration: const InputDecoration(
            hintText: 'البحث عن منتج أو صنف...',
            prefixIcon: Icon(Icons.search),
          ),
          onChanged: (v) => setState(() => _productSearch = v),
        ),
        const SizedBox(height: 16),
        if (filtered.isEmpty)
          const EmptyStateLabel(text: 'لا توجد منتجات مطابقة.')
        else
          ...filtered.map((product) {
            final pVariants = _variants
                .where((v) => v.references['product'] == product.syncId)
                .toList();

            return PanelCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text(product.data.name, style: AppTheme.headline),
                      BadgeLabel(
                        text: product.data.isActive ? 'نشط' : 'غير نشط',
                        backgroundColor: product.data.isActive
                            ? AppTheme.accentTeal.withValues(alpha: 0.15)
                            : AppTheme.slate200,
                        textColor: product.data.isActive
                            ? AppTheme.accentTeal
                            : AppTheme.slate600,
                      ),
                    ],
                  ),
                  if (product.data.description != null &&
                      product.data.description!.isNotEmpty) ...[
                    const SizedBox(height: 4),
                    Text(product.data.description!, style: AppTheme.subhead),
                  ],
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      if (_has('Products.Edit'))
                        SmallActionButton(
                          text: 'تعديل',
                          onPressed: () => _editProduct(product),
                        ),
                      const SizedBox(width: 8),
                      if (_has('Products.Delete'))
                        SmallActionButton(
                          text: 'حذف',
                          backgroundColor: AppTheme.danger,
                          onPressed: () => _deleteProduct(product),
                        ),
                    ],
                  ),
                  if (pVariants.isNotEmpty) ...[
                    const SizedBox(height: 12),
                    SectionTitle(title: 'الأصناف (${pVariants.length})'),
                    ...pVariants.map((v) => Container(
                          padding: const EdgeInsets.all(8),
                          margin: const EdgeInsets.only(top: 6),
                          decoration: BoxDecoration(
                            color: AppTheme.slate100,
                            borderRadius: BorderRadius.circular(6),
                          ),
                          child: Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(v.data.name,
                                      style: const TextStyle(
                                          fontWeight: FontWeight.bold)),
                                  Text(
                                    [v.data.size, v.data.color, v.data.material]
                                        .where((s) => s != null && s.isNotEmpty)
                                        .join(' · '),
                                    style: AppTheme.subhead,
                                  ),
                                ],
                              ),
                            ],
                          ),
                        )),
                  ],
                  if (_has('Products.Create')) ...[
                    const SizedBox(height: 8),
                    SmallActionButton(
                      text: '＋ إضافة صنف',
                      backgroundColor: AppTheme.accentTeal,
                      onPressed: () => _createVariant(product),
                    ),
                  ],
                ],
              ),
            );
          }),
      ],
    );
  }

  // SECTION 2: WAREHOUSE
  Widget _buildWarehouseSection() {
    if (!_has('Stock.View')) {
      return const Center(child: Text('ليس لديك صلاحية لعرض المستودع.'));
    }

    final txLookup = <String, List<FirestoreDataDocument<TransactionRecord>>>{};
    for (final tx in _transactions) {
      final vId = tx.references['productVariant'] ?? '';
      txLookup.putIfAbsent(vId, () => []).add(tx);
    }

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        const SectionTitle(
          title: 'الأرصدة الحالية',
          subtitle: 'اختر صنفاً لتسجيل عملية إدخال أو إخراج مخزني',
        ),
        const SizedBox(height: 8),
        ..._variants.where((v) => v.data.isActive).map((v) {
          final txs = txLookup[v.syncId] ?? [];
          final inQty = txs
              .where((t) => t.data.type == 1)
              .fold<double>(0, (s, t) => s + t.data.quantity);
          final outQty = txs
              .where((t) => t.data.type == 2)
              .fold<double>(0, (s, t) => s + t.data.quantity);
          final available = inQty - outQty - v.data.reservedQuantity;

          final pName = _products
                  .firstWhere((p) => p.syncId == v.references['product'],
                      orElse: () => FirestoreDataDocument(
                            data: ProductRecord(name: 'منتج'),
                            references: {},
                            syncId: '',
                          ))
                  .data
                  .name;

          return PanelCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('$pName · ${v.data.name}', style: AppTheme.headline),
                const SizedBox(height: 6),
                Text(
                  'الوارد: ${inQty.toStringAsFixed(0)}   ·   الصادر: ${outQty.toStringAsFixed(0)}   ·   المتاح: ${available.toStringAsFixed(0)}',
                  style: AppTheme.subhead,
                ),
                const SizedBox(height: 10),
                Row(
                  children: [
                    if (_has('Stock.In'))
                      SmallActionButton(
                        text: 'إدخال مخزون ＋',
                        backgroundColor: AppTheme.accentTeal,
                        onPressed: () => _createTransaction(v, 1, inQty - outQty),
                      ),
                    const SizedBox(width: 8),
                    if (_has('Stock.Out'))
                      SmallActionButton(
                        text: 'إخراج مخزون －',
                        backgroundColor: AppTheme.warning,
                        onPressed: () => _createTransaction(v, 2, available),
                      ),
                  ],
                ),
              ],
            ),
          );
        }),
      ],
    );
  }

  // SECTION 3: REPORTS
  Widget _buildReportsSection() {
    if (!_has('Reports.View')) {
      return const Center(child: Text('ليس لديك صلاحية لعرض التقارير.'));
    }

    double totalIn = 0;
    double totalOut = 0;
    for (final tx in _transactions) {
      if (tx.data.type == 1) totalIn += tx.data.quantity;
      if (tx.data.type == 2) totalOut += tx.data.quantity;
    }

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Row(
          children: [
            Expanded(
              child: StatCard(
                title: 'إجمالي الوارد',
                value: totalIn.toStringAsFixed(0),
                backgroundColor: AppTheme.emerald100,
                textColor: AppTheme.emerald800,
              ),
            ),
            const SizedBox(width: 8),
            Expanded(
              child: StatCard(
                title: 'إجمالي الصادر',
                value: totalOut.toStringAsFixed(0),
                backgroundColor: AppTheme.rose100,
                textColor: AppTheme.rose800,
              ),
            ),
          ],
        ),
        const SizedBox(height: 16),
        PrimaryActionButton(
          text: 'مشاركة / تصدير CSV',
          onPressed: _exportReports,
        ),
      ],
    );
  }

  // SECTION 4: ORDERS
  Widget _buildOrdersSection() {
    if (!_canManageOrders) {
      return const Center(child: Text('طلبات الحجز متاحة للإدارة والسكرتارية فقط.'));
    }

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        const SectionTitle(
          title: 'طلبات الحجز',
          subtitle: 'الطلبات المعلقة تحجز كميات من الأصناف',
        ),
        const SizedBox(height: 12),
        if (_orders.isEmpty)
          const EmptyStateLabel(text: 'لا توجد طلبات حجز حالياً.')
        else
          ..._orders.map((o) => PanelCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(o.data.customerName, style: AppTheme.headline),
                    Text(
                      '${o.data.customerPhone ?? "بدون هاتف"} · الحالة: ${_orderStatus(o.data.status)}',
                      style: AppTheme.subhead,
                    ),
                  ],
                ),
              )),
      ],
    );
  }

  String _orderStatus(int status) {
    switch (status) {
      case 1:
        return 'قيد الانتظار';
      case 2:
        return 'مقبول';
      case 3:
        return 'مرفوض';
      case 4:
        return 'مكتمل';
      case 5:
        return 'ملغي';
      default:
        return 'غير معروف';
    }
  }

  // SECTION 5: USERS
  Widget _buildUsersSection() {
    if (!_has('Users.View')) {
      return const Center(child: Text('ليس لديك صلاحية لعرض المستخدمين.'));
    }

    final filteredUsers = _users.where((u) {
      if (_userSearch.isEmpty) return true;
      return u.data.fullName.toLowerCase().contains(_userSearch.toLowerCase()) ||
          u.data.username.toLowerCase().contains(_userSearch.toLowerCase());
    }).toList();

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        TextField(
          decoration: const InputDecoration(
            hintText: 'البحث عن مستخدم...',
            prefixIcon: Icon(Icons.search),
          ),
          onChanged: (v) => setState(() => _userSearch = v),
        ),
        const SizedBox(height: 12),
        ...filteredUsers.map((u) => PanelCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(u.data.fullName, style: AppTheme.headline),
                  Text('@${u.data.username} · دور: ${_roleName(u.data.role)}',
                      style: AppTheme.subhead),
                ],
              ),
            )),
      ],
    );
  }

  String _roleName(int r) {
    switch (r) {
      case 1:
        return 'مسؤول (Admin)';
      case 2:
        return 'مستودع';
      case 3:
        return 'محاسب';
      case 4:
        return 'سكرتارية';
      default:
        return 'مستخدم';
    }
  }

  // SECTION 6: PERMISSIONS
  Widget _buildPermissionsSection() {
    if (!_has('Permissions.Manage')) {
      return const Center(child: Text('ليس لديك صلاحية لعرض الصلاحيات.'));
    }

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        ..._permissions.map((p) => PanelCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(p.data.name, style: AppTheme.headline),
                  if (p.data.description != null) Text(p.data.description!, style: AppTheme.subhead),
                ],
              ),
            )),
      ],
    );
  }

  // SECTION 7: SETTINGS
  Widget _buildSettingsSection() {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        PanelCard(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(widget.user.fullName, style: AppTheme.headline),
              Text('@${widget.user.username} · ${widget.user.role}', style: AppTheme.subhead),
            ],
          ),
        ),
        const SizedBox(height: 16),
        PrimaryActionButton(
          text: 'تسجيل الخروج',
          backgroundColor: AppTheme.danger,
          onPressed: widget.onLogout,
        ),
      ],
    );
  }
}
