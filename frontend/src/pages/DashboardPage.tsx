import { ArrowRight } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { StatCard } from '../components/dashboard/StatCard'
import { MonthlyMetricChart } from '../components/dashboard/MonthlyMetricChart'
import { Sparkline } from '../components/dashboard/Sparkline'
import { api, ApiError } from '../lib/api'
import type { AlertasAdmin, PacientesPorDoctor, ResumenDiario, SesionesActivas } from '../types/dashboard'
import type { CitaAgenda } from '../types/cita'
import type { EspecialidadMedica, Usuario } from '../types/usuario'
import { ETIQUETA_ESPECIALIDAD } from '../types/personal'
import { coloresParaTema, colorMetodoPago, colorRolParaTema } from '../styles/colors'
import type { MetodoPago } from '../types/cobro'
import { useTheme } from '../theme/ThemeContext'
import './DashboardPage.css'

const ROLES_COBERTURA = ['Doctor', 'Fondos', 'Lemy'] as const

const TAMANO_VENTANA_SPARKLINE = 7

const formateadorMoneda = new Intl.NumberFormat('es-DO', {
  style: 'currency',
  currency: 'DOP',
  maximumFractionDigits: 0,
})

const formateadorEntero = new Intl.NumberFormat('es-DO')

const formateadorFechaLarga = new Intl.DateTimeFormat('es-DO', { dateStyle: 'long' })

function capitalizar(texto: string) {
  return texto.charAt(0).toUpperCase() + texto.slice(1)
}

function fechaISOaTextoLargo(fechaIso: string): string {
  // Se parte a mano (no `new Date(fechaIso)`) para no depender de la zona horaria del
  // navegador: un input type="date" da "2026-08-29" y queremos ese mismo día, no el
  // día anterior si el usuario está en una zona con offset negativo.
  const [anio, mes, dia] = fechaIso.split('-').map(Number)
  return formateadorFechaLarga.format(new Date(anio, mes - 1, dia))
}

interface FilaPacientesPorDoctor {
  doctorId: string
  nombreCompleto: string
  especialidad: string | null
  citasCompletadas: number
  pacientesDistintos: number
}

