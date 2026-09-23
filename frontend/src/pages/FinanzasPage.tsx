import { useEffect, useMemo, useState } from 'react'
import {
  Bar,
  CartesianGrid,
  ComposedChart,
  Legend,
  Line,
  ReferenceLine,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import { exportarCsv } from '../lib/exportarCsv'
import { coloresParaTema } from '../styles/colors'
import { useTheme } from '../theme/ThemeContext'
import type { GranularidadResumen, MovimientoImportante, ResumenMensual, ResumenPeriodo } from '../types/finanzasAdmin'
import './FinanzasPage.css'

type GranularidadSeleccionada = 'Mensual' | GranularidadResumen

interface PuntoGrafico {
  etiqueta: string
  mesNumero: number | null
  ingresos: number
  gastos: number
  ganancia: number
  fondoGanancias: number
  /** Variación % vs. el punto anterior — null en el primer punto, que no tiene con qué compararse. */
  cambioIngresos: number | null
  cambioGastos: number | null
  cambioGanancia: number | null
  cambioFondoGanancias: number | null
}

const CAMBIO_POR_CLAVE: Partial<Record<string, keyof PuntoGrafico>> = {
  ingresos: 'cambioIngresos',
  gastos: 'cambioGastos',
  ganancia: 'cambioGanancia',
  fondoGanancias: 'cambioFondoGanancias',
}

interface TooltipFinanzasProps {
  active?: boolean
  payload?: { value?: number; name?: string; color?: string; dataKey?: string; payload: PuntoGrafico }[]
  label?: string
}

// Tooltip a medida (en vez del `formatter` por defecto de <Tooltip>): además del monto de
// cada serie, muestra el % de cambio vs. el punto anterior -- para eso necesita el
// PuntoGrafico completo bajo el cursor (payload[0].payload, con los cambioX ya
// precalculados en datosGrafico), no solo el valor de la serie tocada.
function TooltipFinanzas({ active, payload, label }: TooltipFinanzasProps) {
  if (!active || !payload?.length) return null
  const fila = payload[0].payload

  return (
    <div className="finanzas-admin-grafico-tooltip">
      <p className="finanzas-admin-grafico-tooltip-label">{label}</p>
      {payload.map((entrada) => {
        const claveCambio = entrada.dataKey ? CAMBIO_POR_CLAVE[entrada.dataKey] : undefined
        const cambio = claveCambio ? (fila[claveCambio] as number | null) : null
        return (
          <p key={entrada.dataKey} className="finanzas-admin-grafico-tooltip-fila" style={{ color: entrada.color }}>
            {entrada.name}: {formateadorMoneda.format(Number(entrada.value))}
            {cambio !== null && (
              <span className={cambio >= 0 ? 'finanzas-admin-grafico-tooltip-cambio-positivo' : 'finanzas-admin-grafico-tooltip-cambio-negativo'}>
                {cambio >= 0 ? ' ▲' : ' ▼'} {Math.abs(cambio).toFixed(1)}%
              </span>
            )}
          </p>
        )
      })}
    </div>
  )
}

const NOMBRES_MES = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic']
const NOMBRES_MES_COMPLETO = [
  'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre',
]

const formateadorMoneda = new Intl.NumberFormat('es-DO', {
  style: 'currency',
  currency: 'DOP',
  maximumFractionDigits: 0,
})
const formateadorFechaHora = new Intl.DateTimeFormat('es-DO', { dateStyle: 'short', timeStyle: 'short' })

function construirRango(anio: number, mes: number | null): { desde: string; hasta: string } {
  if (mes) {
    return {
      desde: new Date(Date.UTC(anio, mes - 1, 1)).toISOString(),
      hasta: new Date(Date.UTC(anio, mes, 1)).toISOString(),
    }
  }
  return {
    desde: new Date(Date.UTC(anio, 0, 1)).toISOString(),
    hasta: new Date(Date.UTC(anio + 1, 0, 1)).toISOString(),
  }
}

export function FinanzasPage() {
  const { tema } = useTheme()
  const chartColors = coloresParaTema(tema)
  const anioActual = new Date().getFullYear()

  const [anio, setAnio] = useState(anioActual)
  const [mes, setMes] = useState<number | null>(null)
  const [resumenAnual, setResumenAnual] = useState<ResumenMensual[]>([])
  const [movimientos, setMovimientos] = useState<MovimientoImportante[]>([])
  const [cargandoResumen, setCargandoResumen] = useState(true)
  const [cargandoMovimientos, setCargandoMovimientos] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Semanal/Diaria solo tienen sentido acotados a un mes (52/365 puntos de golpe no se
  // leen) -- si no hay mes elegido, la granularidad efectiva cae a Mensual sin importar
  // qué botón haya quedado marcado, ver granularidadEfectiva más abajo.
  const [granularidad, setGranularidad] = useState<GranularidadSeleccionada>('Mensual')
  const [resumenPeriodo, setResumenPeriodo] = useState<ResumenPeriodo[]>([])
  const [cargandoPeriodo, setCargandoPeriodo] = useState(false)
  const granularidadEfectiva: GranularidadSeleccionada = mes ? granularidad : 'Mensual'

  useEffect(() => {
    let cancelado = false

    const cargarResumenAnual = (mostrarCargando: boolean) => {
      if (mostrarCargando) setCargandoResumen(true)
      return api
        .get<ResumenMensual[]>(`/api/finanzas-admin/resumen-anual?anio=${anio}`)
        .then((datos) => {
          if (!cancelado) setResumenAnual(datos)
        })
        .catch((err) => {
          if (!cancelado) setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el resumen anual.')
        })
        .finally(() => {
          if (!cancelado && mostrarCargando) setCargandoResumen(false)
        })
    }

    void cargarResumenAnual(true)
    // Ver el mismo comentario en el efecto de movimientos: sin refresco periódico, un
    // cobro o gasto nuevo no se veía reflejado en el gráfico hasta recargar la página.
    const intervalo = setInterval(() => void cargarResumenAnual(false), 20_000)

    return () => {
      cancelado = true
      clearInterval(intervalo)
    }
  }, [anio])

  useEffect(() => {
    let cancelado = false
    const { desde, hasta } = construirRango(anio, mes)

    const cargarMovimientos = (mostrarCargando: boolean) => {
      if (mostrarCargando) setCargandoMovimientos(true)
      return api
        .get<MovimientoImportante[]>(
          `/api/finanzas-admin/movimientos?desde=${encodeURIComponent(desde)}&hasta=${encodeURIComponent(hasta)}`,
        )
        .then((datos) => {
          if (!cancelado) setMovimientos(datos)
        })
        .catch((err) => {
          if (!cancelado) {
            setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar los movimientos.')
          }
        })
        .finally(() => {
          if (!cancelado && mostrarCargando) setCargandoMovimientos(false)
        })
    }

    void cargarMovimientos(true)
    // Sin esto, un cobro registrado en Cobros o un gasto registrado en Caja/Admin no
    // aparecía acá (ni en el gráfico de "Ganancias por mes") hasta recargar la página a
    // mano. Recarga silenciosa (sin tocar `cargandoMovimientos`) cada 20s.
    const intervalo = setInterval(() => void cargarMovimientos(false), 20_000)

    return () => {
      cancelado = true
      clearInterval(intervalo)
    }
  }, [anio, mes])

  useEffect(() => {
    if (granularidadEfectiva === 'Mensual') {
      setResumenPeriodo([])
      return
    }

    let cancelado = false
    const { desde, hasta } = construirRango(anio, mes)

    const cargarResumenPeriodo = (mostrarCargando: boolean) => {
      if (mostrarCargando) setCargandoPeriodo(true)
      return api
        .get<ResumenPeriodo[]>(
          `/api/finanzas-admin/resumen-periodo?desde=${encodeURIComponent(desde)}&hasta=${encodeURIComponent(hasta)}&granularidad=${granularidadEfectiva}`,
        )
        .then((datos) => {
          if (!cancelado) setResumenPeriodo(datos)
        })
        .catch((err) => {
          if (!cancelado) {
            setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar la tendencia.')
          }
        })
        .finally(() => {
          if (!cancelado && mostrarCargando) setCargandoPeriodo(false)
        })
    }

    void cargarResumenPeriodo(true)
    const intervalo = setInterval(() => void cargarResumenPeriodo(false), 20_000)

    return () => {
      cancelado = true
      clearInterval(intervalo)
    }
  }, [anio, mes, granularidadEfectiva])

  // % de cambio vs. el punto anterior de la MISMA serie (no vs. otra serie) -- null en el
  // primer punto, que no tiene predecesor con qué compararse. Se precalcula acá (no en el
  // tooltip) porque el tooltip de Recharts no tiene acceso directo al punto anterior, solo
  // al que está bajo el cursor.
  const datosGrafico: PuntoGrafico[] = useMemo(() => {
    const formateadorDia = new Intl.DateTimeFormat('es-DO', { day: 'numeric', month: 'short' })

    const base =
      granularidadEfectiva === 'Mensual'
        ? resumenAnual.map((m) => ({
            etiqueta: NOMBRES_MES[m.mes - 1],
            mesNumero: m.mes as number | null,
            ingresos: m.ingresos,
            gastos: m.gastos,
            ganancia: m.ganancia,
            fondoGanancias: m.fondoGanancias,
          }))
        : resumenPeriodo.map((p) => ({
            etiqueta:
              granularidadEfectiva === 'Semanal'
                ? `Sem. ${formateadorDia.format(new Date(`${p.periodo}T00:00:00`))}`
                : formateadorDia.format(new Date(`${p.periodo}T00:00:00`)),
            mesNumero: null as number | null,
            ingresos: p.ingresos,
            gastos: p.gastos,
            ganancia: p.ganancia,
            fondoGanancias: p.fondoGanancias,
          }))

    const calcularCambio = (actual: number, anterior: number): number | null => {
      if (anterior === 0) return null
      return ((actual - anterior) / Math.abs(anterior)) * 100
    }

    return base.map((punto, indice) => {
      const anterior = base[indice - 1]
      return {
        ...punto,
        cambioIngresos: anterior ? calcularCambio(punto.ingresos, anterior.ingresos) : null,
        cambioGastos: anterior ? calcularCambio(punto.gastos, anterior.gastos) : null,
        cambioGanancia: anterior ? calcularCambio(punto.ganancia, anterior.ganancia) : null,
        cambioFondoGanancias: anterior ? calcularCambio(punto.fondoGanancias, anterior.fondoGanancias) : null,
      }
    })
  }, [resumenAnual, resumenPeriodo, granularidadEfectiva])

  // Línea de referencia superpuesta en la tendencia: el promedio de ganancia neta de los
  // puntos visibles, para poder ver de un vistazo qué meses/semanas/días quedaron por
  // encima o por debajo de lo habitual, no solo el valor absoluto de cada uno.
  const promedioGanancia = useMemo(() => {
    if (datosGrafico.length === 0) return 0
    return datosGrafico.reduce((acumulado, p) => acumulado + p.ganancia, 0) / datosGrafico.length
  }, [datosGrafico])

  const kpis = useMemo(() => {
    const filas = mes ? resumenAnual.filter((m) => m.mes === mes) : resumenAnual
    return filas.reduce(
      (acumulado, fila) => ({
        ingresos: acumulado.ingresos + fila.ingresos,
        gastos: acumulado.gastos + fila.gastos,
        ganancia: acumulado.ganancia + fila.ganancia,
        fondoGanancias: acumulado.fondoGanancias + fila.fondoGanancias,
      }),
      { ingresos: 0, gastos: 0, ganancia: 0, fondoGanancias: 0 },
    )
  }, [resumenAnual, mes])

  const aniosDisponibles = Array.from({ length: 5 }, (_, i) => anioActual - i)
  const etiquetaPeriodo = mes ? `${NOMBRES_MES_COMPLETO[mes - 1]}_${anio}` : `${anio}`

  const tituloGrafico =
    granularidadEfectiva === 'Mensual'
      ? `Ganancias de ${anio} por mes — click en un mes para filtrar el detalle`
      : `Tendencia de ${mes ? NOMBRES_MES_COMPLETO[mes - 1] : ''} ${anio} — por ${granularidadEfectiva === 'Semanal' ? 'semana' : 'día'}`


  const handleExportar = () => {
    exportarCsv(
      `movimientos_${etiquetaPeriodo}.csv`,
      movimientos.map((movimiento) => ({
        fecha: formateadorFechaHora.format(new Date(movimiento.fecha)),
        origen: movimiento.origen,
        tipo: movimiento.tipo,
        concepto: movimiento.concepto,
        paciente: movimiento.pacienteNombre ?? '',
        monto: movimiento.monto,
      })),
    )
  }

  return (
    <DashboardLayout titulo="Finanzas">
      {error && <p className="finanzas-admin-error">{error}</p>}

      <h1 className="finanzas-admin-print-titulo">
        FUNBIDE — Reporte financiero — {mes ? `${NOMBRES_MES_COMPLETO[mes - 1]} ${anio}` : `Año ${anio}`}
      </h1>

      <div className="finanzas-admin-filtros no-imprimir">
        <select value={anio} onChange={(event) => setAnio(Number(event.target.value))}>
          {aniosDisponibles.map((opcion) => (
            <option key={opcion} value={opcion}>
              {opcion}
            </option>
          ))}
        </select>
        <select value={mes ?? ''} onChange={(event) => setMes(event.target.value ? Number(event.target.value) : null)}>
          <option value="">Todo el año</option>
          {NOMBRES_MES_COMPLETO.map((nombre, indice) => (
            <option key={nombre} value={indice + 1}>
              {nombre}
            </option>
          ))}
        </select>

        <div className="finanzas-admin-granularidad" role="group" aria-label="Granularidad de la tendencia">
          {(['Mensual', 'Semanal', 'Diaria'] as const).map((opcion) => (
            <button
              key={opcion}
              type="button"
              className={granularidadEfectiva === opcion ? 'activo' : ''}
              disabled={opcion !== 'Mensual' && !mes}
              title={opcion !== 'Mensual' && !mes ? 'Elegí un mes primero' : undefined}
              onClick={() => setGranularidad(opcion)}
            >
              {opcion}
            </button>
          ))}
        </div>

        <button type="button" onClick={handleExportar} disabled={movimientos.length === 0}>
          Exportar Excel
        </button>
        <button type="button" onClick={() => window.print()}>
          Imprimir reporte
        </button>
      </div>

      <div className="finanzas-admin-kpis">
        <section className="finanzas-admin-kpi-card">
          <p className="text-secondary">Ingresos</p>
          <p className="finanzas-admin-kpi-monto" style={{ color: chartColors.pacientes }}>
            {formateadorMoneda.format(kpis.ingresos)}
          </p>
        </section>
        <section className="finanzas-admin-kpi-card">
          <p className="text-secondary">Gastos</p>
          <p className="finanzas-admin-kpi-monto" style={{ color: chartColors.gasto }}>
            {formateadorMoneda.format(kpis.gastos)}
          </p>
        </section>
        <section className="finanzas-admin-kpi-card">
          <p className="text-secondary">Ganancia neta</p>
          <p className={`finanzas-admin-kpi-monto ${kpis.ganancia < 0 ? 'finanzas-admin-kpi-negativo' : ''}`}>
            {formateadorMoneda.format(kpis.ganancia)}
          </p>
        </section>
        <section className="finanzas-admin-kpi-card">
          <p className="text-secondary">Fondo de ganancias fundación</p>
          <p className="finanzas-admin-kpi-monto" style={{ color: chartColors.dinero }}>
            {formateadorMoneda.format(kpis.fondoGanancias)}
          </p>
        </section>
      </div>

      <section className="finanzas-admin-grafico-card">
        <p className="finanzas-admin-grafico-titulo text-secondary">{tituloGrafico}</p>
        {cargandoResumen || cargandoPeriodo ? (
          <p className="text-secondary cargando-pulso">Cargando…</p>
        ) : (
          <ResponsiveContainer width="100%" height={280}>
            <ComposedChart data={datosGrafico} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
              <CartesianGrid stroke={chartColors.gridline} vertical={false} />
              <XAxis
                dataKey="etiqueta"
                tickLine={false}
                axisLine={{ stroke: chartColors.baseline }}
                tick={{ fill: chartColors.textMuted, fontSize: 12 }}
              />
              <YAxis
                tickLine={false}
                axisLine={false}
                width={64}
                tick={{ fill: chartColors.textMuted, fontSize: 12 }}
                tickFormatter={(valor: number) => formateadorMoneda.format(valor)}
              />
              <Tooltip cursor={{ fill: chartColors.surface2 }} content={<TooltipFinanzas />} />
              <Legend wrapperStyle={{ fontSize: 12, color: chartColors.textMuted }} />
              <ReferenceLine
                y={promedioGanancia}
                stroke={chartColors.acentoPrimario}
                strokeDasharray="4 4"
                ifOverflow="extendDomain"
                label={{
                  value: `Promedio: ${formateadorMoneda.format(promedioGanancia)}`,
                  fill: chartColors.acentoPrimario,
                  fontSize: 11,
                  position: 'insideTopLeft',
                }}
              />
              <Bar
                dataKey="ingresos"
                name="Ingresos"
                fill={chartColors.pacientes}
                radius={[4, 4, 0, 0]}
                maxBarSize={28}
                onClick={(_datos: unknown, indice: number) => {
                  const mesDelPunto = datosGrafico[indice]?.mesNumero
                  if (mesDelPunto) setMes(mesDelPunto)
                }}
                cursor="pointer"
              />
              <Bar
                dataKey="gastos"
                name="Gastos"
                fill={chartColors.gasto}
                radius={[4, 4, 0, 0]}
                maxBarSize={28}
                onClick={(_datos: unknown, indice: number) => {
                  const mesDelPunto = datosGrafico[indice]?.mesNumero
                  if (mesDelPunto) setMes(mesDelPunto)
                }}
                cursor="pointer"
              />
              <Bar
                dataKey="fondoGanancias"
                name="Fondo de ganancias fundación"
                fill={chartColors.dinero}
                radius={[4, 4, 0, 0]}
                maxBarSize={28}
                onClick={(_datos: unknown, indice: number) => {
                  const mesDelPunto = datosGrafico[indice]?.mesNumero
                  if (mesDelPunto) setMes(mesDelPunto)
                }}
                cursor="pointer"
              />
              <Line
                dataKey="ganancia"
                name="Ganancia neta"
                stroke={chartColors.acentoPrimario}
                strokeWidth={2}
                dot={{ r: 3, fill: chartColors.acentoPrimario }}
                activeDot={{ r: 5 }}
              />
            </ComposedChart>
          </ResponsiveContainer>
        )}
      </section>

      <section className="finanzas-admin-tabla-card">
        <div className="finanzas-admin-tabla-header">
          <p className="finanzas-admin-tabla-titulo">
            Movimientos importantes — {mes ? NOMBRES_MES_COMPLETO[mes - 1] : 'todo el año'} {anio}
          </p>
          {mes && (
            <button type="button" className="finanzas-admin-limpiar-mes no-imprimir" onClick={() => setMes(null)}>
              Ver todo el año
            </button>
          )}
        </div>
        {cargandoMovimientos ? (
          <p className="text-secondary cargando-pulso">Cargando…</p>
        ) : movimientos.length === 0 ? (
          <p className="text-secondary">No hay movimientos en este período.</p>
        ) : (
          <div className="finanzas-admin-tabla-scroll">
            <table className="finanzas-admin-tabla">
              <thead>
                <tr>
                  <th>Fecha</th>
                  <th>Origen</th>
                  <th>Tipo</th>
                  <th>Concepto</th>
                  <th>Paciente</th>
                  <th>Monto</th>
                </tr>
              </thead>
              <tbody>
                {movimientos.map((movimiento) => (
                  <tr key={movimiento.id}>
                    <td className="text-muted">{formateadorFechaHora.format(new Date(movimiento.fecha))}</td>
                    <td className="text-muted">{movimiento.origen === 'Cobro' ? 'Cobro' : 'Manual'}</td>
                    <td className={movimiento.tipo === 'Egreso' ? 'finanzas-admin-tipo-egreso' : 'finanzas-admin-tipo-ingreso'}>
                      {movimiento.tipo}
                    </td>
                    <td>{movimiento.concepto}</td>
                    <td className="text-muted">{movimiento.pacienteNombre ?? '—'}</td>
                    <td>{formateadorMoneda.format(movimiento.monto)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </DashboardLayout>
  )
}
