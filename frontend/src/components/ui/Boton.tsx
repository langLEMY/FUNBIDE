import { Loader2 } from 'lucide-react'
import type { ButtonHTMLAttributes, ReactNode } from 'react'
import './Boton.css'

type VarianteBoton = 'primario' | 'secundario' | 'destructivo' | 'texto'

interface BotonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variante?: VarianteBoton
  /** Deshabilita el botón y muestra un spinner en vez del ícono/label normal —
      pensado para acciones donde un doble-click puede duplicar algo (cobros,
      caja): mientras `cargando` es true, es literalmente imposible re-disparar
      el click. */
  cargando?: boolean
  children: ReactNode
}

export function Boton({ variante = 'primario', cargando = false, disabled, children, className, ...resto }: BotonProps) {
  const clases = ['ui-boton', `ui-boton-${variante}`, className].filter(Boolean).join(' ')

  return (
    <button className={clases} disabled={disabled || cargando} aria-busy={cargando} {...resto}>
      {cargando && <Loader2 className="ui-boton-spinner" size={16} aria-hidden="true" />}
      {children}
    </button>
  )
}
