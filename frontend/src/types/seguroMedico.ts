export interface SeguroMedico {
  id: string
  nombre: string
  porcentajeCobertura: number
  activo: boolean
}

export interface CrearSeguroMedicoRequest {
  nombre: string
  porcentajeCobertura: number
}

export interface EditarSeguroMedicoRequest {
  seguroMedicoId: string
  nombre: string
  porcentajeCobertura: number
}
