import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { StatCard } from '../components/dashboard/StatCard'
import { MonthlyMetricChart } from '../components/dashboard/MonthlyMetricChart'
import { Sparkline } from '../components/dashboard/Sparkline'
import { api, ApiError } from '../lib/api'
import type { AlertasAdmin, PacientesPorDoctor, ResumenDiario, SesionesActivas } from '../types/dashboard'
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

function capitalizar(texto: string) {
  return texto.charAt(0).toUpperCase() + texto.slice(1)
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
                Ver inventario →
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
        <h2>Pacientes atendidos por doctor</h2>
        <p className="text-secondary dashboard-pacientes-doctor-subtitulo">
          Histórico acumulado de citas completadas por cada doctor activo.
        </p>
        {cargando ? (
          <p className="text-secondary cargando-pulso">Cargando…</p>
        ) : pacientesPorDoctor.length === 0 ? (
          <p className="text-secondary">No hay doctores activos registrados.</p>
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
              {pacientesPorDoctor.map((doctor) => (
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
