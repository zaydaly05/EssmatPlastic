class LoginRequest {
  final String username;
  final String password;

  LoginRequest({required this.username, required this.password});

  Map<String, dynamic> toJson() => {
        'username': username,
        'password': password,
      };
}

class LoginResponse {
  String token;
  String? firebaseCustomToken;
  String? firebaseWebApiKey;
  String? firebaseIdToken;
  DateTime expiresAt;
  int userId;
  String username;
  String fullName;
  String role;
  List<String> permissions;

  LoginResponse({
    this.token = '',
    this.firebaseCustomToken,
    this.firebaseWebApiKey,
    this.firebaseIdToken,
    required this.expiresAt,
    this.userId = 0,
    this.username = '',
    this.fullName = '',
    this.role = '',
    List<String>? permissions,
  }) : permissions = permissions ?? [];

  factory LoginResponse.fromJson(Map<String, dynamic> json) {
    return LoginResponse(
      token: json['token'] as String? ?? '',
      firebaseCustomToken: json['firebaseCustomToken'] as String?,
      firebaseWebApiKey: json['firebaseWebApiKey'] as String?,
      firebaseIdToken: json['firebaseIdToken'] as String?,
      expiresAt: json['expiresAt'] != null
          ? DateTime.parse(json['expiresAt'] as String)
          : DateTime.now(),
      userId: json['userId'] as int? ?? 0,
      username: json['username'] as String? ?? '',
      fullName: json['fullName'] as String? ?? '',
      role: json['role'] as String? ?? '',
      permissions: (json['permissions'] as List<dynamic>?)
              ?.map((e) => e as String)
              .toList() ??
          [],
    );
  }

  bool hasPermission(String permission) {
    return permissions
        .any((p) => p.toLowerCase() == permission.toLowerCase());
  }
}
