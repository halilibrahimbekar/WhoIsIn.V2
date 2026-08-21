import { apiFetch } from './client'

export interface EventListItem {
  id: string
  title: string
  categoryId: string | null
  categoryName: string | null
  visibility: string
  startAtUtc: string
  endAtUtc: string | null
  capacity: number
  status: string
}

export interface EventDetail extends EventListItem {
  organizerId: string
  description: string | null
  categoryId: string | null
  categoryName: string | null
  visibility: string
  requireApproval: boolean
  timeZone: string
  locationName: string | null
  locationAddress: string | null
  onlineMeetingUrl: string | null
}

export interface EventSummaryItem {
  id: string
  title: string
  startAtUtc: string
  endAtUtc: string | null
  locationName: string | null
  onlineMeetingUrl: string | null
  capacity: number
  status: string
  acceptedCount: number
  waitlistCount: number
}

export interface EventSummary {
  activeEventCount: number
  acceptedGuestCount: number
  waitlistCount: number
  fillRate: number
  upcomingEvents: EventSummaryItem[]
}

export interface CreateEventRequest {
  title: string
  description: string | null
  categoryId: string | null
  visibility: string
  requireApproval: boolean
  startAtUtc: string
  endAtUtc: string | null
  timeZone: string
  locationName: string | null
  locationAddress: string | null
  onlineMeetingUrl: string | null
  capacity: number
}

export interface Category {
  id: string
  name: string
}

async function throwApiError(response: Response, fallback: string): Promise<never> {
  const errorText = await response.text()
  throw new Error(errorText || fallback)
}

export async function getEvents(): Promise<EventListItem[]> {
  const response = await apiFetch('/api/events')
  if (!response.ok) {
    return throwApiError(response, 'Could not fetch events.')
  }

  return (await response.json()) as EventListItem[]
}

export async function getCategories(): Promise<Category[]> {
  const response = await apiFetch('/api/categories')
  if (!response.ok) {
    return throwApiError(response, 'Could not fetch categories.')
  }

  return (await response.json()) as Category[]
}

export async function getEvent(eventId: string): Promise<EventDetail> {
  const response = await apiFetch(`/api/events/${eventId}`)
  if (!response.ok) {
    return throwApiError(response, 'Could not fetch event details.')
  }

  return (await response.json()) as EventDetail
}

export async function getEventSummary(): Promise<EventSummary> {
  const response = await apiFetch('/api/events/summary')
  if (!response.ok) {
    return throwApiError(response, 'Could not fetch event summary.')
  }

  return (await response.json()) as EventSummary
}

export async function createEvent(request: CreateEventRequest): Promise<EventDetail> {
  const response = await apiFetch('/api/events', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    return throwApiError(response, 'Could not create event.')
  }

  return (await response.json()) as EventDetail
}

export async function updateEventStatus(eventId: string, status: string): Promise<void> {
  const response = await apiFetch(`/api/events/${eventId}/status`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ status }),
  })

  if (!response.ok) {
    return throwApiError(response, 'Could not update event status.')
  }
}
