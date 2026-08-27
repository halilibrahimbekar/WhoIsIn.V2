import { apiFetch } from './client'

export interface NotificationItem {
  id: string
  eventId: string
  eventTitle: string
  message: string
  createdAtUtc: string
}

export async function getNotifications(): Promise<NotificationItem[]> {
  const response = await apiFetch('/api/notifications')
  if (!response.ok) throw new Error('Could not load notifications.')
  return (await response.json()) as NotificationItem[]
}