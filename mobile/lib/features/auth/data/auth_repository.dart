import 'package:dio/dio.dart';

import '../../../core/network/api_error_mapper.dart';
import '../../../core/storage/token_storage.dart';
import '../models/auth_models.dart';

class AuthRepository {
  AuthRepository(this._dio, this._tokenStorage);

  final Dio _dio;
  final TokenStorage _tokenStorage;

  Future<AuthSession> login({required String email, required String password}) =>
      _authenticate('/api/auth/login', {'email': email, 'password': password});

  Future<AuthSession> register({
    required String email,
    required String password,
    required String firstName,
    required String lastName,
  }) =>
      _authenticate('/api/auth/register', {
        'email': email,
        'password': password,
        'firstName': firstName,
        'lastName': lastName,
      });

  Future<AuthSession> _authenticate(String path, Map<String, dynamic> body) async {
    try {
      final response = await _dio.post(path, data: body);
      final session = AuthSession.fromJson(response.data as Map<String, dynamic>);
      await _tokenStorage.saveTokens(accessToken: session.accessToken, refreshToken: session.refreshToken);
      return session;
    } on DioException catch (error) {
      throw toApiException(error);
    }
  }

  Future<CurrentUser?> fetchCurrentUser() async {
    try {
      final response = await _dio.get('/api/auth/me');
      return CurrentUser.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      if (error.response?.statusCode == 401) return null;
      throw toApiException(error);
    }
  }

  Future<void> logout() async {
    final refreshToken = await _tokenStorage.readRefreshToken();
    if (refreshToken != null) {
      try {
        await _dio.post('/api/auth/revoke', data: {'refreshToken': refreshToken});
      } on DioException {
        // ignore network errors on logout, local session is cleared regardless
      }
    }
    await _tokenStorage.clear();
  }
}
