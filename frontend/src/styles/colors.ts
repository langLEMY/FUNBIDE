import type { Tema } from '../theme/ThemeContext'

/**
 * JS-side mirror of the hex values in theme.css, for SVG chart props (Recharts)
 * where relying on CSS var() resolution inside SVG attributes is not worth the risk.
 * Keep these in sync with theme.css by hand — there are only a handful, one set per tema.
 * `surface1`/`surface2` here are intentionally more opaque than the CSS glass surfaces:
 * they back the floating tooltip/cursor highlight, which needs to stay legible over
 * whatever chart content sits behind it.
 */
export const chartColorsPorTema = {
  oscuro: {
    surface1: 'rgba(28, 26, 22, 0.92)',
    surface2: 'rgba(34, 31, 26, 0.6)',
    gridline: 'rgba(243, 242, 238, 0.08)',
    baseline: 'rgba(243, 242, 238, 0.16)',
    textMuted: '#8d9089',
    borderHairline: 'rgba(255, 255, 255, 0.09)',
    dinero: '#e2984f',
    pacientes: '#2fbf8f',
    actividad: '#2fbf8f',
    gasto: '#dd7d70',
  },
  claro: {
    surface1: 'rgba(255, 255, 255, 0.92)',
    surface2: 'rgba(243, 241, 236, 0.6)',
    gridline: 'rgba(32, 34, 31, 0.08)',
    baseline: 'rgba(32, 34, 31, 0.16)',
    textMuted: '#6b6a63',
    borderHairline: 'rgba(0, 0, 0, 0.07)',
    dinero: '#c17830',
    pacientes: '#2fbf8f',
    actividad: '#2fbf8f',
    gasto: '#c4574b',
  },
} as const

export type ChartColors = (typeof chartColorsPorTema)[Tema]

export function coloresParaTema(tema: Tema): ChartColors {
  return chartColorsPorTema[tema]
}

/**
 * Paleta categórica para diferenciar roles en la tarjeta "Personal por área"
 * del dashboard (barras de cobertura, cada una ya rotulada con el nombre del
 * rol — el color es un acento, no el único canal de identidad). Deliberadamente
 * NO reutiliza teal/ámbar/rojo: esos ya son el acento semántico de "pacientes",
 * "dinero" y "alertas" en la misma pantalla, y mezclarlos aquí los confundiría
 * con esas series.
 */
export const coloresRolPorTema = {
  oscuro: {
    Doctor: '#3987e5',
    Fondos: '#9085e9',
    Lemy: '#008300',
  },
  claro: {
    Doctor: '#2a78d6',
    Fondos: '#4a3aa7',
    Lemy: '#008300',
  },
} as const

export function colorRolParaTema(tema: Tema, rol: 'Doctor' | 'Fondos' | 'Lemy'): string {
  return coloresRolPorTema[tema][rol]
}
