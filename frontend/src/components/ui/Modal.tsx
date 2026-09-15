import { useEffect, useRef, type KeyboardEvent, type ReactNode } from 'react'

interface ModalProps {
  /** Controla si el modal está montado. El propio componente maneja el cierre
      con Esc y click-afuera; el padre solo decide si se muestra o no. */
  abierto: boolean
  onCerrar: () => void
  titulo?: ReactNode
  subtitulo?: ReactNode
  children: ReactNode
  /** Botones de la barra inferior (p. ej. Cancelar/Guardar). Opcional: hay
      modales (visores, confirmaciones a medida) que arman sus propias acciones
      dentro de `children`. */
  acciones?: ReactNode
  ancho?: number
  /** id del elemento con el título, para aria-labelledby — solo hace falta
      pasarlo si no se usa la prop `titulo`. */
  tituloId?: string
}

/**
 * Shell visual genérico para diálogos modales: foco atrapado (Tab no se
 * escapa del modal), cierre con Esc, cierre al hacer click en el overlay, y
 * la misma animación de entrada/salida que ya tenían EditarPermisosModal y
 * EditarPersonalModal antes de migrar a este componente. La lógica de cada
 * formulario/vista sigue siendo responsabilidad del contenido (children).
 */
export function Modal({ abierto, onCerrar, titulo, subtitulo, children, acciones, ancho = 480, tituloId }: ModalProps) {
  const modalRef = useRef<HTMLDivElement>(null)
  const idTituloGenerado = useRef(`modal-titulo-${Math.random().toString(36).slice(2)}`).current
  const idTitulo = tituloId ?? (titulo ? idTituloGenerado : undefined)

  useEffect(() => {
    if (!abierto) return

    // Foco inicial dentro del modal, para que Tab arranque atrapado ahí y no
    // se quede en el elemento que abrió el modal (que queda detrás del overlay).
    const previoActivo = document.activeElement as HTMLElement | null
    modalRef.current?.focus()

    return () => {
      previoActivo?.focus?.()
    }
  }, [abierto])

  if (!abierto) {
    return null
  }

  const handleTeclado = (event: KeyboardEvent<HTMLDivElement>) => {
    if (event.key === 'Escape') {
      event.stopPropagation()
      onCerrar()
      return
    }

    if (event.key !== 'Tab' || !modalRef.current) {
      return
    }

    const focosDisponibles = modalRef.current.querySelectorAll<HTMLElement>(
      'a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]), select:not([disabled]), [tabindex]:not([tabindex="-1"])',
    )
    if (focosDisponibles.length === 0) {
      return
    }

    const primero = focosDisponibles[0]
    const ultimo = focosDisponibles[focosDisponibles.length - 1]

    if (event.shiftKey && document.activeElement === primero) {
      event.preventDefault()
      ultimo.focus()
    } else if (!event.shiftKey && document.activeElement === ultimo) {
      event.preventDefault()
      primero.focus()
    }
  }

  return (
    <div className="ui-modal-overlay" onClick={onCerrar}>
      <div
        ref={modalRef}
        className="ui-modal"
        style={{ width: ancho }}
        role="dialog"
        aria-modal="true"
        aria-labelledby={idTitulo}
        tabIndex={-1}
        onClick={(event) => event.stopPropagation()}
        onKeyDown={handleTeclado}
      >
        {titulo && (
          <h2 id={idTitulo} className="ui-modal-titulo">
            {titulo}
          </h2>
        )}
        {subtitulo && <p className="ui-modal-subtitulo">{subtitulo}</p>}

        <div className="ui-modal-contenido">{children}</div>

        {acciones && <div className="ui-modal-acciones">{acciones}</div>}
      </div>
    </div>
  )
}
