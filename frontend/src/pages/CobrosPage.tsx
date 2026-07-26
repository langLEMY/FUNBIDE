import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import type { TurnoCaja } from '../types/turnoCaja'
import type { Paciente, PacientesPaginados } from '../types/paciente'
import type { SeguroMedico } from '../types/seguroMedico'
import type { Cobro, MetodoPago } from '../types/cobro'
import { METODOS_PAGO } from '../types/cobro'
import type { CitaAgenda } from '../types/cita'
import './CobrosPage.css'

const formateadorMoneda = new Intl.NumberFormat('es-DO', {
  style: 'currency',
  currency: 'DOP',
  maximumFractionDigits: 2,
})
const formateadorFechaHora = new Intl.DateTimeFormat('es-DO', { dateStyle: 'short', timeStyle: 'short' })

/** Redondeo half-to-even a 2 decimales, igual que Math.Round(decimal, 2) de C#. */
function redondearBancario(valor: number): number {
  const escalado = valor * 100
  const piso = Math.floor(escalado)
  const resto = escalado - piso
  const epsilon = 1e-9
  let redondeado: number
  if (Math.abs(resto - 0.5) < epsilon) {
    redondeado = piso % 2 === 0 ? piso : piso + 1
  } else {
    redondeado = Math.round(escalado)
  }
  return redondeado / 100
}

type TipoComprobante = 'Factura de consumo' | 'Crédito fiscal' | 'Recibo de ingreso'

