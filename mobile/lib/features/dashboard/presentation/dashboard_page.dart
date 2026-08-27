import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../auth/application/auth_controller.dart';
import '../../events/application/events_providers.dart';
import '../../events/models/event_models.dart';
import '../../../core/localization/app_localizations.dart';

class DashboardPage extends ConsumerWidget {
  const DashboardPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final strings = AppLocalizations.of(context);
    final summaryAsync = ref.watch(eventSummaryProvider);
    final currentUser = ref.watch(authControllerProvider).valueOrNull;

    return Scaffold(
      appBar: AppBar(
        title: Text(strings.text('dashboard')),
        actions: [
          IconButton(
            icon: const Icon(Icons.mail_outline),
            onPressed: () => context.push('/invites'),
          ),
          IconButton(
            icon: const Icon(Icons.notifications_none),
            onPressed: () => context.push('/notifications'),
          ),
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () => ref.read(authControllerProvider.notifier).logout(),
          ),
          PopupMenuButton<String>(
            tooltip: strings.text('language'),
            onSelected: strings.setLanguage,
            itemBuilder: (_) => [
              PopupMenuItem(value: 'tr', child: Text(strings.text('turkish'))),
              PopupMenuItem(value: 'en', child: Text(strings.text('english'))),
            ],
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => context.push('/events'),
        label: Text(strings.text('events')),
        icon: const Icon(Icons.event),
      ),
      body: RefreshIndicator(
        onRefresh: () => ref.refresh(eventSummaryProvider.future),
        child: summaryAsync.when(
          data: (summary) => _DashboardBody(summary: summary, welcomeName: currentUser?.firstName),
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (error, _) => ListView(
            physics: const AlwaysScrollableScrollPhysics(),
            children: [
              const SizedBox(height: 80),
              Center(child: Text(error.toString())),
              const SizedBox(height: 12),
              Center(
                child: FilledButton(
                  onPressed: () => ref.invalidate(eventSummaryProvider),
                  child: Text(strings.text('retry')),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _DashboardBody extends StatelessWidget {
  const _DashboardBody({required this.summary, required this.welcomeName});

  final EventSummary summary;
  final String? welcomeName;

  @override
  Widget build(BuildContext context) {
    final strings = AppLocalizations.of(context);
    final dateFormat = DateFormat('d MMM, HH:mm');

    return ListView(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.all(16),
      children: [
        if (welcomeName != null) Text('Merhaba, $welcomeName', style: Theme.of(context).textTheme.titleLarge),
        const SizedBox(height: 16),
        Row(
          children: [
            Expanded(child: _MetricCard(label: 'Aktif Etkinlik', value: summary.activeEventCount.toString())),
            const SizedBox(width: 8),
            Expanded(child: _MetricCard(label: 'Kabul Eden', value: summary.acceptedGuestCount.toString())),
          ],
        ),
        const SizedBox(height: 8),
        Row(
          children: [
            Expanded(child: _MetricCard(label: 'Bekleme Listesi', value: summary.waitlistCount.toString())),
            Expanded(child: _MetricCard(label: 'Doluluk', value: '${summary.fillRate.toStringAsFixed(0)}%')),
          ],
        ),
        const SizedBox(height: 24),
        Text(strings.text('upcoming'), style: Theme.of(context).textTheme.titleMedium),
        const SizedBox(height: 8),
        if (summary.upcomingEvents.isEmpty) Text(strings.text('noUpcoming')),
        ...summary.upcomingEvents.map((event) => Card(
              child: ListTile(
                title: Text(event.title),
                subtitle: Text(dateFormat.format(event.startAtUtc.toLocal())),
                trailing: Text('${event.acceptedCount}/${event.capacity}'),
                onTap: () => context.push('/events/${event.id}'),
              ),
            )),
      ],
    );
  }
}

class _MetricCard extends StatelessWidget {
  const _MetricCard({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(value, style: Theme.of(context).textTheme.headlineSmall),
            Text(label, style: Theme.of(context).textTheme.bodySmall),
          ],
        ),
      ),
    );
  }
}
