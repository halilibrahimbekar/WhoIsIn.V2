import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { persistAuthSession, register } from '../api/auth'
import { useAuth } from '../auth/AuthContext'

const trustMetrics = [
  { label: 'Etkinlik', value: '2,400+' },
  { label: 'Davet', value: '180K+' },
  { label: 'Aktif Organizer', value: '620+' },
  { label: 'Memnuniyet', value: '%96' },
]

const highlights = [
  {
    title: 'RSVP akisini tek ekranda yonet',
    text: 'Pending, accepted ve waitlist durumlarini canli takip et; kapasite doldugunda sistem otomatik aksiyon alsin.',
  },
  {
    title: 'Rollerle duzenli ekip calismasi',
    text: 'Organizer ve co-organizer yetkilerini ayirarak davet operasyonunu daginik olmadan surdur.',
  },
  {
    title: 'Etkinlik gunune net bir planla gir',
    text: 'Dashboard kartlari sayesinde doluluk, risk ve sonraki adimlari ilk bakista gor.',
  },
]

export function LandingPage() {
  const navigate = useNavigate()
  const { signIn, user } = useAuth()
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [successMessage, setSuccessMessage] = useState('')
  const [errorMessage, setErrorMessage] = useState('')

  async function handleRegisterSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    setErrorMessage('')
    setSuccessMessage('')
    setIsSubmitting(true)

    try {
      const response = await register({
        firstName,
        lastName,
        email,
        password,
      })

      persistAuthSession(response)
      signIn(response)
      setSuccessMessage('Hesap olusturuldu. Panele yonlendiriliyorsun...')
      navigate('/app')
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Kayit sirasinda beklenmeyen bir hata olustu.'
      setErrorMessage(message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="landing-shell">
      <section className="landing-hero" aria-label="WhoIsInV2 tanitim">
        <p className="landing-kicker">WhoIsInV2 Event OS</p>
        <h1>Etkinligini kalabaliga degil, kontrol edilebilir bir akisa donustur.</h1>
        <p>
          WhoIsInV2; davet, RSVP, kapasite ve ekip koordinasyonunu tek merkezde birlestirir.
          Dagilmis notlar yerine anlik durum tablosu ile yonetim gucu kazanirsin.
        </p>

        <div className="landing-hero-actions">
          <a className="primary-btn" href="#register">Ucretsiz kayda basla</a>
          {user ? (
            <Link className="ghost-btn" to="/app">Panele git</Link>
          ) : (
            <Link className="ghost-btn" to="/auth">Girisi olanlar icin</Link>
          )}
        </div>
      </section>

      <section className="landing-metric-grid" aria-label="Platform metrikleri">
        {trustMetrics.map((item) => (
          <article className="landing-metric-card" key={item.label}>
            <p>{item.label}</p>
            <strong>{item.value}</strong>
          </article>
        ))}
      </section>

      <section className="landing-highlights" aria-label="One cikan ozellikler">
        {highlights.map((item) => (
          <article className="landing-highlight-card" key={item.title}>
            <h2>{item.title}</h2>
            <p>{item.text}</p>
          </article>
        ))}
      </section>

      <section className="register-section" id="register" aria-label="Kayit formu">
        <div className="register-copy">
          <p className="landing-kicker">Hemen basla</p>
          <h2>Ilk etkinligini dakikalar icinde yayinla.</h2>
          <p>
            Kaydini olustur, organizer paneline gec ve davetlilerini yonetmeye hemen basla.
            Ilk adim icin sadece temel bilgilerin yeterli.
          </p>
        </div>

        <form className="register-form" onSubmit={handleRegisterSubmit}>
          <label>
            Ad
            <input
              type="text"
              placeholder="Ada"
              value={firstName}
              onChange={(event) => setFirstName(event.target.value)}
              autoComplete="given-name"
              required
            />
          </label>

          <label>
            Soyad
            <input
              type="text"
              placeholder="Lovelace"
              value={lastName}
              onChange={(event) => setLastName(event.target.value)}
              autoComplete="family-name"
              required
            />
          </label>

          <label>
            E-posta
            <input
              type="email"
              placeholder="organizer@whoisin.app"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              autoComplete="email"
              required
            />
          </label>

          <label>
            Sifre
            <input
              type="password"
              placeholder="En az 8 karakter"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              autoComplete="new-password"
              minLength={8}
              required
            />
          </label>

          {errorMessage && <p className="auth-error">{errorMessage}</p>}
          {successMessage && <p className="register-success">{successMessage}</p>}

          <button type="submit" className="primary-btn full-width" disabled={isSubmitting}>
            {isSubmitting ? 'Kayit olusturuluyor...' : 'Kayit ol ve panele gec'}
          </button>
        </form>
      </section>
    </main>
  )
}
