import { Link, useParams } from 'react-router-dom'

export function EventDetailPage() {
  const { eventId } = useParams()

  return (
    <section className="content-page">
      <header className="content-header">
        <h1>Event Detail</h1>
        <p>Route param: {eventId}</p>
      </header>

      <div className="detail-grid">
        <article className="detail-card">
          <h2>Lifecycle</h2>
          <p>{'Draft -> Published -> Completed / Cancelled'}</p>
        </article>
        <article className="detail-card">
          <h2>Capacity</h2>
          <p>Accepted: 120</p>
          <p>Waitlist: 12</p>
        </article>
        <article className="detail-card">
          <h2>Actions</h2>
          <p>Invite guests, update status, promote waitlist participants.</p>
          <Link to="/invites">Go to invites</Link>
        </article>
      </div>
    </section>
  )
}
