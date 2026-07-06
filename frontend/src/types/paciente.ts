export type EstadoPaciente = 'Activo' | 'Seguimiento'

export const ESTADOS_PACIENTE: EstadoPaciente[] = ['Activo', 'Seguimiento']

export interface Paciente {
  id: string
  nombre: string
  apellido: string
  cedula: string
  telefono: string | null
  tieneFotoCedula: boolean
  edad: number | null
  condicion: string | null
  estado: EstadoPaciente
  ultimaVisita: string | null
}

export interface CrearPacienteRequest {
  nombre: string
  apellido: string
  cedula: string
  telefono: string | null
  edad: number | null
  condicion: string | null
}

export interface EditarPacienteRequest {
  pacienteId: string
  nombre: string
  apellido: string
  cedula: string
  telefono: string | null
  edad: number | null
  condicion: string | null
  estado: EstadoPaciente
}

export interface UrlFotoCedula {
  url: string
}
