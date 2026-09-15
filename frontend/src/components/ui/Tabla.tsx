import { ArrowDown, ArrowUp, ArrowUpDown, Inbox } from 'lucide-react'
import { useMemo, useState, type ReactNode } from 'react'
import { Paginacion } from './Paginacion'
import './Tabla.css'

export interface ColumnaTabla<T> {
  /** Debe ser único dentro de la tabla — se usa como key de columna y, si
      `ordenable` es true, como identificador de orden. */
  id: string
  encabezado: ReactNode
  render: (fila: T) => ReactNode
  ordenable?: boolean
  /** Función de comparación para el orden por esta columna. Requerida si
      `ordenable` es true. */
  comparar?: (a: T, b: T) => number
  ancho?: string
  alinear?: 'izquierda' | 'derecha' | 'centro'
}

const ALINEACION_CSS: Record<NonNullable<ColumnaTabla<never>['alinear']>, 'left' | 'right' | 'center'> = {
  izquierda: 'left',
  derecha: 'right',
  centro: 'center',
}

interface TablaProps<T> {
  columnas: ColumnaTabla<T>[]
  filas: T[]
  claveFila: (fila: T) => string
  /** Filas por página. Si se omite, la tabla no pagina (muestra todas las filas). */
  filasPorPagina?: number
  /** Texto del estado vacío cuando `filas` está vacío. */
  textoVacio?: string
  onClickFila?: (fila: T) => void
  /** Clave de `claveFila` de la fila resaltada como activa/seleccionada. */
  filaActivaClave?: string
}

/**
 * Tabla reutilizable: columnas configurables, orden por columna (click en el
 * encabezado), paginación opcional y estado vacío consistente. Reemplaza las
 * tablas <table> escritas a mano en cada página — la lógica de negocio (qué
 * filas mostrar, qué hacer al hacer click) sigue viviendo en cada página.
 */
export function Tabla<T>({
  columnas,
  filas,
  claveFila,
  filasPorPagina,
  textoVacio = 'No hay resultados que coincidan con la búsqueda.',
  onClickFila,
  filaActivaClave,
}: TablaProps<T>) {
  const [ordenPor, setOrdenPor] = useState<{ id: string; direccion: 'asc' | 'desc' } | null>(null)
  const [paginaActual, setPaginaActual] = useState(1)

  const filasOrdenadas = useMemo(() => {
    if (!ordenPor) return filas
    const columna = columnas.find((c) => c.id === ordenPor.id)
    if (!columna?.comparar) return filas

    const copia = [...filas].sort(columna.comparar)
    return ordenPor.direccion === 'asc' ? copia : copia.reverse()
  }, [filas, ordenPor, columnas])

  const totalPaginas = filasPorPagina ? Math.max(1, Math.ceil(filasOrdenadas.length / filasPorPagina)) : 1
  const paginaSegura = Math.min(paginaActual, totalPaginas)
  const filasVisibles = filasPorPagina
    ? filasOrdenadas.slice((paginaSegura - 1) * filasPorPagina, paginaSegura * filasPorPagina)
    : filasOrdenadas

  const alternarOrden = (columna: ColumnaTabla<T>) => {
    if (!columna.ordenable || !columna.comparar) return
    setOrdenPor((actual) => {
      if (actual?.id !== columna.id) return { id: columna.id, direccion: 'asc' }
      if (actual.direccion === 'asc') return { id: columna.id, direccion: 'desc' }
      return null
    })
  }

  if (filas.length === 0) {
    return (
      <div className="ui-tabla-vacia">
        <Inbox size={28} aria-hidden="true" />
        <p>{textoVacio}</p>
      </div>
    )
  }

  return (
    <div className="ui-tabla-contenedor">
      <table className="ui-tabla">
        <thead>
          <tr>
            {columnas.map((columna) => (
              <th
                key={columna.id}
                style={{ width: columna.ancho, textAlign: columna.alinear ? ALINEACION_CSS[columna.alinear] : 'left' }}
                className={columna.ordenable ? 'ui-tabla-encabezado-ordenable' : undefined}
                onClick={() => alternarOrden(columna)}
              >
                <span className="ui-tabla-encabezado-contenido">
                  {columna.encabezado}
                  {columna.ordenable && (
                    <>
                      {ordenPor?.id === columna.id ? (
                        ordenPor.direccion === 'asc' ? (
                          <ArrowUp size={13} aria-hidden="true" />
                        ) : (
                          <ArrowDown size={13} aria-hidden="true" />
                        )
                      ) : (
                        <ArrowUpDown size={13} className="ui-tabla-encabezado-icono-inactivo" aria-hidden="true" />
                      )}
                    </>
                  )}
                </span>
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {filasVisibles.map((fila) => {
            const clave = claveFila(fila)
            return (
              <tr
                key={clave}
                className={[onClickFila && 'ui-tabla-fila-clickeable', filaActivaClave === clave && 'ui-tabla-fila-activa']
                  .filter(Boolean)
                  .join(' ')}
                onClick={() => onClickFila?.(fila)}
              >
                {columnas.map((columna) => (
                  <td key={columna.id} style={{ textAlign: columna.alinear ? ALINEACION_CSS[columna.alinear] : 'left' }}>
                    {columna.render(fila)}
                  </td>
                ))}
              </tr>
            )
          })}
        </tbody>
      </table>

      {filasPorPagina && (
        <Paginacion paginaActual={paginaSegura} totalPaginas={totalPaginas} onCambiarPagina={setPaginaActual} />
      )}
    </div>
  )
}
