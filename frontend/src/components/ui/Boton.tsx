import { Loader2 } from 'lucide-react'
import { useEffect, useRef, useState, type ButtonHTMLAttributes, type MouseEvent, type ReactNode } from 'react'
import './Boton.css'

type VarianteBoton = 'primario' | 'secundario' | 'destructivo' | 'texto'

const DURACION_CONFIRMACION_MS = 2000

interface BotonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variante?: VarianteBoton
  /** Deshabilita el botón y muestra un spinner en vez del ícono/label normal —
      pensado para acciones donde un doble-click puede duplicar algo (cobros,
      caja): mientras `cargando` es true, es literalmente imposible re-disparar
      el click. */
  cargando?: boolean
  /** Para acciones destructivas de bajo riesgo donde abrir un <Modal> completo
      es demasiado: el primer click cambia el propio botón a "¿Seguro?" por 2s
      (se revierte solo si no se confirma); recién el segundo click dispara
      onClick. No usar para acciones irreversibles de alto riesgo — esas siguen
      mereciendo un <Modal> de confirmación real. */
  confirmarAntes?: boolean
  children: ReactNode
}

export function Boton({
  variante = 'primario',
  cargando = false,
  disabled,
  confirmarAntes = false,
  children,
  className,
  onClick,
  ...resto
}: BotonProps) {
  const [confirmando, setConfirmando] = useState(false)
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  useEffect(() => {
    return () => {
      if (timeoutRef.current) clearTimeout(timeoutRef.current)
    }
  }, [])

  const handleClick = (event: MouseEvent<HTMLButtonElement>) => {
    if (!confirmarAntes || confirmando) {
      setConfirmando(false)
      onClick?.(event)
      return
    }

    event.preventDefault()
    setConfirmando(true)
    timeoutRef.current = setTimeout(() => setConfirmando(false), DURACION_CONFIRMACION_MS)
  }

  const clases = [
    'ui-boton',
    `ui-boton-${variante}`,
    confirmando && 'ui-boton-confirmando',
    className,
  ]
    .filter(Boolean)
    .join(' ')

  return (
    <button className={clases} disabled={disabled || cargando} aria-busy={cargando} onClick={handleClick} {...resto}>
      {cargando && <Loader2 className="ui-boton-spinner" size={16} aria-hidden="true" />}
      {confirmando ? '¿Seguro?' : children}
    </button>
  )
}
