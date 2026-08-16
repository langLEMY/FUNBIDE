import { avisarMantenimientoActivo } from './mantenimientoBus'
import { supabase } from './supabaseClient'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL as string

export class ApiError extends Error {
  status: number
  detalle?: string

  constructor(message: string, status: number, detalle?: string) {
    super(message)
    this.status = status
    this.detalle = detalle
  }
}

async function authHeaders(): Promise<HeadersInit> {
  const { data } = await supabase.auth.getSession()
  const token = data.session?.access_token
  return token ? { Authorization: `Bearer ${token}` } : {}
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const isFormBody = init.body instanceof FormData

  const headers: HeadersInit = {
    ...(await authHeaders()),
    ...(init.body && !isFormBody ? { 'Content-Type': 'application/json' } : {}),
    ...init.headers,
  }

  const response = await fetch(`${apiBaseUrl}${path}`, { ...init, headers })

  if (!response.ok) {
    const problema = await response.json().catch(() => null)

    if (response.status === 503) {
      avisarMantenimientoActivo(problema?.detail ?? null)
    }

    throw new ApiError(
      problema?.title ?? `Error ${response.status}`,
      response.status,
      problema?.detail,
    )
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export const api = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'POST', body: body !== undefined ? JSON.stringify(body) : undefined }),
  patch: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'PATCH', body: body !== undefined ? JSON.stringify(body) : undefined }),
  put: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'PUT', body: body !== undefined ? JSON.stringify(body) : undefined }),
  delete: <T>(path: string) => request<T>(path, { method: 'DELETE' }),
  postForm: <T>(path: string, form: FormData) => request<T>(path, { method: 'POST', body: form }),
}
