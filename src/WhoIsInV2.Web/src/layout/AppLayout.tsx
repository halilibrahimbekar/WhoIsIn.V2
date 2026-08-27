import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { useI18n, type Language } from '../i18n'

export function AppLayout() {
  const { signOut, user } = useAuth()
  const { language, setLanguage, t } = useI18n()
  const navItems = [
    { to: '/app', label: t('dashboard'), end: true },
    { to: '/app/events', label: t('events') },
    { to: '/app/invites', label: t('invites') },
    { to: '/app/notifications', label: t('notifications') },
  ]

  return (
    <div className="app-frame">
      <aside className="sidebar">
        <p className="brand">WhoIsInV2</p>
        <p className="brand-sub">{language === 'tr' ? 'Organizatör paneli' : 'Organizer Console'}</p>

        <nav aria-label="Primary">
          <ul className="nav-list">
            {navItems.map((item) => (
              <li key={item.to}>
                <NavLink
                  to={item.to}
                  end={item.end}
                  className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
                >
                  {item.label}
                </NavLink>
              </li>
            ))}
          </ul>
        </nav>

        <div className="sidebar-footer">
          <p className="user-chip">{user?.firstName} {user?.lastName}</p>
          <button type="button" className="signout-btn" onClick={() => void signOut()}>
            {t('signOut')}
          </button>
          <label>
            {t('language')}
            <select value={language} onChange={(event) => setLanguage(event.target.value as Language)}>
              <option value="tr">{t('turkish')}</option>
              <option value="en">{t('english')}</option>
            </select>
          </label>
        </div>
      </aside>

      <main className="content-shell">
        <Outlet />
      </main>
    </div>
  )
}
