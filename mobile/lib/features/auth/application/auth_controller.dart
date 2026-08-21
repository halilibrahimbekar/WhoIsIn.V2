import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/providers/core_providers.dart';
import '../data/auth_repository.dart';
import '../models/auth_models.dart';

final authRepositoryProvider = Provider<AuthRepository>(
  (ref) => AuthRepository(ref.watch(apiClientProvider).dio, ref.watch(tokenStorageProvider)),
);

/// Holds the current session state: null (bootstrapping), a user (signed in) or
/// none (signed out). Errors from login/register are surfaced by the caller.
class AuthController extends AsyncNotifier<CurrentUser?> {
  @override
  Future<CurrentUser?> build() async {
    ref.watch(apiClientProvider).onSessionExpired = () {
      state = const AsyncData(null);
    };
    final token = await ref.read(tokenStorageProvider).readAccessToken();
    if (token == null) return null;
    return ref.read(authRepositoryProvider).fetchCurrentUser();
  }

  Future<void> login({required String email, required String password}) async {
    state = const AsyncLoading();
    state = await AsyncValue.guard(() async {
      final session = await ref.read(authRepositoryProvider).login(email: email, password: password);
      return session.user;
    });
  }

  Future<void> register({
    required String email,
    required String password,
    required String firstName,
    required String lastName,
  }) async {
    state = const AsyncLoading();
    state = await AsyncValue.guard(() async {
      final session = await ref.read(authRepositoryProvider).register(
            email: email,
            password: password,
            firstName: firstName,
            lastName: lastName,
          );
      return session.user;
    });
  }

  Future<void> logout() async {
    await ref.read(authRepositoryProvider).logout();
    state = const AsyncData(null);
  }
}

final authControllerProvider = AsyncNotifierProvider<AuthController, CurrentUser?>(AuthController.new);