export function CobrosPage() {
  const [turno, setTurno] = useState<TurnoCaja | null>(null)
  const [seguros, setSeguros] = useState<SeguroMedico[]>([])
  const [pendientesDeCobro, setPendientesDeCobro] = useState<CitaAgenda[]>([])
  const [historial, setHistorial] = useState<Cobro[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [busqueda, setBusqueda] = useState('')
  const [busquedaDebounced, setBusquedaDebounced] = useState('')
  const [resultados, setResultados] = useState<Paciente[]>([])
  const [buscando, setBuscando] = useState(false)

  const [pacienteSeleccionado, setPacienteSeleccionado] = useState<Paciente | null>(null)
  const [citaId, setCitaId] = useState<string | null>(null)
  const [deudaPaciente, setDeudaPaciente] = useState<number | null>(null)

  const [concepto, setConcepto] = useState('')
  const [montoTotal, setMontoTotal] = useState('')
  const [metodoPago, setMetodoPago] = useState<MetodoPago>('Efectivo')
  const [seguroMedicoId, setSeguroMedicoId] = useState('')
  const [codigoAutorizacion, setCodigoAutorizacion] = useState('')
  const [pagoParcial, setPagoParcial] = useState(false)
  const [montoPagadoParcial, setMontoPagadoParcial] = useState('')
  const [registrando, setRegistrando] = useState(false)
  const [errorCobro, setErrorCobro] = useState<string | null>(null)

  const [ultimoCobro, setUltimoCobro] = useState<Cobro | null>(null)
  const [comprobante, setComprobante] = useState<TipoComprobante | null>(null)

  useEffect(() => {
    const temporizador = setTimeout(() => setBusquedaDebounced(busqueda), 300)
    return () => clearTimeout(temporizador)
  }, [busqueda])

  useEffect(() => {
    let cancelado = false

    Promise.all([
      api.get<TurnoCaja | null>('/api/caja/turnos/actual'),
      api.get<SeguroMedico[]>('/api/seguros-medicos'),
      api.get<CitaAgenda[]>('/api/citas/pendientes-de-cobro'),
    ])
      .then(([turnoActual, segurosActivos, pendientes]) => {
        if (cancelado) return
        setTurno(turnoActual)
        setSeguros(segurosActivos)
        setPendientesDeCobro(pendientes)
      })
      .catch((err) => {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar Cobros.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargando(false)
      })

    return () => {
      cancelado = true
    }
  }, [])

  const cargarHistorial = () => {
    api
      .get<Cobro[]>('/api/cobros/turno-actual')
      .then((datos) => setHistorial(datos))
      .catch(() => undefined)
  }

  useEffect(cargarHistorial, [turno])

  useEffect(() => {
    if (!busquedaDebounced.trim()) {
      setResultados([])
      return
    }

    let cancelado = false
    setBuscando(true)
    api
      .get<PacientesPaginados>(`/api/pacientes?pagina=1&tamanoPagina=10&busqueda=${encodeURIComponent(busquedaDebounced.trim())}`)
      .then((datos) => {
        if (!cancelado) setResultados(datos.items)
      })
      .catch(() => undefined)
      .finally(() => {
        if (!cancelado) setBuscando(false)
      })

    return () => {
      cancelado = true
    }
  }, [busquedaDebounced])

  const seleccionarPaciente = (paciente: Paciente, citaIdPrefill: string | null = null) => {
    setPacienteSeleccionado(paciente)
    setCitaId(citaIdPrefill)
    setBusqueda('')
    setResultados([])
    setDeudaPaciente(null)
    setUltimoCobro(null)
    setComprobante(null)

    api
      .get<{ pacienteId: string; montoTotalAdeudado: number }>(`/api/cobros/deuda/${paciente.id}`)
      .then((res) => setDeudaPaciente(res.montoTotalAdeudado))
      .catch(() => undefined)
  }

  const seleccionarPendiente = (cita: CitaAgenda) => {
    seleccionarPaciente(
      { id: cita.pacienteId, nombre: cita.pacienteNombre, apellido: '', cedula: '', telefono: null, tieneFotoCedula: false, edad: null, condicion: null, estado: 'Activo', ultimaVisita: null },
      cita.id,
    )
    setConcepto(`Consulta — ${cita.motivo}`)
  }

  const seguroSeleccionado = useMemo(() => seguros.find((s) => s.id === seguroMedicoId) ?? null, [seguros, seguroMedicoId])

  const montoTotalNumero = Number(montoTotal) || 0
  // Redondeo bancario (half-to-even), igual que Math.Round de C# en Cobro.cs: con
  // Math.round de JS (half-up) esta vista previa podía diferir en 1 centavo del monto
  // que realmente guarda y devuelve el backend en un empate exacto de medio centavo.
  const montoCobertura = seguroSeleccionado
    ? redondearBancario(montoTotalNumero * (seguroSeleccionado.porcentajeCobertura / 100))
    : 0
  const montoACargoPaciente = montoTotalNumero - montoCobertura
  const montoPagadoFinal = pagoParcial ? Number(montoPagadoParcial) || 0 : montoACargoPaciente

  const limpiarFormulario = () => {
    setConcepto('')
    setMontoTotal('')
    setMetodoPago('Efectivo')
    setSeguroMedicoId('')
    setCodigoAutorizacion('')
    setPagoParcial(false)
    setMontoPagadoParcial('')
  }

  const handleRegistrarCobro = async (event: FormEvent) => {
    event.preventDefault()
    setErrorCobro(null)

    if (!pacienteSeleccionado) {
      setErrorCobro('Selecciona un paciente.')
      return
    }
    if (!concepto.trim()) {
      setErrorCobro('El concepto es obligatorio.')
      return
    }
    if (!montoTotal.trim() || montoTotalNumero <= 0) {
      setErrorCobro('Ingresa un monto válido, mayor que cero.')
      return
    }
    if (seguroMedicoId && !codigoAutorizacion.trim()) {
      setErrorCobro('El código de autorización es obligatorio cuando el cobro usa seguro médico.')
      return
    }

    setRegistrando(true)
    try {
      const cobro = await api.post<Cobro>('/api/cobros', {
        pacienteId: pacienteSeleccionado.id,
        citaId,
        concepto: concepto.trim(),
        montoTotal: montoTotalNumero,
        metodoPago,
        montoPagado: montoPagadoFinal,
        seguroMedicoId: seguroMedicoId || null,
        codigoAutorizacion: seguroMedicoId ? codigoAutorizacion.trim() : null,
      })
      setUltimoCobro(cobro)
      limpiarFormulario()
      cargarHistorial()
      if (citaId) {
        setPendientesDeCobro((actual) => actual.filter((c) => c.id !== citaId))
      }
    } catch (err) {
      setErrorCobro(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo registrar el cobro.')
    } finally {
      setRegistrando(false)
    }
  }

  const imprimir = (tipo: TipoComprobante) => {
    setComprobante(tipo)
    requestAnimationFrame(() => window.print())
  }

  if (cargando) {
    return (
      <DashboardLayout titulo="Cobros">
        <p className="text-secondary cargando-pulso">Cargando…</p>
      </DashboardLayout>
    )
  }

  return (
    <DashboardLayout titulo="Cobros">
      {error && <p className="cobros-error">{error}</p>}

      {!turno && (
        <section className="cobros-aviso-caja-cerrada">
          La caja está cerrada. Abre un turno en la sección Caja antes de registrar cobros.
        </section>
      )}

      <div className="cobros-layout no-imprimir">
        <div className="cobros-columna-principal">
          {pendientesDeCobro.length > 0 && (
            <section className="cobros-pendientes-card">
              <h2>Consultas por cobrar</h2>
              <ul className="cobros-pendientes-lista">
                {pendientesDeCobro.map((cita) => (
                  <li key={cita.id}>
                    <span>
                      {cita.pacienteNombre} — {cita.doctorNombre} — {cita.motivo}
                    </span>
                    <button type="button" onClick={() => seleccionarPendiente(cita)}>
                      Cobrar
                    </button>
                  </li>
                ))}
              </ul>
            </section>
          )}

          <section className="cobros-buscador-card no-imprimir">
            <h2>Buscar paciente</h2>
            <input
              type="search"
              placeholder="Nombre o cédula…"
              value={busqueda}
              onChange={(event) => setBusqueda(event.target.value)}
            />
            {buscando && <p className="text-muted">Buscando…</p>}
            {resultados.length > 0 && (
              <ul className="cobros-resultados-lista">
                {resultados.map((paciente) => (
                  <li key={paciente.id}>
                    <button type="button" onClick={() => seleccionarPaciente(paciente)}>
                      {paciente.nombre} {paciente.apellido} — {paciente.cedula}
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </section>

          {pacienteSeleccionado && (
            <section className="cobros-formulario-card">
              <h2>
                Cobrar a {pacienteSeleccionado.nombre} {pacienteSeleccionado.apellido}
              </h2>
              {deudaPaciente !== null && deudaPaciente > 0 && (
                <p className="cobros-aviso-deuda">
                  Tiene una deuda anterior de {formateadorMoneda.format(deudaPaciente)}.
                </p>
              )}

              <form className="cobros-formulario" onSubmit={(event) => void handleRegistrarCobro(event)}>
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
                  value={montoTotal}
                  onChange={(event) => setMontoTotal(event.target.value)}
                  required
                />

                <label className="cobros-label">
                  Seguro médico (opcional)
                  <select value={seguroMedicoId} onChange={(event) => setSeguroMedicoId(event.target.value)}>
                    <option value="">Sin seguro</option>
                    {seguros.map((seguro) => (
                      <option key={seguro.id} value={seguro.id}>
                        {seguro.nombre} ({seguro.porcentajeCobertura}%)
                      </option>
                    ))}
                  </select>
                </label>

                {seguroSeleccionado && (
                  <>
                    <div className="cobros-cobertura-info">
                      <span>Cobertura ({seguroSeleccionado.porcentajeCobertura}%): {formateadorMoneda.format(montoCobertura)}</span>
                      <span>Co-pago del paciente: {formateadorMoneda.format(montoACargoPaciente)}</span>
                    </div>
                    <input
                      placeholder="Código de autorización"
                      value={codigoAutorizacion}
                      onChange={(event) => setCodigoAutorizacion(event.target.value)}
                      required
                    />
                  </>
                )}

                <select value={metodoPago} onChange={(event) => setMetodoPago(event.target.value as MetodoPago)}>
                  {METODOS_PAGO.map((opcion) => (
                    <option key={opcion} value={opcion}>
                      {opcion}
                    </option>
                  ))}
                </select>

                <label className="cobros-label-checkbox">
                  <input type="checkbox" checked={pagoParcial} onChange={(event) => setPagoParcial(event.target.checked)} />
                  El paciente paga solo una parte ahora
                </label>

                {pagoParcial ? (
                  <input
                    type="number"
                    min={0}
                    max={montoACargoPaciente}
                    step="0.01"
                    placeholder="Monto recibido ahora"
                    value={montoPagadoParcial}
                    onChange={(event) => setMontoPagadoParcial(event.target.value)}
                  />
                ) : (
                  <p className="cobros-monto-a-cobrar">A cobrar: {formateadorMoneda.format(montoACargoPaciente)}</p>
                )}

                <button type="submit" disabled={registrando || !turno}>
                  {registrando ? 'Procesando…' : 'Procesar cobro'}
                </button>
              </form>
              {errorCobro && <p className="cobros-error">{errorCobro}</p>}
            </section>
          )}

          {ultimoCobro && (
            <section className="cobros-impresion-card no-imprimir">
              <h2>Cobro registrado</h2>
              <p className="text-secondary">Elige el comprobante a imprimir:</p>
              <div className="cobros-impresion-botones">
                <button type="button" onClick={() => imprimir('Factura de consumo')}>
                  Factura de consumo
                </button>
                <button type="button" onClick={() => imprimir('Crédito fiscal')}>
                  Crédito fiscal
                </button>
                <button type="button" onClick={() => imprimir('Recibo de ingreso')}>
                  Recibo de ingreso
                </button>
              </div>
            </section>
          )}
        </div>

        <aside className="cobros-historial-card no-imprimir">
          <h2>Movimientos del día</h2>
          {historial.length === 0 ? (
            <p className="text-secondary">Todavía no hay cobros en este turno.</p>
          ) : (
            <ul className="cobros-historial-lista">
              {historial.map((cobro) => (
                <li key={cobro.id}>
                  <span className="text-muted">{formateadorFechaHora.format(new Date(cobro.registradoEn))}</span>
                  <span>{cobro.pacienteNombre}</span>
                  <span>{formateadorMoneda.format(cobro.montoPagado)}</span>
                </li>
              ))}
            </ul>
          )}
        </aside>
      </div>

      {ultimoCobro && comprobante && (
        <div className="cobros-comprobante">
          <h1>{comprobante}</h1>
          <p>Paciente: {ultimoCobro.pacienteNombre}</p>
          <p>Concepto: {ultimoCobro.concepto}</p>
          <p>Fecha: {formateadorFechaHora.format(new Date(ultimoCobro.registradoEn))}</p>
          <p>Monto total: {formateadorMoneda.format(ultimoCobro.montoTotal)}</p>
          {ultimoCobro.seguroMedicoNombre && (
            <>
              <p>Seguro: {ultimoCobro.seguroMedicoNombre} ({ultimoCobro.porcentajeCobertura}%)</p>
              <p>Cubierto por seguro: {formateadorMoneda.format(ultimoCobro.montoCobertura ?? 0)}</p>
              <p>Código de autorización: {ultimoCobro.codigoAutorizacion}</p>
            </>
          )}
          <p>Método de pago: {ultimoCobro.metodoPago}</p>
          <p>Monto pagado: {formateadorMoneda.format(ultimoCobro.montoPagado)}</p>
          {ultimoCobro.montoPendiente > 0 && <p>Saldo pendiente: {formateadorMoneda.format(ultimoCobro.montoPendiente)}</p>}
        </div>
      )}
    </DashboardLayout>
  )
}
