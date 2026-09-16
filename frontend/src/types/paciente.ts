export type EstadoPaciente = 'Activo' | 'Seguimiento'

export const ESTADOS_PACIENTE: EstadoPaciente[] = ['Activo', 'Seguimiento']

export type OrdenPaciente = 'NombreAsc' | 'NombreDesc' | 'MasRecientes' | 'MasAntiguos'

export const OPCIONES_ORDEN_PACIENTE: { valor: OrdenPaciente; etiqueta: string }[] = [
  { valor: 'NombreAsc', etiqueta: 'Nombre (A-Z)' },
  { valor: 'NombreDesc', etiqueta: 'Nombre (Z-A)' },
  { valor: 'MasRecientes', etiqueta: 'Más recientes primero' },
  { valor: 'MasAntiguos', etiqueta: 'Más antiguos primero' },
]

export interface Paciente {
  id: string
  nombre: string
  apellido: string
  cedula: string
  telefono: string | null
  tieneFotoCedula: boolean
  edad: number | null
  condicion: string | null
  estado: EstadoPaciente
  ultimaVisita: string | null
  creadoEn: string
}

export interface CrearPacienteRequest {
  nombre: string
  apellido: string
  cedula: string
  telefono: string | null
  edad: number | null
  condicion: string | null
}

export interface EditarPacienteRequest {
  pacienteId: string
  nombre: string
  apellido: string
  cedula: string
  telefono: string | null
  edad: number | null
  condicion: string | null
  estado: EstadoPaciente
}

export interface UrlFotoCedula {
  url: string
}

export interface ImportarPacientesResultado {
  totalFilas: number
  creados: number
  actualizados: number
  omitidos: number
  identificacionesAjustadas: number
  omisiones: string[]
}

export interface PacientesPaginados {
  items: Paciente[]
  total: number
  pagina: number
  tamanoPagina: number
}
