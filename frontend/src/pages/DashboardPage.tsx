import { useEffect, useState } from 'react'
import { DashboardLayout } from '../components/layout/DashboardLayout'
import { StatCard } from '../components/dashboard/StatCard'
import { MonthlyMetricChart } from '../components/dashboard/MonthlyMetricChart'
import { api, ApiError } from '../lib/api'
import type { ResumenDiario } from '../types/dashboard'
import { coloresParaTema } from '../styles/colors'
import { useTheme } from '../theme/ThemeContext'
import './DashboardPage.css'

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
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelado = false

    async function cargar() {
      setCargando(true)
      setError(null)
      try {
        const [hoy, mes] = await Promise.all([
          api.get<ResumenDiario>('/api/dashboard/resumen-hoy'),
          api.get<ResumenDiario[]>('/api/dashboard/resumen-mes'),
        ])
        if (!cancelado) {
          setResumenHoy(hoy)
          setResumenMes(mes)
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

  return (
    <DashboardLayout titulo="Dashboard">
      {error && <p className="dashboard-error">{error}</p>}

      <section className="dashboard-stats">
        <StatCard
          etiqueta="Pacientes atendidos hoy"
          valor={cargando ? '—' : formateadorEntero.format(resumenHoy?.pacientesAtendidos ?? 0)}
          colorSerie={chartColors.pacientes}
        />
        <StatCard
          etiqueta="Movimientos hoy"
          valor={cargando ? '—' : formateadorMoneda.format(resumenHoy?.dineroMovido ?? 0)}
          colorSerie={chartColors.dinero}
        />
      </section>

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
    </DashboardLayout>
  )
}
