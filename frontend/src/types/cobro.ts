export type MetodoPago = 'Efectivo' | 'Tarjeta' | 'Transferencia'

export const METODOS_PAGO: MetodoPago[] = ['Efectivo', 'Tarjeta', 'Transferencia']

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
  codigoAutorizacion: string | null
  metodoPago: MetodoPago
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
  metodoPago: MetodoPago
  montoPagado: number
  seguroMedicoId: string | null
  codigoAutorizacion: string | null
}

export interface DeudaPaciente {
  pacienteId: string
  montoTotalAdeudado: number
}
