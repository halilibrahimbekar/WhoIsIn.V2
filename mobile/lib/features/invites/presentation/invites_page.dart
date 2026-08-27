import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import '../../auth/application/auth_controller.dart';
import '../../events/application/events_providers.dart';
import '../../events/models/event_models.dart';
import '../../../core/localization/app_localizations.dart';

final invitesPageProvider = FutureProvider.autoDispose<List<EventListItem>>((ref) async => ref.read(eventsRepositoryProvider).getAll());

class InvitesPage extends ConsumerWidget {
  const InvitesPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final user = ref.watch(authControllerProvider).valueOrNull;
    final strings = AppLocalizations.of(context);
    return Scaffold(appBar: AppBar(title: Text(strings.text('invites'))), body: ref.watch(invitesPageProvider).when(
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (error, _) => Center(child: Text(error.toString())),
      data: (events) {
        final own = events.where((event) => event.organizerId == user?.id).toList();
        final invited = events.where((event) => event.organizerId != user?.id).toList();
        return RefreshIndicator(onRefresh: () => ref.refresh(invitesPageProvider.future), child: ListView(
          padding: const EdgeInsets.all(16), children: [
            _InviteEventsSection(title: 'Davet Ettikleriniz', events: own),
            const SizedBox(height: 24),
            _InviteEventsSection(title: 'Davet Edildikleriniz', events: invited),
          ]));
      }));
  }
}

class _InviteEventsSection extends StatelessWidget {
  const _InviteEventsSection({required this.title, required this.events});
  final String title;
  final List<EventListItem> events;
  @override
  Widget build(BuildContext context) => Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
    Text(title, style: Theme.of(context).textTheme.titleMedium), const SizedBox(height: 8),
    if (events.isEmpty) const Text('Davet yok.'),
    ...events.map((event) => Card(child: ListTile(title: Text(event.title), subtitle: Text(DateFormat('d MMM yyyy, HH:mm').format(event.startAtUtc.toLocal())), onTap: () => context.push('/events/${event.id}'))))
  ]);
}