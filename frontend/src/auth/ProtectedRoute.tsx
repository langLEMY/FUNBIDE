import { Navigate } from 'react-router-dom'
import type { ReactNode } from 'react'
import { useAuth } from './AuthContext'
import { EstadoBloqueado } from '../components/EstadoBloqueado'
import type { ModuloPermiso, RolUsuario } from '../types/usuario'

interface ProtectedRouteProps {
  children: ReactNode
  rolesPermitidos?: RolUsuario | RolUsuario[]
  /** Módulo togglable que debe estar en perfil.permisos (ver Gestionar Permisos), además de pasar el chequeo de rol. */
  modulo?: ModuloPermiso
}

export function ProtectedRoute({ children, rolesPermitidos, modulo }: ProtectedRouteProps) {
  const { session, perfil, cargando, perfilError } = useAuth()

  if (cargando) {
    return <div className="pantalla-centrada text-secondary cargando-pulso">Cargando…</div>
  }

  if (!session) {
    return <Navigate to="/login" replace />
  }

  // Ojo: solo bloquea con pantalla completa (spinner o error) cuando todavía NO hay un
  // perfil cargado (primer arranque). Una recarga posterior fallida (p. ej. un blip de
  // red tras editar el perfil propio) también deja perfilError con algo, pero ahí ya hay
  // un perfil previo que seguir mostrando — desmontar todo el árbol en ese momento le
  // hacía perder su estado local a la página (el mensaje de éxito nunca llegaba a verse).
  if (!perfil) {
    if (perfilError) {
      return <EstadoBloqueado mensaje={`No se pudo cargar tu perfil: ${perfilError}`} />
    }

    return <div className="pantalla-centrada text-secondary cargando-pulso">Cargando tu perfil…</div>
  }

  const roles = rolesPermitidos ? (Array.isArray(rolesPermitidos) ? rolesPermitidos : [rolesPermitidos]) : null

  if (roles && !roles.includes(perfil.rol)) {
    return <EstadoBloqueado mensaje={`Tu rol (${perfil.rol}) no tiene acceso a esta sección.`} />
  }

  if (modulo && !(perfil.permisos ?? []).includes(modulo)) {
    return <EstadoBloqueado mensaje="No tenés acceso concedido a esta sección. Pedile a un administrador que te lo habilite en Gestionar Permisos." />
  }

  return <>{children}</>
}
