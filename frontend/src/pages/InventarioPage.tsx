import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { InventarioRow } from '../components/inventario/InventarioRow'
import { api, ApiError } from '../lib/api'
import type { InventarioItem } from '../types/inventario'
import { CATEGORIAS_INVENTARIO } from '../types/inventario'
import './InventarioPage.css'

const FILTRO_TODOS = 'Todos'

export function InventarioPage() {
  const [items, setItems] = useState<InventarioItem[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busqueda, setBusqueda] = useState('')
  const [filtroCategoria, setFiltroCategoria] = useState<(typeof CATEGORIAS_INVENTARIO)[number] | typeof FILTRO_TODOS>(
    FILTRO_TODOS,
  )

  const [codigo, setCodigo] = useState('')
  const [nombre, setNombre] = useState('')
  const [categoria, setCategoria] = useState<(typeof CATEGORIAS_INVENTARIO)[number]>(CATEGORIAS_INVENTARIO[0])
  const [stockInicial, setStockInicial] = useState('')
  const [stockMinimo, setStockMinimo] = useState('')
  const [creando, setCreando] = useState(false)
  const [errorCrear, setErrorCrear] = useState<string | null>(null)

  useEffect(() => {
    let cancelado = false

    api
      .get<InventarioItem[]>('/api/inventario')
      .then((datos) => {
        if (!cancelado) setItems(datos)
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el inventario.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargando(false)
      })

    return () => {
      cancelado = true
    }
  }, [])

  const itemsFiltrados = useMemo(() => {
    const busquedaNormalizada = busqueda.trim().toLowerCase()
    return items.filter((item) => {
      const coincideCategoria = filtroCategoria === FILTRO_TODOS || item.categoria === filtroCategoria
      if (!coincideCategoria) return false
      if (!busquedaNormalizada) return true
      return `${item.codigo} ${item.nombre}`.toLowerCase().includes(busquedaNormalizada)
    })
  }, [items, busqueda, filtroCategoria])

  const ordenar = (lista: InventarioItem[]) => [...lista].sort((a, b) => a.nombre.localeCompare(b.nombre))

  const actualizarEnLista = (item: InventarioItem) => {
    setItems((actual) => actual.map((existente) => (existente.id === item.id ? item : existente)))
  }

  const handleCrear = async (event: FormEvent) => {
    event.preventDefault()
    setErrorCrear(null)

    const stockInicialNumero = Number(stockInicial || 0)
    const stockMinimoNumero = Number(stockMinimo || 0)
    if (!Number.isInteger(stockInicialNumero) || stockInicialNumero < 0) {
      setErrorCrear('El stock inicial debe ser un número entero mayor o igual a cero.')
      return
    }
    if (!Number.isInteger(stockMinimoNumero) || stockMinimoNumero < 0) {
      setErrorCrear('El stock mínimo debe ser un número entero mayor o igual a cero.')
      return
    }

    setCreando(true)
    try {
      const nuevo = await api.post<InventarioItem>('/api/inventario', {
        codigo,
        nombre,
        categoria,
        stockInicial: stockInicialNumero,
        stockMinimo: stockMinimoNumero,
      })
      setItems((actual) => ordenar([...actual, nuevo]))
      setCodigo('')
      setNombre('')
      setStockInicial('')
      setStockMinimo('')
    } catch (err) {
      setErrorCrear(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo agregar el ítem.')
    } finally {
      setCreando(false)
    }
  }

  return (
    <DashboardLayout titulo="Inventario">
      <section className="inventario-crear-card">
        <h2>Agregar medicamento o insumo</h2>
        <form className="inventario-crear-form" onSubmit={(event) => void handleCrear(event)}>
          <input placeholder="Código" value={codigo} onChange={(event) => setCodigo(event.target.value)} required />
          <input placeholder="Nombre" value={nombre} onChange={(event) => setNombre(event.target.value)} required />
          <select value={categoria} onChange={(event) => setCategoria(event.target.value as typeof categoria)}>
            {CATEGORIAS_INVENTARIO.map((opcion) => (
              <option key={opcion} value={opcion}>
                {opcion === 'Medicamento' ? 'Medicamento' : 'Insumo'}
              </option>
            ))}
          </select>
          <input
            type="number"
            min={0}
            placeholder="Stock inicial"
            value={stockInicial}
            onChange={(event) => setStockInicial(event.target.value)}
            required
          />
          <input
            type="number"
            min={0}
            placeholder="Stock mínimo"
            value={stockMinimo}
            onChange={(event) => setStockMinimo(event.target.value)}
            required
          />
          <button type="submit" disabled={creando}>
            {creando ? 'Agregando…' : 'Agregar'}
          </button>
        </form>
        {errorCrear && <p className="inventario-error">{errorCrear}</p>}
      </section>

      <div className="inventario-buscador">
        <input
          type="search"
          placeholder="Buscar por código o nombre…"
          value={busqueda}
          onChange={(event) => setBusqueda(event.target.value)}
        />
      </div>

      <div className="inventario-tabs" role="tablist">
        <button
          type="button"
          role="tab"
          aria-selected={filtroCategoria === FILTRO_TODOS}
          className={`inventario-tab${filtroCategoria === FILTRO_TODOS ? ' activo' : ''}`}
          onClick={() => setFiltroCategoria(FILTRO_TODOS)}
        >
          Todos
        </button>
        {CATEGORIAS_INVENTARIO.map((opcionCategoria) => (
          <button
            key={opcionCategoria}
            type="button"
            role="tab"
            aria-selected={filtroCategoria === opcionCategoria}
            className={`inventario-tab${filtroCategoria === opcionCategoria ? ' activo' : ''}`}
            onClick={() => setFiltroCategoria(opcionCategoria)}
          >
            {opcionCategoria === 'Medicamento' ? 'Medicamentos' : 'Insumos'}
          </button>
        ))}
      </div>

      <section className="inventario-tabla-card">
        {error && <p className="inventario-error">{error}</p>}

        {cargando ? (
          <p className="text-secondary">Cargando inventario…</p>
        ) : itemsFiltrados.length === 0 ? (
          <p className="text-secondary">
            {items.length === 0
              ? 'Todavía no hay ítems de inventario registrados.'
              : 'No hay artículos que coincidan con la búsqueda o el filtro.'}
          </p>
        ) : (
          <table className="inventario-tabla">
            <thead>
              <tr>
                <th>Artículo</th>
                <th>Categoría</th>
                <th>Existencia</th>
                <th>Mínimo</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {itemsFiltrados.map((item) => (
                <InventarioRow
                  key={item.id}
                  item={item}
                  onDespachado={actualizarEnLista}
                  onEditado={actualizarEnLista}
                />
              ))}
            </tbody>
          </table>
        )}
      </section>
    </DashboardLayout>
  )
}
