export type RolUsuario = 'Admin' | 'Doctor' | 'Fondos' | 'Farmacia' | 'Lemy'

export interface Usuario {
  id: string
  nombreCompleto: string
  correo: string
  rol: RolUsuario
  activo: boolean
  fotoPerfilUrl: string | null
}
