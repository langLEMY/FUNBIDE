export type CategoriaInventario = 'Medicamento' | 'Insumo'

export const CATEGORIAS_INVENTARIO: CategoriaInventario[] = ['Medicamento', 'Insumo']

export interface InventarioItem {
  id: string
  codigo: string
  nombre: string
  stockActual: number
  categoria: CategoriaInventario
  stockMinimo: number
}

export interface CrearInventarioItemRequest {
  codigo: string
  nombre: string
  stockInicial: number
  categoria: CategoriaInventario
  stockMinimo: number
}

// Ya no trae stockActual: el stock ya no se corrige escribiendo un número absoluto acá —
// ver RegistrarEntradaInventarioRequest/DescargarInventarioRequest, los únicos dos caminos
// para mover stock ahora, ambos con historial (MovimientoInventarioDto).
export interface EditarInventarioItemRequest {
  inventarioItemId: string
  nombre: string
  stockMinimo: number
}

export interface DescargarInventarioRequest {
  inventarioItemId: string
  cantidad: number
  referencia: string | null
}

export interface RegistrarEntradaInventarioRequest {
  inventarioItemId: string
  cantidad: number
  referencia: string | null
}

export interface MovimientoInventarioDto {
  id: string
  inventarioItemId: string
  codigoItem: string
  cantidad: number
  stockResultante: number
  registradoEn: string
}

export interface ImportarInventarioResultado {
  totalFilas: number
  creados: number
  actualizados: number
  omitidos: number
  omisiones: string[]
}
