import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../../core/network/api_exception.dart';
import '../../auth/application/auth_controller.dart';
import '../application/events_providers.dart';
import '../models/event_models.dart';

class EventDetailPage extends ConsumerWidget {
  const EventDetailPage({super.key, required this.eventId});

  final String eventId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final detailAsync = ref.watch(eventDetailProvider(eventId));

    return Scaffold(
      appBar: AppBar(title: const Text('Etkinlik Detayi')),
      body: detailAsync.when(
        data: (event) => _EventDetailBody(event: event),
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, _) => Center(child: Text(error.toString())),
      ),
    );
  }
}

String _participantStatusLabel(String status) => switch (status) {
      'Confirmed' => 'Kabul edildi',
      'Waitlisted' => 'Bekleme listesinde',
      'Declined' => 'Reddedildi',
      'PendingApproval' => 'Onay bekliyor',
      _ => status,
    };

class _EventDetailBody extends ConsumerWidget {
  const _EventDetailBody({required this.event});

  final EventDetail event;

  Future<void> _runAction(BuildContext context, WidgetRef ref, Future<void> Function() action) async {
    String message;
    try {
      await action();
      ref.invalidate(eventDetailProvider(event.id));
      ref.invalidate(eventInvitesProvider(event.id));
      ref.invalidate(eventParticipantsProvider(event.id));
      message = 'Islem basarili.';
    } catch (error) {
      message = error is ApiException ? error.message : 'Islem basarisiz oldu.';
    }
    if (!context.mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final currentUser = ref.watch(authControllerProvider).valueOrNull;
    final isOrganizer = currentUser != null && currentUser.id == event.organizerId;
    final dateFormat = DateFormat('d MMM yyyy, HH:mm');

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Text(event.title, style: Theme.of(context).textTheme.headlineSmall),
        const SizedBox(height: 8),
        Text('${dateFormat.format(event.startAtUtc.toLocal())} - ${event.status}'),
        if (event.locationName != null) ...[
          const SizedBox(height: 4),
          Text(event.locationName!),
        ],
        if (event.description != null) ...[
          const SizedBox(height: 12),
          Text(event.description!),
        ],
        const SizedBox(height: 16),
        Text('Kapasite: ${event.capacity}'),
        const SizedBox(height: 24),
        if (isOrganizer) _OrganizerActions(event: event, onAction: (action) => _runAction(context, ref, action)),
        if (!isOrganizer && event.canRespond)
          Row(
            children: [
              FilledButton(
                onPressed: () => _runAction(context, ref, () => ref.read(eventsRepositoryProvider).rsvp(event.id, 'Accepted')),
                child: const Text('Request Participant'),
              ),
              const SizedBox(width: 12),
              OutlinedButton(
                onPressed: () => _runAction(context, ref, () => ref.read(eventsRepositoryProvider).rsvp(event.id, 'Declined')),
                child: const Text('Reddet'),
              ),
            ],
          ),
        if (!isOrganizer && !event.canRespond && event.myParticipantStatus != null)
          Chip(label: Text('Katilim durumun: ${_participantStatusLabel(event.myParticipantStatus!)}')),
        const SizedBox(height: 24),
        if (isOrganizer) ...[
          Text('Davetler', style: Theme.of(context).textTheme.titleMedium),
          const SizedBox(height: 8),
          _InviteSection(eventId: event.id, onAction: (action) => _runAction(context, ref, action)),
          const SizedBox(height: 24),
          Text('Katilimcilar', style: Theme.of(context).textTheme.titleMedium),
          const SizedBox(height: 8),
          _ParticipantsSection(eventId: event.id, onAction: (action) => _runAction(context, ref, action)),
        ],
      ],
    );
  }
}

class _OrganizerActions extends StatelessWidget {
  const _OrganizerActions({required this.event, required this.onAction});

  final EventDetail event;
  final void Function(Future<void> Function()) onAction;

  @override
  Widget build(BuildContext context) {
    return Consumer(
      builder: (context, ref, _) => Wrap(
        spacing: 8,
        children: [
          FilledButton.tonal(
            onPressed: () => context.push('/events/${event.id}/edit', extra: event),
            child: const Text('Duzenle'),
          ),
          if (event.status == 'Draft')
            FilledButton(
              onPressed: () => onAction(() => ref.read(eventsRepositoryProvider).updateStatus(event.id, 'Published')),
              child: const Text('Yayinla'),
            ),
          if (event.status == 'Published') ...[
            OutlinedButton(
              onPressed: () => onAction(() => ref.read(eventsRepositoryProvider).updateStatus(event.id, 'Cancelled')),
              child: const Text('Iptal Et'),
            ),
            OutlinedButton(
              onPressed: () => onAction(() => ref.read(eventsRepositoryProvider).updateStatus(event.id, 'Completed')),
              child: const Text('Tamamlandi Isaretle'),
            ),
          ],
        ],
      ),
    );
  }
}

class _InviteSection extends ConsumerStatefulWidget {
  const _InviteSection({required this.eventId, required this.onAction});

  final String eventId;
  final void Function(Future<void> Function()) onAction;

  @override
  ConsumerState<_InviteSection> createState() => _InviteSectionState();
}

class _InviteSectionState extends ConsumerState<_InviteSection> {
  final _emailController = TextEditingController();

  @override
  void dispose() {
    _emailController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final invitesAsync = ref.watch(eventInvitesProvider(widget.eventId));

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Expanded(
              child: TextField(
                controller: _emailController,
                decoration: const InputDecoration(labelText: 'Davet edilecek e-posta'),
              ),
            ),
            const SizedBox(width: 8),
            FilledButton(
              onPressed: () {
                final email = _emailController.text.trim();
                if (email.isEmpty) return;
                widget.onAction(() => ref.read(eventsRepositoryProvider).invite(widget.eventId, [email]));
                _emailController.clear();
              },
              child: const Text('Davet Et'),
            ),
          ],
        ),
        const SizedBox(height: 8),
        invitesAsync.when(
          data: (invites) => Column(
            children: invites
                .map((invite) => ListTile(
                      dense: true,
                      title: Text(invite.email),
                      trailing: Text(invite.status),
                    ))
                .toList(),
          ),
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (error, _) => Text(error.toString()),
        ),
      ],
    );
  }
}

class _ParticipantsSection extends ConsumerWidget {
  const _ParticipantsSection({required this.eventId, required this.onAction});

  final String eventId;
  final void Function(Future<void> Function()) onAction;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final participantsAsync = ref.watch(eventParticipantsProvider(eventId));

    return participantsAsync.when(
      data: (participants) => Column(
        children: [
          ...participants.map((participant) => ListTile(
                dense: true,
                title: Text(participant.displayName),
                subtitle: Text(participant.email),
                trailing: Text(participant.status),
              )),
          if (participants.any((item) => item.status == 'Waitlisted'))
            TextButton(
              onPressed: () => onAction(() => ref.read(eventsRepositoryProvider).promoteWaitlisted(eventId)),
              child: const Text('Bekleme listesinden yukselt'),
            ),
        ],
      ),
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (error, _) => Text(error.toString()),
    );
  }
}
