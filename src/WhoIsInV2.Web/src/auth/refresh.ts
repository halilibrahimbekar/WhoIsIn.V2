import type { CurrentUserResponse } from '../api/auth'
import { API_BASE_URL } from '../api/config'
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
