import { HelpCircle } from 'lucide-react'
import { useEffect, useId, useRef, useState, type ReactNode } from 'react'
import './Tooltip.css'

const DELAY_APARICION_MS = 150

interface TooltipProps {
  texto: string
  /** Elemento disparador. Si se omite, se usa un ícono "?" — el caso de uso típico:
      ayuda para un campo técnico junto a su label. */
  children?: ReactNode
}

/**
 * Tooltip simple: aparece al pasar el mouse o al enfocar con teclado (accesible sin
 * mouse), con un pequeño delay (150ms) para no sentirse pegajoso al recorrer varios
 * elementos rápido, y desaparece instantáneo al salir/desenfocar. Pensado para
 * textos de ayuda cortos en campos técnicos, no para contenido interactivo.
 */
export function Tooltip({ texto, children }: TooltipProps) {
  const [visible, setVisible] = useState(false)
  const idTooltip = useId()
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  useEffect(() => {
    return () => {
      if (timeoutRef.current) clearTimeout(timeoutRef.current)
    }
  }, [])

  const mostrarConDelay = () => {
    if (timeoutRef.current) clearTimeout(timeoutRef.current)
    timeoutRef.current = setTimeout(() => setVisible(true), DELAY_APARICION_MS)
  }

  const ocultarYa = () => {
    if (timeoutRef.current) clearTimeout(timeoutRef.current)
    setVisible(false)
  }

  return (
    <span
      className="ui-tooltip"
      onMouseEnter={mostrarConDelay}
      onMouseLeave={ocultarYa}
      onFocus={mostrarConDelay}
      onBlur={ocultarYa}
    >
      <button
        type="button"
        className="ui-tooltip-disparador"
        aria-describedby={visible ? idTooltip : undefined}
        tabIndex={0}
      >
        {children ?? <HelpCircle size={14} aria-hidden="true" />}
      </button>
      {visible && (
        <span role="tooltip" id={idTooltip} className="ui-tooltip-globo">
          {texto}
        </span>
      )}
    </span>
  )
}
