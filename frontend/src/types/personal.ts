import type { RolUsuario } from './usuario'

export interface CrearUsuarioRequest {
  nombreCompleto: string
  correo: string
  contrasenaTemporal: string
  rol: RolUsuario
}

export interface EditarUsuarioRequest {
  usuarioId: string
  nombreCompleto: string
  correo: string
}

export interface CambiarRolRequest {
  usuarioId: string
  nuevoRol: RolUsuario
}

export interface CambiarContrasenaRequest {
  usuarioId: string
  nuevaContrasena: string
}

export const ROLES_ASIGNABLES: RolUsuario[] = ['Admin', 'Doctor', 'Fondos', 'Farmacia', 'Lemy']

/** Solo una cuenta Lemy puede asignar el rol Lemy a otra cuenta (ver PersonalController). */
export function rolesAsignablesPara(rolActor: RolUsuario | undefined): RolUsuario[] {
  return rolActor === 'Lemy' ? ROLES_ASIGNABLES : ROLES_ASIGNABLES.filter((rol) => rol !== 'Lemy')
}
