import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import {
  getCurrentUser,
  revokeCurrentSession,
  type AuthTokenResponse,
  type CurrentUserResponse,
} from '../api/auth'
import {
  clearAuthSession,
  hasAccessToken,
  persistStoredUser,
  readStoredUser,
} from './session'

interface AuthContextValue {
  user: CurrentUserResponse | null
  isInitializing: boolean
  signIn: (payload: AuthTokenResponse) => void
  signOut: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUserResponse | null>(() => readStoredUser())
  const [isInitializing, setIsInitializing] = useState(true)

  useEffect(() => {
    let isMounted = true

    async function hydrateSession() {
      if (!hasAccessToken()) {
        if (isMounted) {
          setIsInitializing(false)
        }

        return
      }

      try {
        const currentUser = await getCurrentUser()

        if (isMounted) {
          setUser(currentUser)
        }

        persistStoredUser(currentUser)
      } catch {
        clearAuthSession()

        if (isMounted) {
          setUser(null)
        }
      } finally {
        if (isMounted) {
          setIsInitializing(false)
        }
      }
    }

    hydrateSession()

    return () => {
      isMounted = false
    }
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isInitializing,
      signIn: (payload) => {
        persistStoredUser(payload.user)
        setUser(payload.user)
      },
      signOut: async () => {
        await revokeCurrentSession()
        clearAuthSession()
        setUser(null)
      },
    }),
    [isInitializing, user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider.')
  }

  return context
}
