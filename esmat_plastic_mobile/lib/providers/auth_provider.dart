import 'package:flutter/material.dart';
import '../models/auth_models.dart';
import '../services/api_client.dart';
import '../services/firebase_auth_client.dart';
import '../services/firestore_client.dart';

/// Port of `EsmatPlastic.Shared.ViewModels.LoginViewModel` —
/// Handles login flow: API auth → Firebase custom token exchange → ID token.
class AuthProvider extends ChangeNotifier {
  final ApiClient apiClient;
  final FirebaseAuthClient firebaseAuthClient;
  final FirebaseFirestoreClient firestoreClient;

  String username = '';
  String password = '';
  String statusMessage = '';
  bool isLoading = false;
  LoginResponse? currentUser;

  bool get isLoggedIn => currentUser != null;

  AuthProvider({
    ApiClient? apiClient,
    FirebaseAuthClient? firebaseAuthClient,
    FirebaseFirestoreClient? firestoreClient,
  })  : apiClient = apiClient ?? ApiClient(),
        firebaseAuthClient = firebaseAuthClient ?? FirebaseAuthClient(),
        firestoreClient = firestoreClient ?? FirebaseFirestoreClient();

  Future<bool> login() async {
    if (username.trim().isEmpty || password.trim().isEmpty) {
      statusMessage = 'يرجى إدخال اسم المستخدم وكلمة المرور';
      notifyListeners();
      return false;
    }

    isLoading = true;
    statusMessage = '';
    notifyListeners();

    try {
      final response = await apiClient.postAsync(
        'api/Auth/login',
        LoginRequest(username: username, password: password).toJson(),
        LoginResponse.fromJson,
      );

      if (response != null && response.token.isNotEmpty) {
        // Exchange custom token for Firebase ID token
        final firebaseIdToken = await firebaseAuthClient.exchangeCustomToken(
          response.firebaseCustomToken ?? '',
          response.firebaseWebApiKey ?? '',
        );
        response.firebaseIdToken = firebaseIdToken;
        firestoreClient.setIdToken(firebaseIdToken);
        apiClient.setToken(response.token);
        currentUser = response;
        isLoading = false;
        notifyListeners();
        return true;
      }

      statusMessage = 'فشل تسجيل الدخول. يرجى التحقق من البيانات';
      isLoading = false;
      notifyListeners();
      return false;
    } catch (e) {
      statusMessage = 'حدث خطأ في الاتصال بالخادم';
      isLoading = false;
      notifyListeners();
      return false;
    }
  }

  void logout() {
    currentUser = null;
    username = '';
    password = '';
    statusMessage = '';
    apiClient.setToken('');
    firestoreClient.setIdToken('');
    notifyListeners();
  }
}
