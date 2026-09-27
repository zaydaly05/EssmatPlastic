import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import '../theme/app_theme.dart';
import 'dashboard_screen.dart';

/// Port of LoginPage.xaml + LoginPage.xaml.cs —
/// Username/password login form with Arabic branding.
class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen>
    with SingleTickerProviderStateMixin {
  final _usernameController = TextEditingController();
  final _passwordController = TextEditingController();
  final _usernameFocus = FocusNode();
  final _passwordFocus = FocusNode();
  late AnimationController _animController;
  late Animation<double> _fadeAnimation;
  late Animation<Offset> _slideAnimation;

  @override
  void initState() {
    super.initState();
    _animController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 600),
    );
    _fadeAnimation = CurvedAnimation(
      parent: _animController,
      curve: Curves.easeOut,
    );
    _slideAnimation = Tween<Offset>(
      begin: const Offset(0, 0.08),
      end: Offset.zero,
    ).animate(CurvedAnimation(
      parent: _animController,
      curve: Curves.easeOut,
    ));
    _animController.forward();
  }

  @override
  void dispose() {
    _usernameController.dispose();
    _passwordController.dispose();
    _usernameFocus.dispose();
    _passwordFocus.dispose();
    _animController.dispose();
    super.dispose();
  }

  Future<void> _handleLogin(AuthProvider auth) async {
    auth.username = _usernameController.text;
    auth.password = _passwordController.text;
    final success = await auth.login();
    if (success && mounted) {
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(
          builder: (_) => DashboardScreen(user: auth.currentUser!),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.background,
      body: Consumer<AuthProvider>(
        builder: (context, auth, _) {
          return Center(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(30),
              child: FadeTransition(
                opacity: _fadeAnimation,
                child: SlideTransition(
                  position: _slideAnimation,
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      // Logo
                      Container(
                        width: 120,
                        height: 120,
                        decoration: BoxDecoration(
                          color: AppTheme.primary.withValues(alpha: 0.1),
                          shape: BoxShape.circle,
                        ),
                        child: const Icon(
                          Icons.precision_manufacturing_rounded,
                          size: 60,
                          color: AppTheme.primary,
                        ),
                      ),
                      const SizedBox(height: 20),

                      // Title
                      const Text(
                        'إسمت بلاستيك',
                        style: TextStyle(
                          fontSize: 32,
                          fontWeight: FontWeight.bold,
                          color: AppTheme.ink,
                        ),
                      ),
                      const SizedBox(height: 8),

                      const Text(
                        'تسجيل الدخول',
                        style: TextStyle(fontSize: 18, color: AppTheme.muted),
                      ),
                      const SizedBox(height: 32),

                      // Username field
                      TextField(
                        controller: _usernameController,
                        focusNode: _usernameFocus,
                        textAlign: TextAlign.center,
                        textInputAction: TextInputAction.next,
                        decoration: const InputDecoration(
                          hintText: 'اسم المستخدم',
                        ),
                        onSubmitted: (_) => _passwordFocus.requestFocus(),
                      ),
                      const SizedBox(height: 16),

                      // Password field
                      TextField(
                        controller: _passwordController,
                        focusNode: _passwordFocus,
                        textAlign: TextAlign.center,
                        obscureText: true,
                        textInputAction: TextInputAction.go,
                        decoration: const InputDecoration(
                          hintText: 'كلمة المرور',
                        ),
                        onSubmitted: (_) => _handleLogin(auth),
                      ),
                      const SizedBox(height: 24),

                      // Login button
                      SizedBox(
                        width: double.infinity,
                        height: 50,
                        child: ElevatedButton(
                          onPressed: auth.isLoading
                              ? null
                              : () => _handleLogin(auth),
                          child: const Text(
                            'دخول',
                            style: TextStyle(
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(height: 16),

                      // Loading indicator
                      if (auth.isLoading)
                        const CircularProgressIndicator(
                          color: AppTheme.primary,
                        ),

                      // Error message
                      if (auth.statusMessage.isNotEmpty) ...[
                        const SizedBox(height: 12),
                        Text(
                          auth.statusMessage,
                          textAlign: TextAlign.center,
                          style: const TextStyle(
                            color: AppTheme.error,
                            fontSize: 14,
                          ),
                        ),
                      ],
                    ],
                  ),
                ),
              ),
            ),
          );
        },
      ),
    );
  }
}
