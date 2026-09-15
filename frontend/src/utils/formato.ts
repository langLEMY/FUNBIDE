// Helpers centralizados de formato — antes cada página armaba su propio
// Intl.NumberFormat/DateTimeFormat (con pequeñas variaciones entre sí, p. ej.
// CobrosPage.tsx usaba 'es-DO'/'DOP'). Centralizarlos acá evita que una
// página se desalinee del resto en cómo se ve un monto o una fecha.

const formateadorMonedaRD = new Intl.NumberFormat('es-DO', {
  style: 'currency',
  currency: 'DOP',
  maximumFractionDigits: 2,
})

const formateadorFechaRD = new Intl.DateTimeFormat('es-DO', { dateStyle: 'short' })
const formateadorFechaHoraRD = new Intl.DateTimeFormat('es-DO', { dateStyle: 'short', timeStyle: 'short' })

/** RD$ con separador de miles, p. ej. formatearMoneda(1234.5) -> "RD$1,234.50" */
export function formatearMoneda(monto: number): string {
  return formateadorMonedaRD.format(monto)
}

/** dd/mm/aaaa. Acepta Date, string ISO o timestamp. */
export function formatearFecha(fecha: Date | string | number): string {
  return formateadorFechaRD.format(new Date(fecha))
}

/** dd/mm/aaaa hh:mm. Acepta Date, string ISO o timestamp. */
export function formatearFechaHora(fecha: Date | string | number): string {
  return formateadorFechaHoraRD.format(new Date(fecha))
}
