import { useEffect, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import type { TurnoCaja, ResumenCaja } from '../types/turnoCaja'
import type { Cobro } from '../types/cobro'
import type { MovimientoFinanciero } from '../types/finanzas'
import './CajaPage.css'

const formateadorMoneda = new Intl.NumberFormat('es-DO', {
  style: 'currency',
  currency: 'DOP',
  maximumFractionDigits: 0,
})
const formateadorFechaHora = new Intl.DateTimeFormat('es-DO', { dateStyle: 'short', timeStyle: 'short' })

interface MovimientoTimeline {
  id: string
  registradoEn: string
  descripcion: string
  monto: number
  esIngreso: boolean
}

export function CajaPage() {
  const [turno, setTurno] = useState<TurnoCaja | null>(null)
  const [resumen, setResumen] = useState<ResumenCaja | null>(null)
  const [timeline, setTimeline] = useState<MovimientoTimeline[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [recargarClave, setRecargarClave] = useState(0)

  const [montoInicial, setMontoInicial] = useState('')
  const [abriendo, setAbriendo] = useState(false)
  const [errorAbrir, setErrorAbrir] = useState<string | null>(null)

  const [mostrarCierre, setMostrarCierre] = useState(false)
  const [montoFinalContado, setMontoFinalContado] = useState('')
  const [notasCierre, setNotasCierre] = useState('')
  const [cerrando, setCerrando] = useState(false)
  const [errorCerrar, setErrorCerrar] = useState<string | null>(null)
  const [ultimoCierre, setUltimoCierre] = useState<TurnoCaja | null>(null)

  const [egresoConcepto, setEgresoConcepto] = useState('')
  const [egresoMonto, setEgresoMonto] = useState('')
  const [registrandoEgreso, setRegistrandoEgreso] = useState(false)
  const [errorEgreso, setErrorEgreso] = useState<string | null>(null)

  const recargar = () => setRecargarClave((clave) => clave + 1)

  useEffect(() => {
    let cancelado = false

    setCargando(true)
    api
      .get<TurnoCaja | null>('/api/caja/turnos/actual')
      .then(async (turnoActual) => {
        if (cancelado) return
        setTurno(turnoActual)

        const resumenActual = await api.get<ResumenCaja>('/api/caja/resumen')
        if (cancelado) return
        setResumen(resumenActual)

        if (!turnoActual) {
          setTimeline([])
          return
        }

        const [cobros, egresos] = await Promise.all([
          api.get<Cobro[]>('/api/cobros/turno-actual'),
          api.get<MovimientoFinanciero[]>(`/api/finanzas/movimientos?turnoCajaId=${turnoActual.id}`),
        ])
        if (cancelado) return

        const combinado: MovimientoTimeline[] = [
          ...cobros.map((c) => ({
            id: `cobro-${c.id}`,
            registradoEn: c.registradoEn,
            descripcion: `Cobro — ${c.pacienteNombre} — ${c.concepto}`,
            monto: c.montoPagado,
            esIngreso: true,
          })),
          ...egresos.map((m) => ({
            id: `movimiento-${m.id}`,
            registradoEn: m.registradoEn,
            descripcion: m.concepto,
            monto: m.monto,
            esIngreso: m.tipo === 'Ingreso',
          })),
        ].sort((a, b) => new Date(b.registradoEn).getTime() - new Date(a.registradoEn).getTime())

        setTimeline(combinado)
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el estado de caja.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargando(false)
      })

    return () => {
      cancelado = true
    }
  }, [recargarClave])

  const handleAbrir = async (event: FormEvent) => {
    event.preventDefault()
    setErrorAbrir(null)

    const monto = Number(montoInicial)
    if (!montoInicial.trim() || !Number.isFinite(monto) || monto < 0) {
      setErrorAbrir('Ingresa un monto inicial válido.')
      return
    }

    setAbriendo(true)
    try {
      await api.post<TurnoCaja>('/api/caja/turnos', { montoInicial: monto })
      setMontoInicial('')
      recargar()
    } catch (err) {
      setErrorAbrir(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo abrir la caja.')
    } finally {
      setAbriendo(false)
    }
  }

  const handleCerrar = async (event: FormEvent) => {
    event.preventDefault()
    setErrorCerrar(null)

    const monto = Number(montoFinalContado)
    if (!montoFinalContado.trim() || !Number.isFinite(monto) || monto < 0) {
      setErrorCerrar('Ingresa el monto final contado.')
      return
    }

    setCerrando(true)
    try {
      const cerrado = await api.patch<TurnoCaja>('/api/caja/turnos/cerrar', {
        montoFinalContado: monto,
        notas: notasCierre.trim() || null,
      })
      setUltimoCierre(cerrado)
      setMontoFinalContado('')
      setNotasCierre('')
      setMostrarCierre(false)
      recargar()
    } catch (err) {
      setErrorCerrar(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cerrar la caja.')
    } finally {
      setCerrando(false)
    }
  }

  const handleRegistrarEgreso = async (event: FormEvent) => {
    event.preventDefault()
    setErrorEgreso(null)

    const monto = Number(egresoMonto)
    if (!egresoMonto.trim() || !Number.isFinite(monto) || monto <= 0) {
      setErrorEgreso('Ingresa un monto válido, mayor que cero.')
      return
    }
    if (!egresoConcepto.trim()) {
      setErrorEgreso('El concepto es obligatorio.')
      return
    }

    setRegistrandoEgreso(true)
    try {
      await api.post<MovimientoFinanciero>('/api/finanzas/movimientos', {
        tipo: 'Egreso',
        monto,
        concepto: egresoConcepto.trim(),
        citaId: null,
      })
      setEgresoConcepto('')
      setEgresoMonto('')
      recargar()
    } catch (err) {
      setErrorEgreso(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo registrar la salida.')
    } finally {
      setRegistrandoEgreso(false)
    }
  }

  if (cargando) {
    return (
      <DashboardLayout titulo="Caja">
        <p className="text-secondary cargando-pulso">Cargando…</p>
      </DashboardLayout>
    )
  }

  return (
    <DashboardLayout titulo="Caja">
      {error && <p className="caja-error">{error}</p>}

      {!turno ? (
        <section className="caja-apertura-card">
          <h2>Abrir caja</h2>
          <p className="text-secondary caja-card-subtitulo">
            Registra el monto inicial en efectivo antes de empezar a operar.
          </p>
          <form className="caja-apertura-form" onSubmit={(event) => void handleAbrir(event)}>
            <input
              type="number"
              min={0}
              step="0.01"
              placeholder="Monto inicial en efectivo"
              value={montoInicial}
              onChange={(event) => setMontoInicial(event.target.value)}
              required
            />
            <button type="submit" disabled={abriendo}>
              {abriendo ? 'Abriendo…' : 'Abrir caja'}
            </button>
          </form>
          {errorAbrir && <p className="caja-error">{errorAbrir}</p>}
        </section>
      ) : (
        <>
          <div className="caja-grilla-balance">
            <section className="caja-balance-card">
              <p className="text-secondary">Efectivo en caja</p>
              <p className="caja-balance-monto">{formateadorMoneda.format(resumen?.efectivoEnCaja ?? 0)}</p>
            </section>
            <section className="caja-balance-card">
              <p className="text-secondary">Facturado hoy</p>
              <p className="caja-balance-monto">{formateadorMoneda.format(resumen?.totalFacturadoHoy ?? 0)}</p>
            </section>
            <section className="caja-balance-card">
              <p className="text-secondary">Salidas autorizadas</p>
              <p className="caja-balance-monto caja-balance-monto-negativo">
                {formateadorMoneda.format(resumen?.salidasAutorizadas ?? 0)}
              </p>
            </section>
            <section className="caja-balance-card">
              <p className="text-secondary">Pacientes en espera</p>
              <p className="caja-balance-monto">{resumen?.pacientesEnEspera ?? 0}</p>
            </section>
          </div>

          <section className="caja-desglose-card">
            <h2>Desglose por método de pago</h2>
            <div className="caja-desglose-grilla">
              <div>
                <p className="text-muted">Efectivo</p>
                <p>{formateadorMoneda.format(resumen?.totalEfectivo ?? 0)}</p>
              </div>
              <div>
                <p className="text-muted">Tarjeta</p>
                <p>{formateadorMoneda.format(resumen?.totalTarjeta ?? 0)}</p>
              </div>
              <div>
                <p className="text-muted">Transferencia</p>
                <p>{formateadorMoneda.format(resumen?.totalTransferencia ?? 0)}</p>
              </div>
              <div>
                <p className="text-muted">Cubierto por seguro</p>
                <p>{formateadorMoneda.format(resumen?.totalCubiertoPorSeguro ?? 0)}</p>
              </div>
            </div>
          </section>

          {((resumen?.consultasPendientesDeCobro ?? 0) > 0 || (resumen?.pacientesConDeudaPendiente ?? 0) > 0) && (
            <section className="caja-alertas-card">
              {resumen && resumen.consultasPendientesDeCobro > 0 && (
                <p className="caja-alerta">
                  {resumen.consultasPendientesDeCobro} doctor(es) finalizaron consulta — pasa a caja.
                </p>
              )}
              {resumen && resumen.pacientesConDeudaPendiente > 0 && (
                <p className="caja-alerta caja-alerta-deuda">
                  {resumen.pacientesConDeudaPendiente} paciente(s) con deuda pendiente.
                </p>
              )}
            </section>
          )}

          <section className="caja-egreso-card">
            <h2>Registrar salida autorizada</h2>
            <form className="caja-egreso-form" onSubmit={(event) => void handleRegistrarEgreso(event)}>
              <input
                placeholder="Concepto"
                value={egresoConcepto}
                onChange={(event) => setEgresoConcepto(event.target.value)}
                required
              />
              <input
                type="number"
                min={0.01}
                step="0.01"
                placeholder="Monto"
                value={egresoMonto}
                onChange={(event) => setEgresoMonto(event.target.value)}
                required
              />
              <button type="submit" disabled={registrandoEgreso}>
                {registrandoEgreso ? 'Registrando…' : 'Registrar salida'}
              </button>
            </form>
            {errorEgreso && <p className="caja-error">{errorEgreso}</p>}
          </section>

          <section className="caja-timeline-card">
            <h2>Movimientos del turno</h2>
            {timeline.length === 0 ? (
              <p className="text-secondary">Todavía no hay movimientos en este turno.</p>
            ) : (
              <ul className="caja-timeline-lista">
                {timeline.map((movimiento) => (
                  <li key={movimiento.id}>
                    <span className="text-muted">{formateadorFechaHora.format(new Date(movimiento.registradoEn))}</span>
                    <span>{movimiento.descripcion}</span>
                    <span className={movimiento.esIngreso ? 'caja-timeline-ingreso' : 'caja-timeline-egreso'}>
                      {movimiento.esIngreso ? '+' : '−'}
                      {formateadorMoneda.format(movimiento.monto)}
                    </span>
                  </li>
                ))}
              </ul>
            )}
          </section>

          <section className="caja-cierre-card">
            {ultimoCierre && ultimoCierre.diferencia !== null && (
              <p className={ultimoCierre.diferencia === 0 ? 'caja-arqueo-cuadrado' : 'caja-arqueo-diferencia'}>
                {ultimoCierre.diferencia === 0
                  ? 'Arqueo cuadrado sin diferencias.'
                  : `Arqueo con ${ultimoCierre.diferencia > 0 ? 'sobrante' : 'faltante'} de ${formateadorMoneda.format(Math.abs(ultimoCierre.diferencia))} (esperado ${formateadorMoneda.format(ultimoCierre.montoEsperado ?? 0)}, contado ${formateadorMoneda.format(ultimoCierre.montoFinalContado ?? 0)}).`}
              </p>
            )}
            {resumen && resumen.gastosAdminEnElTurno > 0 && (
              <p className="caja-aviso-gastos-admin">
                Admin registró {formateadorMoneda.format(resumen.gastosAdminEnElTurno)} en gastos durante este turno
                (no incluidos en "Salidas autorizadas" de Caja) — revisá que no se dupliquen.
              </p>
            )}
            {!mostrarCierre ? (
              <button type="button" className="caja-boton-cerrar" onClick={() => setMostrarCierre(true)}>
                Cerrar caja
              </button>
            ) : (
              <>
                <h2>Cierre de caja — arqueo final</h2>
                {resumen && <p className="caja-efectivo-esperado">Efectivo esperado: {formateadorMoneda.format(resumen.efectivoEnCaja)}</p>}
                <form className="caja-cierre-form" onSubmit={(event) => void handleCerrar(event)}>
                  <input
                    type="number"
                    min={0}
                    step="0.01"
                    placeholder="Monto final contado"
                    value={montoFinalContado}
                    onChange={(event) => setMontoFinalContado(event.target.value)}
                    required
                  />
                  <textarea
                    placeholder="Notas del arqueo (opcional)"
                    value={notasCierre}
                    onChange={(event) => setNotasCierre(event.target.value)}
                  />
                  <div className="caja-cierre-acciones">
                    <button type="submit" disabled={cerrando}>
                      {cerrando ? 'Cerrando…' : 'Confirmar cierre'}
                    </button>
                    <button type="button" onClick={() => setMostrarCierre(false)} disabled={cerrando}>
                      Cancelar
                    </button>
                  </div>
                </form>
                {errorCerrar && <p className="caja-error">{errorCerrar}</p>}
              </>
            )}
          </section>
        </>
      )}
    </DashboardLayout>
  )
}
