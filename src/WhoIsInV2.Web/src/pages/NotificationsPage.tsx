import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getNotifications, type NotificationItem } from '../api/notifications'

export function NotificationsPage() {
  const [items, setItems] = useState<NotificationItem[]>([])
  const [error, setError] = useState('')

  useEffect(() => { getNotifications().then(setItems).catch((reason: unknown) => setError(reason instanceof Error ? reason.message : 'Could not load notifications.')) }, [])

  return <section className="content-page">
    <header className="content-header"><h1>Notifications</h1><p>Participation requests for your events.</p></header>
    <div className="table-card">
      {error && <p className="auth-error">{error}</p>}
      {!error && items.length === 0 && <p>No new notifications.</p>}
      {items.map((item) => <article className="notification-row" key={item.id}>
        <div><strong>{item.eventTitle}</strong><p>{item.message}</p></div>
        <Link to={`/app/events/${item.eventId}`}>Review</Link>
      </article>)}
    </div>
  </section>
}