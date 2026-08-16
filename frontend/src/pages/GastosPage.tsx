import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import type { ResumenMensual } from '../types/finanzasAdmin'
import './GastosPage.css'

const NOMBRES_MES_COMPLETO = [
  'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre',
]

const formateadorMoneda = new Intl.NumberFormat('es-DO', {
  style: 'currency',
  currency: 'DOP',
  maximumFractionDigits: 0,
})

const UMBRAL_ETIQUETA_INTERNA = 15

export function GastosPage() {
  const anioActual = new Date().getFullYear()

  const [anio, setAnio] = useState(anioActual)
  const [mes, setMes] = useState<number | null>(null)
  const [resumenAnual, setResumenAnual] = useState<ResumenMensual[]>([])
  const [cargandoResumen, setCargandoResumen] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [concepto, setConcepto] = useState('')
  const [monto, setMonto] = useState('')
  const [registrando, setRegistrando] = useState(false)
  const [errorGasto, setErrorGasto] = useState<string | null>(null)
  const [exitoGasto, setExitoGasto] = useState(false)

  const [recargarClave, setRecargarClave] = useState(0)
  const recargar = () => setRecargarClave((clave) => clave + 1)

  useEffect(() => {
    let cancelado = false
    setCargandoResumen(true)
    api
      .get<ResumenMensual[]>(`/api/finanzas-admin/resumen-anual?anio=${anio}`)
      .then((datos) => {
        if (!cancelado) setResumenAnual(datos)
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el resumen.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargandoResumen(false)
      })

    return () => {
      cancelado = true
    }
  }, [anio, recargarClave])

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

  const total = kpis.ingresos + kpis.gastos
  const ingresosPct = total > 0 ? (kpis.ingresos / total) * 100 : 0
  const gastosPct = total > 0 ? (kpis.gastos / total) * 100 : 0
  const margenPct = kpis.ingresos > 0 ? (kpis.ganancia / kpis.ingresos) * 100 : 0

  const handleRegistrarGasto = async (event: FormEvent) => {
    event.preventDefault()
    setErrorGasto(null)
    setExitoGasto(false)

    const montoNumero = Number(monto)
    if (!concepto.trim()) {
      setErrorGasto('El concepto es obligatorio.')
      return
    }
    if (!monto.trim() || !Number.isFinite(montoNumero) || montoNumero <= 0) {
      setErrorGasto('Ingresa un monto válido, mayor que cero.')
      return
    }

    setRegistrando(true)
    try {
      await api.post('/api/finanzas-admin/gastos', { concepto: concepto.trim(), monto: montoNumero })
      setConcepto('')
      setMonto('')
      setExitoGasto(true)
      recargar()
    } catch (err) {
      setErrorGasto(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo registrar el gasto.')
    } finally {
      setRegistrando(false)
    }
  }

  const aniosDisponibles = Array.from({ length: 5 }, (_, i) => anioActual - i)
  const etiquetaPeriodo = mes ? `${NOMBRES_MES_COMPLETO[mes - 1]} ${anio}` : `todo ${anio}`

  return (
    <DashboardLayout titulo="Gastos">
      {error && <p className="gastos-error">{error}</p>}

      <div className="gastos-filtros">
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
      </div>

      <div className="gastos-kpis">
        <section className="gastos-kpi-card">
          <p className="text-secondary">Ingresos</p>
          <p className="gastos-kpi-monto gastos-kpi-ingresos">{formateadorMoneda.format(kpis.ingresos)}</p>
        </section>
        <section className="gastos-kpi-card">
          <p className="text-secondary">Gastos</p>
          <p className="gastos-kpi-monto gastos-kpi-gastos">{formateadorMoneda.format(kpis.gastos)}</p>
        </section>
        <section className="gastos-kpi-card">
          <p className="text-secondary">Ganancia neta</p>
          <p className={`gastos-kpi-monto ${kpis.ganancia < 0 ? 'gastos-kpi-negativo' : ''}`}>
            {formateadorMoneda.format(kpis.ganancia)}
          </p>
        </section>
        <section className="gastos-kpi-card">
          <p className="text-secondary">Margen</p>
          <p className={`gastos-kpi-monto ${margenPct < 0 ? 'gastos-kpi-negativo' : ''}`}>
            {kpis.ingresos > 0 ? `${margenPct.toFixed(0)}%` : '—'}
          </p>
        </section>
      </div>

      <section className="gastos-balance-card">
        <p className="gastos-balance-titulo text-secondary">Ganancias vs. gastos — {etiquetaPeriodo}</p>

        {cargandoResumen ? (
          <p className="text-secondary cargando-pulso">Cargando…</p>
        ) : total === 0 ? (
          <p className="text-secondary">No hay movimientos en este período.</p>
        ) : (
          <>
            <div className="gastos-barra-track" role="img" aria-label={`Ingresos ${ingresosPct.toFixed(0)}%, gastos ${gastosPct.toFixed(0)}%`}>
              <div className="gastos-barra-segmento gastos-barra-ingresos" style={{ width: `${ingresosPct}%` }}>
                {ingresosPct >= UMBRAL_ETIQUETA_INTERNA && <span>{ingresosPct.toFixed(0)}%</span>}
              </div>
              <div className="gastos-barra-segmento gastos-barra-gastos" style={{ width: `${gastosPct}%` }}>
                {gastosPct >= UMBRAL_ETIQUETA_INTERNA && <span>{gastosPct.toFixed(0)}%</span>}
              </div>
            </div>

            <ul className="gastos-leyenda">
              <li>
                <span className="gastos-leyenda-punto gastos-leyenda-ingresos" />
                Ingresos — {ingresosPct.toFixed(0)}% ({formateadorMoneda.format(kpis.ingresos)})
              </li>
              <li>
                <span className="gastos-leyenda-punto gastos-leyenda-gastos" />
                Gastos — {gastosPct.toFixed(0)}% ({formateadorMoneda.format(kpis.gastos)})
              </li>
            </ul>
          </>
        )}
      </section>

      <section className="gastos-registrar-card">
        <h2>Registrar gasto</h2>
        <p className="text-secondary gastos-registrar-subtitulo">
          Alquiler, nómina, insumos u otro gasto operativo de la fundación — no requiere una caja abierta.
        </p>
        <form className="gastos-registrar-form" onSubmit={(event) => void handleRegistrarGasto(event)}>
          <input
            placeholder="Concepto"
            value={concepto}
            onChange={(event) => setConcepto(event.target.value)}
            required
          />
          <input
            type="number"
            min={0.01}
            step="0.01"
            placeholder="Monto"
            value={monto}
            onChange={(event) => setMonto(event.target.value)}
            required
          />
          <button type="submit" disabled={registrando}>
            {registrando ? 'Registrando…' : 'Registrar gasto'}
          </button>
        </form>
        {errorGasto && <p className="gastos-error">{errorGasto}</p>}
        {exitoGasto && <p className="gastos-exito">Gasto registrado correctamente.</p>}
      </section>
    </DashboardLayout>
  )
}