export function DashboardPage() {
  const { tema } = useTheme()
  const chartColors = coloresParaTema(tema)
  const [resumenHoy, setResumenHoy] = useState<ResumenDiario | null>(null)
  const [resumenMes, setResumenMes] = useState<ResumenDiario[]>([])
  const [alertas, setAlertas] = useState<AlertasAdmin | null>(null)
  const [personal, setPersonal] = useState<Usuario[]>([])
  const [pacientesPorDoctor, setPacientesPorDoctor] = useState<PacientesPorDoctor[]>([])
  const [sesionesActivas, setSesionesActivas] = useState<SesionesActivas | null>(null)
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Filtro de "Pacientes atendidos por doctor": vacío = histórico acumulado (comportamiento
  // original). Con fecha, se reconsulta la agenda de ese día en vez de pedirle al backend un
  // endpoint nuevo — /api/citas/agenda ya soporta ?fecha= y ya es accesible para Admin.
  const [filtroFechaDoctor, setFiltroFechaDoctor] = useState('')
  const [citasDelDia, setCitasDelDia] = useState<CitaAgenda[]>([])
  const [cargandoCitasDelDia, setCargandoCitasDelDia] = useState(false)
  const [errorCitasDelDia, setErrorCitasDelDia] = useState<string | null>(null)
  const [mostrarDoctoresSinActividad, setMostrarDoctoresSinActividad] = useState(false)

  useEffect(() => {
    let cancelado = false

    async function cargar() {
      setCargando(true)
      setError(null)
      try {
        const [hoy, mes, alertasAdmin, personalLista, porDoctor, sesiones] = await Promise.all([
          api.get<ResumenDiario>('/api/dashboard/resumen-hoy'),
          api.get<ResumenDiario[]>('/api/dashboard/resumen-mes'),
          api.get<AlertasAdmin>('/api/dashboard/alertas'),
          api.get<Usuario[]>('/api/personal'),
          api.get<PacientesPorDoctor[]>('/api/dashboard/pacientes-por-doctor'),
          api.get<SesionesActivas>('/api/sesiones/activas'),
        ])
        if (!cancelado) {
          setResumenHoy(hoy)
          setResumenMes(mes)
          setAlertas(alertasAdmin)
          setPersonal(personalLista)
          setPacientesPorDoctor(porDoctor)
          setSesionesActivas(sesiones)
        }
      } catch (err) {
        if (!cancelado) {
          setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar el dashboard.')
        }
      } finally {
        if (!cancelado) {
          setCargando(false)
        }
      }
    }

    void cargar()
    return () => {
      cancelado = true
    }
  }, [])

  useEffect(() => {
    if (!filtroFechaDoctor) {
      setCitasDelDia([])
      setErrorCitasDelDia(null)
      return
    }
    let cancelado = false
    setCargandoCitasDelDia(true)
    setErrorCitasDelDia(null)
    api
      .get<CitaAgenda[]>(`/api/citas/agenda?fecha=${filtroFechaDoctor}`)
      .then((datos) => {
        if (!cancelado) setCitasDelDia(datos)
      })
      .catch((err) => {
        if (!cancelado) {
          setErrorCitasDelDia(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo cargar la agenda de ese día.')
        }
      })
      .finally(() => {
        if (!cancelado) setCargandoCitasDelDia(false)
      })
    return () => {
      cancelado = true
    }
  }, [filtroFechaDoctor])

  // Con fecha seleccionada, se arma la fila por doctor a mano a partir de las citas de ese
  // día (agrupando por doctorId); sin fecha, se usa tal cual el histórico que ya trae el
  // backend en pacientesPorDoctor.
  const filasPorDoctor: FilaPacientesPorDoctor[] = useMemo(() => {
    if (!filtroFechaDoctor) return pacientesPorDoctor

    const porDoctor = new Map<string, { nombre: string; citas: number; pacientes: Set<string> }>()
    citasDelDia
      .filter((cita) => cita.estado === 'Completada')
      .forEach((cita) => {
        const actual = porDoctor.get(cita.doctorId) ?? { nombre: cita.doctorNombre, citas: 0, pacientes: new Set<string>() }
        actual.citas += 1
        actual.pacientes.add(cita.pacienteId)
        porDoctor.set(cita.doctorId, actual)
      })

    return Array.from(porDoctor.entries())
      .map(([doctorId, datos]) => ({
        doctorId,
        nombreCompleto: datos.nombre,
        especialidad: personal.find((u) => u.id === doctorId)?.especialidad ?? null,
        citasCompletadas: datos.citas,
        pacientesDistintos: datos.pacientes.size,
      }))
      .sort((a, b) => b.pacientesDistintos - a.pacientesDistintos || a.nombreCompleto.localeCompare(b.nombreCompleto))
  }, [filtroFechaDoctor, citasDelDia, pacientesPorDoctor, personal])

  const doctoresConActividad = filasPorDoctor.filter((d) => d.citasCompletadas > 0)
  const doctoresSinActividadCount = filasPorDoctor.length - doctoresConActividad.length
  const filasDoctorVisibles = mostrarDoctoresSinActividad ? filasPorDoctor : doctoresConActividad
  const cargandoTablaDoctor = filtroFechaDoctor ? cargandoCitasDelDia : cargando

  const nombreMes = capitalizar(new Date().toLocaleDateString('es-DO', { month: 'long', year: 'numeric' }))

  const ventanaMes = resumenMes.slice(-TAMANO_VENTANA_SPARKLINE)
  const sparkPacientes = ventanaMes.map((r) => r.pacientesAtendidos)
  const sparkDinero = ventanaMes.map((r) => r.dineroMovido)

  const totalStaffCount = personal.length
  const activeStaffCount = personal.filter((u) => u.activo).length

  const coberturaPorArea = ROLES_COBERTURA.map((rol) => {
    const delRol = personal.filter((u) => u.rol === rol)
    const activos = delRol.filter((u) => u.activo).length
    const total = delRol.length
    return {
      rol,
      color: colorRolParaTema(tema, rol),
      activos,
      total,
      porcentaje: total > 0 ? Math.round((activos / total) * 100) : 0,
    }
  })

  const stockBajoCount = alertas?.stockBajo.length ?? 0
  const pacientesConDeudaCount = alertas?.pacientesConDeuda.length ?? 0

  const METODOS: MetodoPago[] = ['Efectivo', 'Tarjeta', 'Transferencia']

  function desglosePorMetodo(efectivo: number, tarjeta: number, transferencia: number) {
    const total = efectivo + tarjeta + transferencia
    const montos: Record<MetodoPago, number> = { Efectivo: efectivo, Tarjeta: tarjeta, Transferencia: transferencia }
    return METODOS.map((metodo) => ({
      metodo,
      monto: montos[metodo],
      color: colorMetodoPago(metodo),
      porcentaje: total > 0 ? Math.round((montos[metodo] / total) * 100) : 0,
    }))
  }

  const desgloseHoy = desglosePorMetodo(
    resumenHoy?.dineroEfectivo ?? 0, resumenHoy?.dineroTarjeta ?? 0, resumenHoy?.dineroTransferencia ?? 0,
  )
  const desgloseMes = desglosePorMetodo(
    resumenMes.reduce((acumulado, r) => acumulado + r.dineroEfectivo, 0),
    resumenMes.reduce((acumulado, r) => acumulado + r.dineroTarjeta, 0),
    resumenMes.reduce((acumulado, r) => acumulado + r.dineroTransferencia, 0),
  )

  return (
    <DashboardLayout titulo="Dashboard">
      {error && <p className="dashboard-error">{error}</p>}

      <section className="dashboard-stats">
        <StatCard
          etiqueta="Pacientes atendidos hoy"
          valor={cargando ? '—' : formateadorEntero.format(resumenHoy?.pacientesAtendidos ?? 0)}
          colorSerie={chartColors.pacientes}
          icono="medical"
          sparkline={
            sparkPacientes.length > 0 && (
              <Sparkline valores={sparkPacientes} color={chartColors.pacientes} modo="linea" />
            )
          }
        />
        <StatCard
          etiqueta="Movimientos hoy"
          valor={cargando ? '—' : formateadorMoneda.format(resumenHoy?.dineroMovido ?? 0)}
          colorSerie={chartColors.dinero}
          icono="dollar"
          sparkline={
            sparkDinero.length > 0 && <Sparkline valores={sparkDinero} color={chartColors.dinero} modo="barras" />
          }
        />
        <StatCard
          etiqueta="Personal activo"
          valor={cargando ? '—' : `${activeStaffCount} de ${totalStaffCount}`}
          colorSerie={chartColors.actividad}
          icono="users"
        />
        <StatCard
          etiqueta="Sesiones activas"
          valor={cargando ? '—' : formateadorEntero.format(sesionesActivas?.cantidad ?? 0)}
          colorSerie={chartColors.pacientes}
          icono="activity"
        />
      </section>

      {alertas && (alertas.stockBajo.length > 0 || alertas.pacientesConDeuda.length > 0) && (
        <section className="dashboard-alertas">
          {alertas.stockBajo.length > 0 && (
            <div className="dashboard-alerta-card">
              <div className="dashboard-alerta-header">
                <h2>Inventario con stock bajo</h2>
                <span className="dashboard-alerta-contador">{alertas.stockBajo.length}</span>
              </div>
              <ul className="dashboard-alerta-lista">
                {alertas.stockBajo.slice(0, 5).map((item) => (
                  <li key={item.id}>
                    <span>{item.nombre}</span>
                    <span className="text-muted">
                      {item.stockActual} / {item.stockMinimo} mín.
                    </span>
                  </li>
                ))}
              </ul>
              <Link to="/inventario" className="dashboard-alerta-link">
                Ver inventario <ArrowRight size={14} aria-hidden="true" />
              </Link>
            </div>
          )}

          {alertas.pacientesConDeuda.length > 0 && (
            <div className="dashboard-alerta-card">
              <div className="dashboard-alerta-header">
                <h2>Pacientes con deuda pendiente</h2>
                <span className="dashboard-alerta-contador">{alertas.pacientesConDeuda.length}</span>
              </div>
              <ul className="dashboard-alerta-lista">
                {alertas.pacientesConDeuda.slice(0, 5).map((paciente) => (
                  <li key={paciente.pacienteId}>
                    <span>{paciente.pacienteNombre}</span>
                    <span className="text-muted">{formateadorMoneda.format(paciente.montoAdeudado)}</span>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </section>
      )}

      <section className="dashboard-vista-mes">
        <div className="dashboard-vista-mes-encabezado">
          <h2>Vista del mes</h2>
          <span className="text-muted dashboard-vista-mes-periodo">{nombreMes}</span>
        </div>

        <div className="dashboard-vista-mes-graficos">
          <MonthlyMetricChart
            titulo="Pacientes atendidos"
            datos={resumenMes}
            dataKey="pacientesAtendidos"
            color={chartColors.pacientes}
            formatearValor={(valor) => formateadorEntero.format(valor)}
          />
          <MonthlyMetricChart
            titulo="Dinero movido"
            datos={resumenMes}
            dataKey="dineroMovido"
            color={chartColors.dinero}
            formatearValor={(valor) => formateadorMoneda.format(valor)}
          />
        </div>
      </section>

      <section className="dashboard-pacientes-doctor-card">
        <div className="dashboard-pacientes-doctor-encabezado">
          <div>
            <h2>Pacientes atendidos por doctor</h2>
            <p className="text-secondary dashboard-pacientes-doctor-subtitulo">
              {filtroFechaDoctor
                ? `Citas completadas el ${fechaISOaTextoLargo(filtroFechaDoctor)}.`
                : 'Histórico acumulado de citas completadas por cada doctor activo.'}
            </p>
          </div>
          <div className="dashboard-pacientes-doctor-filtros">
            <input
              type="date"
              value={filtroFechaDoctor}
              max={new Date().toISOString().slice(0, 10)}
              onChange={(event) => {
                setFiltroFechaDoctor(event.target.value)
                setMostrarDoctoresSinActividad(false)
              }}
              aria-label="Filtrar por fecha específica"
            />
            {filtroFechaDoctor && (
              <button
                type="button"
                className="dashboard-pacientes-doctor-limpiar"
                onClick={() => {
                  setFiltroFechaDoctor('')
                  setMostrarDoctoresSinActividad(false)
                }}
              >
                Ver histórico
              </button>
            )}
          </div>
        </div>

        {errorCitasDelDia && <p className="dashboard-error">{errorCitasDelDia}</p>}

        {cargandoTablaDoctor ? (
          <p className="text-secondary cargando-pulso">Cargando…</p>
        ) : filasDoctorVisibles.length === 0 ? (
          <p className="text-secondary">
            {filtroFechaDoctor ? 'Ningún doctor completó citas ese día.' : 'No hay doctores activos registrados.'}
          </p>
        ) : (
          <table className="dashboard-pacientes-doctor-tabla">
            <thead>
              <tr>
                <th>Doctor</th>
                <th>Especialidad</th>
                <th>Citas completadas</th>
                <th>Pacientes distintos</th>
              </tr>
            </thead>
            <tbody>
              {filasDoctorVisibles.map((doctor) => (
                <tr key={doctor.doctorId}>
                  <td>{doctor.nombreCompleto}</td>
                  <td className="text-muted">
                    {doctor.especialidad ? ETIQUETA_ESPECIALIDAD[doctor.especialidad as EspecialidadMedica] : '—'}
                  </td>
                  <td>{formateadorEntero.format(doctor.citasCompletadas)}</td>
                  <td className="dashboard-pacientes-doctor-destacado">{formateadorEntero.format(doctor.pacientesDistintos)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        {!cargandoTablaDoctor && doctoresSinActividadCount > 0 && (
          <button
            type="button"
            className="dashboard-pacientes-doctor-toggle"
            onClick={() => setMostrarDoctoresSinActividad((valor) => !valor)}
          >
            {mostrarDoctoresSinActividad
              ? 'Ocultar doctores sin actividad'
              : `Mostrar ${doctoresSinActividadCount} doctor${doctoresSinActividadCount === 1 ? '' : 'es'} sin actividad`}
          </button>
        )}
      </section>

      <section className="dashboard-metodos-pago">
        <div className="dashboard-cobertura-card">
          <h2>Cómo entra el dinero — hoy</h2>
          {desgloseHoy.every((m) => m.monto === 0) ? (
            <p className="text-secondary">Todavía no se registró ningún cobro hoy.</p>
          ) : (
            desgloseHoy.map((m) => (
              <div key={m.metodo} className="dashboard-cobertura-fila">
                <div className="dashboard-cobertura-etiqueta">
                  <span className="dashboard-cobertura-punto" style={{ background: m.color }} />
                  {m.metodo}
                  <span className="text-muted">{formateadorMoneda.format(m.monto)}</span>
                </div>
                <div className="dashboard-cobertura-barra">
                  <div className="dashboard-cobertura-relleno" style={{ background: m.color, width: `${m.porcentaje}%` }} />
                </div>
              </div>
            ))
          )}
        </div>

        <div className="dashboard-cobertura-card">
          <h2>Cómo entra el dinero — {nombreMes}</h2>
          {desgloseMes.every((m) => m.monto === 0) ? (
            <p className="text-secondary">Todavía no se registró ningún cobro este mes.</p>
          ) : (
            desgloseMes.map((m) => (
              <div key={m.metodo} className="dashboard-cobertura-fila">
                <div className="dashboard-cobertura-etiqueta">
                  <span className="dashboard-cobertura-punto" style={{ background: m.color }} />
                  {m.metodo}
                  <span className="text-muted">{formateadorMoneda.format(m.monto)}</span>
                </div>
                <div className="dashboard-cobertura-barra">
                  <div className="dashboard-cobertura-relleno" style={{ background: m.color, width: `${m.porcentaje}%` }} />
                </div>
              </div>
            ))
          )}
        </div>
      </section>

      <section className="dashboard-detalle">
        <div className="dashboard-cobertura-card">
          <h2>Personal por área</h2>
          {coberturaPorArea.map((area) => (
            <div key={area.rol} className="dashboard-cobertura-fila">
              <div className="dashboard-cobertura-etiqueta">
                <span className="dashboard-cobertura-punto" style={{ background: area.color }} />
                {area.rol}
                <span className="text-muted">{area.total > 0 ? `${area.porcentaje}%` : '—'}</span>
              </div>
              <div className="dashboard-cobertura-barra">
                <div
                  className="dashboard-cobertura-relleno"
                  style={{ background: area.color, width: `${area.porcentaje}%` }}
                />
              </div>
            </div>
          ))}
        </div>

        <div className="dashboard-insights-card">
          <h2>Insights operativos</h2>
          <div className="dashboard-insight-item">
            <p className="text-muted">Alertas de inventario</p>
            <p className="dashboard-insight-valor" style={{ color: stockBajoCount > 0 ? 'var(--danger)' : undefined }}>
              {stockBajoCount} {stockBajoCount === 1 ? 'insumo' : 'insumos'} por agotarse
            </p>
          </div>
          <div className="dashboard-insight-item">
            <p className="text-muted">Pacientes con deuda</p>
            <p
              className="dashboard-insight-valor"
              style={{ color: pacientesConDeudaCount > 0 ? chartColors.dinero : undefined }}
            >
              {pacientesConDeudaCount} con saldo pendiente
            </p>
          </div>
          <div className="dashboard-insight-item">
            <p className="text-muted">Estado del personal</p>
            <p className="dashboard-insight-valor" style={{ color: chartColors.pacientes }}>
              {activeStaffCount} activos hoy
            </p>
          </div>
        </div>
      </section>
    </DashboardLayout>
  )
}
