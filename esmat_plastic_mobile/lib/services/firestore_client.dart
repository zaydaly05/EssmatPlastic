import 'dart:convert';
import 'package:crypto/crypto.dart';
import 'package:http/http.dart' as http;
import '../models/firestore_document.dart';

/// Port of `EsmatPlastic.Shared.Services.FirebaseFirestoreClient` —
/// Full Firestore REST API client with CRUD, pagination, tombstones, and auth.
class FirebaseFirestoreClient {
  static final http.Client _httpClient = http.Client();
  final String _projectId;
  String? _idToken;

  FirebaseFirestoreClient([this._projectId = 'esmat-plastic-app']);

  void setIdToken(String idToken) => _idToken = idToken;
  void clearIdToken() => _idToken = null;

  /// Extract authenticated user ID from the Firebase ID token JWT.
  String? get authenticatedUserId {
    if (_idToken == null || _idToken!.isEmpty) return null;
    try {
      final parts = _idToken!.split('.');
      if (parts.length < 2) return null;
      var payload = parts[1].replaceAll('-', '+').replaceAll('_', '/');
      payload =
          payload.padRight(payload.length + (4 - payload.length % 4) % 4, '=');
      final decoded = utf8.decode(base64Decode(payload));
      final json = jsonDecode(decoded) as Map<String, dynamic>;
      return json['sub'] as String?;
    } catch (_) {
      return null;
    }
  }

  String get _baseUrl =>
      'https://firestore.googleapis.com/v1/projects/${Uri.encodeComponent(_projectId)}/databases/(default)/documents';

  Map<String, String> get _authHeaders => {
        'Authorization': 'Bearer $_idToken',
        'Content-Type': 'application/json',
      };

  void _ensureAuthenticated() {
    if (_idToken == null || _idToken!.isEmpty) {
      throw Exception('Sign in to Firebase before accessing Firestore.');
    }
  }

  /// Read all documents from a collection with pagination (500 per page).
  Future<List<FirestoreDataDocument<T>>> getCollection<T>(
    String collectionName,
    T Function(Map<String, dynamic>) fromJson,
  ) async {
    _ensureAuthenticated();
    final results = <FirestoreDataDocument<T>>[];
    String? pageToken;

    do {
      var uri = '$_baseUrl/${Uri.encodeComponent(collectionName)}?pageSize=500';
      if (pageToken != null) {
        uri += '&pageToken=${Uri.encodeComponent(pageToken)}';
      }

      final response = await _httpClient
          .get(Uri.parse(uri), headers: _authHeaders)
          .timeout(const Duration(seconds: 15));

      if (response.statusCode < 200 || response.statusCode >= 300) {
        throw Exception(
            "Firestore read failed for '$collectionName': ${response.body}");
      }

      final document = jsonDecode(response.body) as Map<String, dynamic>;
      if (document.containsKey('documents')) {
        final documents = document['documents'] as List<dynamic>;
        for (final cloudDoc in documents) {
          final doc = cloudDoc as Map<String, dynamic>;
          final fields = doc['fields'] as Map<String, dynamic>;
          final payloadJsonField =
              fields['payloadJson'] as Map<String, dynamic>?;
          final payloadJson = payloadJsonField?['stringValue'] as String?;
          if (payloadJson == null || payloadJson.isEmpty) continue;

          T payload;
          try {
            payload =
                fromJson(jsonDecode(payloadJson) as Map<String, dynamic>);
          } catch (_) {
            continue;
          }

          final references = _readReferences(fields);
          final documentName = doc['name'] as String? ?? '';
          final syncId = documentName.split('/').last;
          results.add(FirestoreDataDocument<T>(
            data: payload,
            references: references,
            syncId: syncId,
          ));
        }
      }

      pageToken = document['nextPageToken'] as String?;
    } while (pageToken != null && pageToken.isNotEmpty);

    return results;
  }

  /// Read a single document by collection and document ID.
  Future<FirestoreDataDocument<T>> getDocument<T>(
    String collectionName,
    String documentId,
    T Function(Map<String, dynamic>) fromJson,
  ) async {
    _ensureAuthenticated();
    final uri =
        '$_baseUrl/${Uri.encodeComponent(collectionName)}/${Uri.encodeComponent(documentId)}';
    final response = await _httpClient
        .get(Uri.parse(uri), headers: _authHeaders)
        .timeout(const Duration(seconds: 15));

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception(
          "Firestore read failed for '$collectionName/$documentId': ${response.body}");
    }

