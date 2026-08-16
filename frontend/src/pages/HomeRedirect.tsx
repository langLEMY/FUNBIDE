import { Navigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { EstadoBloqueado } from '../components/EstadoBloqueado'
import type { ModuloPermiso } from '../types/usuario'

const RUTA_POR_ROL: Partial<Record<string, { ruta: string; modulo?: ModuloPermiso }>> = {
  Admin: { ruta: '/dashboard', modulo: 'Dashboard' },
  Lemy: { ruta: '/personal' },
  Doctor: { ruta: '/dashboard-doctor' },
  Fondos: { ruta: '/caja', modulo: 'Caja' },
}

/** "/" no es una página en sí: manda a cada rol a su panel. */
export function HomeRedirect() {
  const { session, perfil, cargando, perfilCargando, perfilError } = useAuth()

  if (cargando) {
    return <div className="pantalla-centrada text-secondary cargando-pulso">Cargando…</div>
  }

  if (!session) {
    return <Navigate to="/login" replace />
  }

  if (perfilError) {
    return <EstadoBloqueado mensaje={`No se pudo cargar tu perfil: ${perfilError}`} />
  }

  if (perfilCargando || !perfil) {
    return <div className="pantalla-centrada text-secondary cargando-pulso">Cargando tu perfil…</div>
  }

  const destino = RUTA_POR_ROL[perfil.rol]
  if (destino) {
    // Si el módulo de aterrizaje por defecto del rol fue revocado, no lo mandamos a
    // una pantalla que va a bloquearlo: mejor mostrar el propio perfil.
    const bloqueado = destino.modulo && !(perfil.permisos ?? []).includes(destino.modulo)
    return <Navigate to={bloqueado ? '/mi-perfil' : destino.ruta} replace />
  }

  return <EstadoBloqueado mensaje={`Todavía no hay un panel para tu rol (${perfil.rol}).`} />
}
