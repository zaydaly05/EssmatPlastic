/// Generic wrapper for Firestore documents that carry a payloadJson field
/// plus a references map — mirrors the C# `FirestoreDataDocument<T>`.
class FirestoreDataDocument<T> {
  final T data;
  final Map<String, String> references;
  final String syncId;

  FirestoreDataDocument({
    required this.data,
    required this.references,
    this.syncId = '',
  });
}
