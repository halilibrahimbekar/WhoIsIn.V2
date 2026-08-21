import 'package:dio/dio.dart';

import 'api_exception.dart';

/// Converts a [DioException] into a user-facing [ApiException].
ApiException toApiException(DioException error) {
  final response = error.response;
  if (response?.data is Map && (response!.data as Map).containsKey('title')) {
    return ApiException(response.data['title'] as String, statusCode: response.statusCode);
  }
  if (response?.data is String && (response!.data as String).isNotEmpty) {
    return ApiException(response.data as String, statusCode: response.statusCode);
  }
  switch (error.type) {
    case DioExceptionType.connectionTimeout:
    case DioExceptionType.sendTimeout:
    case DioExceptionType.receiveTimeout:
      return ApiException('Sunucuya ulasilamadi, lutfen tekrar deneyin.');
    case DioExceptionType.connectionError:
      return ApiException('Internet baglantisi yok veya sunucuya erisilemiyor.');
    default:
      return ApiException(
        response?.statusCode == null ? 'Beklenmeyen bir hata olustu.' : 'Islem basarisiz oldu.',
        statusCode: response?.statusCode,
      );
  }
}