    final document = jsonDecode(response.body) as Map<String, dynamic>;
    final fields = document['fields'] as Map<String, dynamic>;
    final payloadJson =
        (fields['payloadJson'] as Map<String, dynamic>)['stringValue']
            as String?;
    if (payloadJson == null || payloadJson.isEmpty) {
      throw Exception(
          "Firestore document '$collectionName/$documentId' has no valid payload.");
    }

    final payload = fromJson(jsonDecode(payloadJson) as Map<String, dynamic>);
    return FirestoreDataDocument<T>(
      data: payload,
      references: _readReferences(fields),
      syncId: documentId,
    );
  }

  /// Write (upsert) a document with payloadJson + references.
  Future<void> writeDocument(
    String collectionName,
    String documentId,
    Map<String, dynamic> payloadJsonMap, [
    Map<String, String>? references,
  ]) async {
    _ensureAuthenticated();

    final now = DateTime.now().toUtc();
    final syncId = documentId;

    final referenceFields = <String, dynamic>{};
    if (references != null) {
      for (final entry in references.entries) {
        referenceFields[entry.key] = {'stringValue': entry.value};
      }
    }

    final body = jsonEncode({
      'fields': {
        'syncId': {'stringValue': syncId},
        'updatedAt': {'timestampValue': now.toIso8601String()},
        'payloadJson': {'stringValue': jsonEncode(payloadJsonMap)},
        'references': {
          'mapValue': {'fields': referenceFields}
        },
      }
    });

    final uri =
        '$_baseUrl/${Uri.encodeComponent(collectionName)}/${Uri.encodeComponent(syncId)}';
    final response = await _httpClient
        .patch(Uri.parse(uri), headers: _authHeaders, body: body)
        .timeout(const Duration(seconds: 15));

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception(
          "Firestore write failed for '$collectionName': ${response.body}");
    }
  }

  /// Write a tombstone for a deleted record.
  Future<void> writeTombstone(String entityType, String recordKey) async {
    final bytes = utf8.encode('$entityType|$recordKey');
    final digest = sha256.convert(bytes);
    final hash = digest.toString(); // lowercase hex
    await _writeRawFields('deletedRecords', hash, {
      'entityType': {'stringValue': entityType},
      'recordKey': {'stringValue': recordKey},
      'deletedAt': {
        'timestampValue': DateTime.now().toUtc().toIso8601String()
      },
    });
  }

  /// Write user credential hash (server-side only collection).
  Future<void> writeUserCredential(
      String userSyncId, String passwordHash) async {
    await _writeRawFields('userCredentials', userSyncId, {
      'passwordHash': {'stringValue': passwordHash},
      'updatedAt': {
        'timestampValue': DateTime.now().toUtc().toIso8601String()
      },
    });
  }

  /// Delete a document from Firestore.
  Future<void> deleteDocument(
      String collectionName, String documentId) async {
    _ensureAuthenticated();
    final uri =
        '$_baseUrl/${Uri.encodeComponent(collectionName)}/${Uri.encodeComponent(documentId)}';
    final response = await _httpClient
        .delete(Uri.parse(uri), headers: _authHeaders)
        .timeout(const Duration(seconds: 15));

    if ((response.statusCode < 200 || response.statusCode >= 300) &&
        response.statusCode != 404) {
      throw Exception(
          "Firestore delete failed for '$collectionName': ${response.body}");
    }
  }

  Future<void> _writeRawFields(String collectionName, String documentId,
      Map<String, dynamic> fields) async {
    _ensureAuthenticated();
    final body = jsonEncode({'fields': fields});
    final uri =
        '$_baseUrl/${Uri.encodeComponent(collectionName)}/${Uri.encodeComponent(documentId)}';
    final response = await _httpClient
        .patch(Uri.parse(uri), headers: _authHeaders, body: body)
        .timeout(const Duration(seconds: 15));

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception(
          "Firestore write failed for '$collectionName': ${response.body}");
    }
  }

  Map<String, String> _readReferences(Map<String, dynamic> fields) {
    final references = <String, String>{};
    final referencesField = fields['references'] as Map<String, dynamic>?;
    if (referencesField == null) return references;
    final mapValue = referencesField['mapValue'] as Map<String, dynamic>?;
    if (mapValue == null) return references;
    final values = mapValue['fields'] as Map<String, dynamic>?;
    if (values == null) return references;

    for (final entry in values.entries) {
      final value = entry.value as Map<String, dynamic>?;
      if (value != null && value.containsKey('stringValue')) {
        references[entry.key] = value['stringValue'] as String? ?? '';
      }
    }
    return references;
  }
}
