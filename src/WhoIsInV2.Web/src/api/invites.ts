import { apiFetch } from './client'
import type { PagedResponse } from './events'

export interface EventInvite {
  id: string
  email: string
  status: string
  invitedAtUtc: string
  respondedAtUtc: string | null
}

export interface EventParticipant {
  id: string
  email: string
  displayName: string
  status: string
  addedAtUtc: string
}

export interface RsvpResponse {
  eventId: string
  email: string
  inviteStatus: string
  participantStatus: string
}

async function throwInviteError(response: Response, fallback: string): Promise<never> {
  let message = fallback
  try {
    const json = await response.json() as { detail?: string; title?: string }
    message = json.detail ?? json.title ?? fallback
  } catch {
    const text = await response.text().catch(() => '')
    if (text) message = text
  }
  throw new Error(message)
}

export async function getEventInvites(eventId: string, page = 1, pageSize = 50): Promise<PagedResponse<EventInvite>> {
  const response = await apiFetch(`/api/events/${eventId}/invites?page=${page}&pageSize=${pageSize}`)
  if (!response.ok) {
    return throwInviteError(response, 'Could not fetch event invites.')
  }

  return (await response.json()) as PagedResponse<EventInvite>
}

export async function sendEventInvites(eventId: string, emails: string[]): Promise<EventInvite[]> {
  const response = await apiFetch(`/api/events/${eventId}/invites`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ emails }),
  })

  if (!response.ok) {
    return throwInviteError(response, 'Could not send event invites.')
  }

  return (await response.json()) as EventInvite[]
}

export async function getEventParticipants(eventId: string, page = 1, pageSize = 50): Promise<PagedResponse<EventParticipant>> {
  const response = await apiFetch(`/api/events/${eventId}/participants?page=${page}&pageSize=${pageSize}`)
  if (!response.ok) {
    return throwInviteError(response, 'Could not fetch event participants.')
  }

  return (await response.json()) as PagedResponse<EventParticipant>
}

export async function updateParticipantStatus(
  eventId: string,
  participantId: string,
  status: string,
): Promise<void> {
  const response = await apiFetch(`/api/events/${eventId}/participants/${participantId}`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ status }),
  })

  if (!response.ok) {
    return throwInviteError(response, 'Could not update participant status.')
  }
}

export async function promoteWaitlistedParticipant(eventId: string): Promise<void> {
  const response = await apiFetch(`/api/events/${eventId}/waitlist/promote`, {
    method: 'POST',
  })

  if (!response.ok) {
    return throwInviteError(response, 'Could not promote waitlisted participant.')
  }
}

export async function submitRsvp(eventId: string, decision: 'Accepted' | 'Declined'): Promise<RsvpResponse> {
  const response = await apiFetch(`/api/events/${eventId}/rsvp`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ decision }),
  })

  if (!response.ok) {
    return throwInviteError(response, 'Could not submit RSVP.')
  }

  return (await response.json()) as RsvpResponse
}
