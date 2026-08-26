export interface ResumenDiario {
  fecha: string
  pacientesAtendidos: number
  dineroMovido: number
  dineroEfectivo: number
  dineroTarjeta: number
  dineroTransferencia: number
}

export interface ItemStockBajo {
  id: string
  codigo: string
  nombre: string
  stockActual: number
  stockMinimo: number
}

export interface PacienteConDeuda {
  pacienteId: string
  pacienteNombre: string
  montoAdeudado: number
}

export interface AlertasAdmin {
  stockBajo: ItemStockBajo[]
  pacientesConDeuda: PacienteConDeuda[]
}

export interface SesionesActivas {
  cantidad: number
}

export interface PacientesPorDoctor {
  doctorId: string
  nombreCompleto: string
  especialidad: string | null
  citasCompletadas: number
  pacientesDistintos: number
}
