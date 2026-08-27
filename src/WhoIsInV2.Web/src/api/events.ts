import { apiFetch } from './client'

export interface EventListItem {
  id: string
  organizerId: string
  title: string
  categoryId: string | null
  categoryName: string | null
  visibility: string
  startAtUtc: string
  endAtUtc: string | null
  capacity: number
  status: string
}

export interface PagedResponse<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface EventListQuery {
  search?: string
  status?: string
  page?: number
  pageSize?: number
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

export async function getEvents(query?: EventListQuery): Promise<PagedResponse<EventListItem>> {
  const params = new URLSearchParams()
  if (query?.search) params.set('search', query.search)
  if (query?.status) params.set('status', query.status)
  if (query?.page != null) params.set('page', String(query.page))
  if (query?.pageSize != null) params.set('pageSize', String(query.pageSize))
  const qs = params.toString()
  const response = await apiFetch(`/api/events${qs ? `?${qs}` : ''}`)
  if (!response.ok) {
    return throwApiError(response, 'Could not fetch events.')
  }

  return (await response.json()) as PagedResponse<EventListItem>
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
