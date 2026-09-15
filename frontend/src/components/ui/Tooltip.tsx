import { HelpCircle } from 'lucide-react'
import { useId, useState, type ReactNode } from 'react'
import './Tooltip.css'

interface TooltipProps {
  texto: string
  /** Elemento disparador. Si se omite, se usa un ícono "?" — el caso de uso típico:
      ayuda para un campo técnico junto a su label. */
  children?: ReactNode
}

/**
 * Tooltip simple: aparece al pasar el mouse o al enfocar con teclado (accesible sin
 * mouse), desaparece al salir/desenfocar. Pensado para textos de ayuda cortos en
 * campos técnicos, no para contenido interactivo.
 */
export function Tooltip({ texto, children }: TooltipProps) {
  const [visible, setVisible] = useState(false)
  const idTooltip = useId()

  return (
    <span
      className="ui-tooltip"
      onMouseEnter={() => setVisible(true)}
      onMouseLeave={() => setVisible(false)}
      onFocus={() => setVisible(true)}
      onBlur={() => setVisible(false)}
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
