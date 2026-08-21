import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getEvent, updateEventStatus, type EventDetail } from '../api/events'
import { useAuth } from '../auth/AuthContext'
import {
  getEventParticipants,
  promoteWaitlistedParticipant,
  sendEventInvites,
  submitRsvp,
  updateParticipantStatus,
  type EventParticipant,
} from '../api/invites'

export function EventDetailPage() {
  const { eventId } = useParams()
  const { user } = useAuth()
  const [event, setEvent] = useState<EventDetail | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [isSavingStatus, setIsSavingStatus] = useState(false)
  const [participants, setParticipants] = useState<EventParticipant[]>([])
  const [inviteEmails, setInviteEmails] = useState('')
  const [isSendingInvites, setIsSendingInvites] = useState(false)
  const [inviteMessage, setInviteMessage] = useState('')
  const [participantMessage, setParticipantMessage] = useState('')
  const [isUpdatingParticipant, setIsUpdatingParticipant] = useState(false)
  const [rsvpMessage, setRsvpMessage] = useState('')
  const [isSubmittingRsvp, setIsSubmittingRsvp] = useState(false)

  useEffect(() => {
    let isMounted = true

    async function loadEvent() {
      if (!eventId) {
        setErrorMessage('Event id is missing.')
        setIsLoading(false)
        return
      }

      try {
        const response = await getEvent(eventId)
        if (isMounted) {
          setEvent(response)
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(error instanceof Error ? error.message : 'Could not load event.')
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    loadEvent()

    return () => {
      isMounted = false
    }
  }, [eventId])

  useEffect(() => {
    let isMounted = true

    async function loadParticipants() {
      if (!eventId || !event || !user || event.organizerId !== user.id) {
        return
      }

      try {
        const response = await getEventParticipants(eventId)
        if (isMounted) {
          setParticipants(response)
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(error instanceof Error ? error.message : 'Could not load participants.')
        }
      }
    }

    loadParticipants()

    return () => {
      isMounted = false
    }
  }, [event, eventId, user])

  if (isLoading) {
    return <section className="content-page"><p>Loading event...</p></section>
  }

  if (errorMessage || !event) {
    return <section className="content-page"><p className="auth-error">{errorMessage || 'Event not found.'}</p></section>
  }

  const isOrganizer = user?.id === event.organizerId

  async function handleStatusChange(currentEvent: EventDetail, nextStatus: string) {
    setErrorMessage('')
    setIsSavingStatus(true)

    try {
      await updateEventStatus(currentEvent.id, nextStatus)
      setEvent({ ...currentEvent, status: nextStatus })
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Could not update event status.')
    } finally {
      setIsSavingStatus(false)
    }
  }

  async function handleSendInvites(currentEvent: EventDetail, formEvent: React.FormEvent<HTMLFormElement>) {
    formEvent.preventDefault()
    const emails = inviteEmails
      .split(/[\s,;]+/)
      .map((email) => email.trim().toLowerCase())
      .filter(Boolean)

    if (emails.length === 0) {
      setInviteMessage('Enter at least one email address.')
      return
    }

    setInviteMessage('')
    setIsSendingInvites(true)

    try {
      const created = await sendEventInvites(currentEvent.id, emails)
      setInviteEmails('')
      setInviteMessage(`${created.length} invite(s) created.`)
    } catch (error) {
      setInviteMessage(error instanceof Error ? error.message : 'Could not send invites.')
    } finally {
      setIsSendingInvites(false)
    }
  }

  async function handleParticipantStatusChange(participant: EventParticipant, nextStatus: string) {
    if (!event) {
      return
    }

    setParticipantMessage('')
    setIsUpdatingParticipant(true)

    try {
      await updateParticipantStatus(event.id, participant.id, nextStatus)
      setParticipants((current) => current.map((item) => (
        item.id === participant.id ? { ...item, status: nextStatus } : item
      )))
    } catch (error) {
      setParticipantMessage(error instanceof Error ? error.message : 'Could not update participant.')
    } finally {
      setIsUpdatingParticipant(false)
    }
  }

  async function handlePromoteWaitlist() {
    if (!event) {
      return
    }

    setParticipantMessage('')
    setIsUpdatingParticipant(true)

    try {
      await promoteWaitlistedParticipant(event.id)
      const firstWaitlistedId = participants.find((participant) => participant.status === 'Waitlisted')?.id
      if (firstWaitlistedId) {
        setParticipants((current) => current.map((participant) => (
          participant.id === firstWaitlistedId ? { ...participant, status: 'Confirmed' } : participant
        )))
      }
      setParticipantMessage('The next waitlisted participant was promoted.')
    } catch (error) {
      setParticipantMessage(error instanceof Error ? error.message : 'Could not promote waitlisted participant.')
    } finally {
      setIsUpdatingParticipant(false)
    }
  }

  async function handleRsvp() {
    if (!event) return
    setIsSubmittingRsvp(true)
    setRsvpMessage('')
    try {
      const response = await submitRsvp(event.id, 'Accepted')
      setRsvpMessage(`Your participation status: ${response.participantStatus}`)
    } catch (error) {
      setRsvpMessage(error instanceof Error ? error.message : 'Could not submit RSVP.')
    } finally {
      setIsSubmittingRsvp(false)
    }
  }

  return (
    <section className="content-page">
      <header className="content-header">
        <h1>{event.title}</h1>
        <p>{event.categoryName || 'Event'} | {event.visibility} | {event.status}</p>
      </header>

      <div className="detail-grid">
        {isOrganizer && <article className="detail-card">
          <h2>Lifecycle</h2>
          <p>Current status: {event.status}</p>
          <p>{formatEventDate(event.startAtUtc, event.endAtUtc)}</p>
            <label className="field-label">
            Change status
              <span className="select-wrap">
                <select
                  className="select-control"
                  value={event.status}
                  onChange={(changeEvent) => void handleStatusChange(event, changeEvent.target.value)}
                  disabled={isSavingStatus || event.status === 'Cancelled' || event.status === 'Completed'}
                >
                  <option value={event.status}>{event.status}</option>
                  {getNextStatuses(event.status).map((status) => <option value={status} key={status}>{status}</option>)}
                </select>
              </span>
          </label>
        </article>}
        <article className="detail-card">
          <h2>Capacity</h2>
          <p>Capacity: {event.capacity}</p>
          <p>Accepted and waitlist counts are available from participants.</p>
        </article>
        {!isOrganizer && event.visibility === 'Public' && <article className="detail-card">
          <h2>Join event</h2>
          <p>{event.requireApproval ? 'The organizer will approve your participation.' : 'Join this public event.'}</p>
          {rsvpMessage && <p className="auth-error">{rsvpMessage}</p>}
          <button type="button" className="primary-btn" disabled={isSubmittingRsvp} onClick={() => void handleRsvp()}>
            {isSubmittingRsvp ? 'Submitting...' : 'Request participation'}
          </button>
        </article>}
        {isOrganizer && <article className="detail-card">
          <h2>Actions</h2>
          <p>Invite guests and monitor participant status.</p>
          <form className="auth-form" onSubmit={(formEvent) => void handleSendInvites(event, formEvent)}>
            <label>
              Guest emails
              <textarea
                value={inviteEmails}
                onChange={(formEvent) => setInviteEmails(formEvent.target.value)}
                placeholder="guest@example.com, another@example.com"
                rows={3}
              />
            </label>
            {inviteMessage && <p className="auth-error">{inviteMessage}</p>}
            <button type="submit" className="primary-btn" disabled={isSendingInvites}>
              {isSendingInvites ? 'Sending...' : 'Send invites'}
            </button>
          </form>
          <Link to="/app/invites">Go to invites</Link>
        </article>}
      </div>

      <div className="detail-card">
        <h2>Location</h2>
        <p>{event.locationName || event.locationAddress || event.onlineMeetingUrl || 'Location not specified.'}</p>
        {event.description && <p>{event.description}</p>}
      </div>

      {isOrganizer && <div className="table-card">
        <h2>Participants</h2>
        <button type="button" className="ghost-btn" onClick={() => void handlePromoteWaitlist()} disabled={isUpdatingParticipant}>
          Promote next waitlisted
        </button>
        {participantMessage && <p className="auth-error">{participantMessage}</p>}
        {participants.length === 0 && <p>No participants yet.</p>}
        {participants.length > 0 && (
          <table>
            <thead>
              <tr><th>Name</th><th>Email</th><th>Status</th><th>Update</th></tr>
            </thead>
            <tbody>
              {participants.map((participant) => (
                <tr key={participant.id}>
                  <td>{participant.displayName}</td>
                  <td>{participant.email}</td>
                  <td>
                    <span className="select-wrap">
                      <select
                        className="select-control"
                        value={participant.status}
                        onChange={(changeEvent) => void handleParticipantStatusChange(participant, changeEvent.target.value)}
                        disabled={isUpdatingParticipant}
                      >
                        {['Confirmed', 'PendingApproval', 'Waitlisted', 'Declined', 'CheckedIn'].map((status) => (
                          <option value={status} key={status}>{status}</option>
                        ))}
                      </select>
                    </span>
                  </td>
                  <td>{participant.addedAtUtc ? new Date(participant.addedAtUtc).toLocaleString() : '-'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>}
    </section>
  )
}

function formatEventDate(startAtUtc: string, endAtUtc: string | null): string {
  const start = new Date(startAtUtc).toLocaleString()
  return endAtUtc ? `${start} - ${new Date(endAtUtc).toLocaleTimeString()}` : start
}

function getNextStatuses(status: string): string[] {
  if (status === 'Draft') {
    return ['Published', 'Cancelled']
  }

  if (status === 'Published') {
    return ['Cancelled', 'Completed']
  }

  return []
}
