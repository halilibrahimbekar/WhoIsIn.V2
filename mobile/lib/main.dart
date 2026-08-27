import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'core/router/app_router.dart';
import 'core/theme/app_theme.dart';
import 'features/auth/application/auth_controller.dart';
import 'core/localization/app_localizations.dart';

void main() {
  runApp(const ProviderScope(child: WhoIsInApp()));
}

class WhoIsInApp extends ConsumerStatefulWidget {
  const WhoIsInApp({super.key});

  @override
  ConsumerState<WhoIsInApp> createState() => _WhoIsInAppState();
}

class _WhoIsInAppState extends ConsumerState<WhoIsInApp> {
  final _language = LanguageNotifier();

  @override
  Widget build(BuildContext context) {
    // Ensure auth bootstrap starts as soon as the app launches.
    ref.watch(authControllerProvider);
    final router = ref.watch(routerProvider);

    return AppLocalizations(
      notifier: _language,
      child: MaterialApp.router(title: 'WhoIsIn', routerConfig: router, theme: AppTheme.light()),
    );
  }

  @override
  void dispose() {
    _language.dispose();
    super.dispose();
  }
}
