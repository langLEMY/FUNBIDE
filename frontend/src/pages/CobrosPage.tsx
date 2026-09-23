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
import type { DoctorSimple } from '../types/doctor'
import { agruparDoctoresPorEspecialidad } from '../lib/agruparDoctores'
import { imprimirVentana } from '../lib/imprimir'
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

  const [doctores, setDoctores] = useState<DoctorSimple[]>([])
  const [doctorId, setDoctorId] = useState('')

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
      api.get<DoctorSimple[]>('/api/personal/doctores'),
    ])
      .then(([turnoActual, segurosActivos, pendientes, serviciosActivos, doctoresActivos]) => {
        if (cancelado) return
        setTurno(turnoActual)
        setSeguros(segurosActivos)
        setPendientesDeCobro(pendientes)
        setServicios(serviciosActivos)
        setDoctores(doctoresActivos)
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

  const seleccionarPaciente = (paciente: Paciente, citaIdPrefill: string | null = null, doctorIdPrefill = '') => {
    setPacienteSeleccionado(paciente)
    setCitaId(citaIdPrefill)
    setDoctorId(doctorIdPrefill)
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
      {
        id: cita.pacienteId, nombre: cita.pacienteNombre, apellido: '', cedula: '', telefono: null,
        tieneFotoCedula: false, edad: null, condicion: null, estado: 'Activo', ultimaVisita: null,
        creadoEn: new Date(0).toISOString(),
      },
      cita.id,
      cita.doctorId,
    )
    setConcepto(`Consulta — ${cita.motivo}`)
  }

  const gruposDoctores = useMemo(() => agruparDoctoresPorEspecialidad(doctores), [doctores])

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
  // El cálculo automático por % de cobertura está desactivado (ver
  // RegistrarCobroRequestValidator): con seguro, el monto que cubre la aseguradora
  // sale siempre del procedimiento elegido en el tarifario — nunca de un porcentaje.
  const montoCobertura = procedimientoSeleccionado ? procedimientoSeleccionado.montoSeguro : 0
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
    setDoctorId('')
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
    if (!doctorId) {
      setErrorCobro('Selecciona con qué doctor se va a atender.')
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
    if (seguroMedicoId && !tarifarioProcedimientoId) {
      setErrorCobro('Selecciona un procedimiento del tarifario — el cálculo automático por % de cobertura está desactivado.')
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
        doctorId: doctorId || null,
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
    requestAnimationFrame(imprimirVentana)
  }

  /** Reimprime un cobro ya registrado (historial de "Movimientos del día"), no solo el recién creado. */
  const reimprimirCobro = (cobro: Cobro, tipo: TipoComprobante = 'Recibo de ingreso') => {
    setUltimoCobro(cobro)
    setComprobante(tipo)
    requestAnimationFrame(imprimirVentana)
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
                  Doctor que atiende
                  <select value={doctorId} onChange={(event) => setDoctorId(event.target.value)} required>
                    <option value="">— Selecciona un doctor —</option>
                    {gruposDoctores.map((grupo) => (
                      <optgroup key={grupo.etiqueta} label={grupo.etiqueta}>
                        {grupo.doctores.map((doctor) => (
                          <option key={doctor.id} value={doctor.id}>
                            {doctor.nombreCompleto}
                          </option>
                        ))}
                      </optgroup>
                    ))}
                  </select>
                </label>

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
                        {seguro.nombre}
                      </option>
                    ))}
                  </select>
                </label>

                {seguroSeleccionado && !tieneTarifario && (
                  <p className="cobros-error">
                    {seguroSeleccionado.nombre} no tiene tarifario cargado — agregalo en Aseguradoras antes de poder
                    cobrar con esta aseguradora (el cálculo automático por % de cobertura está desactivado).
                  </p>
                )}

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
                        required
                      >
                        <option value="">— Selecciona un procedimiento —</option>
                        {tarifario.map((t) => (
                          <option key={t.id} value={t.id}>
                            {t.procedimiento} — {formateadorMoneda.format(t.montoTotal)}
                          </option>
                        ))}
                      </select>
                    </label>
                  </>
                )}

                {!seguroSeleccionado && (
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
                  readOnly={!!procedimientoSeleccionado || !!seguroSeleccionado}
                  required
                />
                <input
                  type="number"
                  min={0.01}
                  step="0.01"
                  placeholder="Monto"
                  value={montoTotal}
                  onChange={(event) => setMontoTotal(event.target.value)}
                  readOnly={!!procedimientoSeleccionado || !!seguroSeleccionado}
                  required
                />

                {seguroSeleccionado && (
                  <>
                    <div className="cobros-cobertura-info">
                      <span>
                        Cobertura (tarifario): {formateadorMoneda.format(montoCobertura)}
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
                <button
                  type="button"
                  className="cobros-boton-imprimir-factura"
                  disabled={!ultimoCobro}
                  title={ultimoCobro ? 'Imprimir la factura del último cobro procesado' : 'Procesa un cobro primero para poder imprimir su factura'}
                  onClick={() => imprimir('Factura de consumo')}
                >
                  Imprimir factura
                </button>
              </form>
              {errorCobro && <p className="cobros-error">{errorCobro}</p>}
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
                  <button type="button" className="cobros-boton-reimprimir" onClick={() => reimprimirCobro(cobro)} title="Imprimir comprobante">
                    Imprimir
                  </button>
                </li>
              ))}
            </ul>
          )}
        </aside>
      </div>

      {ultimoCobro && comprobante && (
        <div className="cobros-comprobante">
          <header className="cobros-comprobante-membrete">
            {/* Logo con dithering Floyd-Steinberg, pensado para imprimir reconocible en
                blanco y negro puro en la térmica — distinto del logo a color de
                pantalla/sidebar (/logo-funbide.png), que no rinde bien en ese modo. */}
            <img className="cobros-comprobante-logo" src="/logo-funbide-recibo-termico.png" alt="" />
            <span className="cobros-comprobante-clinica">FUNDACIÓN BIENESTAR Y DESARROLLO</span>
            <span className="cobros-comprobante-direccion">
              Calle Guaroa No. 4, Esq. Simón Orozco, Invivienda
              <br />
              Santo Domingo Este, Hainamosa, Distrito Nacional
            </span>
            <span className="cobros-comprobante-direccion">RNC: 430090387</span>
          </header>

          <div className="cobros-comprobante-divisor" />
          <h1 className="cobros-comprobante-tipo">{comprobante}</h1>
          <p className="cobros-comprobante-fecha">{formateadorFechaHora.format(new Date(ultimoCobro.registradoEn))}</p>
          <div className="cobros-comprobante-divisor" />

          <dl className="cobros-comprobante-datos">
            <dt>Paciente</dt>
            <dd>{ultimoCobro.pacienteNombre}</dd>
            {ultimoCobro.doctorNombre && (
              <>
                <dt>Doctor</dt>
                <dd>{ultimoCobro.doctorNombre}</dd>
              </>
            )}
            <dt>Servicio</dt>
            <dd>{ultimoCobro.concepto}</dd>
          </dl>

          <div className="cobros-comprobante-divisor" />

          <dl className="cobros-comprobante-datos">
            <dt className="cobros-comprobante-dato-destacado">Monto total</dt>
            <dd className="cobros-comprobante-dato-destacado">{formateadorMoneda.format(ultimoCobro.montoTotal)}</dd>
            {ultimoCobro.seguroMedicoNombre && (
              <>
                <dt>Seguro</dt>
                <dd>
                  {ultimoCobro.seguroMedicoNombre}{' '}
                  ({ultimoCobro.porcentajeCobertura !== null ? `${ultimoCobro.porcentajeCobertura}%` : 'tarifario'})
                </dd>
                <dt>Cubierto por seguro</dt>
                <dd>{formateadorMoneda.format(ultimoCobro.montoCobertura ?? 0)}</dd>
                <dt>Código autorización</dt>
                <dd>{ultimoCobro.codigoAutorizacion}</dd>
                {!!ultimoCobro.montoFondo && (
                  <>
                    <dt>Fondo interno de la fundación</dt>
                    <dd>{formateadorMoneda.format(ultimoCobro.montoFondo)}</dd>
                  </>
                )}
              </>
            )}
            <dt>{ultimoCobro.pagos.length > 1 ? 'Métodos de pago' : 'Método de pago'}</dt>
            <dd>
              {ultimoCobro.pagos.length === 0
                ? 'Sin pagar (a deuda)'
                : ultimoCobro.pagos.map((p) => `${p.metodo} ${formateadorMoneda.format(p.monto)}`).join(' + ')}
            </dd>
            <dt className="cobros-comprobante-dato-destacado">Monto pagado</dt>
            <dd className="cobros-comprobante-dato-destacado">{formateadorMoneda.format(ultimoCobro.montoPagado)}</dd>
            {ultimoCobro.montoPendiente > 0 && (
              <>
                <dt>Saldo pendiente</dt>
                <dd>{formateadorMoneda.format(ultimoCobro.montoPendiente)}</dd>
              </>
            )}
          </dl>

          <div className="cobros-comprobante-divisor" />
          <p className="cobros-comprobante-gracias">¡Gracias por confiar en nosotros!</p>
        </div>
      )}
    </DashboardLayout>
  )
}
