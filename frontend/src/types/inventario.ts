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

export interface EditarInventarioItemRequest {
  inventarioItemId: string
  nombre: string
  stockActual: number
  stockMinimo: number
}

export interface DescargarInventarioRequest {
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
