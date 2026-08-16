export type MetodoPago = 'Efectivo' | 'Tarjeta' | 'Transferencia'

export const METODOS_PAGO: MetodoPago[] = ['Efectivo', 'Tarjeta', 'Transferencia']

export interface Pago {
  metodo: MetodoPago
  monto: number
}

export interface Cobro {
  id: string
  pacienteId: string
  pacienteNombre: string
  citaId: string | null
  turnoCajaId: string
  concepto: string
  montoTotal: number
  seguroMedicoId: string | null
  seguroMedicoNombre: string | null
  porcentajeCobertura: number | null
  montoCobertura: number | null
  tarifarioProcedimientoId: string | null
  codigoAutorizacion: string | null
  /** Desglose de cómo se pagó — 0, 1 o varias líneas (ej. una parte con tarjeta y otra en efectivo). */
  pagos: Pago[]
  montoACargoPaciente: number
  montoPagado: number
  montoPendiente: number
  usuarioId: string
  registradoEn: string
}

export interface RegistrarCobroRequest {
  pacienteId: string
  citaId: string | null
  concepto: string
  montoTotal: number
  pagos: Pago[]
  seguroMedicoId: string | null
  codigoAutorizacion: string | null
  tarifarioProcedimientoId?: string | null
}

export interface DeudaPaciente {
  pacienteId: string
  montoTotalAdeudado: number
}
