import { useState } from 'react'
import { api, ApiError } from '../../lib/api'
import type { InventarioItem, MovimientoInventarioDto } from '../../types/inventario'

interface InventarioRowProps {
  item: InventarioItem
  onDespachado: (item: InventarioItem) => void
}

export function InventarioRow({ item, onDespachado }: InventarioRowProps) {
  const [despachando, setDespachando] = useState(false)
  const [cantidad, setCantidad] = useState('')
  const [referencia, setReferencia] = useState('')
  const [enviando, setEnviando] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const bajoMinimo = item.stockActual < item.stockMinimo

  const cancelar = () => {
    setDespachando(false)
    setCantidad('')
    setReferencia('')
    setError(null)
  }

  const confirmarDespacho = async () => {
    const cantidadNumero = Number(cantidad)
    if (!cantidad.trim() || !Number.isInteger(cantidadNumero) || cantidadNumero <= 0) {
      setError('Ingresa una cantidad válida.')
      return
    }

    setError(null)
    setEnviando(true)
    try {
      const movimiento = await api.post<MovimientoInventarioDto>('/api/inventario/descargo', {
        inventarioItemId: item.id,
        cantidad: cantidadNumero,
        referencia: referencia.trim() || null,
      })
      onDespachado({ ...item, stockActual: movimiento.stockResultante })
      cancelar()
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo despachar el ítem.')
    } finally {
      setEnviando(false)
    }
  }

  return (
    <>
      <tr>
        <td>
          {item.nombre} <span className="text-muted">({item.codigo})</span>
        </td>
        <td className="text-muted">{item.categoria === 'Medicamento' ? 'Medicamentos' : 'Insumos'}</td>
        <td>{item.stockActual}</td>
        <td className="text-muted">{item.stockMinimo}</td>
        <td>
          <span className={`inventario-badge ${bajoMinimo ? 'inventario-badge-bajo' : 'inventario-badge-ok'}`}>
            {bajoMinimo ? 'Bajo' : 'OK'}
          </span>
        </td>
        <td className="inventario-acciones">
          {despachando ? (
            <div className="inventario-despacho-form">
              <input
                type="number"
                min={1}
                placeholder="Cantidad"
                value={cantidad}
                onChange={(event) => setCantidad(event.target.value)}
              />
              <input
                placeholder="Referencia (opcional)"
                value={referencia}
                onChange={(event) => setReferencia(event.target.value)}
              />
              <button type="button" onClick={() => void confirmarDespacho()} disabled={enviando}>
                {enviando ? 'Despachando…' : 'Confirmar'}
              </button>
              <button type="button" onClick={cancelar} disabled={enviando}>
                Cancelar
              </button>
            </div>
          ) : (
            <button type="button" onClick={() => setDespachando(true)} disabled={item.stockActual <= 0}>
              Despachar
            </button>
          )}
        </td>
      </tr>
      {error && (
        <tr>
          <td colSpan={6} className="inventario-row-error">
            {error}
          </td>
        </tr>
      )}
    </>
  )
}
