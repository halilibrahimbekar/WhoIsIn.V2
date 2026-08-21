import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../application/events_providers.dart';
import '../models/event_models.dart';

class EventsListPage extends ConsumerWidget {
  const EventsListPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final eventsAsync = ref.watch(eventsListProvider);

    return Scaffold(
      appBar: AppBar(title: const Text('Etkinlikler')),
      floatingActionButton: FloatingActionButton(
        onPressed: () => context.push('/events/new'),
        child: const Icon(Icons.add),
      ),
      body: RefreshIndicator(
        onRefresh: () => ref.refresh(eventsListProvider.future),
        child: eventsAsync.when(
          data: (events) => _EventsListView(events: events),
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (error, _) => _ErrorView(message: error.toString(), onRetry: () => ref.invalidate(eventsListProvider)),
        ),
      ),
    );
  }
}

class _EventsListView extends StatelessWidget {
  const _EventsListView({required this.events});

  final List<EventListItem> events;

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
    return ListView.separated(
      physics: const AlwaysScrollableScrollPhysics(),
      itemCount: events.length,
      separatorBuilder: (context, index) => const Divider(height: 1),
      itemBuilder: (context, index) {
        final event = events[index];
        return ListTile(
          title: Text(event.title),
          subtitle: Text('${dateFormat.format(event.startAtUtc.toLocal())} - ${event.status}'),
          trailing: Text(event.categoryName ?? ''),
          onTap: () => context.push('/events/${event.id}'),
        );
      },
    );
  }
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
