import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../application/events_providers.dart';
import '../models/event_models.dart';
import '../../auth/application/auth_controller.dart';

class EventsListPage extends ConsumerWidget {
  const EventsListPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final eventsAsync = ref.watch(eventsListProvider);
    final currentUser = ref.watch(authControllerProvider).valueOrNull;

    return Scaffold(
      appBar: AppBar(title: const Text('Etkinlikler')),
      floatingActionButton: FloatingActionButton(
        onPressed: () => context.push('/events/new'),
        child: const Icon(Icons.add),
      ),
      body: RefreshIndicator(
        onRefresh: () => ref.refresh(eventsListProvider.future),
        child: eventsAsync.when(
          data: (events) => _EventsListView(events: events, currentUserId: currentUser?.id),
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (error, _) => _ErrorView(message: error.toString(), onRetry: () => ref.invalidate(eventsListProvider)),
        ),
      ),
    );
  }
}

class _EventsListView extends StatelessWidget {
  const _EventsListView({required this.events, required this.currentUserId});

  final List<EventListItem> events;
  final String? currentUserId;

  @override
  Widget build(BuildContext context) {
    if (events.isEmpty) {
      return LayoutBuilder(
        builder: (context, constraints) => SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          child: SizedBox(
            height: constraints.maxHeight,
            child: const Center(child: Text('Henuz etkinlik yok.')),
          ),
        ),
      );
    }

    final dateFormat = DateFormat('d MMM yyyy, HH:mm');
    final ownedEvents = events.where((event) => event.organizerId == currentUserId).toList();
    final otherEvents = events.where((event) => event.organizerId != currentUserId).toList();
    return ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.all(16),
      children: [
        _EventSection(title: 'Kendi Etkinlikleriniz', events: ownedEvents, dateFormat: dateFormat),
        const SizedBox(height: 20),
        _EventSection(title: 'Davet Edildiğiniz Etkinlikler', events: otherEvents, dateFormat: dateFormat),
      ],
    );
  }
}

class _EventSection extends StatelessWidget {
  const _EventSection({required this.title, required this.events, required this.dateFormat});
  final String title;
  final List<EventListItem> events;
  final DateFormat dateFormat;

  @override
  Widget build(BuildContext context) => Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Text(title, style: Theme.of(context).textTheme.titleMedium),
        const SizedBox(height: 8),
        if (events.isEmpty) const Text('Etkinlik yok.'),
        ...events.map((event) => Card(child: ListTile(
              contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              title: Text(event.title, style: const TextStyle(fontWeight: FontWeight.w700)),
              subtitle: Text('${dateFormat.format(event.startAtUtc.toLocal())} - ${event.status}'),
              trailing: Text(event.categoryName ?? ''),
              onTap: () => context.push('/events/${event.id}'),
            )))
      ]);
    }

class _ErrorView extends StatelessWidget {
  const _ErrorView({required this.message, required this.onRetry});

  final String message;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(
      builder: (context, constraints) => SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        child: SizedBox(
          height: constraints.maxHeight,
          child: Center(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Text(message, textAlign: TextAlign.center),
                const SizedBox(height: 12),
                FilledButton(onPressed: onRetry, child: const Text('Tekrar dene')),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
