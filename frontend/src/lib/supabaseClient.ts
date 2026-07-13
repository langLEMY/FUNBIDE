import { createClient } from '@supabase/supabase-js'
import { localAuthClient } from '../auth/localAuthClient'
import type { ClienteAutenticacion } from '../auth/tiposSesion'

const authMode = import.meta.env.VITE_AUTH_MODE as string | undefined

/** Para páginas que necesitan avisar de una limitación real de la instalación offline (ver MiPerfilPage). */
export const esModoLocal = authMode === 'local'

/**
 * VITE_AUTH_MODE=local activa la instalación offline/USB (sin Supabase, ver
 * localAuthClient.ts). Cualquier otro valor (o ausente) usa el cliente real de
 * Supabase, como siempre. AuthContext.tsx no sabe ni le importa cuál de los dos está
 * detrás: ambos implementan la misma interfaz `auth.*` (ver tiposSesion.ts).
 */
export const supabase: ClienteAutenticacion =
  esModoLocal ? localAuthClient : crearClienteSupabase()

function crearClienteSupabase(): ClienteAutenticacion {
  const supabaseUrl = import.meta.env.VITE_SUPABASE_URL as string
  const supabaseAnonKey = import.meta.env.VITE_SUPABASE_ANON_KEY as string

  if (!supabaseUrl || !supabaseAnonKey) {
    throw new Error(
      'Faltan VITE_SUPABASE_URL / VITE_SUPABASE_ANON_KEY. Copia .env.example a .env y complétalo.',
    )
  }

  return createClient(supabaseUrl, supabaseAnonKey)
}
