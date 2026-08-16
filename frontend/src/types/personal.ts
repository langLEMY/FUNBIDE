import type { EspecialidadMedica, RolUsuario } from './usuario'

export interface CrearUsuarioRequest {
  nombreCompleto: string
  correo: string
  nombreUsuario: string
  contrasenaTemporal: string
  rol: RolUsuario
  especialidad: EspecialidadMedica | null
}

export interface EditarUsuarioRequest {
  usuarioId: string
  nombreCompleto: string
  correo: string
  nombreUsuario: string
}

export interface CambiarRolRequest {
  usuarioId: string
  nuevoRol: RolUsuario
}

export interface CambiarEspecialidadRequest {
  usuarioId: string
  especialidad: EspecialidadMedica
}

export const ESPECIALIDADES: EspecialidadMedica[] = [
  'Sonografia',
  'Odontologia',
  'Pediatria',
  'Cardiologia',
  'Ginecologia',
  'MedicinaGeneralYFamiliar',
  'Diabetologia',
  'Psicologia',
  'Oftalmologia',
  'MedicinaInterna',
  'Ortopedia',
  'Nutricion',
]

export const ETIQUETA_ESPECIALIDAD: Record<EspecialidadMedica, string> = {
  Sonografia: 'Sonografía',
  Odontologia: 'Odontología',
  Pediatria: 'Pediatría',
  Cardiologia: 'Cardiología',
  Ginecologia: 'Ginecología',
  MedicinaGeneralYFamiliar: 'Medicina General y Familiar',
  Diabetologia: 'Diabetología',
  Psicologia: 'Psicología',
  Oftalmologia: 'Oftalmología',
  MedicinaInterna: 'Medicina Interna',
  Ortopedia: 'Ortopedia y Traumatología',
  Nutricion: 'Nutrición',
}

export interface CambiarContrasenaRequest {
  usuarioId: string
  nuevaContrasena: string
}

export const ROLES_ASIGNABLES: RolUsuario[] = ['Admin', 'Doctor', 'Fondos', 'Lemy']

/** Solo una cuenta Lemy puede asignar el rol Lemy a otra cuenta (ver PersonalController). */
export function rolesAsignablesPara(rolActor: RolUsuario | undefined): RolUsuario[] {
  return rolActor === 'Lemy' ? ROLES_ASIGNABLES : ROLES_ASIGNABLES.filter((rol) => rol !== 'Lemy')
}
