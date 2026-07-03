import { apiFetch } from './client'
import { clearAuthSession, getRefreshToken, persistAuthTokens, persistStoredUser } from '../auth/session'

export interface LoginRequest {
  email: string
  password: string
}

export interface CurrentUserResponse {
  id: string
  email: string
  firstName: string
  lastName: string
  createdAtUtc: string
}

export interface AuthTokenResponse {
  accessToken: string
  accessTokenExpiresAtUtc: string
  refreshToken: string
  refreshTokenExpiresAtUtc: string
  user: CurrentUserResponse
}

export async function login(request: LoginRequest): Promise<AuthTokenResponse> {
  const response = await apiFetch('/api/auth/login', {
    withAuth: false,
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    const errorText = await response.text()
    throw new Error(errorText || 'Login request failed.')
  }

  return (await response.json()) as AuthTokenResponse
}

export function persistAuthSession(payload: AuthTokenResponse) {
  persistAuthTokens(payload.accessToken, payload.refreshToken)
  persistStoredUser(payload.user)
}

export async function getCurrentUser(): Promise<CurrentUserResponse> {
  const response = await apiFetch('/api/auth/me')

  if (response.status === 401) {
    clearAuthSession()
    throw new Error('Session expired. Please sign in again.')
  }

  if (!response.ok) {
    const errorText = await response.text()
    throw new Error(errorText || 'Could not fetch current user.')
  }

  return (await response.json()) as CurrentUserResponse
}

export async function revokeCurrentSession(): Promise<void> {
  const refreshToken = getRefreshToken()
  if (!refreshToken) {
    return
  }

  try {
    await apiFetch('/api/auth/revoke', {
      withAuth: false,
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ refreshToken }),
    })
  } catch {
    // Ignore network errors on logout; local session will still be cleared.
  }
}
