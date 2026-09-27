import 'package:flutter_test/flutter_test.dart';
import 'package:provider/provider.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:esmat_plastic_mobile/main.dart';
import 'package:esmat_plastic_mobile/services/localization_service.dart';
import 'package:esmat_plastic_mobile/providers/auth_provider.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  testWidgets('App smoke test', (WidgetTester tester) async {
    SharedPreferences.setMockInitialValues({'language': 'ar'});
    final locService = LocalizationService();
    await locService.init();

    await tester.pumpWidget(
      MultiProvider(
        providers: [
          ChangeNotifierProvider<LocalizationService>.value(value: locService),
          ChangeNotifierProvider<AuthProvider>(create: (_) => AuthProvider()),
        ],
        child: const EsmatPlasticApp(),
      ),
    );
    expect(find.byType(EsmatPlasticApp), findsOneWidget);
  });
}
