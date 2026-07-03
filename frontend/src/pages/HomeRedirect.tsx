import { Navigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { EstadoBloqueado } from '../components/EstadoBloqueado'

const RUTA_POR_ROL: Partial<Record<string, string>> = {
  Admin: '/dashboard',
  Lemy: '/personal',
}

/**
 * "/" no es una página en sí: manda a cada rol a su panel. Para roles que
 * todavía no tienen panel (Doctor, Fondos, Farmacia) muestra un mensaje
 * honesto de "todavía no existe", no un falso "acceso denegado" — y siempre
 * con salida (cerrar sesión), nunca un punto muerto.
 */
export function HomeRedirect() {
  const { session, perfil, cargando, perfilCargando, perfilError } = useAuth()

  if (cargando) {
    return <div className="pantalla-centrada text-secondary">Cargando…</div>
  }

  if (!session) {
    return <Navigate to="/login" replace />
  }

  if (perfilError) {
    return <EstadoBloqueado mensaje={`No se pudo cargar tu perfil: ${perfilError}`} />
  }

  if (perfilCargando || !perfil) {
    return <div className="pantalla-centrada text-secondary">Cargando tu perfil…</div>
  }

  const ruta = RUTA_POR_ROL[perfil.rol]
  if (ruta) {
    return <Navigate to={ruta} replace />
  }

  return <EstadoBloqueado mensaje={`Todavía no hay un panel para tu rol (${perfil.rol}).`} />
}
