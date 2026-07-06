import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import type { MovimientoFinanciero, TipoMovimientoFinanciero } from '../types/finanzas'
import { TIPOS_MOVIMIENTO_FINANCIERO } from '../types/finanzas'
import './FinanzasPage.css'

const formateadorMoneda = new Intl.NumberFormat('es-DO', {
  style: 'currency',
  currency: 'DOP',
  maximumFractionDigits: 0,
})
const formateadorFechaHora = new Intl.DateTimeFormat('es-DO', { dateStyle: 'short', timeStyle: 'short' })

export function FinanzasPage() {
  const [movimientos, setMovimientos] = useState<MovimientoFinanciero[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [tipo, setTipo] = useState<TipoMovimientoFinanciero>('Ingreso')
  const [monto, setMonto] = useState('')
  const [concepto, setConcepto] = useState('')
  const [registrando, setRegistrando] = useState(false)
  const [errorRegistrar, setErrorRegistrar] = useState<string | null>(null)

  useEffect(() => {
    let cancelado = false

    api
      .get<MovimientoFinanciero[]>('/api/finanzas/movimientos')
      .then((datos) => {
        if (!cancelado) setMovimientos(datos)
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar los movimientos.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargando(false)
      })

    return () => {
      cancelado = true
    }
  }, [])

  const neto = useMemo(
    () => movimientos.reduce((acumulado, m) => acumulado + (m.tipo === 'Ingreso' ? m.monto : -m.monto), 0),
    [movimientos],
  )

  const handleRegistrar = async (event: FormEvent) => {
    event.preventDefault()
    setErrorRegistrar(null)

    const montoNumero = Number(monto)
    if (!monto.trim() || !Number.isFinite(montoNumero) || montoNumero <= 0) {
      setErrorRegistrar('Ingresa un monto válido, mayor que cero.')
      return
    }
    if (!concepto.trim()) {
      setErrorRegistrar('El concepto es obligatorio.')
      return
    }

    setRegistrando(true)
    try {
      const nuevo = await api.post<MovimientoFinanciero>('/api/finanzas/movimientos', {
        tipo,
        monto: montoNumero,
        concepto,
        citaId: null,
      })
      setMovimientos((actual) => [nuevo, ...actual])
      setMonto('')
      setConcepto('')
    } catch (err) {
      setErrorRegistrar(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo registrar el movimiento.')
    } finally {
      setRegistrando(false)
    }
  }

  return (
    <DashboardLayout titulo="Finanzas">
      <section className="finanzas-resumen-card">
        <p className="text-secondary">Neto acumulado</p>
        <p className={`finanzas-neto ${neto < 0 ? 'finanzas-neto-negativo' : 'finanzas-neto-positivo'}`}>
          {formateadorMoneda.format(neto)}
        </p>
      </section>

      <section className="finanzas-crear-card">
        <h2>Registrar movimiento</h2>
        <form className="finanzas-crear-form" onSubmit={(event) => void handleRegistrar(event)}>
          <select value={tipo} onChange={(event) => setTipo(event.target.value as TipoMovimientoFinanciero)}>
            {TIPOS_MOVIMIENTO_FINANCIERO.map((opcion) => (
              <option key={opcion} value={opcion}>
                {opcion}
              </option>
            ))}
          </select>
          <input
            type="number"
            min={0.01}
            step="0.01"
            placeholder="Monto"
            value={monto}
            onChange={(event) => setMonto(event.target.value)}
            required
          />
          <input
            placeholder="Concepto"
            value={concepto}
            onChange={(event) => setConcepto(event.target.value)}
            required
          />
          <button type="submit" disabled={registrando}>
            {registrando ? 'Registrando…' : 'Registrar'}
          </button>
        </form>
        {errorRegistrar && <p className="finanzas-error">{errorRegistrar}</p>}
      </section>

      <section className="finanzas-tabla-card">
        {error && <p className="finanzas-error">{error}</p>}

        {cargando ? (
          <p className="text-secondary">Cargando movimientos…</p>
        ) : movimientos.length === 0 ? (
          <p className="text-secondary">Todavía no hay movimientos registrados.</p>
        ) : (
          <table className="finanzas-tabla">
            <thead>
              <tr>
                <th>Fecha</th>
                <th>Tipo</th>
                <th>Monto</th>
                <th>Concepto</th>
              </tr>
            </thead>
            <tbody>
              {movimientos.map((movimiento) => (
                <tr key={movimiento.id}>
                  <td className="text-muted">{formateadorFechaHora.format(new Date(movimiento.registradoEn))}</td>
                  <td className={movimiento.tipo === 'Egreso' ? 'finanzas-tipo-egreso' : 'finanzas-tipo-ingreso'}>
                    {movimiento.tipo}
                  </td>
                  <td>{formateadorMoneda.format(movimiento.monto)}</td>
                  <td className="text-muted">{movimiento.concepto}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>
    </DashboardLayout>
  )
}
