export interface Empleado {
  id: string
  nombreCompleto: string
  cargo: string | null
}

export interface CrearEmpleadoRequest {
  nombreCompleto: string
  cargo: string | null
}

export interface EditarEmpleadoRequest {
  empleadoId: string
  nombreCompleto: string
  cargo: string | null
}
