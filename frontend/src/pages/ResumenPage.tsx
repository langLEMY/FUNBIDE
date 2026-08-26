import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import { coloresParaTema } from '../styles/colors'
import { useTheme } from '../theme/ThemeContext'
import type { DoctorSimple } from '../types/doctor'
import './ResumenPage.css'

interface ResumenPorDoctor {
  pacientesAtendidos: number
  dineroGenerado: number
}

interface RegistroAuditoria {
  id: string
  usuarioId: string | null
  accion: string
  recurso: string
  codigoRespuestaHttp: number | null
  detalle: string
  registradoEn: string
}

interface DetalleMovimiento {
  Tipo: 'Ingreso' | 'Egreso'
  Monto: number
  Concepto: string
}

function parsearDetalleMovimiento(detalle: string): DetalleMovimiento | null {
  try {
    const datos = JSON.parse(detalle) as Partial<DetalleMovimiento>
    if (typeof datos.Monto !== 'number' || typeof datos.Concepto !== 'string' || !datos.Tipo) {
      return null
    }
    return datos as DetalleMovimiento
  } catch {
    return null
  }
}

function aFechaISO(fecha: Date): string {
  return fecha.toISOString().slice(0, 10)
}

function inicioDeHoy(): Date {
  const ahora = new Date()
  ahora.setHours(0, 0, 0, 0)
  return ahora
}

function sumarDias(fecha: Date, dias: number): Date {
  const copia = new Date(fecha)
  copia.setDate(copia.getDate() + dias)
  return copia
}

const PRESETS: { etiqueta: string; calcularInicio: (fin: Date) => Date }[] = [
  { etiqueta: 'Hoy', calcularInicio: (fin) => fin },
  { etiqueta: 'Últimos 7 días', calcularInicio: (fin) => sumarDias(fin, -7) },
  { etiqueta: 'Últimos 30 días', calcularInicio: (fin) => sumarDias(fin, -30) },
  { etiqueta: 'Este mes', calcularInicio: (fin) => new Date(fin.getFullYear(), fin.getMonth(), 1) },
]

const formateadorFechaHora = new Intl.DateTimeFormat('es-DO', { dateStyle: 'short', timeStyle: 'short' })
const formateadorFecha = new Intl.DateTimeFormat('es-DO', { dateStyle: 'medium' })
const formateadorMoneda = new Intl.NumberFormat('es-DO', {
  style: 'currency',
  currency: 'DOP',
  maximumFractionDigits: 0,
})
const formateadorEntero = new Intl.NumberFormat('es-DO')

