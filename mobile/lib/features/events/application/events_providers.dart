import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/providers/core_providers.dart';
import '../data/events_repository.dart';
import '../models/event_models.dart';

final eventsRepositoryProvider = Provider<EventsRepository>(
  (ref) => EventsRepository(ref.watch(apiClientProvider).dio),
);

final eventsListProvider = FutureProvider.autoDispose<List<EventListItem>>(
  (ref) => ref.watch(eventsRepositoryProvider).getAll(),
);

final eventSummaryProvider = FutureProvider.autoDispose<EventSummary>(
  (ref) => ref.watch(eventsRepositoryProvider).getSummary(),
);

final eventDetailProvider = FutureProvider.autoDispose.family<EventDetail, String>(
  (ref, eventId) => ref.watch(eventsRepositoryProvider).getById(eventId),
);

final eventInvitesProvider = FutureProvider.autoDispose.family<List<EventInvite>, String>(
  (ref, eventId) => ref.watch(eventsRepositoryProvider).getInvites(eventId),
);

final eventParticipantsProvider = FutureProvider.autoDispose.family<List<EventParticipant>, String>(
  (ref, eventId) => ref.watch(eventsRepositoryProvider).getParticipants(eventId),
);
