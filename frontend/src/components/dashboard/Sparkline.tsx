import { useId } from 'react'
import { construirSparklineBarras, construirSparklineLinea } from '../../lib/sparkline'

interface SparklineProps {
  valores: number[]
  color: string
  modo?: 'linea' | 'barras'
  ancho?: number
  alto?: number
}

/** Mini-gráfico de tendencia para las stat cards del dashboard (sin ejes ni tooltip). */
export function Sparkline({ valores, color, modo = 'linea', ancho = 120, alto = 44 }: SparklineProps) {
  const idGradiente = `sparkline-gradiente-${useId()}`

  if (modo === 'barras') {
    const { barras } = construirSparklineBarras(valores, ancho, alto)
    return (
      <svg width="100%" height={alto} viewBox={`0 0 ${ancho} ${alto}`} preserveAspectRatio="none" aria-hidden="true">
        {barras.map((barra, i) => (
          <rect
            key={i}
            x={barra.x}
            y={barra.y}
            width={barra.ancho}
            height={barra.alto}
            rx="2"
            fill={color}
            opacity="0.75"
          />
        ))}
      </svg>
    )
  }

  const { path, areaPath } = construirSparklineLinea(valores, ancho, alto)
  return (
    <svg width="100%" height={alto} viewBox={`0 0 ${ancho} ${alto}`} preserveAspectRatio="none" aria-hidden="true">
      <defs>
        <linearGradient id={idGradiente} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor={color} stopOpacity="0.3" />
          <stop offset="100%" stopColor={color} stopOpacity="0" />
        </linearGradient>
      </defs>
      <path d={areaPath} fill={`url(#${idGradiente})`} stroke="none" />
      <path d={path} fill="none" stroke={color} strokeWidth="2" />
    </svg>
  )
}
