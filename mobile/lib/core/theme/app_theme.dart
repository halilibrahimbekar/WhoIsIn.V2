import 'package:flutter/material.dart';

/// Green palette shared with the web frontend.
class AppTheme {
  AppTheme._();

  static const Color background = Color(0xFFF2FAF4);
  static const Color surface = Color(0xFFFBFEFB);
  static const Color textDark = Color(0xFF173B2A);
  static const Color textMuted = Color(0xFF3F7655);
  static const Color primary = Color(0xFF3F9362);
  static const Color secondary = Color(0xFFA8D3B5);
  static const Color borderColor = Color(0x4A3F9362);
  static const Color darkButton = Color(0xFF173B2A);
  static const Color creamText = Color(0xFFF4FAF5);

  static ThemeData light() {
    final colorScheme = ColorScheme.fromSeed(
      seedColor: primary,
      brightness: Brightness.light,
      primary: primary,
      secondary: secondary,
      surface: surface,
    );

    return ThemeData(
      useMaterial3: true,
      colorScheme: colorScheme,
      scaffoldBackgroundColor: background,
      appBarTheme: const AppBarTheme(
        backgroundColor: background,
        foregroundColor: textDark,
        elevation: 0,
        centerTitle: false,
        titleTextStyle: TextStyle(color: textDark, fontSize: 20, fontWeight: FontWeight.w700),
      ),
      cardTheme: CardThemeData(
        color: surface,
        elevation: 0,
        margin: EdgeInsets.zero,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(18),
          side: const BorderSide(color: borderColor),
        ),
      ),
      dividerTheme: const DividerThemeData(color: borderColor, thickness: 1),
      textTheme: const TextTheme(
        headlineSmall: TextStyle(color: textDark, fontWeight: FontWeight.w700),
        titleLarge: TextStyle(color: textDark, fontWeight: FontWeight.w700),
        titleMedium: TextStyle(color: textDark, fontWeight: FontWeight.w700),
        bodyMedium: TextStyle(color: textDark),
        bodySmall: TextStyle(color: textMuted),
      ),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          backgroundColor: darkButton,
          foregroundColor: creamText,
          shape: const StadiumBorder(),
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
          textStyle: const TextStyle(fontWeight: FontWeight.w700),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          foregroundColor: textDark,
          side: const BorderSide(color: borderColor),
          shape: const StadiumBorder(),
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
          textStyle: const TextStyle(fontWeight: FontWeight.w700),
        ),
      ),
      floatingActionButtonTheme: const FloatingActionButtonThemeData(
        backgroundColor: primary,
        foregroundColor: Colors.white,
      ),
      chipTheme: ChipThemeData(
        backgroundColor: secondary.withValues(alpha: 0.22),
        labelStyle: const TextStyle(color: textDark, fontWeight: FontWeight.w600),
        side: const BorderSide(color: borderColor),
        shape: const StadiumBorder(),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: surface,
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: borderColor),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: borderColor),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
          borderSide: const BorderSide(color: primary, width: 2),
        ),
        labelStyle: const TextStyle(color: textMuted),
      ),
      listTileTheme: const ListTileThemeData(textColor: textDark, iconColor: textMuted),
    );
  }
}
