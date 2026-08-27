import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../features/auth/application/auth_controller.dart';
import '../../features/auth/presentation/login_page.dart';
import '../../features/auth/presentation/register_page.dart';
import '../../features/dashboard/presentation/dashboard_page.dart';
import '../../features/events/models/event_models.dart';
import '../../features/events/presentation/event_detail_page.dart';
import '../../features/events/presentation/event_form_page.dart';
import '../../features/events/presentation/events_list_page.dart';
import '../../features/invites/presentation/invites_page.dart';
import '../../features/notifications/presentation/notifications_page.dart';

final routerProvider = Provider<GoRouter>((ref) {
  final authNotifier = ValueNotifier<int>(0);
  ref.listen(authControllerProvider, (previous, next) => authNotifier.value++);
  ref.onDispose(authNotifier.dispose);

  return GoRouter(
    initialLocation: '/',
    refreshListenable: authNotifier,
    redirect: (context, state) {
      final authState = ref.read(authControllerProvider);
      final isBootstrapping = authState.isLoading && !authState.hasValue;
      if (isBootstrapping) return null;

      final isSignedIn = authState.valueOrNull != null;
      final isAuthRoute = state.matchedLocation == '/login' || state.matchedLocation == '/register';

      if (!isSignedIn && !isAuthRoute) return '/login';
      if (isSignedIn && isAuthRoute) return '/';
      return null;
    },
    routes: [
      GoRoute(path: '/login', builder: (context, state) => const LoginPage()),
      GoRoute(path: '/register', builder: (context, state) => const RegisterPage()),
      GoRoute(path: '/', builder: (context, state) => const DashboardPage()),
      GoRoute(path: '/events', builder: (context, state) => const EventsListPage()),
      GoRoute(path: '/invites', builder: (context, state) => const InvitesPage()),
      GoRoute(path: '/notifications', builder: (context, state) => const NotificationsPage()),
      GoRoute(path: '/events/new', builder: (context, state) => const EventFormPage()),
      GoRoute(
        path: '/events/:id',
        builder: (context, state) => EventDetailPage(eventId: state.pathParameters['id']!),
      ),
      GoRoute(
        path: '/events/:id/edit',
        builder: (context, state) => EventFormPage(existingEvent: state.extra as EventDetail?),
      ),
    ],
  );
});
