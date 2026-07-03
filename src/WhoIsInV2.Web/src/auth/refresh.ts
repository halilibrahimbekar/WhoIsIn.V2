import type { CurrentUserResponse } from '../api/auth'
import {
  clearAuthSession,
  getRefreshToken,
  persistAuthTokens,
  persistStoredUser,
} from './session'

interface RefreshResponse {
  accessToken: string
  accessTokenExpiresAtUtc: string
  refreshToken: string
  refreshTokenExpiresAtUtc: string
  user: CurrentUserResponse
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5032'

let inFlightRefresh: Promise<boolean> | null = null

export function tryRefreshAccessToken(): Promise<boolean> {
  if (inFlightRefresh) {
    return inFlightRefresh
  }

  inFlightRefresh = executeRefresh()
  return inFlightRefresh
}

async function executeRefresh(): Promise<boolean> {
  const refreshToken = getRefreshToken()
  if (!refreshToken) {
    clearAuthSession()
    inFlightRefresh = null
    return false
  }

  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ refreshToken }),
    })

    if (!response.ok) {
      clearAuthSession()
      return false
    }

    const payload = (await response.json()) as RefreshResponse
    persistAuthTokens(payload.accessToken, payload.refreshToken)
    persistStoredUser(payload.user)
    return true
  } catch {
    clearAuthSession()
    return false
  } finally {
    inFlightRefresh = null
  }
}
