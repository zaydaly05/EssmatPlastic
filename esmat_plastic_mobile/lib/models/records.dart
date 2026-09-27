class ProductRecord {
  String name;
  String? description;
  String? imagePath;
  bool isActive;
  DateTime createdAt;
  DateTime updatedAt;

  ProductRecord({
    this.name = '',
    this.description,
    this.imagePath,
    this.isActive = true,
    DateTime? createdAt,
    DateTime? updatedAt,
  })  : createdAt = createdAt ?? DateTime.now(),
        updatedAt = updatedAt ?? DateTime.now();

  factory ProductRecord.fromJson(Map<String, dynamic> json) {
    return ProductRecord(
      name: json['name'] as String? ?? '',
      description: json['description'] as String?,
      imagePath: json['imagePath'] as String?,
      isActive: json['isActive'] as bool? ?? true,
      createdAt: _parseDate(json['createdAt']),
      updatedAt: _parseDate(json['updatedAt']),
    );
  }

  Map<String, dynamic> toJson() => {
        'name': name,
        'description': description,
        'imagePath': imagePath,
        'isActive': isActive,
        'createdAt': createdAt.toUtc().toIso8601String(),
        'updatedAt': updatedAt.toUtc().toIso8601String(),
      };

  static DateTime _parseDate(dynamic value) {
    if (value == null) return DateTime.now();
    if (value is String) return DateTime.tryParse(value) ?? DateTime.now();
    return DateTime.now();
  }
}

class VariantRecord {
  String name;
  String? size;
  String? color;
  String? capType;
  String? material;
  String? imagePath;
  bool isActive;
  double reservedQuantity;
  DateTime createdAt;
  DateTime updatedAt;

  VariantRecord({
    this.name = '',
    this.size,
    this.color,
    this.capType,
    this.material,
    this.imagePath,
    this.isActive = true,
    this.reservedQuantity = 0,
    DateTime? createdAt,
    DateTime? updatedAt,
  })  : createdAt = createdAt ?? DateTime.now(),
        updatedAt = updatedAt ?? DateTime.now();

  factory VariantRecord.fromJson(Map<String, dynamic> json) {
    return VariantRecord(
      name: json['name'] as String? ?? '',
      size: json['size'] as String?,
      color: json['color'] as String?,
      capType: json['capType'] as String?,
      material: json['material'] as String?,
      imagePath: json['imagePath'] as String?,
      isActive: json['isActive'] as bool? ?? true,
      reservedQuantity: (json['reservedQuantity'] as num?)?.toDouble() ?? 0,
      createdAt: ProductRecord._parseDate(json['createdAt']),
      updatedAt: ProductRecord._parseDate(json['updatedAt']),
    );
  }

  Map<String, dynamic> toJson() => {
        'name': name,
        'size': size,
        'color': color,
        'capType': capType,
        'material': material,
        'imagePath': imagePath,
        'isActive': isActive,
        'reservedQuantity': reservedQuantity,
        'createdAt': createdAt.toUtc().toIso8601String(),
        'updatedAt': updatedAt.toUtc().toIso8601String(),
      };
}

class TransactionRecord {
  int type; // 1 = Stock In, 2 = Stock Out
  double quantity;
  String? notes;
  DateTime createdAt;
  DateTime updatedAt;

  TransactionRecord({
    this.type = 0,
    this.quantity = 0,
    this.notes,
    DateTime? createdAt,
    DateTime? updatedAt,
  })  : createdAt = createdAt ?? DateTime.now(),
        updatedAt = updatedAt ?? DateTime.now();

  factory TransactionRecord.fromJson(Map<String, dynamic> json) {
    return TransactionRecord(
      type: json['type'] as int? ?? 0,
      quantity: (json['quantity'] as num?)?.toDouble() ?? 0,
      notes: json['notes'] as String?,
      createdAt: ProductRecord._parseDate(json['createdAt']),
      updatedAt: ProductRecord._parseDate(json['updatedAt']),
    );
  }

  Map<String, dynamic> toJson() => {
        'type': type,
        'quantity': quantity,
        'notes': notes,
        'createdAt': createdAt.toUtc().toIso8601String(),
        'updatedAt': updatedAt.toUtc().toIso8601String(),
      };
}

