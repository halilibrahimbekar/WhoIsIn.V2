import '../../../core/localization/app_localizations.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../core/providers/core_providers.dart';

class NotificationItem {
  const NotificationItem({required this.id, required this.eventId, required this.eventTitle, required this.message});
  factory NotificationItem.fromJson(Map<String, dynamic> json) => NotificationItem(
        id: json['id'] as String,
        eventId: json['eventId'] as String,
        eventTitle: json['eventTitle'] as String,
        message: json['message'] as String,
      );
  final String id;
  final String eventId;
  final String eventTitle;
  final String message;
}

final notificationsProvider = FutureProvider.autoDispose<List<NotificationItem>>((ref) async {
  final response = await ref.read(apiClientProvider).dio.get('/api/notifications');
  return (response.data as List<dynamic>)
      .map((item) => NotificationItem.fromJson(item as Map<String, dynamic>))
      .toList();
});

class NotificationsPage extends ConsumerWidget {
  const NotificationsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) => Scaffold(
        appBar: AppBar(title: Text(AppLocalizations.of(context).text('notifications'))),
        body: ref.watch(notificationsProvider).when(
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (error, _) => Center(child: Text(error.toString())),
              data: (items) => RefreshIndicator(
                onRefresh: () => ref.refresh(notificationsProvider.future),
                child: items.isEmpty
                    ? ListView(children: const [SizedBox(height: 120), Center(child: Text('Yeni bildirim yok.'))])
                    : ListView.builder(
                        physics: const AlwaysScrollableScrollPhysics(),
                        itemCount: items.length,
                        itemBuilder: (context, index) {
                          final item = items[index];
                          return ListTile(
                            title: Text(item.eventTitle),
                            subtitle: Text(item.message),
                            leading: const Icon(Icons.notifications_none),
                            onTap: () => context.push('/events/${item.eventId}'),
                          );
                        },
                      ),
              ),
            ),
      );
}
