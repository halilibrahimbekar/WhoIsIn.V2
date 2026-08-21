class CurrentUser {
  CurrentUser({
    required this.id,
    required this.email,
    required this.firstName,
    required this.lastName,
    required this.createdAtUtc,
  });

  factory CurrentUser.fromJson(Map<String, dynamic> json) => CurrentUser(
        id: json['id'] as String,
        email: json['email'] as String,
        firstName: json['firstName'] as String,
        lastName: json['lastName'] as String,
        createdAtUtc: DateTime.parse(json['createdAtUtc'] as String),
      );

  final String id;
  final String email;
  final String firstName;
  final String lastName;
  final DateTime createdAtUtc;

  String get displayName => '$firstName $lastName';
}

class AuthSession {
  AuthSession({
    required this.accessToken,
    required this.accessTokenExpiresAtUtc,
    required this.refreshToken,
    required this.refreshTokenExpiresAtUtc,
    required this.user,
  });

  factory AuthSession.fromJson(Map<String, dynamic> json) => AuthSession(
        accessToken: json['accessToken'] as String,
        accessTokenExpiresAtUtc: DateTime.parse(json['accessTokenExpiresAtUtc'] as String),
        refreshToken: json['refreshToken'] as String,
        refreshTokenExpiresAtUtc: DateTime.parse(json['refreshTokenExpiresAtUtc'] as String),
        user: CurrentUser.fromJson(json['user'] as Map<String, dynamic>),
      );

  final String accessToken;
  final DateTime accessTokenExpiresAtUtc;
  final String refreshToken;
  final DateTime refreshTokenExpiresAtUtc;
  final CurrentUser user;
}
