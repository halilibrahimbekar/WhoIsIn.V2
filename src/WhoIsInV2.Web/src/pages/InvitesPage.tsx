const inviteRows = [
  { email: 'ayse@example.com', state: 'Pending', channel: 'Email' },
  { email: 'can@example.com', state: 'Accepted', channel: 'Link' },
  { email: 'elif@example.com', state: 'Waitlisted', channel: 'Email' },
]

export function InvitesPage() {
  return (
    <section className="content-page">
      <header className="content-header">
        <h1>Invites</h1>
        <p>Manage RSVP states and invitation channels.</p>
      </header>

      <div className="table-card">
        <table>
          <thead>
            <tr>
              <th>Email</th>
              <th>Status</th>
              <th>Channel</th>
            </tr>
          </thead>
          <tbody>
            {inviteRows.map((row) => (
              <tr key={row.email}>
                <td>{row.email}</td>
                <td>{row.state}</td>
                <td>{row.channel}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}
