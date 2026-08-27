import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import 'core/router/app_router.dart';
import 'core/theme/app_theme.dart';
import 'features/auth/application/auth_controller.dart';

void main() {
  runApp(const ProviderScope(child: WhoIsInApp()));
}

class WhoIsInApp extends ConsumerWidget {
  const WhoIsInApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    // Ensure auth bootstrap starts as soon as the app launches.
    ref.watch(authControllerProvider);
    final router = ref.watch(routerProvider);

    return MaterialApp.router(
      title: 'WhoIsIn',
      routerConfig: router,
      theme: AppTheme.light(),
    );
  }
}
