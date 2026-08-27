import 'package:dio/dio.dart';

import '../../../core/network/api_error_mapper.dart';
import '../models/event_models.dart';

class EventCreateInput {
  EventCreateInput({
    required this.title,
    this.description,
    this.categoryId,
    required this.visibility,
    required this.requireApproval,
    required this.startAtUtc,
    this.endAtUtc,
    required this.timeZone,
    this.locationName,
    this.locationAddress,
    this.onlineMeetingUrl,
    required this.capacity,
  });

  final String title;
  final String? description;
  final String? categoryId;
  final String visibility;
  final bool requireApproval;
  final DateTime startAtUtc;
  final DateTime? endAtUtc;
  final String timeZone;
  final String? locationName;
  final String? locationAddress;
  final String? onlineMeetingUrl;
  final int capacity;

  Map<String, dynamic> toJson() => {
        'title': title,
        'description': description,
        'categoryId': categoryId,
        'visibility': visibility,
        'requireApproval': requireApproval,
        'startAtUtc': startAtUtc.toUtc().toIso8601String(),
        'endAtUtc': endAtUtc?.toUtc().toIso8601String(),
        'timeZone': timeZone,
        'locationName': locationName,
        'locationAddress': locationAddress,
        'onlineMeetingUrl': onlineMeetingUrl,
        'capacity': capacity,
      };
}

class EventsRepository {
  EventsRepository(this._dio);

  final Dio _dio;

  Future<List<EventListItem>> getAll() async {
    try {
      final response = await _dio.get('/api/events');
      final data = response.data as Map<String, dynamic>;
      return (data['items'] as List<dynamic>)
          .map((item) => EventListItem.fromJson(item as Map<String, dynamic>))
          .toList();
    } on DioException catch (error) {
      throw toApiException(error);
    }
  }

  Future<EventSummary> getSummary() async {
    try {
      final response = await _dio.get('/api/events/summary');
      return EventSummary.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw toApiException(error);
    }
  }

  Future<EventDetail> getById(String id) async {
    try {
      final response = await _dio.get('/api/events/$id');
      return EventDetail.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw toApiException(error);
    }
  }

  Future<EventDetail> create(EventCreateInput input) async {
    try {
      final response = await _dio.post('/api/events', data: input.toJson());
      return EventDetail.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw toApiException(error);
    }
  }

  Future<EventDetail> update(String id, EventCreateInput input) async {
    try {
      final response = await _dio.put('/api/events/$id', data: input.toJson());
      return EventDetail.fromJson(response.data as Map<String, dynamic>);
    } on DioException catch (error) {
      throw toApiException(error);
    }
  }

  Future<void> updateStatus(String id, String status) async {
    try {
      await _dio.patch('/api/events/$id/status', data: {'status': status});
    } on DioException catch (error) {
      throw toApiException(error);
    }
  }

  Future<List<EventInvite>> getInvites(String eventId) async {
    try {
      final response = await _dio.get('/api/events/$eventId/invites');
      final data = response.data as Map<String, dynamic>;
      return (data['items'] as List<dynamic>)
          .map((item) => EventInvite.fromJson(item as Map<String, dynamic>))
          .toList();
    } on DioException catch (error) {
      throw toApiException(error);
    }
  }

  Future<List<EventInvite>> invite(String eventId, List<String> emails) async {
    try {
      final response = await _dio.post('/api/events/$eventId/invites', data: {'emails': emails});
      return (response.data as List<dynamic>)
          .map((item) => EventInvite.fromJson(item as Map<String, dynamic>))
          .toList();
    } on DioException catch (error) {
      throw toApiException(error);
    }
  }

  Future<void> rsvp(String eventId, String decision) async {
    try {
      await _dio.post('/api/events/$eventId/rsvp', data: {'decision': decision});
    } on DioException catch (error) {
      throw toApiException(error);
    }
  }

  Future<List<EventParticipant>> getParticipants(String eventId) async {
    try {
      final response = await _dio.get('/api/events/$eventId/participants');
      final data = response.data as Map<String, dynamic>;
      return (data['items'] as List<dynamic>)
          .map((item) => EventParticipant.fromJson(item as Map<String, dynamic>))
          .toList();
    } on DioException catch (error) {
      throw toApiException(error);
    }
  }

  Future<void> updateParticipantStatus(String eventId, String participantId, String status) async {
    try {
      await _dio.patch('/api/events/$eventId/participants/$participantId', data: {'status': status});
    } on DioException catch (error) {
      throw toApiException(error);
    }
  }

  Future<void> promoteWaitlisted(String eventId) async {
    try {
      await _dio.post('/api/events/$eventId/waitlist/promote');
    } on DioException catch (error) {
      throw toApiException(error);
    }
  }
}
