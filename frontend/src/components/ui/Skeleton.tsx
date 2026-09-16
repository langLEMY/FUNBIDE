import './Skeleton.css'

interface SkeletonProps {
  /** Ancho del placeholder — cualquier valor CSS válido (px, %, ch...). */
  ancho?: string
  alto?: string
  /** 'texto' da un radio chico (línea de texto), 'bloque' usa --radius-control
      (tarjeta/campo/avatar cuadrado). */
  variante?: 'texto' | 'bloque'
  className?: string
}

/**
 * Placeholder con shimmer para reemplazar estados de carga en blanco (un
 * "Cargando…" sin nada visual se lee como colgado). Usa los tokens de
 * superficie del tema, así que ya respeta modo claro/oscuro y
 * prefers-reduced-motion (el shimmer es una animation, cae bajo la regla
 * global de theme.css) sin nada adicional acá.
 */
export function Skeleton({ ancho = '100%', alto = '14px', variante = 'texto', className }: SkeletonProps) {
  const clases = ['ui-skeleton', `ui-skeleton-${variante}`, className].filter(Boolean).join(' ')
  return <div className={clases} style={{ width: ancho, height: alto }} aria-hidden="true" />
}
