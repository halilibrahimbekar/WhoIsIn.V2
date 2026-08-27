import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getEventSummary, getEvents, type EventListItem, type EventSummary } from '../api/events'

export function DashboardPage() {
  const [summary, setSummary] = useState<EventSummary | null>(null)
  const [events, setEvents] = useState<EventListItem[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')

  useEffect(() => {
    let isMounted = true

    async function loadSummary() {
      try {
        const [response, eventResponse] = await Promise.all([getEventSummary(), getEvents()])
        if (isMounted) {
          setSummary(response)
          setEvents(eventResponse.items)
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(error instanceof Error ? error.message : 'Could not load dashboard.')
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    loadSummary()

    return () => {
      isMounted = false
    }
  }, [])

  const metrics = summary
    ? [
        { label: 'Active events', value: summary.activeEventCount.toLocaleString() },
        { label: 'Accepted guests', value: summary.acceptedGuestCount.toLocaleString() },
        { label: 'Waitlist', value: summary.waitlistCount.toLocaleString() },
        { label: 'Fill rate', value: `${summary.fillRate}%` },
      ]
    : []

  return (
    <section className="page-shell">
      <header className="hero">
        <p className="eyebrow">WhoIsInV2 Web</p>
        <h1>Plan events, track RSVP flow, and steer capacity in real time.</h1>
        <p className="subtitle">
          The organizer dashboard surfaces the next bottleneck before your event day gets messy.
        </p>
      </header>

      <section className="metric-grid" aria-label="Summary metrics">
        {isLoading && <p>Loading dashboard...</p>}
        {!isLoading && errorMessage && <p className="auth-error">{errorMessage}</p>}
        {!isLoading && !errorMessage && metrics.map((item) => (
          <article className="metric-card" key={item.label}>
            <p>{item.label}</p>
            <strong>{item.value}</strong>
          </article>
        ))}
      </section>

      <section className="board" aria-label="Upcoming events">
        <div className="board-header">
          <h2>Upcoming Events</h2>
          <Link to="/app/events">See all</Link>
        </div>
        <div className="event-list">
          {!isLoading && !errorMessage && events.length === 0 && <p>No upcoming events.</p>}
          {events.map((event) => (
            <article className="event-card" key={event.id}>
              <div>
                <p className="event-title">{event.title}</p>
                <p className="event-meta">
                  {formatEventDate(event.startAtUtc, event.endAtUtc)} | {event.categoryName || 'Event'} | {event.visibility}
                </p>
              </div>
              <div className="event-side">
                <p>{event.capacity} places</p>
                <span>{event.status}</span>
              </div>
            </article>
          ))}
        </div>
      </section>
    </section>
  )
}

function formatEventDate(startAtUtc: string, endAtUtc: string | null): string {
  const start = new Date(startAtUtc).toLocaleString()
  return endAtUtc ? `${start} - ${new Date(endAtUtc).toLocaleTimeString()}` : start
}
