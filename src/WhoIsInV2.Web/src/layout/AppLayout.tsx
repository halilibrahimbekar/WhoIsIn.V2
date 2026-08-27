import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

const navItems = [
  { to: '/app', label: 'Dashboard', end: true },
  { to: '/app/events', label: 'Events' },
  { to: '/app/invites', label: 'Invites' },
  { to: '/app/notifications', label: 'Notifications' },
]

export function AppLayout() {
  const { signOut, user } = useAuth()

  return (
    <div className="app-frame">
      <aside className="sidebar">
        <p className="brand">WhoIsInV2</p>
        <p className="brand-sub">Organizer Console</p>

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
            Sign out
          </button>
        </div>
      </aside>

      <main className="content-shell">
        <Outlet />
      </main>
    </div>
  )
}
