import { Navigate, Route, Routes } from 'react-router-dom'
import './App.css'
import { useAuth } from './auth/AuthContext'
import { AppLayout } from './layout/AppLayout'
import { AuthPage } from './pages/AuthPage'
import { DashboardPage } from './pages/DashboardPage'
import { EventDetailPage } from './pages/EventDetailPage'
import { EventsPage } from './pages/EventsPage'
import { InvitesPage } from './pages/InvitesPage'
import { LandingPage } from './pages/LandingPage'
import { NotificationsPage } from './pages/NotificationsPage'
import { LanguageSelector, useI18n } from './i18n'

function App() {
  const { isInitializing, user } = useAuth()
  const { t } = useI18n()

  if (isInitializing) {
    return <div className="boot-screen"><LanguageSelector /><p>{t('checkingSession')}</p></div>
  }

  return (
    <><LanguageSelector /><Routes>
      <Route path="/" element={<LandingPage />} />
      <Route path="/auth" element={user ? <Navigate to="/app" replace /> : <AuthPage />} />
      <Route path="/app" element={user ? <AppLayout /> : <Navigate to="/auth" replace />}>
        <Route index element={<DashboardPage />} />
        <Route path="events" element={<EventsPage />} />
        <Route path="events/:eventId" element={<EventDetailPage />} />
        <Route path="invites" element={<InvitesPage />} />
        <Route path="notifications" element={<NotificationsPage />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes></>
  )
}

export default App
