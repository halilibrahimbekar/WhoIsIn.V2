import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useNavigate } from 'react-router-dom'
import { login, persistAuthSession } from '../api/auth'
import { useAuth } from '../auth/AuthContext'

export function AuthPage() {
  const navigate = useNavigate()
  const { signIn } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState('')

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setErrorMessage('')
    setIsSubmitting(true)

    try {
      const response = await login({ email, password })
      persistAuthSession(response)
      signIn(response)
      navigate('/app')
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unexpected login error.'
      setErrorMessage(message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="auth-shell">
      <div className="auth-card">
        <p className="eyebrow">WhoIsInV2</p>
        <h1>Sign in</h1>
        <p>Sign in with your organizer account to continue.</p>

        <form className="auth-form" onSubmit={handleSubmit}>
          <label>
            Email
            <input
              type="email"
              placeholder="organizer@whoisin.app"
              autoComplete="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
            />
          </label>
          <label>
            Password
            <input
              type="password"
              placeholder="********"
              autoComplete="current-password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
            />
          </label>
          {errorMessage && <p className="auth-error">{errorMessage}</p>}
          <button type="submit" className="primary-btn full-width" disabled={isSubmitting}>
            {isSubmitting ? 'Signing in...' : 'Continue'}
          </button>
        </form>

        <Link to="/#register">Create an account</Link>
      </div>
    </section>
  )
}
