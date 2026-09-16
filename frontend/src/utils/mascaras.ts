// Máscaras de formato en vivo (mientras se escribe, no solo al validar al enviar).
// Cada formateador recibe el valor crudo del input y devuelve el valor ya formateado
// para reasignar a e.target.value antes de que react-hook-form lea el evento — ver
// components/ui/CampoTexto.tsx, que es quien las aplica.

/** Cédula dominicana: 000-0000000-0 (11 dígitos). */
export function formatearCedulaEnVivo(valor: string): string {
  const digitos = valor.replace(/\D/g, '').slice(0, 11)
  const p1 = digitos.slice(0, 3)
  const p2 = digitos.slice(3, 10)
  const p3 = digitos.slice(10, 11)
  return [p1, p2, p3].filter(Boolean).join('-')
}

/** Teléfono dominicano: 000-000-0000 (10 dígitos, con o sin código de área). */
export function formatearTelefonoEnVivo(valor: string): string {
  const digitos = valor.replace(/\D/g, '').slice(0, 10)
  const p1 = digitos.slice(0, 3)
  const p2 = digitos.slice(3, 6)
  const p3 = digitos.slice(6, 10)
  return [p1, p2, p3].filter(Boolean).join('-')
}

/** Monto en RD$ con separador de miles mientras se escribe, p. ej. "150000" -> "150,000". */
export function formatearMontoEnVivo(valor: string): string {
  const limpio = valor.replace(/[^\d.]/g, '')
  const [enteroCrudo, ...resto] = limpio.split('.')
  const entero = enteroCrudo.replace(/^0+(?=\d)/, '').replace(/\B(?=(\d{3})+(?!\d))/g, ',')
  const decimales = resto.join('').slice(0, 2)
  return resto.length > 0 ? `${entero || '0'}.${decimales}` : entero
}

/** Inverso de formatearMontoEnVivo: "150,000.50" -> 150000.5, para mandar al backend. */
export function desformatearMonto(valor: string): number {
  const numero = Number(valor.replace(/,/g, ''))
  return Number.isFinite(numero) ? numero : 0
}
