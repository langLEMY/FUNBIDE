import { Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import type { ResumenDiario } from '../../types/dashboard'
import { coloresParaTema } from '../../styles/colors'
import { useTheme } from '../../theme/ThemeContext'
import './MonthlyMetricChart.css'

// Espejo JS de --duracion-lenta (theme.css) -- igual que chartColorsPorTema en
// colors.ts, recharts recibe milisegundos como número, no puede leer un var()
// CSS. Mantener sincronizado a mano si --duracion-lenta cambia.
const DURACION_ANIMACION_MS = 320

interface MonthlyMetricChartProps {
  titulo: string
  datos: ResumenDiario[]
  dataKey: 'pacientesAtendidos' | 'dineroMovido'
  color: string
  formatearValor: (valor: number) => string
}

interface TooltipVidrioProps {
  active?: boolean
  payload?: { value?: number }[]
  label?: number | string
  titulo: string
  formatearValor: (valor: number) => string
}

function TooltipVidrio({ active, payload, label, titulo, formatearValor }: TooltipVidrioProps) {
  if (!active || !payload?.length) return null
  return (
    <div className="monthly-chart-tooltip">
      <p className="monthly-chart-tooltip-label">Día {label}</p>
      <p className="monthly-chart-tooltip-valor">
        {formatearValor(Number(payload[0].value))} · {titulo}
      </p>
    </div>
  )
}

export function MonthlyMetricChart({ titulo, datos, dataKey, color, formatearValor }: MonthlyMetricChartProps) {
  const { tema } = useTheme()
  const chartColors = coloresParaTema(tema)
  const gradientId = `monthly-chart-fill-${dataKey}`
  const animar =
    typeof window === 'undefined' || !window.matchMedia('(prefers-reduced-motion: reduce)').matches

  const puntos = datos.map((resumen) => ({
    dia: Number(resumen.fecha.slice(-2)),
    valor: resumen[dataKey],
  }))

  return (
    <div className="monthly-chart-card">
      <p className="monthly-chart-titulo text-secondary">{titulo}</p>

      {puntos.length === 0 ? (
        <div className="monthly-chart-vacio text-muted">Sin movimientos registrados este mes todavía.</div>
      ) : (
        <div className="monthly-chart-area">
          <ResponsiveContainer width="100%" height={220}>
            <AreaChart data={puntos} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
              <defs>
                <linearGradient id={gradientId} x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor={color} stopOpacity={0.25} />
                  <stop offset="100%" stopColor={color} stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid stroke={chartColors.gridline} vertical={false} />
              <XAxis
                dataKey="dia"
                tickLine={false}
                axisLine={{ stroke: chartColors.baseline }}
                tick={{ fill: chartColors.textMuted, fontSize: 12 }}
              />
              <YAxis
                tickLine={false}
                axisLine={false}
                width={44}
                tick={{ fill: chartColors.textMuted, fontSize: 12 }}
                tickFormatter={(valor: number) => formatearValor(valor)}
              />
              <Tooltip
                cursor={{ stroke: chartColors.baseline, strokeWidth: 1 }}
                content={<TooltipVidrio titulo={titulo} formatearValor={formatearValor} />}
              />
              <Area
                type="monotone"
                dataKey="valor"
                stroke={color}
                strokeWidth={2}
                fill={`url(#${gradientId})`}
                dot={false}
                activeDot={{ r: 4, fill: color, stroke: chartColors.surface1, strokeWidth: 2 }}
                isAnimationActive={animar}
                animationDuration={DURACION_ANIMACION_MS}
                animationEasing="ease-out"
              />
            </AreaChart>
          </ResponsiveContainer>
        </div>
      )}
    </div>
  )
}
