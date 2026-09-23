import { Fragment, useEffect, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { useAuth } from '../auth/AuthContext'
import { api, ApiError } from '../lib/api'
import { useCajaTiempoReal } from '../lib/cajaHub'
import { imprimirVentana } from '../lib/imprimir'
import type { TurnoCaja, ResumenCaja, ReporteCierreCaja } from '../types/turnoCaja'
import type { Cobro } from '../types/cobro'
import type { MovimientoFinanciero } from '../types/finanzas'
import './CajaPage.css'

const formateadorMoneda = new Intl.NumberFormat('es-DO', {
  style: 'currency',
  currency: 'DOP',
  maximumFractionDigits: 2,
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
  const { perfil } = useAuth()
  const puedeRegistrarGastos = perfil?.rol === 'Admin'

  const [turno, setTurno] = useState<TurnoCaja | null>(null)
  const [resumen, setResumen] = useState<ResumenCaja | null>(null)
  const [timeline, setTimeline] = useState<MovimientoTimeline[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [recargarClave, setRecargarClave] = useState(0)

  const [mostrarCierre, setMostrarCierre] = useState(false)
  const [montoFinalContado, setMontoFinalContado] = useState('')
  const [notasCierre, setNotasCierre] = useState('')
  const [cerrando, setCerrando] = useState(false)
  const [errorCerrar, setErrorCerrar] = useState<string | null>(null)
  const [ultimoCierre, setUltimoCierre] = useState<TurnoCaja | null>(null)
  const [reporteCierre, setReporteCierre] = useState<ReporteCierreCaja | null>(null)

  const [egresoConcepto, setEgresoConcepto] = useState('')
  const [egresoMonto, setEgresoMonto] = useState('')
  const [registrandoEgreso, setRegistrandoEgreso] = useState(false)
  const [errorEgreso, setErrorEgreso] = useState<string | null>(null)

  const [ingresoConcepto, setIngresoConcepto] = useState('')
  const [ingresoMonto, setIngresoMonto] = useState('')
  const [ingresoEsFondo, setIngresoEsFondo] = useState(false)
  const [registrandoIngreso, setRegistrandoIngreso] = useState(false)
  const [errorIngreso, setErrorIngreso] = useState<string | null>(null)

  const recargar = () => setRecargarClave((clave) => clave + 1)

  const actualizarResumen = () => {
    api
      .get<ResumenCaja>('/api/caja/resumen')
      .then((datos) => setResumen(datos))
      .catch(() => undefined)
  }

  useEffect(() => {
    let cancelado = false

    const cargarEstado = async (mostrarCargando: boolean) => {
      if (mostrarCargando) setCargando(true)
      try {
        const turnoActual = await api.get<TurnoCaja | null>('/api/caja/turnos/actual')
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
      } catch (err) {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el estado de caja.')
        }
      } finally {
        if (!cancelado && mostrarCargando) setCargando(false)
      }
    }

    void cargarEstado(true)

    // Sin esto, un cobro registrado en Cobros (otra pantalla/pestaña) no se reflejaba
    // acá hasta salir y volver a entrar a Caja — el turno abierto, el efectivo y el
    // timeline se quedaban congelados con los datos de cuando se montó la página.
    // Recarga silenciosa (sin tocar `cargando`, para no parpadear la pantalla) cada 20s.
    const intervalo = setInterval(() => void cargarEstado(false), 20_000)

    return () => {
      cancelado = true
      clearInterval(intervalo)
    }
  }, [recargarClave])

  // El resumen/timeline se refresca solo (sondeo de 20s, ver arriba); esto además lo
  // empuja al instante cuando SignalR avisa un cobro/movimiento/cierre nuevo, sin esperar
  // esos 20s -- ver useCajaTiempoReal.
  useCajaTiempoReal(() => recargar())

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

      // Mejor esfuerzo: si el reporte detallado falla, el cierre ya quedó guardado igual
      // -- el banner de arqueo de arriba alcanza para confirmar que cerró bien.
      try {
        const reporte = await api.get<ReporteCierreCaja>(`/api/caja/turnos/${cerrado.id}/reporte-cierre`)
        setReporteCierre(reporte)
      } catch {
        setReporteCierre(null)
      }
    } catch (err) {
      setErrorCerrar(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cerrar la caja.')
    } finally {
      setCerrando(false)
    }
  }

  const handleImprimirCierre = () => {
    requestAnimationFrame(imprimirVentana)
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

  const handleRegistrarIngreso = async (event: FormEvent) => {
    event.preventDefault()
    setErrorIngreso(null)

    const monto = Number(ingresoMonto)
    if (!ingresoMonto.trim() || !Number.isFinite(monto) || monto <= 0) {
      setErrorIngreso('Ingresa un monto válido, mayor que cero.')
      return
    }
    if (!ingresoConcepto.trim()) {
      setErrorIngreso('El concepto es obligatorio.')
      return
    }

    setRegistrandoIngreso(true)
    try {
      // El "fondo de ganancias" de la fundación (gráfico y tarjeta de Finanzas) detecta
      // este tipo de ingreso por la frase "ganancia de la fundación" en el concepto — ver
      // ObtenerResumenAnualUseCase.EsGananciaDeLaFundacion.
      const concepto = ingresoEsFondo && !/ganancia de la fundaci[oó]n/i.test(ingresoConcepto)
        ? `Ganancia de la fundación — ${ingresoConcepto.trim()}`
        : ingresoConcepto.trim()

      await api.post<MovimientoFinanciero>('/api/finanzas/movimientos', {
        tipo: 'Ingreso',
        monto,
        concepto,
        citaId: null,
      })
      setIngresoConcepto('')
      setIngresoMonto('')
      setIngresoEsFondo(false)
      recargar()
    } catch (err) {
      setErrorIngreso(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo registrar el ingreso.')
    } finally {
      setRegistrandoIngreso(false)
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

      {ultimoCierre && ultimoCierre.diferencia !== null && (
        <section className="caja-resultado-arqueo-card no-imprimir">
          <p className={ultimoCierre.diferencia === 0 ? 'caja-arqueo-cuadrado' : 'caja-arqueo-diferencia'}>
            {ultimoCierre.diferencia === 0
              ? 'Arqueo cuadrado sin diferencias.'
              : `Arqueo con ${ultimoCierre.diferencia > 0 ? 'sobrante' : 'faltante'} de ${formateadorMoneda.format(Math.abs(ultimoCierre.diferencia))} (esperado ${formateadorMoneda.format(ultimoCierre.montoEsperado ?? 0)}, contado ${formateadorMoneda.format(ultimoCierre.montoFinalContado ?? 0)}).`}
          </p>
          <div className="caja-resultado-arqueo-acciones">
            {reporteCierre && (
              <button type="button" onClick={handleImprimirCierre}>
                Imprimir cierre
              </button>
            )}
            <button type="button" onClick={() => setUltimoCierre(null)}>
              Entendido
            </button>
          </div>
        </section>
      )}

      {!turno ? (
        <section className="caja-apertura-card">
          <h2>Caja cerrada</h2>
          <p className="text-secondary caja-card-subtitulo">
            La caja se abre sola, con el fondo fijo de {formateadorMoneda.format(2000)}, en cuanto se registre el
            primer cobro o movimiento del día — no hace falta abrirla a mano.
          </p>
        </section>
      ) : (
        <div className="no-imprimir">
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
                  {resumen.consultasPendientesDeCobro} consulta{resumen.consultasPendientesDeCobro === 1 ? '' : 's'} finalizada
                  {resumen.consultasPendientesDeCobro === 1 ? '' : 's'} sin cobrar — pasa a Cobros.
                </p>
              )}
              {resumen && resumen.pacientesConDeudaPendiente > 0 && (
                <p className="caja-alerta caja-alerta-deuda">
                  {resumen.pacientesConDeudaPendiente} paciente(s) con deuda pendiente.
                </p>
              )}
            </section>
          )}

          {puedeRegistrarGastos && (
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
          )}

          {puedeRegistrarGastos && (
            <section className="caja-egreso-card">
              <h2>Registrar ingreso manual</h2>
              <form className="caja-egreso-form" onSubmit={(event) => void handleRegistrarIngreso(event)}>
                <input
                  placeholder="Concepto"
                  value={ingresoConcepto}
                  onChange={(event) => setIngresoConcepto(event.target.value)}
                  required
                />
                <input
                  type="number"
                  min={0.01}
                  step="0.01"
                  placeholder="Monto"
                  value={ingresoMonto}
                  onChange={(event) => setIngresoMonto(event.target.value)}
                  required
                />
                <label className="caja-ingreso-fondo-check">
                  <input
                    type="checkbox"
                    checked={ingresoEsFondo}
                    onChange={(event) => setIngresoEsFondo(event.target.checked)}
                  />
                  Es ganancia de la fundación
                </label>
                <button type="submit" disabled={registrandoIngreso}>
                  {registrandoIngreso ? 'Registrando…' : 'Registrar ingreso'}
                </button>
              </form>
              {errorIngreso && <p className="caja-error">{errorIngreso}</p>}
            </section>
          )}

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
            {puedeRegistrarGastos && resumen && resumen.gastosAdminEnElTurno > 0 && (
              <p className="caja-aviso-gastos-admin">
                Admin registró {formateadorMoneda.format(resumen.gastosAdminEnElTurno)} en gastos durante este turno
                (no incluidos en "Salidas autorizadas" de Caja) — revisá que no se dupliquen.
              </p>
            )}
            {!mostrarCierre ? (
              <button
                type="button"
                className="caja-boton-cerrar"
                onClick={() => {
                  actualizarResumen()
                  setMostrarCierre(true)
                }}
              >
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
        </div>
      )}

      {reporteCierre && (
        <div className="caja-comprobante-cierre">
          <header className="caja-comprobante-membrete">
            <h1>FUNBIDE</h1>
            <p>Reporte de cierre de caja</p>
          </header>
          <hr />
          <dl>
            <dt>Turno</dt>
            <dd>{reporteCierre.turnoId}</dd>
            <dt>Abierto por</dt>
            <dd>{reporteCierre.usuarioAperturaNombre} — {formateadorFechaHora.format(new Date(reporteCierre.abiertoEn))}</dd>
            {reporteCierre.usuarioCierreNombre && (
              <>
                <dt>Cerrado por</dt>
                <dd>
                  {reporteCierre.usuarioCierreNombre}
                  {reporteCierre.cerradoEn ? ` — ${formateadorFechaHora.format(new Date(reporteCierre.cerradoEn))}` : ''}
                </dd>
              </>
            )}
          </dl>
          <hr />
          <dl>
            <dt>Fondo inicial</dt>
            <dd>{formateadorMoneda.format(reporteCierre.fondoInicial)}</dd>
            <dt>Cobros del turno</dt>
            <dd>{reporteCierre.cantidadCobros}</dd>
            <dt>Total facturado</dt>
            <dd>{formateadorMoneda.format(reporteCierre.totalFacturado)}</dd>
          </dl>
          <hr />
          <p className="caja-comprobante-subtitulo">Desglose por método de pago</p>
          <dl>
            {Object.entries(reporteCierre.totalesPorMetodoPago).map(([metodo, monto]) => (
              <Fragment key={metodo}>
                <dt>{metodo}</dt>
                <dd>{formateadorMoneda.format(monto)}</dd>
              </Fragment>
            ))}
          </dl>
          {reporteCierre.ingresosManuales.length > 0 && (
            <>
              <hr />
              <p className="caja-comprobante-subtitulo">Ingresos manuales</p>
              <ul>
                {reporteCierre.ingresosManuales.map((m, i) => (
                  <li key={i}>
                    {m.concepto} — {formateadorMoneda.format(m.monto)}
                  </li>
                ))}
              </ul>
            </>
          )}
          {reporteCierre.egresos.length > 0 && (
            <>
              <hr />
              <p className="caja-comprobante-subtitulo">Salidas autorizadas</p>
              <ul>
                {reporteCierre.egresos.map((m, i) => (
                  <li key={i}>
                    {m.concepto} — {formateadorMoneda.format(m.monto)}
                  </li>
                ))}
              </ul>
            </>
          )}
          <hr />
          <dl>
            <dt>Efectivo esperado</dt>
            <dd>{formateadorMoneda.format(reporteCierre.montoEsperado)}</dd>
            <dt>Efectivo contado</dt>
            <dd>{formateadorMoneda.format(reporteCierre.montoFinalContado)}</dd>
            <dt>Diferencia</dt>
            <dd>{formateadorMoneda.format(reporteCierre.diferencia)}</dd>
          </dl>
          {reporteCierre.notas && (
            <>
              <hr />
              <p>Notas: {reporteCierre.notas}</p>
            </>
          )}
        </div>
      )}
    </DashboardLayout>
  )
}
