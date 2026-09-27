import 'package:flutter/material.dart';
import '../theme/app_theme.dart';

/// Reusable stat card widget used on the dashboard.
class StatCard extends StatelessWidget {
  final String title;
  final String value;
  final Color backgroundColor;
  final Color borderColor;
  final Color textColor;
  final VoidCallback? onTap;

  const StatCard({
    super.key,
    required this.title,
    required this.value,
    required this.backgroundColor,
    this.borderColor = AppTheme.border,
    this.textColor = AppTheme.ink,
    this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 200),
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: backgroundColor,
          borderRadius: BorderRadius.circular(8),
          border: Border.all(color: borderColor),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              title,
              style: TextStyle(color: textColor.withValues(alpha: 0.8), fontSize: 14),
            ),
            const SizedBox(height: 6),
            Text(
              value,
              style: TextStyle(
                color: textColor,
                fontSize: 28,
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// A card panel with white background and border — matches the MAUI `Panel()` helper.
class PanelCard extends StatelessWidget {
  final Widget child;
  final EdgeInsetsGeometry padding;

  const PanelCard({
    super.key,
    required this.child,
    this.padding = const EdgeInsets.all(15),
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: padding,
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppTheme.border),
      ),
      child: child,
    );
  }
}

/// Info card matching the MAUI `Card()` helper — title, detail, badge.
class InfoCard extends StatelessWidget {
  final String title;
  final String detail;
  final String badge;

  const InfoCard({
    super.key,
    required this.title,
    required this.detail,
    required this.badge,
  });

  @override
  Widget build(BuildContext context) {
    return PanelCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(title,
              style: const TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                  color: AppTheme.ink)),
          if (detail.isNotEmpty) ...[
            const SizedBox(height: 5),
            Text(detail,
                style:
                    const TextStyle(fontSize: 13, color: AppTheme.subtleMuted)),
          ],
          if (badge.isNotEmpty) ...[
            const SizedBox(height: 5),
            BadgeLabel(text: badge),
          ],
        ],
      ),
    );
  }
}

/// A small colored badge — matches the MAUI `Badge()` helper.
class BadgeLabel extends StatelessWidget {
  final String text;
  final Color backgroundColor;
  final Color textColor;

  const BadgeLabel({
    super.key,
    required this.text,
    this.backgroundColor = AppTheme.badgeBg,
    this.textColor = AppTheme.ink,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(
        color: backgroundColor,
        borderRadius: BorderRadius.circular(4),
      ),
      child: Text(
        text,
        style: TextStyle(
          fontSize: 11,
          fontWeight: FontWeight.bold,
          color: textColor,
        ),
      ),
    );
  }
}

/// Section title text widget with optional subtitle.
class SectionTitle extends StatelessWidget {
  final String title;
  final String? subtitle;

  const SectionTitle({
    super.key,
    required this.title,
    this.subtitle,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(top: 8, bottom: 4, left: 2, right: 2),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            title,
            style: const TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.bold,
              color: AppTheme.ink,
            ),
          ),
          if (subtitle != null && subtitle!.isNotEmpty) ...[
            const SizedBox(height: 2),
            Text(
              subtitle!,
              style: const TextStyle(
                fontSize: 13,
                color: AppTheme.subtleMuted,
              ),
            ),
          ],
        ],
      ),
    );
  }
}

/// Small action button matching the MAUI `SmallButton()` helper.
class SmallActionButton extends StatelessWidget {
  final String text;
  final VoidCallback onPressed;
  final Color backgroundColor;
  final Color textColor;

  const SmallActionButton({
    super.key,
    required this.text,
    required this.onPressed,
    this.backgroundColor = AppTheme.subtleMuted,
    this.textColor = Colors.white,
  });

  @override
  Widget build(BuildContext context) {
    return ElevatedButton(
      onPressed: onPressed,
      style: ElevatedButton.styleFrom(
        backgroundColor: backgroundColor,
        foregroundColor: textColor,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(6)),
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 7),
        minimumSize: Size.zero,
        textStyle: const TextStyle(fontSize: 13),
      ),
      child: Text(text),
    );
  }
}

/// Primary action button (blue) matching the MAUI `PrimaryButton()` helper.
class PrimaryActionButton extends StatelessWidget {
  final String text;
  final VoidCallback onPressed;
  final Color backgroundColor;

  const PrimaryActionButton({
    super.key,
    required this.text,
    required this.onPressed,
    this.backgroundColor = const Color(0xFF2563EB),
  });

  @override
  Widget build(BuildContext context) {
    return ElevatedButton(
      onPressed: onPressed,
      style: ElevatedButton.styleFrom(
        backgroundColor: backgroundColor,
        foregroundColor: Colors.white,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(6)),
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 7),
        minimumSize: Size.zero,
      ),
      child: Text(text),
    );
  }
}

/// Empty state label.
class EmptyStateLabel extends StatelessWidget {
  final String text;

  const EmptyStateLabel({super.key, required this.text});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 28, horizontal: 12),
      child: Center(
        child: Text(
          text,
          textAlign: TextAlign.center,
          style: const TextStyle(color: AppTheme.subtleMuted),
        ),
      ),
    );
  }
}
