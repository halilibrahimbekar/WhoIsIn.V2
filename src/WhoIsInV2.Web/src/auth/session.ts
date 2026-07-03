import type { CurrentUserResponse } from '../api/auth'

const ACCESS_TOKEN_KEY = 'whoisin.accessToken'
const REFRESH_TOKEN_KEY = 'whoisin.refreshToken'
const USER_KEY = 'whoisin.user'

export function getAccessToken(): string | null {
  return localStorage.getItem(ACCESS_TOKEN_KEY)
}

export function getRefreshToken(): string | null {
  return localStorage.getItem(REFRESH_TOKEN_KEY)
}

export function hasAccessToken(): boolean {
  return Boolean(getAccessToken())
}

export function readStoredUser(): CurrentUserResponse | null {
  const raw = localStorage.getItem(USER_KEY)
  if (!raw) {
    return null
  }

  try {
    return JSON.parse(raw) as CurrentUserResponse
  } catch {
    localStorage.removeItem(USER_KEY)
    return null
  }
}

export function persistAuthTokens(accessToken: string, refreshToken: string) {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken)
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
}

export function persistStoredUser(user: CurrentUserResponse) {
  localStorage.setItem(USER_KEY, JSON.stringify(user))
}

export function clearAuthSession() {
  localStorage.removeItem(ACCESS_TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
  localStorage.removeItem(USER_KEY)
}
