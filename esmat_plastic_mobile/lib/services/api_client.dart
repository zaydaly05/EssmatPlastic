import 'dart:convert';
import 'package:http/http.dart' as http;
import '../config/api_config.dart';

/// Port of `EsmatPlastic.Shared.Services.ApiClient` —
/// HTTP client with Bearer token auth for the EsmatPlastic REST API.
class ApiClient {
  late http.Client _httpClient;
  late String _baseUrl;
  String? _token;

  ApiClient() {
    _httpClient = http.Client();
    _baseUrl = ApiConfig.getBaseUrl();
    if (!_baseUrl.endsWith('/')) _baseUrl += '/';
  }

  void setToken(String token) {
    _token = token;
  }

  void clearToken() {
    _token = null;
  }

  Map<String, String> get _headers {
    final headers = <String, String>{
      'Content-Type': 'application/json',
    };
    if (_token != null) {
      headers['Authorization'] = 'Bearer $_token';
    }
    return headers;
  }

  Future<Map<String, dynamic>?> getHealthStatus() async {
    try {
      final response = await _httpClient
          .get(
            Uri.parse('${_baseUrl}api/Health/status'),
            headers: _headers,
          )
          .timeout(const Duration(seconds: 3));
      if (response.statusCode == 200) {
        return jsonDecode(response.body) as Map<String, dynamic>;
      }
    } catch (_) {
      // API unreachable or connection timed out
    }
    return {
      'status': 'Offline',
      'mode': 'LocalNetworkPrimary',
      'isOnline': false,
      'isNeonBackupOnline': false,
      'primaryDatabase': 'Local Offline Cache',
    };
  }

  Future<T?> getAsync<T>(
      String endpoint, T Function(Map<String, dynamic>) fromJson) async {
    final response = await _httpClient
        .get(
          Uri.parse('$_baseUrl$endpoint'),
          headers: _headers,
        )
        .timeout(const Duration(seconds: 15));
    await _ensureSuccess(response);
    return fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<List<T>> getListAsync<T>(
      String endpoint, T Function(Map<String, dynamic>) fromJson) async {
    final response = await _httpClient
        .get(
          Uri.parse('$_baseUrl$endpoint'),
          headers: _headers,
        )
        .timeout(const Duration(seconds: 15));
    await _ensureSuccess(response);
    final list = jsonDecode(response.body) as List<dynamic>;
    return list
        .map((item) => fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<T?> postAsync<T>(String endpoint, Map<String, dynamic> request,
      T Function(Map<String, dynamic>) fromJson) async {
    final response = await _httpClient
        .post(
          Uri.parse('$_baseUrl$endpoint'),
          headers: _headers,
          body: jsonEncode(request),
        )
        .timeout(const Duration(seconds: 15));
    await _ensureSuccess(response);
    return fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<T?> putAsync<T>(String endpoint, Map<String, dynamic> request,
      T Function(Map<String, dynamic>) fromJson) async {
    final response = await _httpClient
        .put(
          Uri.parse('$_baseUrl$endpoint'),
          headers: _headers,
          body: jsonEncode(request),
        )
        .timeout(const Duration(seconds: 15));
    await _ensureSuccess(response);
    return fromJson(jsonDecode(response.body) as Map<String, dynamic>);
  }

  Future<void> deleteAsync(String endpoint) async {
    final response = await _httpClient
        .delete(
          Uri.parse('$_baseUrl$endpoint'),
          headers: _headers,
        )
        .timeout(const Duration(seconds: 15));
    await _ensureSuccess(response);
  }

  Future<void> _ensureSuccess(http.Response response) async {
    if (response.statusCode >= 200 && response.statusCode < 300) return;

    String message = response.reasonPhrase ?? 'HTTP ${response.statusCode}';
    final body = response.body;

    if (body.isNotEmpty) {
      try {
        final json = jsonDecode(body);
        if (json is Map<String, dynamic> && json.containsKey('message')) {
          message = json['message'] as String;
        } else {
          message = body;
        }
      } on FormatException {
        message = body;
      }
    }

    throw ApiException(message, response.statusCode);
  }
}

class ApiException implements Exception {
  final String message;
  final int statusCode;

  ApiException(this.message, this.statusCode);

  @override
  String toString() => 'ApiException($statusCode): $message';
}
