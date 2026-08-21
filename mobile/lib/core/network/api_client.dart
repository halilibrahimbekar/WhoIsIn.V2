import 'dart:async';

import 'package:dio/dio.dart';

import 'api_config.dart';
import '../storage/token_storage.dart';

/// Wraps [Dio] with base URL, auth header injection and 401 refresh-and-retry.
class ApiClient {
  ApiClient(this._tokenStorage) {
    _dio = Dio(BaseOptions(baseUrl: ApiConfig.baseUrl));
    _refreshDio = Dio(BaseOptions(baseUrl: ApiConfig.baseUrl));
    _dio.interceptors.add(
      InterceptorsWrapper(onRequest: _onRequest, onError: _onError),
    );
  }

  final TokenStorage _tokenStorage;
  late final Dio _dio;
  late final Dio _refreshDio;

  bool _isRefreshing = false;
  final List<Completer<void>> _pendingRequests = [];

  /// Called when the refresh token is no longer valid; caller should log out.
  void Function()? onSessionExpired;

  Dio get dio => _dio;

  static const _authPaths = ['/api/auth/login', '/api/auth/register', '/api/auth/refresh'];

  bool _isAuthPath(String path) => _authPaths.any(path.contains);

  Future<void> _onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    if (!_isAuthPath(options.path)) {
      final token = await _tokenStorage.readAccessToken();
      if (token != null) {
        options.headers['Authorization'] = 'Bearer $token';
      }
    }
    handler.next(options);
  }

  Future<void> _onError(DioException err, ErrorInterceptorHandler handler) async {
    final statusCode = err.response?.statusCode;
    final requestPath = err.requestOptions.path;

    if (statusCode != 401 || _isAuthPath(requestPath)) {
      handler.next(err);
      return;
    }

    if (_isRefreshing) {
      final completer = Completer<void>();
      _pendingRequests.add(completer);
      await completer.future;
      try {
        handler.resolve(await _retry(err.requestOptions));
      } catch (_) {
        handler.next(err);
      }
      return;
    }

    _isRefreshing = true;
    try {
      final refreshToken = await _tokenStorage.readRefreshToken();
      if (refreshToken == null) throw Exception('No refresh token');

      final response = await _refreshDio.post(
        '/api/auth/refresh',
        data: {'refreshToken': refreshToken},
      );
      await _tokenStorage.saveTokens(
        accessToken: response.data['accessToken'] as String,
        refreshToken: response.data['refreshToken'] as String,
      );

      _isRefreshing = false;
      for (final completer in _pendingRequests) {
        completer.complete();
      }
      _pendingRequests.clear();

      handler.resolve(await _retry(err.requestOptions));
    } catch (_) {
      _isRefreshing = false;
      for (final completer in _pendingRequests) {
        completer.complete();
      }
      _pendingRequests.clear();
      await _tokenStorage.clear();
      onSessionExpired?.call();
      handler.next(err);
    }
  }

  Future<Response<dynamic>> _retry(RequestOptions requestOptions) async {
    final token = await _tokenStorage.readAccessToken();
    final options = Options(method: requestOptions.method, headers: {
      ...requestOptions.headers,
      if (token != null) 'Authorization': 'Bearer $token',
    });
    return _dio.request(
      requestOptions.path,
      data: requestOptions.data,
      queryParameters: requestOptions.queryParameters,
      options: options,
    );
  }
}
