import { Link } from 'react-router-dom'

const metrics = [
  { label: 'Active events', value: '24' },
  { label: 'Accepted guests', value: '1,482' },
  { label: 'Waitlist', value: '91' },
  { label: 'Fill rate', value: '83%' },
]

const upcomingEvents = [
  {
    id: 'launch-meetup',
    title: 'Product Launch Meetup',
    date: '05 July, 19:00',
    venue: 'Karakoy Loft',
    capacity: '120 / 160',
    status: 'Published',
  },
  {
    id: 'frontend-night',
    title: 'Frontend Community Night',
    date: '08 July, 20:30',
    venue: 'Online - Zoom',
    capacity: '340 / 400',
    status: 'Published',
  },
  {
    id: 'mvp-demo-day',
    title: 'WhoIsIn MVP Demo Day',
    date: '12 July, 18:00',
    venue: 'Levent Hub',
    capacity: '72 / 80',
    status: 'Almost full',
  },
]

export function DashboardPage() {
  return (
    <section className="page-shell">
      <header className="hero">
        <p className="eyebrow">WhoIsInV2 Web</p>
        <h1>Plan events, track RSVP flow, and steer capacity in real time.</h1>
        <p className="subtitle">
          The organizer dashboard surfaces the next bottleneck before your event day gets messy.
        </p>
        <div className="hero-actions">
          <button type="button" className="primary-btn">
            Create Event
          </button>
          <button type="button" className="ghost-btn">
            Load Demo Data
          </button>
        </div>
      </header>

      <section className="metric-grid" aria-label="Summary metrics">
        {metrics.map((item) => (
          <article className="metric-card" key={item.label}>
            <p>{item.label}</p>
            <strong>{item.value}</strong>
          </article>
        ))}
      </section>

      <section className="board" aria-label="Upcoming events">
        <div className="board-header">
          <h2>Upcoming Events</h2>
          <Link to="/events">See all</Link>
        </div>
        <div className="event-list">
          {upcomingEvents.map((event) => (
            <article className="event-card" key={event.id}>
              <div>
                <p className="event-title">{event.title}</p>
                <p className="event-meta">
                  {event.date} | {event.venue}
                </p>
              </div>
              <div className="event-side">
                <p>{event.capacity}</p>
                <span>{event.status}</span>
              </div>
            </article>
          ))}
        </div>
      </section>
    </section>
  )
}
