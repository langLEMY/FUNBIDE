export interface SeguroMedico {
  id: string
  nombre: string
  porcentajeCobertura: number
  activo: boolean
  /** Si tiene al menos un procedimiento cargado en el tarifario (ver TarifarioProcedimiento) — reemplaza el viejo chequeo por nombre "SENASA". */
  tieneTarifario: boolean
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
