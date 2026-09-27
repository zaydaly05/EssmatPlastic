import 'dart:io' show Platform;

class ApiConfig {
  static String getBaseUrl() {
    // Android emulator uses 10.0.2.2 to reach host machine's localhost
    if (Platform.isAndroid) {
      return 'http://10.0.2.2:5023';
    }
    return 'http://localhost:5023';
  }
}
