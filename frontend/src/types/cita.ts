export type EstadoCita = 'Pendiente' | 'Programada' | 'EnEspera' | 'Completada' | 'Cancelada'

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

export interface CancelarCitaRequest {
  citaId: string
}

export interface AgendarCitaRequest {
  pacienteId: string
  doctorId: string
  motivo: string
  inicio: string
  fin: string
}

export interface RegistrarLlegadaRequest {
  citaId: string
}

export interface RegistrarLlegadaSinCitaRequest {
  pacienteId: string
  doctorId: string
  motivo: string
}

// Paciente que alguna vez tuvo una cita con el doctor autenticado (Dashboard de Doctor).
export interface PacienteDelDoctor {
  pacienteId: string
  nombreCompleto: string
}

// Variante de Cita enriquecida con nombres, para Agenda/Recepción (listan citas de todos los doctores).
export interface CitaAgenda {
  id: string
  pacienteId: string
  pacienteNombre: string
  doctorId: string
  doctorNombre: string
  motivo: string
  estado: EstadoCita
  inicio: string | null
  fin: string | null
  deudaPendiente: number
}
