export type RolUsuario = 'Admin' | 'Doctor' | 'Fondos' | 'Lemy'

/** Debe coincidir exactamente con FUNBIDE.Domain.Enums.ModuloPermiso. */
export type ModuloPermiso =
  | 'Dashboard'
  | 'Resumen'
  | 'Finanzas'
  | 'Gastos'
  | 'Donaciones'
  | 'Operaciones'
  | 'Inventario'
  | 'Aseguradoras'
  | 'Actividad'
  | 'Pacientes'
  | 'HistorialClinico'
  | 'Directorio'
  | 'Caja'
  | 'Cobros'
  | 'Agenda'
  | 'Recepcion'

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
  | 'MedicinaInterna'
  | 'Ortopedia'
  | 'Nutricion'

export interface Usuario {
  id: string
  nombreCompleto: string
  correo: string
  nombreUsuario: string
  rol: RolUsuario
  activo: boolean
  fotoPerfilUrl: string | null
  especialidad: EspecialidadMedica | null
  /** Módulos togglables efectivos (default del rol + overrides). Solo viene poblado en /api/mi-perfil; en el resto (p. ej. /api/personal) es null. */
  permisos: ModuloPermiso[] | null
}
