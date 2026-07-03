import { Navigate } from 'react-router-dom'
import type { ReactNode } from 'react'
import { useAuth } from './AuthContext'
import { EstadoBloqueado } from '../components/EstadoBloqueado'
import type { RolUsuario } from '../types/usuario'

interface ProtectedRouteProps {
  children: ReactNode
  rolesPermitidos?: RolUsuario | RolUsuario[]
}

export function ProtectedRoute({ children, rolesPermitidos }: ProtectedRouteProps) {
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

  const roles = rolesPermitidos ? (Array.isArray(rolesPermitidos) ? rolesPermitidos : [rolesPermitidos]) : null

  if (roles && !roles.includes(perfil.rol)) {
    return <EstadoBloqueado mensaje={`Tu rol (${perfil.rol}) no tiene acceso a esta sección.`} />
  }

  return <>{children}</>
}