class OrderRecord {
  String customerName;
  String? customerPhone;
  DateTime requestedAt;
  DateTime updatedAt;
  int status; // 1=Pending, 2=Approved, 3=Rejected, 4=Completed, 5=Cancelled

  OrderRecord({
    this.customerName = '',
    this.customerPhone,
    DateTime? requestedAt,
    DateTime? updatedAt,
    this.status = 1,
  })  : requestedAt = requestedAt ?? DateTime.now(),
        updatedAt = updatedAt ?? DateTime.now();

  factory OrderRecord.fromJson(Map<String, dynamic> json) {
    return OrderRecord(
      customerName: json['customerName'] as String? ?? '',
      customerPhone: json['customerPhone'] as String?,
      requestedAt: ProductRecord._parseDate(json['requestedAt']),
      updatedAt: ProductRecord._parseDate(json['updatedAt']),
      status: json['status'] as int? ?? 1,
    );
  }

  Map<String, dynamic> toJson() => {
        'customerName': customerName,
        'customerPhone': customerPhone,
        'requestedAt': requestedAt.toUtc().toIso8601String(),
        'updatedAt': updatedAt.toUtc().toIso8601String(),
        'status': status,
      };
}

class OrderItemRecord {
  double quantity;
  DateTime updatedAt;

  OrderItemRecord({
    this.quantity = 0,
    DateTime? updatedAt,
  }) : updatedAt = updatedAt ?? DateTime.now();

  factory OrderItemRecord.fromJson(Map<String, dynamic> json) {
    return OrderItemRecord(
      quantity: (json['quantity'] as num?)?.toDouble() ?? 0,
      updatedAt: ProductRecord._parseDate(json['updatedAt']),
    );
  }

  Map<String, dynamic> toJson() => {
        'quantity': quantity,
        'updatedAt': updatedAt.toUtc().toIso8601String(),
      };
}

class UserRecord {
  String username;
  String fullName;
  int role;
  bool isActive;
  DateTime createdAt;
  DateTime updatedAt;

  UserRecord({
    this.username = '',
    this.fullName = '',
    this.role = 2,
    this.isActive = true,
    DateTime? createdAt,
    DateTime? updatedAt,
  })  : createdAt = createdAt ?? DateTime.now(),
        updatedAt = updatedAt ?? DateTime.now();

  factory UserRecord.fromJson(Map<String, dynamic> json) {
    return UserRecord(
      username: json['username'] as String? ?? '',
      fullName: json['fullName'] as String? ?? '',
      role: json['role'] as int? ?? 2,
      isActive: json['isActive'] as bool? ?? true,
      createdAt: ProductRecord._parseDate(json['createdAt']),
      updatedAt: ProductRecord._parseDate(json['updatedAt']),
    );
  }

  Map<String, dynamic> toJson() => {
        'username': username,
        'fullName': fullName,
        'role': role,
        'isActive': isActive,
        'createdAt': createdAt.toUtc().toIso8601String(),
        'updatedAt': updatedAt.toUtc().toIso8601String(),
      };
}

class PermissionRecord {
  String name;
  String? description;
  bool isActive;

  PermissionRecord({
    this.name = '',
    this.description,
    this.isActive = true,
  });

  factory PermissionRecord.fromJson(Map<String, dynamic> json) {
    return PermissionRecord(
      name: json['name'] as String? ?? '',
      description: json['description'] as String?,
      isActive: json['isActive'] as bool? ?? true,
    );
  }

  Map<String, dynamic> toJson() => {
        'name': name,
        'description': description,
        'isActive': isActive,
      };
}

class UserPermissionRecord {
  DateTime updatedAt;

  UserPermissionRecord({DateTime? updatedAt})
      : updatedAt = updatedAt ?? DateTime.now();

  factory UserPermissionRecord.fromJson(Map<String, dynamic> json) {
    return UserPermissionRecord(
      updatedAt: ProductRecord._parseDate(json['updatedAt']),
    );
  }

  Map<String, dynamic> toJson() => {
        'updatedAt': updatedAt.toUtc().toIso8601String(),
      };
}
