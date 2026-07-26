export type RolUsuario = 'Admin' | 'Doctor' | 'Fondos' | 'Lemy'

/** Debe coincidir exactamente con FUNBIDE.Domain.Enums.EspecialidadMedica. Solo aplica a rol Doctor. */
export type EspecialidadMedica =
  | 'Sonografia'
  | 'Odontologia'
  | 'Pediatria'
  | 'Cardiologia'
  | 'Ginecologia'
  | 'MedicinaGeneralYFamiliar'
  | 'Diabetologia'
  | 'Psicologia'
  | 'Oftalmologia'

export interface Usuario {
  id: string
  nombreCompleto: string
  correo: string
  rol: RolUsuario
  activo: boolean
  fotoPerfilUrl: string | null
  especialidad: EspecialidadMedica | null
}
