export interface Paciente {
  id: string
  nombre: string
  apellido: string
  cedula: string
  telefono: string | null
  tieneFotoCedula: boolean
}

export interface CrearPacienteRequest {
  nombre: string
  apellido: string
  cedula: string
  telefono: string | null
}

export interface EditarPacienteRequest {
  pacienteId: string
  nombre: string
  apellido: string
  cedula: string
  telefono: string | null
}

export interface UrlFotoCedula {
  url: string
}
