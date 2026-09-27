import 'dart:convert';
import 'package:http/http.dart' as http;

/// Port of `EsmatPlastic.Shared.Services.FirebaseAuthClient` —
/// Exchanges a custom token from the API for a Firebase ID token
/// using the Google Identity Toolkit REST endpoint.
class FirebaseAuthClient {
  static final http.Client _httpClient = http.Client();

  /// Sends the API-issued custom token to Firebase Identity Toolkit
  /// and returns the resulting ID token.
  Future<String> exchangeCustomToken(
      String customToken, String webApiKey) async {
    if (customToken.isEmpty || webApiKey.isEmpty) {
      throw Exception('Firebase sign-in is not configured.');
    }

    final endpoint =
        'https://identitytoolkit.googleapis.com/v1/accounts:signInWithCustomToken?key=${Uri.encodeComponent(webApiKey)}';

    final response = await _httpClient
        .post(
          Uri.parse(endpoint),
          headers: {'Content-Type': 'application/json'},
          body: jsonEncode({
            'token': customToken,
            'returnSecureToken': true,
          }),
        )
        .timeout(const Duration(seconds: 15));

    final responseBody = response.body;
    Map<String, dynamic>? result;
    try {
      result = jsonDecode(responseBody) as Map<String, dynamic>?;
    } on FormatException {
      // ignore parse errors
    }

    if (response.statusCode < 200 ||
        response.statusCode >= 300 ||
        result == null ||
        (result['idToken'] as String?)?.isEmpty != false) {
      throw Exception('Firebase sign-in failed: $responseBody');
    }

    return result['idToken'] as String;
  }
}
