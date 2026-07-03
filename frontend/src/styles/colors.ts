/**
 * JS-side mirror of the hex values in theme.css, for SVG chart props (Recharts)
 * where relying on CSS var() resolution inside SVG attributes is not worth the risk.
 * Keep these in sync with theme.css by hand — there are only a handful.
 */
export const chartColors = {
  surface1: '#1a1a19',
  surface2: '#202020',
  gridline: '#2c2c2a',
  baseline: '#383835',
  textMuted: '#898781',
  borderHairline: 'rgba(255, 255, 255, 0.1)',
  dinero: '#3987e5',
  pacientes: '#199e70',
  actividad: '#9085e9',
} as const
