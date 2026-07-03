import { Link } from 'react-router-dom'

const events = [
  {
    id: 'launch-meetup',
    name: 'Product Launch Meetup',
    date: '05 July, 19:00',
    status: 'Published',
    accepted: 120,
    waitlist: 12,
  },
  {
    id: 'frontend-night',
    name: 'Frontend Community Night',
    date: '08 July, 20:30',
    status: 'Published',
    accepted: 340,
    waitlist: 41,
  },
  {
    id: 'mvp-demo-day',
    name: 'WhoIsIn MVP Demo Day',
    date: '12 July, 18:00',
    status: 'Draft',
    accepted: 72,
    waitlist: 0,
  },
]

export function EventsPage() {
  return (
    <section className="content-page">
      <header className="content-header">
        <h1>Events</h1>
        <p>Review current statuses and jump into event details.</p>
      </header>

      <div className="table-card">
        <table>
          <thead>
            <tr>
              <th>Event</th>
              <th>Date</th>
              <th>Status</th>
              <th>Accepted</th>
              <th>Waitlist</th>
              <th>Open</th>
            </tr>
          </thead>
          <tbody>
            {events.map((event) => (
              <tr key={event.id}>
                <td>{event.name}</td>
                <td>{event.date}</td>
                <td>{event.status}</td>
                <td>{event.accepted}</td>
                <td>{event.waitlist}</td>
                <td>
                  <Link to={`/events/${event.id}`}>Detail</Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}
