import 'package:flutter/material.dart';

/// App-wide theme matching the existing MAUI design system colors.
class AppTheme {
  // Brand colors from the MAUI design
  static const Color primary = Color(0xFF0D9488);      // Teal
  static const Color primaryDark = Color(0xFF0F766E);   // Darker teal
  static const Color accentTeal = Color(0xFF0D9488);
  static const Color ink = Color(0xFF1E293B);           // Dark text
  static const Color muted = Color(0xFF64748B);         // Muted text
  static const Color subtleMuted = Color(0xFF475569);   // Subtle muted
  static const Color background = Color(0xFFF1F5F9);    // Light bg
  static const Color surface = Colors.white;
  static const Color border = Color(0xFFE2E8F0);
  static const Color error = Color(0xFFEF4444);         // Red
  static const Color danger = Color(0xFFEF4444);
  static const Color warning = Color(0xFFF59E0B);
  static const Color errorBg = Color(0xFFFEE2E2);
  static const Color errorDark = Color(0xFFB91C1C);

  static const Color emerald100 = Color(0xFFD1FAE5);
  static const Color emerald800 = Color(0xFF065F46);
  static const Color rose100 = Color(0xFFFFE4E6);
  static const Color rose800 = Color(0xFF9F1239);
  static const Color slate100 = Color(0xFFF1F5F9);
  static const Color slate200 = Color(0xFFE2E8F0);
  static const Color slate600 = Color(0xFF475569);

  // Text Styles
  static const TextStyle headline = TextStyle(
    fontSize: 16,
    fontWeight: FontWeight.bold,
    color: ink,
  );

  static const TextStyle subhead = TextStyle(
    fontSize: 13,
    color: muted,
  );

  // Stat card colors
  static const Color productCardBg = Color(0xFFE8F6F6);
  static const Color productCardBorder = Color(0xFFC7ECE7);
  static const Color variantCardBg = Color(0xFFFFF1EC);
  static const Color variantCardBorder = Color(0xFFF4D9CC);
  static const Color stockCardBg = Color(0xFFEEF1FF);
  static const Color stockCardBorder = Color(0xFFC7D2FE);
  static const Color inCardBg = Color(0xFFF2F8E9);
  static const Color inCardBorder = Color(0xFFD8E8C7);
  static const Color outCardBg = Color(0xFFFFF7DE);
  static const Color outCardBorder = Color(0xFFF0E0B7);

  // Sidebar
  static const Color sidebarBg = Color(0xFF0F172A);
  static const Color sidebarText = Color(0xFFE2E8F0);
  static const Color sidebarMuted = Color(0xFF94A3B8);

  // Badge
  static const Color badgeBg = Color(0xFFE8F6F3);
  static const Color badgeBorder = Color(0xFFB6D9D4);
  static const Color badgeText = Color(0xFF145B55);
  static const Color activeBg = Color(0xFFD1FAE5);

  static ThemeData get lightTheme {
    return ThemeData(
      useMaterial3: true,
      brightness: Brightness.light,
      colorScheme: ColorScheme.fromSeed(
        seedColor: primary,
        brightness: Brightness.light,
      ),
      scaffoldBackgroundColor: background,
      fontFamily: 'Roboto',
      appBarTheme: const AppBarTheme(
        backgroundColor: surface,
        foregroundColor: ink,
        elevation: 0.5,
        centerTitle: false,
        titleTextStyle: TextStyle(
          color: ink,
          fontSize: 21,
          fontWeight: FontWeight.bold,
        ),
      ),
      cardTheme: CardThemeData(
        color: surface,
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(8),
          side: const BorderSide(color: border),
        ),
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: primary,
          foregroundColor: Colors.white,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
          minimumSize: const Size(double.infinity, 50),
          textStyle: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: surface,
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(8),
          borderSide: const BorderSide(color: border),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(8),
          borderSide: const BorderSide(color: border),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(8),
          borderSide: const BorderSide(color: primary, width: 2),
        ),
        hintStyle: const TextStyle(color: muted),
      ),
    );
  }

  static ThemeData get darkTheme => lightTheme;
}
