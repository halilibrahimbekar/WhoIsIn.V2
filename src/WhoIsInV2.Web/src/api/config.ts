const rawApiBaseUrl = import.meta.env.VITE_API_BASE_URL

function resolveApiBaseUrl(): string {
  const candidate = rawApiBaseUrl?.trim()
  const fallback = 'http://localhost:5032'
  const base = candidate ? candidate : fallback

  // Remove trailing slashes to keep path concatenation predictable.
  return base.replace(/\/+$/, '')
}

export const API_BASE_URL = resolveApiBaseUrl()
