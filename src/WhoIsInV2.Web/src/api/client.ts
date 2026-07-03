import { getAccessToken } from '../auth/session'
import { tryRefreshAccessToken } from '../auth/refresh'
import { API_BASE_URL } from './config'

interface ApiFetchOptions extends RequestInit {
  withAuth?: boolean
}

export async function apiFetch(path: string, options?: ApiFetchOptions): Promise<Response> {
  const { withAuth = true, headers, ...rest } = options ?? {}
  const composedHeaders = new Headers(headers)

  if (withAuth) {
    const token = getAccessToken()
    if (token) {
      composedHeaders.set('Authorization', `Bearer ${token}`)
    }
  }

  let response = await fetch(`${API_BASE_URL}${path}`, {
    ...rest,
    headers: composedHeaders,
  })

  if (withAuth && response.status === 401) {
    const refreshed = await tryRefreshAccessToken()

    if (refreshed) {
      const retryHeaders = new Headers(headers)
      const latestAccessToken = getAccessToken()

      if (latestAccessToken) {
        retryHeaders.set('Authorization', `Bearer ${latestAccessToken}`)
      }

      response = await fetch(`${API_BASE_URL}${path}`, {
        ...rest,
        headers: retryHeaders,
      })
    }
  }

  return response
}
