// Paleta de 5 colores ya existentes en theme.css (nada nuevo/decorativo): el
// mismo nombre siempre cae en el mismo color, así que un avatar sin foto se
// puede reconocer de un vistazo dentro de una lista larga (Personal,
// Permisos) sin depender de colorPorRol.ts, que agrupa a todos los usuarios
// del mismo rol bajo un único color y los vuelve indistinguibles entre sí.
const PALETA = [
  'var(--acento-primario)',
  'var(--series-dinero)',
  'var(--avatar-azul)',
  'var(--acento-alerta)',
  'var(--danger)',
]

export function colorPorNombre(nombre: string): string {
  let hash = 0
  for (let i = 0; i < nombre.length; i++) {
    hash = (hash * 31 + nombre.charCodeAt(i)) >>> 0
  }
  return PALETA[hash % PALETA.length]
}
