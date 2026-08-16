import { useEffect, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import type { Donacion } from '../types/donacion'
import './DonacionesPage.css'

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

export function DonacionesPage() {
  const anioActual = new Date().getFullYear()

  const [anio, setAnio] = useState(anioActual)
  const [mes, setMes] = useState<number | null>(null)
  const [donaciones, setDonaciones] = useState<Donacion[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [donanteNombre, setDonanteNombre] = useState('')
  const [donanteContacto, setDonanteContacto] = useState('')
  const [concepto, setConcepto] = useState('')
  const [monto, setMonto] = useState('')
  const [registrando, setRegistrando] = useState(false)
  const [errorRegistrar, setErrorRegistrar] = useState<string | null>(null)
  const [exito, setExito] = useState(false)

  const [recargarClave, setRecargarClave] = useState(0)
  const recargar = () => setRecargarClave((clave) => clave + 1)

  useEffect(() => {
    let cancelado = false
    const { desde, hasta } = construirRango(anio, mes)
    setCargando(true)
    api
      .get<Donacion[]>(`/api/donaciones?desde=${encodeURIComponent(desde)}&hasta=${encodeURIComponent(hasta)}`)
      .then((datos) => {
        if (!cancelado) setDonaciones(datos)
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar las donaciones.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargando(false)
      })

    return () => {
      cancelado = true
    }
  }, [anio, mes, recargarClave])

  const totalDonado = donaciones.reduce((acumulado, donacion) => acumulado + donacion.monto, 0)
  const aniosDisponibles = Array.from({ length: 5 }, (_, i) => anioActual - i)

  const handleRegistrar = async (event: FormEvent) => {
    event.preventDefault()
    setErrorRegistrar(null)
    setExito(false)

    const montoNumero = Number(monto)
    if (!donanteNombre.trim()) {
      setErrorRegistrar('El nombre del donante es obligatorio.')
      return
    }
    if (!concepto.trim()) {
      setErrorRegistrar('El concepto es obligatorio.')
      return
    }
    if (!monto.trim() || !Number.isFinite(montoNumero) || montoNumero <= 0) {
      setErrorRegistrar('Ingresa un monto válido, mayor que cero.')
      return
    }

    setRegistrando(true)
    try {
      await api.post('/api/donaciones', {
        donanteNombre: donanteNombre.trim(),
        donanteContacto: donanteContacto.trim() || null,
        monto: montoNumero,
        concepto: concepto.trim(),
      })
      setDonanteNombre('')
      setDonanteContacto('')
      setConcepto('')
      setMonto('')
      setExito(true)
      recargar()
    } catch (err) {
      setErrorRegistrar(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo registrar la donación.')
    } finally {
      setRegistrando(false)
    }
  }

  return (
    <DashboardLayout titulo="Donaciones">
      {error && <p className="donaciones-error">{error}</p>}

      <div className="donaciones-filtros">
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

      <section className="donaciones-total-card">
        <p className="text-secondary">Total donado en el período</p>
        <p className="donaciones-total-monto">{formateadorMoneda.format(totalDonado)}</p>
      </section>

      <section className="donaciones-tabla-card">
        <p className="donaciones-tabla-titulo">Donaciones registradas</p>
        {cargando ? (
          <p className="text-secondary cargando-pulso">Cargando…</p>
        ) : donaciones.length === 0 ? (
          <p className="text-secondary">No hay donaciones registradas en este período.</p>
        ) : (
          <div className="donaciones-tabla-scroll">
            <table className="donaciones-tabla">
              <thead>
                <tr>
                  <th>Fecha</th>
                  <th>Donante</th>
                  <th>Contacto</th>
                  <th>Concepto</th>
                  <th>Monto</th>
                </tr>
              </thead>
              <tbody>
                {donaciones.map((donacion) => (
                  <tr key={donacion.id}>
                    <td className="text-muted">{formateadorFechaHora.format(new Date(donacion.registradoEn))}</td>
                    <td>{donacion.donanteNombre}</td>
                    <td className="text-muted">{donacion.donanteContacto ?? '—'}</td>
                    <td>{donacion.concepto}</td>
                    <td>{formateadorMoneda.format(donacion.monto)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section className="donaciones-registrar-card">
        <h2>Registrar donación</h2>
        <p className="text-secondary donaciones-registrar-subtitulo">
          Donaciones externas de la fundación — se suman a los ingresos de Finanzas automáticamente.
        </p>
        <form className="donaciones-registrar-form" onSubmit={(event) => void handleRegistrar(event)}>
          <input
            placeholder="Nombre del donante"
            value={donanteNombre}
            onChange={(event) => setDonanteNombre(event.target.value)}
            required
          />
          <input
            placeholder="Contacto (opcional)"
            value={donanteContacto}
            onChange={(event) => setDonanteContacto(event.target.value)}
          />
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
            {registrando ? 'Registrando…' : 'Registrar donación'}
          </button>
        </form>
        {errorRegistrar && <p className="donaciones-error">{errorRegistrar}</p>}
        {exito && <p className="donaciones-exito">Donación registrada correctamente.</p>}
      </section>
    </DashboardLayout>
  )
}
