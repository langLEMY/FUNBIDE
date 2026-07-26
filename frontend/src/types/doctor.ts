import type { EspecialidadMedica } from './usuario'

export interface DoctorSimple {
  id: string
  nombreCompleto: string
  especialidad: EspecialidadMedica | null
}
