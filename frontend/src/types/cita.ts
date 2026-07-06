export type EstadoCita = 'Pendiente' | 'Programada' | 'Completada' | 'Cancelada'

export interface Cita {
  id: string
  pacienteId: string
  doctorId: string
  motivo: string
  estado: EstadoCita
  inicio: string | null
  fin: string | null
  notasCierre: string | null
}

export interface CrearCitaRequest {
  pacienteId: string
  motivo: string
}

export interface ProgramarCitaRequest {
  citaId: string
  inicio: string
  fin: string
}

export interface CompletarCitaRequest {
  citaId: string
  notasCierre: string
}
