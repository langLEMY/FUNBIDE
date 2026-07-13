import { useState } from 'react'
import { api, ApiError } from '../../lib/api'
import type { InventarioItem, MovimientoInventarioDto } from '../../types/inventario'

interface InventarioRowProps {
  item: InventarioItem
  onDespachado: (item: InventarioItem) => void
  onEditado: (item: InventarioItem) => void
  onEliminado: (itemId: string) => void
}

export function InventarioRow({ item, onDespachado, onEditado, onEliminado }: InventarioRowProps) {
  const [despachando, setDespachando] = useState(false)
  const [cantidad, setCantidad] = useState('')
  const [referencia, setReferencia] = useState('')

  const [editando, setEditando] = useState(false)
  const [nombreEdit, setNombreEdit] = useState(item.nombre)
  const [stockActualEdit, setStockActualEdit] = useState(item.stockActual.toString())
  const [stockMinimoEdit, setStockMinimoEdit] = useState(item.stockMinimo.toString())

  const [enviando, setEnviando] = useState(false)
  const [eliminando, setEliminando] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const bajoMinimo = item.stockActual < item.stockMinimo

  const cancelarDespacho = () => {
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
      cancelarDespacho()
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo despachar el ítem.')
    } finally {
      setEnviando(false)
    }
  }

  // Resincroniza los campos con el item vigente al ENTRAR a edición (no solo al montar el
  // componente): sin esto, despachar stock desde esta misma fila y después abrir "Editar"
  // sin recargar la página mostraba el stock viejo, y guardar sin tocarlo lo revertía en
  // silencio (AjustarStock toma stockActual como valor absoluto, no como delta).
  const iniciarEdicion = () => {
    setNombreEdit(item.nombre)
    setStockActualEdit(item.stockActual.toString())
    setStockMinimoEdit(item.stockMinimo.toString())
    setEditando(true)
  }

  const cancelarEdicion = () => {
    setNombreEdit(item.nombre)
    setStockActualEdit(item.stockActual.toString())
    setStockMinimoEdit(item.stockMinimo.toString())
    setEditando(false)
    setError(null)
  }

  const guardarEdicion = async () => {
    const stockActualNumero = Number(stockActualEdit)
    const stockMinimoNumero = Number(stockMinimoEdit)
    if (!nombreEdit.trim()) {
      setError('El nombre es obligatorio.')
      return
    }
    if (!Number.isInteger(stockActualNumero) || stockActualNumero < 0) {
      setError('La disponibilidad debe ser un número entero mayor o igual a cero.')
      return
    }
    if (!Number.isInteger(stockMinimoNumero) || stockMinimoNumero < 0) {
      setError('El stock mínimo debe ser un número entero mayor o igual a cero.')
      return
    }

    setError(null)
    setEnviando(true)
    try {
      const actualizado = await api.patch<InventarioItem>('/api/inventario', {
        inventarioItemId: item.id,
        nombre: nombreEdit.trim(),
        stockActual: stockActualNumero,
        stockMinimo: stockMinimoNumero,
      })
      onEditado(actualizado)
      setEditando(false)
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo guardar el cambio.')
    } finally {
      setEnviando(false)
    }
  }

  const handleEliminar = async () => {
    if (!window.confirm(`¿Eliminar "${item.nombre}" (${item.codigo}) del inventario?`)) {
      return
    }

    setError(null)
    setEliminando(true)
    try {
      await api.delete(`/api/inventario/${item.id}`)
      onEliminado(item.id)
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo eliminar el ítem.')
      setEliminando(false)
    }
  }

  if (editando) {
    return (
      <tr>
        <td className="inventario-celda-doble">
          <input value={nombreEdit} onChange={(event) => setNombreEdit(event.target.value)} placeholder="Nombre" />
          <span className="text-muted">({item.codigo})</span>
        </td>
        <td className="text-muted">{item.categoria === 'Medicamento' ? 'Medicamentos' : 'Insumos'}</td>
        <td>
          <input
            type="number"
            min={0}
            value={stockActualEdit}
            onChange={(event) => setStockActualEdit(event.target.value)}
          />
        </td>
        <td>
          <input
            type="number"
            min={0}
            value={stockMinimoEdit}
            onChange={(event) => setStockMinimoEdit(event.target.value)}
          />
        </td>
        <td className="text-muted">—</td>
        <td className="inventario-acciones">
          <button type="button" onClick={() => void guardarEdicion()} disabled={enviando}>
            {enviando ? 'Guardando…' : 'Guardar'}
          </button>
          <button type="button" onClick={cancelarEdicion} disabled={enviando}>
            Cancelar
          </button>
          {error && <p className="inventario-row-error-inline">{error}</p>}
        </td>
      </tr>
    )
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
              <button type="button" onClick={cancelarDespacho} disabled={enviando}>
                Cancelar
              </button>
            </div>
          ) : (
            <>
              <button type="button" onClick={iniciarEdicion} disabled={eliminando}>
                Editar
              </button>
              <button
                type="button"
                onClick={() => setDespachando(true)}
                disabled={item.stockActual <= 0 || eliminando}
              >
                Despachar
              </button>
              <button
                type="button"
                className="inventario-boton-peligro"
                onClick={() => void handleEliminar()}
                disabled={eliminando}
              >
                {eliminando ? 'Eliminando…' : 'Eliminar'}
              </button>
            </>
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
