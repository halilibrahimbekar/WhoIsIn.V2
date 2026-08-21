/// API base URL, override at build/run time with:
/// --dart-define=API_BASE_URL=https://api.example.com
class ApiConfig {
  static const String baseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'https://localhost:7042',
  );
}
