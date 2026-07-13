import type { ClienteAutenticacion, SesionMinima } from './tiposSesion'

/**
 * Reemplaza al cliente de Supabase Auth cuando VITE_AUTH_MODE=local (instalación
 * offline/USB, sin Supabase): el login pasa por POST /api/auth/login en vez de ir
 * directo a la nube, y la sesión se guarda en localStorage en vez de en el SDK de
 * Supabase. Implementa el mismo subconjunto de `supabase.auth.*` que AuthContext.tsx
 * y RestablecerContrasenaPage.tsx ya usan (ver tiposSesion.ts) para que ninguno de
 * los dos necesite saber cuál de los dos clientes está activo.
 */
const CLAVE_STORAGE = 'funbide-local-token'
const apiBaseUrl = import.meta.env.VITE_API_BASE_URL as string

interface TokenGuardado {
  accessToken: string
  expiraEn: string
}

type Escuchador = (event: string, session: SesionMinima | null) => void

const escuchadores = new Set<Escuchador>()

function leerTokenGuardado(): TokenGuardado | null {
  const crudo = localStorage.getItem(CLAVE_STORAGE)
  if (!crudo) return null
  try {
    return JSON.parse(crudo) as TokenGuardado
  } catch {
    return null
  }
}

function sesionDesdeToken(token: TokenGuardado | null): SesionMinima | null {
  if (!token || new Date(token.expiraEn).getTime() <= Date.now()) {
    return null
  }
  return { access_token: token.accessToken }
}

function notificar(event: string, session: SesionMinima | null) {
  escuchadores.forEach((callback) => callback(event, session))
}

async function leerDetalleError(respuesta: Response, fallback: string): Promise<Error> {
  const problema = await respuesta.json().catch(() => null)
  return new Error(problema?.detail ?? fallback)
}

export const localAuthClient: ClienteAutenticacion = {
  auth: {
    async getSession() {
      return { data: { session: sesionDesdeToken(leerTokenGuardado()) } }
    },

    onAuthStateChange(callback) {
      escuchadores.add(callback)
      return {
        data: {
          subscription: {
            unsubscribe: () => escuchadores.delete(callback),
          },
        },
      }
    },

    async signInWithPassword({ email, password }) {
      const respuesta = await fetch(`${apiBaseUrl}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ correo: email, contrasena: password }),
      })

      if (!respuesta.ok) {
        return { error: await leerDetalleError(respuesta, 'Correo o contraseña incorrectos.') }
      }

      const cuerpo = (await respuesta.json()) as { accessToken: string; expiraEn: string }
      localStorage.setItem(
        CLAVE_STORAGE,
        JSON.stringify({ accessToken: cuerpo.accessToken, expiraEn: cuerpo.expiraEn } satisfies TokenGuardado),
      )
      notificar('SIGNED_IN', { access_token: cuerpo.accessToken })
      return { error: null }
    },

    async signOut() {
      // Ignora `opciones.scope`: en modo local no hay multi-dispositivo, así que
      // "cerrar todas las sesiones" y "cerrar sesión" son la misma operación.
      localStorage.removeItem(CLAVE_STORAGE)
      notificar('SIGNED_OUT', null)
      return { error: null }
    },

    async resetPasswordForEmail() {
      return {
        error: new Error(
          'La recuperación de contraseña por correo no está disponible en esta instalación local. Pedile a un administrador (rol Lemy) que te cambie la contraseña desde Personal.',
        ),
      }
    },

    async updateUser({ password }) {
      const token = leerTokenGuardado()
      if (!token) {
        return { error: new Error('No hay una sesión activa.') }
      }

      const respuesta = await fetch(`${apiBaseUrl}/api/mi-perfil/contrasena`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token.accessToken}`,
        },
        body: JSON.stringify({ nuevaContrasena: password }),
      })

      if (!respuesta.ok) {
        return { error: await leerDetalleError(respuesta, 'No se pudo actualizar la contraseña.') }
      }

      return { error: null }
    },
  },
}
