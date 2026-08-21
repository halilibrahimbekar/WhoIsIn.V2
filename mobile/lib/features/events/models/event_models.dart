class EventListItem {
  EventListItem({
    required this.id,
    required this.title,
    required this.categoryId,
    required this.categoryName,
    required this.visibility,
    required this.startAtUtc,
    required this.endAtUtc,
    required this.capacity,
    required this.status,
  });

  factory EventListItem.fromJson(Map<String, dynamic> json) => EventListItem(
        id: json['id'] as String,
        title: json['title'] as String,
        categoryId: json['categoryId'] as String?,
        categoryName: json['categoryName'] as String?,
        visibility: json['visibility'] as String,
        startAtUtc: DateTime.parse(json['startAtUtc'] as String),
        endAtUtc: json['endAtUtc'] == null ? null : DateTime.parse(json['endAtUtc'] as String),
        capacity: json['capacity'] as int,
        status: json['status'] as String,
      );

  final String id;
  final String title;
  final String? categoryId;
  final String? categoryName;
  final String visibility;
  final DateTime startAtUtc;
  final DateTime? endAtUtc;
  final int capacity;
  final String status;
}

class EventDetail {
  EventDetail({
    required this.id,
    required this.organizerId,
    required this.title,
    required this.description,
    required this.categoryId,
    required this.categoryName,
    required this.visibility,
    required this.requireApproval,
    required this.startAtUtc,
    required this.endAtUtc,
    required this.timeZone,
    required this.locationName,
    required this.locationAddress,
    required this.onlineMeetingUrl,
    required this.capacity,
    required this.status,
  });

  factory EventDetail.fromJson(Map<String, dynamic> json) => EventDetail(
        id: json['id'] as String,
        organizerId: json['organizerId'] as String,
        title: json['title'] as String,
        description: json['description'] as String?,
        categoryId: json['categoryId'] as String?,
        categoryName: json['categoryName'] as String?,
        visibility: json['visibility'] as String,
        requireApproval: json['requireApproval'] as bool,
        startAtUtc: DateTime.parse(json['startAtUtc'] as String),
        endAtUtc: json['endAtUtc'] == null ? null : DateTime.parse(json['endAtUtc'] as String),
        timeZone: json['timeZone'] as String,
        locationName: json['locationName'] as String?,
        locationAddress: json['locationAddress'] as String?,
        onlineMeetingUrl: json['onlineMeetingUrl'] as String?,
        capacity: json['capacity'] as int,
        status: json['status'] as String,
      );

  final String id;
  final String organizerId;
  final String title;
  final String? description;
  final String? categoryId;
  final String? categoryName;
  final String visibility;
  final bool requireApproval;
  final DateTime startAtUtc;
  final DateTime? endAtUtc;
  final String timeZone;
  final String? locationName;
  final String? locationAddress;
  final String? onlineMeetingUrl;
  final int capacity;
  final String status;
}

class EventInvite {
  EventInvite({
    required this.id,
    required this.email,
    required this.status,
    required this.invitedAtUtc,
    required this.respondedAtUtc,
  });

  factory EventInvite.fromJson(Map<String, dynamic> json) => EventInvite(
        id: json['id'] as String,
        email: json['email'] as String,
        status: json['status'] as String,
        invitedAtUtc: DateTime.parse(json['invitedAtUtc'] as String),
        respondedAtUtc:
            json['respondedAtUtc'] == null ? null : DateTime.parse(json['respondedAtUtc'] as String),
      );

  final String id;
  final String email;
  final String status;
  final DateTime invitedAtUtc;
  final DateTime? respondedAtUtc;
}

class EventParticipant {
  EventParticipant({
    required this.id,
    required this.email,
    required this.displayName,
    required this.status,
    required this.addedAtUtc,
  });

  factory EventParticipant.fromJson(Map<String, dynamic> json) => EventParticipant(
        id: json['id'] as String,
        email: json['email'] as String,
        displayName: json['displayName'] as String,
        status: json['status'] as String,
        addedAtUtc: DateTime.parse(json['addedAtUtc'] as String),
      );

  final String id;
  final String email;
  final String displayName;
  final String status;
  final DateTime addedAtUtc;
}

class EventSummary {
  EventSummary({
    required this.activeEventCount,
    required this.acceptedGuestCount,
    required this.waitlistCount,
    required this.fillRate,
    required this.upcomingEvents,
  });

  factory EventSummary.fromJson(Map<String, dynamic> json) => EventSummary(
        activeEventCount: json['activeEventCount'] as int,
        acceptedGuestCount: json['acceptedGuestCount'] as int,
        waitlistCount: json['waitlistCount'] as int,
        fillRate: (json['fillRate'] as num).toDouble(),
        upcomingEvents: (json['upcomingEvents'] as List<dynamic>)
            .map((item) => EventSummaryItem.fromJson(item as Map<String, dynamic>))
            .toList(),
      );

  final int activeEventCount;
  final int acceptedGuestCount;
  final int waitlistCount;
  final double fillRate;
  final List<EventSummaryItem> upcomingEvents;
}

class EventSummaryItem {
  EventSummaryItem({
    required this.id,
    required this.title,
    required this.startAtUtc,
    required this.endAtUtc,
    required this.locationName,
    required this.onlineMeetingUrl,
    required this.capacity,
    required this.status,
    required this.acceptedCount,
    required this.waitlistCount,
  });

  factory EventSummaryItem.fromJson(Map<String, dynamic> json) => EventSummaryItem(
        id: json['id'] as String,
        title: json['title'] as String,
        startAtUtc: DateTime.parse(json['startAtUtc'] as String),
        endAtUtc: json['endAtUtc'] == null ? null : DateTime.parse(json['endAtUtc'] as String),
        locationName: json['locationName'] as String?,
        onlineMeetingUrl: json['onlineMeetingUrl'] as String?,
        capacity: json['capacity'] as int,
        status: json['status'] as String,
        acceptedCount: json['acceptedCount'] as int,
        waitlistCount: json['waitlistCount'] as int,
      );

  final String id;
  final String title;
  final DateTime startAtUtc;
  final DateTime? endAtUtc;
  final String? locationName;
  final String? onlineMeetingUrl;
  final int capacity;
  final String status;
  final int acceptedCount;
  final int waitlistCount;
}
