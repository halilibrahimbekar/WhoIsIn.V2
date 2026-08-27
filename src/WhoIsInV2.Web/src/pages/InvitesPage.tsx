import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { getEventInvites, submitRsvp, type EventInvite } from '../api/invites'
import { getEvents, type EventListItem } from '../api/events'
import { useAuth } from '../auth/AuthContext'

export function InvitesPage() {
  const { user } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()
  const [events, setEvents] = useState<EventListItem[]>([])
  const [inviteRows, setInviteRows] = useState<EventInvite[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [rsvpEmail, setRsvpEmail] = useState('')
  const [rsvpMessage, setRsvpMessage] = useState('')
  const selectedEventId = searchParams.get('eventId') || ''

  useEffect(() => {
    let isMounted = true

    async function loadEvents() {
      try {
        const response = await getEvents()
        if (isMounted) {
          setEvents(response.items)
          if (!selectedEventId && response.items.length > 0) {
            setSearchParams({ eventId: response.items[0].id }, { replace: true })
          }
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(error instanceof Error ? error.message : 'Could not load events.')
          setIsLoading(false)
        }
      }
    }

    loadEvents()

    return () => {
      isMounted = false
    }
  }, [selectedEventId, setSearchParams])

  useEffect(() => {
    let isMounted = true

    async function loadInvites() {
      if (!selectedEventId) {
        setIsLoading(events.length === 0)
        return
      }

      setIsLoading(true)
      setErrorMessage('')

      try {
        const response = await getEventInvites(selectedEventId)
        if (isMounted) {
          setInviteRows(response.items)
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(error instanceof Error ? error.message : 'Could not load invites.')
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    loadInvites()

    return () => {
      isMounted = false
    }
  }, [events.length, selectedEventId])

  async function handleRsvp(email: string, decision: 'Accepted' | 'Declined') {
    if (!selectedEventId) {
      return
    }

    setRsvpEmail(email)
    setRsvpMessage('')

    try {
      const response = await submitRsvp(selectedEventId, decision)
      setInviteRows((current) => current.map((invite) => (
        invite.email === email ? { ...invite, status: response.inviteStatus, respondedAtUtc: new Date().toISOString() } : invite
      )))
      setRsvpMessage(`${email}: ${response.inviteStatus}`)
    } catch (error) {
      setRsvpMessage(error instanceof Error ? error.message : 'Could not submit RSVP.')
    } finally {
      setRsvpEmail('')
    }
  }

  return (
    <section className="content-page">
      <header className="content-header">
        <h1>Invites</h1>
        <p>Manage RSVP states and invitation channels.</p>
      </header>

      <label className="field-label">
        Event
        <span className="select-wrap">
          <select
            className="select-control"
            value={selectedEventId}
            onChange={(event) => setSearchParams({ eventId: event.target.value })}
            disabled={events.length === 0}
          >
            <option value="">Select an event</option>
            {events.map((event) => (
              <option value={event.id} key={event.id}>{event.title}</option>
            ))}
          </select>
        </span>
      </label>

      <div className="table-card">
        {rsvpMessage && <p className="auth-error">{rsvpMessage}</p>}
        {isLoading && <p>Loading invites...</p>}
        {!isLoading && errorMessage && <p className="auth-error">{errorMessage}</p>}
        {!isLoading && !errorMessage && !selectedEventId && <p>Select an event to view invites.</p>}
        {!isLoading && !errorMessage && selectedEventId && inviteRows.length === 0 && <p>No invites found.</p>}
        {!isLoading && !errorMessage && inviteRows.length > 0 && (
          <table>
            <thead>
              <tr>
                <th>Email</th>
                <th>Status</th>
                <th>Invited</th>
                <th>Responded</th>
                <th>RSVP</th>
              </tr>
            </thead>
            <tbody>
              {inviteRows.map((row) => (
                <tr key={row.id}>
                  <td>{row.email}</td>
                  <td>{row.status}</td>
                  <td>{new Date(row.invitedAtUtc).toLocaleString()}</td>
                  <td>{row.respondedAtUtc ? new Date(row.respondedAtUtc).toLocaleString() : '-'}</td>
                  <td>
                    {user?.email === row.email ? <>
                      {row.status !== 'Accepted' && <button type="button" className="ghost-btn" disabled={Boolean(rsvpEmail)} onClick={() => void handleRsvp(row.email, 'Accepted')}>
                        Accept
                      </button>}
                      {row.status !== 'Declined' && <button type="button" className="ghost-btn" disabled={Boolean(rsvpEmail)} onClick={() => void handleRsvp(row.email, 'Declined')}>
                        Decline
                      </button>}
                    </> : <span>Organizer view</span>}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </section>
  )
}
