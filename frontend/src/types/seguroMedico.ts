export interface SeguroMedico {
  id: string
  nombre: string
  /** Vestigial: ya no se pide al crear/editar — el cálculo automático por % que usaba esto está desactivado (ver CobrosPage, Tarifario por procedimiento). Solo puede venir cargado en aseguradoras viejas. */
  porcentajeCobertura: number | null
  activo: boolean
  /** Si tiene al menos un procedimiento cargado en el tarifario (ver TarifarioProcedimiento) — reemplaza el viejo chequeo por nombre "SENASA". */
  tieneTarifario: boolean
}

export interface CrearSeguroMedicoRequest {
  nombre: string
}

export interface EditarSeguroMedicoRequest {
  seguroMedicoId: string
  nombre: string
}
