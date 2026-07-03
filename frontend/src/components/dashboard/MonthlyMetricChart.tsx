import { Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import type { ResumenDiario } from '../../types/dashboard'
import { chartColors } from '../../styles/colors'
import './MonthlyMetricChart.css'

interface MonthlyMetricChartProps {
  titulo: string
  datos: ResumenDiario[]
  dataKey: 'pacientesAtendidos' | 'dineroMovido'
  color: string
  formatearValor: (valor: number) => string
}

export function MonthlyMetricChart({ titulo, datos, dataKey, color, formatearValor }: MonthlyMetricChartProps) {
  const gradientId = `monthly-chart-fill-${dataKey}`

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
                contentStyle={{
                  background: chartColors.surface2,
                  border: `1px solid ${chartColors.borderHairline}`,
                  borderRadius: 8,
                  fontSize: 13,
                }}
                labelStyle={{ color: chartColors.textMuted }}
                labelFormatter={(dia) => `Día ${dia}`}
                formatter={(valor) => [formatearValor(Number(valor)), titulo]}
              />
              <Area
                type="monotone"
                dataKey="valor"
                stroke={color}
                strokeWidth={2}
                fill={`url(#${gradientId})`}
                dot={false}
                activeDot={{ r: 4, fill: color, stroke: chartColors.surface1, strokeWidth: 2 }}
              />
            </AreaChart>
          </ResponsiveContainer>
        </div>
      )}
    </div>
  )
}
