import { createContext, useContext, useState, type ReactNode } from 'react'

export type Language = 'tr' | 'en'

const translations = {
  tr: {
    dashboard: 'Panel', events: 'Etkinlikler', invites: 'Davetler', notifications: 'Bildirimler',
    signOut: 'Çıkış yap', language: 'Dil', turkish: 'Türkçe', english: 'English',
    checkingSession: 'Oturum kontrol ediliyor...', loading: 'Yükleniyor...', retry: 'Tekrar dene',
    noUpcomingEvents: 'Yaklaşan etkinlik yok.', seeAll: 'Tümünü gör', places: 'yer',
  },
  en: {
    dashboard: 'Dashboard', events: 'Events', invites: 'Invites', notifications: 'Notifications',
    signOut: 'Sign out', language: 'Language', turkish: 'Türkçe', english: 'English',
    checkingSession: 'Checking session...', loading: 'Loading...', retry: 'Try again',
    noUpcomingEvents: 'No upcoming events.', seeAll: 'See all', places: 'places',
  },
} as const

type TranslationKey = keyof typeof translations.en
type I18nContextValue = { language: Language; setLanguage: (language: Language) => void; t: (key: TranslationKey) => string }

const I18nContext = createContext<I18nContextValue | null>(null)

export function I18nProvider({ children }: { children: ReactNode }) {
  const [language, setLanguageState] = useState<Language>(() => (localStorage.getItem('whoisin-language') as Language) || 'tr')
  function setLanguage(next: Language) {
    localStorage.setItem('whoisin-language', next)
    setLanguageState(next)
  }
  return <I18nContext.Provider value={{ language, setLanguage, t: (key) => translations[language][key] }}>{children}</I18nContext.Provider>
}

export function useI18n() {
  const value = useContext(I18nContext)
  if (!value) throw new Error('useI18n must be used inside I18nProvider')
  return value
}

export function LanguageSelector() {
  const { language, setLanguage, t } = useI18n()
  return (
    <label className="language-selector">
      {t('language')}
      <select value={language} onChange={(event) => setLanguage(event.target.value as Language)}>
        <option value="tr">{t('turkish')}</option>
        <option value="en">{t('english')}</option>
      </select>
    </label>
  )
}