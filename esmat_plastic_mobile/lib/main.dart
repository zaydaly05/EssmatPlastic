import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'providers/auth_provider.dart';
import 'screens/dashboard_screen.dart';
import 'screens/login_screen.dart';
import 'services/localization_service.dart';
import 'theme/app_theme.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  final locService = LocalizationService();
  await locService.init();

  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider<LocalizationService>.value(value: locService),
        ChangeNotifierProvider<AuthProvider>(create: (_) => AuthProvider()),
      ],
      child: const EsmatPlasticApp(),
    ),
  );
}

class EsmatPlasticApp extends StatelessWidget {
  const EsmatPlasticApp({super.key});

  @override
  Widget build(BuildContext context) {
    return Consumer2<LocalizationService, AuthProvider>(
      builder: (context, loc, auth, _) {
        return MaterialApp(
          title: 'عصمت بلاستيك - Esmat Plastic',
          debugShowCheckedModeBanner: false,
          theme: AppTheme.lightTheme,
          darkTheme: AppTheme.darkTheme,
          themeMode: ThemeMode.light,
          builder: (context, child) {
            return Directionality(
              textDirection: loc.isArabic ? TextDirection.rtl : TextDirection.ltr,
              child: child!,
            );
          },
          home: auth.isLoggedIn && auth.currentUser != null
              ? DashboardScreen(user: auth.currentUser!)
              : const LoginScreen(),
        );
      },
    );
  }
}
