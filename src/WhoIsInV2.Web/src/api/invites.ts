import { apiFetch } from './client'

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

export async function getEventInvites(eventId: string): Promise<EventInvite[]> {
  const response = await apiFetch(`/api/events/${eventId}/invites`)
  if (!response.ok) {
    const errorText = await response.text()
    throw new Error(errorText || 'Could not fetch event invites.')
  }

  return (await response.json()) as EventInvite[]
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
    const errorText = await response.text()
    throw new Error(errorText || 'Could not send event invites.')
  }

  return (await response.json()) as EventInvite[]
}

export async function getEventParticipants(eventId: string): Promise<EventParticipant[]> {
  const response = await apiFetch(`/api/events/${eventId}/participants`)
  if (!response.ok) {
    const errorText = await response.text()
    throw new Error(errorText || 'Could not fetch event participants.')
  }

  return (await response.json()) as EventParticipant[]
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
    const errorText = await response.text()
    throw new Error(errorText || 'Could not update participant status.')
  }
}

export async function promoteWaitlistedParticipant(eventId: string): Promise<void> {
  const response = await apiFetch(`/api/events/${eventId}/waitlist/promote`, {
    method: 'POST',
  })

  if (!response.ok) {
    const errorText = await response.text()
    throw new Error(errorText || 'Could not promote waitlisted participant.')
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
    const errorText = await response.text()
    throw new Error(errorText || 'Could not submit RSVP.')
  }

  return (await response.json()) as RsvpResponse
}