export function ResumenPage() {
  const { tema } = useTheme()
  const chartColors = coloresParaTema(tema)
  const hoy = inicioDeHoy()
  const [desde, setDesde] = useState(aFechaISO(sumarDias(hoy, -7)))
  const [hasta, setHasta] = useState(aFechaISO(hoy))
  const [presetActivo, setPresetActivo] = useState<string | null>('Últimos 7 días')
  const [registros, setRegistros] = useState<RegistroAuditoria[]>([])
  const [cargando, setCargando] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [doctores, setDoctores] = useState<DoctorSimple[]>([])
  const [doctorSeleccionado, setDoctorSeleccionado] = useState('')
  const [resumenDoctor, setResumenDoctor] = useState<ResumenPorDoctor | null>(null)
  const [cargandoDoctor, setCargandoDoctor] = useState(false)
  const [errorDoctor, setErrorDoctor] = useState<string | null>(null)

  const cargar = async (desdeValor: string, hastaValor: string) => {
    setCargando(true)
    setError(null)
    try {
      const desdeIso = encodeURIComponent(`${desdeValor}T00:00:00.000Z`)
      const hastaIso = encodeURIComponent(`${hastaValor}T23:59:59.999Z`)
      const datos = await api.get<RegistroAuditoria[]>(`/api/auditoria?desde=${desdeIso}&hasta=${hastaIso}`)
      // El Resumen de ADMIN es sobre movimientos financieros — no sobre lo que gestiona
      // LEMY (altas/bajas de personal), aunque ambos queden en la misma auditoría.
      setRegistros(datos.filter((registro) => registro.accion.startsWith('finanzas.')))
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar la actividad.')
    } finally {
      setCargando(false)
    }
  }

  const cargarResumenDoctor = async (doctorId: string, desdeValor: string, hastaValor: string) => {
    if (!doctorId) return
    setCargandoDoctor(true)
    setErrorDoctor(null)
    try {
      const desdeIso = encodeURIComponent(`${desdeValor}T00:00:00.000Z`)
      const hastaIso = encodeURIComponent(`${hastaValor}T23:59:59.999Z`)
      const datos = await api.get<ResumenPorDoctor>(
        `/api/resumen/por-doctor?doctorId=${doctorId}&desde=${desdeIso}&hasta=${hastaIso}`,
      )
      setResumenDoctor(datos)
    } catch (err) {
      setErrorDoctor(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el resumen del doctor.')
    } finally {
      setCargandoDoctor(false)
    }
  }

  useEffect(() => {
    void cargar(desde, hasta)
    api
      .get<DoctorSimple[]>('/api/personal/doctores')
      .then((datos) => {
        setDoctores(datos)
        if (datos.length > 0) {
          setDoctorSeleccionado(datos[0].id)
        }
      })
      .catch(() => {
        setErrorDoctor('No se pudo cargar la lista de doctores.')
      })
    // Solo al montar: los filtros posteriores se disparan por el usuario, no por cambios de estado.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    if (doctorSeleccionado) {
      void cargarResumenDoctor(doctorSeleccionado, desde, hasta)
    }
    // Cambiar de doctor sí refresca solo: no hay un botón "aplicar" aparte para ese selector.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [doctorSeleccionado])

  const aplicarPreset = (preset: (typeof PRESETS)[number]) => {
    const fin = inicioDeHoy()
    const inicio = preset.calcularInicio(fin)
    const desdeValor = aFechaISO(inicio)
    const hastaValor = aFechaISO(fin)
    setDesde(desdeValor)
    setHasta(hastaValor)
    setPresetActivo(preset.etiqueta)
    void cargar(desdeValor, hastaValor)
    void cargarResumenDoctor(doctorSeleccionado, desdeValor, hastaValor)
  }

  const handleFiltrar = (event: FormEvent) => {
    event.preventDefault()
    void cargar(desde, hasta)
    void cargarResumenDoctor(doctorSeleccionado, desde, hasta)
  }

  const datosGrafico = useMemo(() => {
    const netoPorDia = new Map<string, number>()
    for (const registro of registros) {
      const detalle = parsearDetalleMovimiento(registro.detalle)
      if (!detalle) {
        continue
      }
      const dia = registro.registradoEn.slice(0, 10)
      const montoConSigno = detalle.Tipo === 'Ingreso' ? detalle.Monto : -detalle.Monto
      netoPorDia.set(dia, (netoPorDia.get(dia) ?? 0) + montoConSigno)
    }
    return Array.from(netoPorDia.entries())
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([dia, neto]) => ({ dia: dia.slice(5), neto }))
  }, [registros])

  return (
    <DashboardLayout titulo="Resumen">
      <section className="resumen-filtros no-imprimir">
        <div className="resumen-presets">
          {PRESETS.map((preset) => (
            <button
              key={preset.etiqueta}
              type="button"
              className={presetActivo === preset.etiqueta ? 'activo' : undefined}
              onClick={() => aplicarPreset(preset)}
            >
              {preset.etiqueta}
            </button>
          ))}
        </div>
        <form className="resumen-rango" onSubmit={handleFiltrar}>
          <label>
            Desde
            <input
              type="date"
              value={desde}
              max={hasta}
              onChange={(event) => {
                setDesde(event.target.value)
                setPresetActivo(null)
              }}
            />
          </label>
          <label>
            Hasta
            <input
              type="date"
              value={hasta}
              min={desde}
              onChange={(event) => {
                setHasta(event.target.value)
                setPresetActivo(null)
              }}
            />
          </label>
          <button type="submit" disabled={cargando}>
            {cargando ? 'Filtrando…' : 'Filtrar'}
          </button>
          <button type="button" className="resumen-imprimir" onClick={() => window.print()}>
            Imprimir
          </button>
        </form>
      </section>

      <p className="resumen-encabezado-impresion">
        Movimientos financieros — del {formateadorFecha.format(new Date(`${desde}T00:00:00`))} al{' '}
        {formateadorFecha.format(new Date(`${hasta}T00:00:00`))}
      </p>

      {error && <p className="resumen-error">{error}</p>}

      <section className="resumen-doctor-card">
        <div className="resumen-doctor-header">
          <h2>Por doctor</h2>
          <label className="resumen-doctor-selector no-imprimir">
            Doctor
            <select value={doctorSeleccionado} onChange={(event) => setDoctorSeleccionado(event.target.value)}>
              {doctores.map((doctor) => (
                <option key={doctor.id} value={doctor.id}>
                  {doctor.nombreCompleto}
                </option>
              ))}
            </select>
          </label>
        </div>

        {errorDoctor && <p className="resumen-error">{errorDoctor}</p>}

        {doctores.length === 0 ? (
          <p className="text-secondary">No hay doctores registrados.</p>
        ) : (
          <div className="resumen-doctor-widgets">
            <div className="resumen-doctor-widget">
              <p className="text-secondary">Pacientes atendidos</p>
              <p className="resumen-doctor-widget-valor">
                {cargandoDoctor ? '—' : formateadorEntero.format(resumenDoctor?.pacientesAtendidos ?? 0)}
              </p>
            </div>
            <div className="resumen-doctor-widget">
              <p className="text-secondary">Dinero generado</p>
              <p className="resumen-doctor-widget-valor" style={{ color: chartColors.actividad }}>
                {cargandoDoctor ? '—' : formateadorMoneda.format(resumenDoctor?.dineroGenerado ?? 0)}
              </p>
            </div>
          </div>
        )}
      </section>

      <section className="resumen-grafico-card no-imprimir">
        <p className="resumen-grafico-titulo text-secondary">Neto por día (ingresos − egresos)</p>
        {datosGrafico.length === 0 ? (
          <div className="resumen-vacio text-muted">Sin movimientos registrados en este rango.</div>
        ) : (
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={datosGrafico} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
              <CartesianGrid stroke={chartColors.gridline} vertical={false} />
              <XAxis
                dataKey="dia"
                tickLine={false}
                axisLine={{ stroke: chartColors.baseline }}
                tick={{ fill: chartColors.textMuted, fontSize: 12 }}
              />
              <YAxis
                tickLine={false}
                axisLine={false}
                width={56}
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
                formatter={(valor) => [formateadorMoneda.format(Number(valor)), 'Neto']}
              />
              <Bar dataKey="neto" fill={chartColors.actividad} radius={[4, 4, 0, 0]} maxBarSize={24} />
            </BarChart>
          </ResponsiveContainer>
        )}
      </section>

      <section className="resumen-tabla-card">
        <p className="resumen-tabla-titulo">Detalle ({registros.length} movimientos)</p>
        {cargando ? (
          <p className="text-secondary cargando-pulso">Cargando…</p>
        ) : registros.length === 0 ? (
          <p className="text-secondary">Sin movimientos registrados en este rango.</p>
        ) : (
          <table className="resumen-tabla">
            <thead>
              <tr>
                <th>Fecha</th>
                <th>Tipo</th>
                <th>Monto</th>
                <th>Concepto</th>
              </tr>
            </thead>
            <tbody>
              {registros.map((registro) => {
                const detalle = parsearDetalleMovimiento(registro.detalle)
                return (
                  <tr key={registro.id}>
                    <td>{formateadorFechaHora.format(new Date(registro.registradoEn))}</td>
                    <td className={detalle?.Tipo === 'Egreso' ? 'resumen-tipo-egreso' : 'resumen-tipo-ingreso'}>
                      {detalle?.Tipo ?? '—'}
                    </td>
                    <td>{detalle ? formateadorMoneda.format(detalle.Monto) : '—'}</td>
                    <td className="text-muted">{detalle?.Concepto ?? registro.recurso}</td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        )}
      </section>
    </DashboardLayout>
  )
}
