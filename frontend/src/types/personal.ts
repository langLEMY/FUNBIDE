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
