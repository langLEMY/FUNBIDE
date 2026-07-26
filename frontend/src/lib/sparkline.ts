/**
 * Generador de sparklines livianas (SVG path / barras) para las stat cards del
 * dashboard. Sin dependencias de charting — son demasiado pequeñas y simples
 * para justificar Recharts (que sí se usa para los gráficos grandes de "Vista
 * del mes").
 */

const PADDING = 3

export interface SparklineLinea {
  tipo: 'linea'
  ancho: number
  alto: number
  path: string
  areaPath: string
}

export interface BarraSparkline {
  x: number
  y: number
  ancho: number
  alto: number
}

export interface SparklineBarras {
  tipo: 'barras'
  ancho: number
  alto: number
  barras: BarraSparkline[]
}

export function construirSparklineLinea(valores: number[], ancho = 120, alto = 44): SparklineLinea {
  const w = ancho - PADDING * 2
  const h = alto - PADDING * 2
  const max = Math.max(...valores, 1)
  const min = Math.min(...valores, 0)
  const rango = max - min || 1
  const n = valores.length
  const pasoX = n > 1 ? w / (n - 1) : 0

  const puntos = valores.map((valor, i) => ({
    x: +(PADDING + pasoX * i).toFixed(1),
    y: +(PADDING + h - ((valor - min) / rango) * h).toFixed(1),
  }))

  const path = puntos.map((p, i) => (i === 0 ? 'M' : 'L') + p.x + ' ' + p.y).join(' ')
  const primero = puntos[0]
  const ultimo = puntos[puntos.length - 1]
  const base = (PADDING + h).toFixed(1)
  const areaPath = `${path} L ${ultimo.x} ${base} L ${primero.x} ${base} Z`

  return { tipo: 'linea', ancho, alto, path, areaPath }
}

export function construirSparklineBarras(valores: number[], ancho = 120, alto = 44): SparklineBarras {
  const w = ancho - PADDING * 2
  const h = alto - PADDING * 2
  const max = Math.max(...valores, 1)
  const min = Math.min(...valores, 0)
  const rango = max - min || 1
  const n = valores.length || 1
  const gap = 3
  const anchoBarra = (w - gap * (n - 1)) / n

  const barras = valores.map((valor, i) => {
    const altoBarra = Math.max(2, ((valor - min) / rango) * h)
    return {
      x: +(PADDING + i * (anchoBarra + gap)).toFixed(1),
      y: +(PADDING + h - altoBarra).toFixed(1),
      ancho: +anchoBarra.toFixed(1),
      alto: +altoBarra.toFixed(1),
    }
  })

  return { tipo: 'barras', ancho, alto, barras }
}
