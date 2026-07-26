import { useEffect, useMemo, useState } from 'react'
import { Bar, BarChart, CartesianGrid, Legend, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import { exportarCsv } from '../lib/exportarCsv'
import { coloresParaTema } from '../styles/colors'
import { useTheme } from '../theme/ThemeContext'
import type { MovimientoImportante, ResumenMensual } from '../types/finanzasAdmin'
import './FinanzasPage.css'

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

  useEffect(() => {
    let cancelado = false
    setCargandoResumen(true)
    api
      .get<ResumenMensual[]>(`/api/finanzas-admin/resumen-anual?anio=${anio}`)
      .then((datos) => {
        if (!cancelado) setResumenAnual(datos)
      })
      .catch((err) => {
        if (!cancelado) setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el resumen anual.')
      })
      .finally(() => {
        if (!cancelado) setCargandoResumen(false)
      })
    return () => {
      cancelado = true
    }
  }, [anio])

  const cargarMovimientos = () => {
    const { desde, hasta } = construirRango(anio, mes)
    setCargandoMovimientos(true)
    api
      .get<MovimientoImportante[]>(
        `/api/finanzas-admin/movimientos?desde=${encodeURIComponent(desde)}&hasta=${encodeURIComponent(hasta)}`,
      )
      .then((datos) => setMovimientos(datos))
      .catch((err) => {
        setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar los movimientos.')
      })
      .finally(() => setCargandoMovimientos(false))
  }

  useEffect(cargarMovimientos, [anio, mes])

  const datosGrafico = useMemo(
    () =>
      resumenAnual.map((m) => ({
        mes: NOMBRES_MES[m.mes - 1],
        mesNumero: m.mes,
        ingresos: m.ingresos,
        gastos: m.gastos,
        ganancia: m.ganancia,
      })),
    [resumenAnual],
  )

  const kpis = useMemo(() => {
    const filas = mes ? resumenAnual.filter((m) => m.mes === mes) : resumenAnual
    return filas.reduce(
      (acumulado, fila) => ({
        ingresos: acumulado.ingresos + fila.ingresos,
        gastos: acumulado.gastos + fila.gastos,
        ganancia: acumulado.ganancia + fila.ganancia,
      }),
      { ingresos: 0, gastos: 0, ganancia: 0 },
    )
  }, [resumenAnual, mes])

  const aniosDisponibles = Array.from({ length: 5 }, (_, i) => anioActual - i)
  const etiquetaPeriodo = mes ? `${NOMBRES_MES_COMPLETO[mes - 1]}_${anio}` : `${anio}`

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
      </div>

      <section className="finanzas-admin-grafico-card">
        <p className="finanzas-admin-grafico-titulo text-secondary">
          Ganancias de {anio} por mes — click en un mes para filtrar el detalle
        </p>
        {cargandoResumen ? (
          <p className="text-secondary cargando-pulso">Cargando…</p>
        ) : (
          <ResponsiveContainer width="100%" height={280}>
            <BarChart data={datosGrafico} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
              <CartesianGrid stroke={chartColors.gridline} vertical={false} />
              <XAxis
                dataKey="mes"
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
              <Tooltip
                cursor={{ fill: chartColors.surface2 }}
                contentStyle={{
                  background: chartColors.surface2,
                  border: `1px solid ${chartColors.borderHairline}`,
                  borderRadius: 8,
                  fontSize: 13,
                }}
                labelStyle={{ color: chartColors.textMuted }}
                formatter={(valor, nombre) => [formateadorMoneda.format(Number(valor)), nombre]}
              />
              <Legend wrapperStyle={{ fontSize: 12, color: chartColors.textMuted }} />
              <Bar
                dataKey="ingresos"
                name="Ingresos"
                fill={chartColors.pacientes}
                radius={[4, 4, 0, 0]}
                maxBarSize={28}
                onClick={(_datos: unknown, indice: number) => setMes(datosGrafico[indice]?.mesNumero ?? null)}
                cursor="pointer"
              />
              <Bar
                dataKey="gastos"
                name="Gastos"
                fill={chartColors.gasto}
                radius={[4, 4, 0, 0]}
                maxBarSize={28}
                onClick={(_datos: unknown, indice: number) => setMes(datosGrafico[indice]?.mesNumero ?? null)}
                cursor="pointer"
              />
            </BarChart>
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
