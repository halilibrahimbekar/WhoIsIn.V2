import 'package:flutter/material.dart';

class AppLocalizations extends InheritedNotifier<LanguageNotifier> {
  const AppLocalizations({super.key, required super.child, required LanguageNotifier notifier}) : super(notifier: notifier);

  static LanguageNotifier of(BuildContext context) => context.dependOnInheritedWidgetOfExactType<AppLocalizations>()!.notifier!;
}

class LanguageNotifier extends ChangeNotifier {
  Locale locale = const Locale('tr');

  void setLanguage(String languageCode) {
    locale = Locale(languageCode);
    notifyListeners();
  }

  String text(String key) => _translations[locale.languageCode]?[key] ?? _translations['en']![key] ?? key;
}

class LanguageMenu extends StatelessWidget {
  const LanguageMenu({super.key});

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    return PopupMenuButton<String>(
      tooltip: strings.text('language'),
      onSelected: strings.setLanguage,
      itemBuilder: (_) => [
        PopupMenuItem(value: 'tr', child: Text(strings.text('turkish'))),
        PopupMenuItem(value: 'en', child: Text(strings.text('english'))),
      ],
      icon: const Icon(Icons.language),
    );
  }
}

const _translations = <String, Map<String, String>>{
  'tr': {
    'dashboard': 'Panel', 'events': 'Etkinlikler', 'invites': 'Davetler', 'notifications': 'Bildirimler',
    'signOut': 'Çıkış yap', 'language': 'Dil', 'turkish': 'Türkçe', 'english': 'English',
    'retry': 'Tekrar dene', 'noEvents': 'Henüz etkinlik yok.', 'upcoming': 'Yaklaşan Etkinlikler',
    'noUpcoming': 'Yaklaşan etkinlik yok.', 'login': 'Giriş Yap', 'register': 'Kayıt Ol',
  },
  'en': {
    'dashboard': 'Dashboard', 'events': 'Events', 'invites': 'Invites', 'notifications': 'Notifications',
    'signOut': 'Sign out', 'language': 'Language', 'turkish': 'Türkçe', 'english': 'English',
    'retry': 'Try again', 'noEvents': 'No events yet.', 'upcoming': 'Upcoming Events',
    'noUpcoming': 'No upcoming events.', 'login': 'Sign in', 'register': 'Create account',
  },
};