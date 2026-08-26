import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { api, ApiError } from '../lib/api'
import type { TurnoCaja } from '../types/turnoCaja'
import type { Paciente, PacientesPaginados, CrearPacienteRequest } from '../types/paciente'
import type { SeguroMedico } from '../types/seguroMedico'
import type { Cobro, MetodoPago, Pago } from '../types/cobro'
import { METODOS_PAGO } from '../types/cobro'
import type { CitaAgenda } from '../types/cita'
import { ETIQUETA_PLAN, PLANES_ASEGURADORA, type PlanAseguradora, type TarifarioProcedimiento } from '../types/tarifarioProcedimiento'
import type { Servicio } from '../types/servicio'
import type { EspecialidadMedica } from '../types/usuario'
import { ESPECIALIDADES, ETIQUETA_ESPECIALIDAD } from '../types/personal'
import './CobrosPage.css'

const SIN_ESPECIALIDAD = ''
const OTRO_SERVICIO = ''

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

  const [mostrarNuevoPaciente, setMostrarNuevoPaciente] = useState(false)
  const [nuevoNombre, setNuevoNombre] = useState('')
  const [nuevoApellido, setNuevoApellido] = useState('')
  const [nuevoCedula, setNuevoCedula] = useState('')
  const [nuevoTelefono, setNuevoTelefono] = useState('')
  const [creandoPaciente, setCreandoPaciente] = useState(false)
  const [errorCrearPaciente, setErrorCrearPaciente] = useState<string | null>(null)

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
  const [dividirPago, setDividirPago] = useState(false)
  const [lineasPago, setLineasPago] = useState<{ metodo: MetodoPago; monto: string }[]>([])
  const [registrando, setRegistrando] = useState(false)
  const [errorCobro, setErrorCobro] = useState<string | null>(null)

  const [planTarifario, setPlanTarifario] = useState<PlanAseguradora>('Estandar')
  const [tarifario, setTarifario] = useState<TarifarioProcedimiento[]>([])
  const [tarifarioProcedimientoId, setTarifarioProcedimientoId] = useState('')

  // Selección encadenada para pago particular (sin seguro): especialidad → servicio, en
  // vez de escribir el concepto y el monto a mano — ver ServiciosPage/catálogo de precios
  // privados. "Otro" deja el concepto/monto editables como hasta ahora.
  const [servicios, setServicios] = useState<Servicio[]>([])
  const [especialidadServicio, setEspecialidadServicio] = useState<EspecialidadMedica | typeof SIN_ESPECIALIDAD>(SIN_ESPECIALIDAD)
  const [servicioId, setServicioId] = useState('')

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
      api.get<Servicio[]>('/api/servicios'),
    ])
      .then(([turnoActual, segurosActivos, pendientes, serviciosActivos]) => {
        if (cancelado) return
        setTurno(turnoActual)
        setSeguros(segurosActivos)
        setPendientesDeCobro(pendientes)
        setServicios(serviciosActivos)
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

  const handleCrearPaciente = async (event: FormEvent) => {
    event.preventDefault()
    setErrorCrearPaciente(null)
    setCreandoPaciente(true)
    try {
      const request: CrearPacienteRequest = {
        nombre: nuevoNombre.trim(),
        apellido: nuevoApellido.trim(),
        cedula: nuevoCedula.trim(),
        telefono: nuevoTelefono.trim() || null,
        edad: null,
        condicion: null,
      }
      const paciente = await api.post<Paciente>('/api/pacientes', request)
      setNuevoNombre('')
      setNuevoApellido('')
      setNuevoCedula('')
      setNuevoTelefono('')
      setMostrarNuevoPaciente(false)
      seleccionarPaciente(paciente)
    } catch (err) {
      setErrorCrearPaciente(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo agregar al paciente.')
    } finally {
      setCreandoPaciente(false)
    }
  }

  const seleccionarPendiente = (cita: CitaAgenda) => {
    seleccionarPaciente(
      { id: cita.pacienteId, nombre: cita.pacienteNombre, apellido: '', cedula: '', telefono: null, tieneFotoCedula: false, edad: null, condicion: null, estado: 'Activo', ultimaVisita: null },
      cita.id,
    )
    setConcepto(`Consulta — ${cita.motivo}`)
  }

  const seguroSeleccionado = useMemo(() => seguros.find((s) => s.id === seguroMedicoId) ?? null, [seguros, seguroMedicoId])
  // Cualquier aseguradora con tarifario cargado (Senasa, Renacer, Aps...) activa el
  // selector de procedimiento — ya no depende de que el nombre contenga "SENASA".
  const tieneTarifario = seguroSeleccionado?.tieneTarifario ?? false

  useEffect(() => {
    setTarifarioProcedimientoId('')
    setPlanTarifario('Estandar')
  }, [seguroMedicoId])

  useEffect(() => {
    if (!tieneTarifario || !seguroMedicoId) {
      setTarifario([])
      return
    }

    let cancelado = false
    api
      .get<TarifarioProcedimiento[]>(`/api/tarifario-procedimientos?seguroMedicoId=${seguroMedicoId}&plan=${planTarifario}`)
      .then((datos) => {
        if (!cancelado) setTarifario(datos)
      })
      .catch(() => {
        if (!cancelado) setTarifario([])
      })

    return () => {
      cancelado = true
    }
  }, [tieneTarifario, seguroMedicoId, planTarifario])

  const procedimientoSeleccionado = useMemo(
    () => tarifario.find((t) => t.id === tarifarioProcedimientoId) ?? null,
    [tarifario, tarifarioProcedimientoId],
  )

  const seleccionarProcedimiento = (id: string) => {
    setTarifarioProcedimientoId(id)
    const procedimiento = tarifario.find((t) => t.id === id)
    if (procedimiento) {
      setConcepto(procedimiento.procedimiento)
      setMontoTotal(String(procedimiento.montoTotal))
    }
  }

  // Pago particular (sin seguro): mismo patrón de selección encadenada, pero el precio no
  // es autoritativo del lado del servidor (a diferencia del tarifario de aseguradora), así
  // que solo precarga concepto/monto — el cajero puede seguir ajustándolos a mano.
  const serviciosFiltrados = useMemo(
    () => (especialidadServicio ? servicios.filter((s) => s.especialidad === especialidadServicio) : servicios),
    [servicios, especialidadServicio],
  )

  const seleccionarServicio = (id: string) => {
    setServicioId(id)
    if (!id) return
    const servicio = servicios.find((s) => s.id === id)
    if (servicio) {
      setConcepto(servicio.nombre)
      setMontoTotal(String(servicio.precio1))
    }
  }

  const montoTotalNumero = Number(montoTotal) || 0
  // Redondeo bancario (half-to-even), igual que Math.Round de C# en Cobro.cs: con
  // Math.round de JS (half-up) esta vista previa podía diferir en 1 centavo del monto
  // que realmente guarda y devuelve el backend en un empate exacto de medio centavo.
  // Si hay un procedimiento del tarifario elegido, sus montos son fijos — no se derivan
  // de ningún porcentaje (ver TarifarioProcedimiento / RegistrarCobroUseCase).
  const montoCobertura = procedimientoSeleccionado
    ? procedimientoSeleccionado.montoSeguro
    : seguroSeleccionado
      ? redondearBancario(montoTotalNumero * (seguroSeleccionado.porcentajeCobertura / 100))
      : 0
  const montoACargoPaciente = montoTotalNumero - montoCobertura
  const montoPagadoFinal = pagoParcial ? Number(montoPagadoParcial) || 0 : montoACargoPaciente

  const totalLineasPago = lineasPago.reduce((acumulado, linea) => acumulado + (Number(linea.monto) || 0), 0)

  const handleToggleDividirPago = (activar: boolean) => {
    setDividirPago(activar)
    if (activar) {
      // Arranca con una sola línea que hereda lo que ya había en el flujo simple, para
      // no perder lo que el cajero ya tipeó al activar "dividir pago".
      setLineasPago([{ metodo: metodoPago, monto: montoPagadoFinal > 0 ? String(montoPagadoFinal) : '' }])
    }
  }

  const agregarLineaPago = () => {
    const metodoLibre = METODOS_PAGO.find((m) => !lineasPago.some((linea) => linea.metodo === m))
    if (!metodoLibre) return
    setLineasPago((actual) => [...actual, { metodo: metodoLibre, monto: '' }])
  }

  const quitarLineaPago = (indice: number) => {
    setLineasPago((actual) => actual.filter((_, i) => i !== indice))
  }

  const actualizarLineaPago = (indice: number, cambios: Partial<{ metodo: MetodoPago; monto: string }>) => {
    setLineasPago((actual) => actual.map((linea, i) => (i === indice ? { ...linea, ...cambios } : linea)))
  }

  // Lo que realmente viaja al backend: si no se dividió el pago, una sola línea con el
  // método simple de siempre (o ninguna, si el cobro queda 100% a deuda).
  const pagosParaEnviar: Pago[] = dividirPago
    ? lineasPago
        .filter((linea) => (Number(linea.monto) || 0) > 0)
        .map((linea) => ({ metodo: linea.metodo, monto: Number(linea.monto) }))
    : montoPagadoFinal > 0
      ? [{ metodo: metodoPago, monto: montoPagadoFinal }]
      : []

  const limpiarFormulario = () => {
    setConcepto('')
    setMontoTotal('')
    setMetodoPago('Efectivo')
    setSeguroMedicoId('')
    setCodigoAutorizacion('')
    setPagoParcial(false)
    setMontoPagadoParcial('')
    setTarifarioProcedimientoId('')
    setDividirPago(false)
    setLineasPago([])
    setEspecialidadServicio(SIN_ESPECIALIDAD)
    setServicioId('')
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
    if (dividirPago && totalLineasPago > montoACargoPaciente + 0.001) {
      setErrorCobro('La suma de los pagos no puede superar el monto a cargo del paciente.')
      return
    }

    setRegistrando(true)
    try {
      const cobro = await api.post<Cobro>('/api/cobros', {
        pacienteId: pacienteSeleccionado.id,
        citaId,
        concepto: concepto.trim(),
        montoTotal: montoTotalNumero,
        pagos: pagosParaEnviar,
        seguroMedicoId: seguroMedicoId || null,
        codigoAutorizacion: seguroMedicoId ? codigoAutorizacion.trim() : null,
        tarifarioProcedimientoId: tarifarioProcedimientoId || null,
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
            <div className="cobros-buscador-fila">
              <input
                type="search"
                placeholder="Nombre o cédula…"
                value={busqueda}
                onChange={(event) => setBusqueda(event.target.value)}
              />
              <button
                type="button"
                className="cobros-boton-nuevo-paciente"
                onClick={() => {
                  setMostrarNuevoPaciente((actual) => !actual)
                  setErrorCrearPaciente(null)
                }}
              >
                {mostrarNuevoPaciente ? 'Cancelar' : '+ Nuevo paciente'}
              </button>
            </div>
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

            {mostrarNuevoPaciente && (
              <form className="cobros-nuevo-paciente-form" onSubmit={(event) => void handleCrearPaciente(event)}>
                <input
                  placeholder="Nombre"
                  value={nuevoNombre}
                  onChange={(event) => setNuevoNombre(event.target.value)}
                  required
                />
                <input
                  placeholder="Apellido"
                  value={nuevoApellido}
                  onChange={(event) => setNuevoApellido(event.target.value)}
                  required
                />
                <input
                  placeholder="Cédula"
                  value={nuevoCedula}
                  onChange={(event) => setNuevoCedula(event.target.value)}
                  required
                />
                <input
                  placeholder="Teléfono (opcional)"
                  value={nuevoTelefono}
                  onChange={(event) => setNuevoTelefono(event.target.value)}
                />
                <button type="submit" disabled={creandoPaciente}>
                  {creandoPaciente ? 'Agregando…' : 'Agregar y cobrar'}
                </button>
                {errorCrearPaciente && <p className="cobros-error">{errorCrearPaciente}</p>}
              </form>
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
                <label className="cobros-label">
                  Seguro médico (opcional)
                  <select
                    value={seguroMedicoId}
                    onChange={(event) => {
                      setSeguroMedicoId(event.target.value)
                      setServicioId('')
                    }}
                  >
                    <option value="">Sin seguro</option>
                    {seguros.map((seguro) => (
                      <option key={seguro.id} value={seguro.id}>
                        {seguro.nombre} ({seguro.porcentajeCobertura}%)
                      </option>
                    ))}
                  </select>
                </label>

                {tieneTarifario && (
                  <>
                    <label className="cobros-label">
                      Plan
                      <select value={planTarifario} onChange={(event) => setPlanTarifario(event.target.value as PlanAseguradora)}>
                        {PLANES_ASEGURADORA.map((plan) => (
                          <option key={plan} value={plan}>
                            {ETIQUETA_PLAN[plan]}
                          </option>
                        ))}
                      </select>
                    </label>
                    <label className="cobros-label">
                      Procedimiento del tarifario
                      <select
                        value={tarifarioProcedimientoId}
                        onChange={(event) => seleccionarProcedimiento(event.target.value)}
                      >
                        <option value="">— Manual (sin tarifario) —</option>
                        {tarifario.map((t) => (
                          <option key={t.id} value={t.id}>
                            {t.procedimiento} — {formateadorMoneda.format(t.montoTotal)}
                          </option>
                        ))}
                      </select>
                    </label>
                  </>
                )}

                {!procedimientoSeleccionado && (
                  <>
                    <label className="cobros-label">
                      Área (opcional)
                      <select
                        value={especialidadServicio}
                        onChange={(event) => {
                          setEspecialidadServicio(event.target.value as EspecialidadMedica)
                          setServicioId('')
                        }}
                      >
                        <option value={SIN_ESPECIALIDAD}>Todas las áreas</option>
                        {ESPECIALIDADES.map((opcion) => (
                          <option key={opcion} value={opcion}>
                            {ETIQUETA_ESPECIALIDAD[opcion]}
                          </option>
                        ))}
                      </select>
                    </label>
                    <label className="cobros-label">
                      Servicio (opcional — precarga concepto y monto)
                      <select value={servicioId} onChange={(event) => seleccionarServicio(event.target.value)}>
                        <option value={OTRO_SERVICIO}>Otro (escribir concepto a mano)</option>
                        {serviciosFiltrados.map((servicio) => (
                          <option key={servicio.id} value={servicio.id}>
                            {servicio.nombre} — {formateadorMoneda.format(servicio.precio1)}
                          </option>
                        ))}
                      </select>
                    </label>
                  </>
                )}

                <input
                  placeholder="Concepto"
                  value={concepto}
                  onChange={(event) => setConcepto(event.target.value)}
                  readOnly={!!procedimientoSeleccionado}
                  required
                />
                <input
                  type="number"
                  min={0.01}
                  step="0.01"
                  placeholder="Monto"
                  value={montoTotal}
                  onChange={(event) => setMontoTotal(event.target.value)}
                  readOnly={!!procedimientoSeleccionado}
                  required
                />

                {seguroSeleccionado && (
                  <>
                    <div className="cobros-cobertura-info">
                      <span>
                        Cobertura {procedimientoSeleccionado ? '(tarifario)' : `(${seguroSeleccionado.porcentajeCobertura}%)`}:{' '}
                        {formateadorMoneda.format(montoCobertura)}
                      </span>
                      <span>Co-pago del paciente: {formateadorMoneda.format(montoACargoPaciente)}</span>
                      {!!procedimientoSeleccionado?.montoFondo && (
                        <>
                          <span>Fondo interno de la fundación: {formateadorMoneda.format(procedimientoSeleccionado.montoFondo)}</span>
                          <span>
                            Reclamo total a la ARS: {formateadorMoneda.format(montoCobertura + procedimientoSeleccionado.montoFondo)}
                          </span>
                        </>
                      )}
                    </div>
                    <input
                      placeholder="Código de autorización"
                      value={codigoAutorizacion}
                      onChange={(event) => setCodigoAutorizacion(event.target.value)}
                      required
                    />
                  </>
                )}

                {!dividirPago && (
                  <>
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
                  </>
                )}

                <label className="cobros-label-checkbox">
                  <input
                    type="checkbox"
                    checked={dividirPago}
                    onChange={(event) => handleToggleDividirPago(event.target.checked)}
                  />
                  Dividir el pago entre varios métodos (ej. parte con tarjeta, parte en efectivo)
                </label>

                {dividirPago && (
                  <div className="cobros-pagos-divididos">
                    {lineasPago.map((linea, indice) => (
                      <div key={indice} className="cobros-linea-pago">
                        <select
                          value={linea.metodo}
                          onChange={(event) => actualizarLineaPago(indice, { metodo: event.target.value as MetodoPago })}
                        >
                          {METODOS_PAGO.filter(
                            (m) => m === linea.metodo || !lineasPago.some((otra) => otra.metodo === m),
                          ).map((opcion) => (
                            <option key={opcion} value={opcion}>
                              {opcion}
                            </option>
                          ))}
                        </select>
                        <input
                          type="number"
                          min={0}
                          step="0.01"
                          placeholder="Monto"
                          value={linea.monto}
                          onChange={(event) => actualizarLineaPago(indice, { monto: event.target.value })}
                        />
                        <button type="button" onClick={() => quitarLineaPago(indice)} title="Quitar línea">
                          ×
                        </button>
                      </div>
                    ))}
                    {lineasPago.length < METODOS_PAGO.length && (
                      <button type="button" className="cobros-agregar-linea-pago" onClick={agregarLineaPago}>
                        + Agregar método de pago
                      </button>
                    )}
                    <p
                      className={
                        totalLineasPago > montoACargoPaciente + 0.001 ? 'cobros-error' : 'cobros-monto-a-cobrar'
                      }
                    >
                      Pagos ingresados: {formateadorMoneda.format(totalLineasPago)} de {formateadorMoneda.format(montoACargoPaciente)} a cobrar
                      {totalLineasPago < montoACargoPaciente - 0.001 && ' (el resto queda como deuda pendiente)'}
                    </p>
                  </div>
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
              <p>
                Seguro: {ultimoCobro.seguroMedicoNombre}{' '}
                ({ultimoCobro.porcentajeCobertura !== null ? `${ultimoCobro.porcentajeCobertura}%` : 'tarifario'})
              </p>
              <p>Cubierto por seguro: {formateadorMoneda.format(ultimoCobro.montoCobertura ?? 0)}</p>
              {!!ultimoCobro.montoFondo && (
                <>
                  <p>Fondo interno de la fundación: {formateadorMoneda.format(ultimoCobro.montoFondo)}</p>
                  <p>
                    Reclamo total a la ARS: {formateadorMoneda.format((ultimoCobro.montoCobertura ?? 0) + ultimoCobro.montoFondo)}
                  </p>
                </>
              )}
              <p>Código de autorización: {ultimoCobro.codigoAutorizacion}</p>
            </>
          )}
          {ultimoCobro.pagos.length === 0 ? (
            <p>Pago: nada pagado todavía (a deuda)</p>
          ) : ultimoCobro.pagos.length === 1 ? (
            <p>Método de pago: {ultimoCobro.pagos[0].metodo}</p>
          ) : (
            <p>
              Métodos de pago: {ultimoCobro.pagos.map((p) => `${p.metodo} ${formateadorMoneda.format(p.monto)}`).join(' + ')}
            </p>
          )}
          <p>Monto pagado: {formateadorMoneda.format(ultimoCobro.montoPagado)}</p>
          {ultimoCobro.montoPendiente > 0 && <p>Saldo pendiente: {formateadorMoneda.format(ultimoCobro.montoPendiente)}</p>}
        </div>
      )}
    </DashboardLayout>
  )
}
