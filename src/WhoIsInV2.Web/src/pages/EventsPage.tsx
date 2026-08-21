import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { createEvent, getCategories, getEvents, type Category, type EventListItem } from '../api/events'

export function EventsPage() {
  const [events, setEvents] = useState<EventListItem[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState('')
  const [isCreateOpen, setIsCreateOpen] = useState(false)
  const [isCreating, setIsCreating] = useState(false)
  const [createError, setCreateError] = useState('')
  const [refreshKey, setRefreshKey] = useState(0)
  const [title, setTitle] = useState('')
  const [startAt, setStartAt] = useState('')
  const [endAt, setEndAt] = useState('')
  const [timeZone, setTimeZone] = useState(Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC')
  const [capacity, setCapacity] = useState('50')
  const [locationName, setLocationName] = useState('')
  const [description, setDescription] = useState('')
  const [categories, setCategories] = useState<Category[]>([])
  const [categoryId, setCategoryId] = useState('')
  const [visibility, setVisibility] = useState('Public')
  const [requireApproval, setRequireApproval] = useState(false)

  useEffect(() => {
    let isMounted = true

    async function loadEvents() {
      try {
        const [response, categoryResponse] = await Promise.all([getEvents(), getCategories()])
        if (isMounted) {
          setEvents(response)
          setCategories(categoryResponse)
        }
      } catch (error) {
        if (isMounted) {
          setErrorMessage(error instanceof Error ? error.message : 'Could not load events.')
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    loadEvents()

    return () => {
      isMounted = false
    }
  }, [refreshKey])

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setCreateError('')
    setIsCreating(true)

    try {
      await createEvent({
        title,
        description: description || null,
        categoryId: categoryId || null,
        visibility,
        requireApproval,
        startAtUtc: new Date(startAt).toISOString(),
        endAtUtc: endAt ? new Date(endAt).toISOString() : null,
        timeZone,
        locationName: locationName || null,
        locationAddress: null,
        onlineMeetingUrl: null,
        capacity: Number(capacity),
      })
      setTitle('')
      setStartAt('')
      setEndAt('')
      setCapacity('50')
      setLocationName('')
      setDescription('')
      setCategoryId('')
      setVisibility('Public')
      setRequireApproval(false)
      setIsCreateOpen(false)
      setRefreshKey((value) => value + 1)
    } catch (error) {
      setCreateError(error instanceof Error ? error.message : 'Could not create event.')
    } finally {
      setIsCreating(false)
    }
  }

  return (
    <section className="content-page">
      <header className="content-header">
        <h1>Events</h1>
        <p>Review current statuses and jump into event details.</p>
        <button type="button" className="primary-btn" onClick={() => setIsCreateOpen((value) => !value)}>
          {isCreateOpen ? 'Close' : 'Create Event'}
        </button>
      </header>

      {isCreateOpen && (
        <form className="auth-form table-card" onSubmit={handleCreate}>
          <h2>Create event</h2>
          <label>
            Title
            <input value={title} onChange={(event) => setTitle(event.target.value)} required />
          </label>
          <label>
            Description
            <textarea value={description} onChange={(event) => setDescription(event.target.value)} rows={3} />
          </label>
          <label>
            Category
            <select value={categoryId} onChange={(event) => setCategoryId(event.target.value)} required>
              <option value="">Select a category</option>
              {categories.map((category) => <option value={category.id} key={category.id}>{category.name}</option>)}
            </select>
          </label>
          <label>
            Event access
            <select value={visibility} onChange={(event) => setVisibility(event.target.value)}>
              <option value="Public">Everyone can see and join</option>
              <option value="InviteOnly">Invitees only</option>
            </select>
          </label>
          <label>
            <input type="checkbox" checked={requireApproval} onChange={(event) => setRequireApproval(event.target.checked)} />
            Organizer approval required
          </label>
          <label>
            Starts
            <input type="datetime-local" value={startAt} onChange={(event) => setStartAt(event.target.value)} required />
          </label>
          <label>
            Ends
            <input type="datetime-local" value={endAt} onChange={(event) => setEndAt(event.target.value)} />
          </label>
          <label>
            Time zone
            <input value={timeZone} onChange={(event) => setTimeZone(event.target.value)} required />
          </label>
          <label>
            Capacity
            <input type="number" min="1" value={capacity} onChange={(event) => setCapacity(event.target.value)} required />
          </label>
          <label>
            Location
            <input value={locationName} onChange={(event) => setLocationName(event.target.value)} />
          </label>
          {createError && <p className="auth-error">{createError}</p>}
          <button type="submit" className="primary-btn" disabled={isCreating}>
            {isCreating ? 'Creating...' : 'Create event'}
          </button>
        </form>
      )}

      <div className="table-card">
        {isLoading && <p>Loading events...</p>}
        {!isLoading && errorMessage && <p className="auth-error">{errorMessage}</p>}
        {!isLoading && !errorMessage && events.length === 0 && <p>No events found.</p>}
        {!isLoading && !errorMessage && events.length > 0 && (
          <table>
            <thead>
              <tr>
                <th>Event</th>
                <th>Date</th>
                <th>Status</th>
                <th>Capacity</th>
                <th>Accepted</th>
                <th>Waitlist</th>
                <th>Open</th>
              </tr>
            </thead>
            <tbody>
              {events.map((event) => (
                <tr key={event.id}>
                  <td>{event.title}<br /><small>{event.categoryName || 'Uncategorized'} | {event.visibility}</small></td>
                  <td>{formatEventDate(event.startAtUtc, event.endAtUtc)}</td>
                  <td>{event.status}</td>
                  <td>{event.capacity}</td>
                  <td>-</td>
                  <td>-</td>
                  <td>
                    <Link to={`/app/events/${event.id}`}>Detail</Link>
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

function formatEventDate(startAtUtc: string, endAtUtc: string | null): string {
  const start = new Date(startAtUtc).toLocaleString()
  if (!endAtUtc) {
    return start
  }

  return `${start} - ${new Date(endAtUtc).toLocaleTimeString()}`
}
