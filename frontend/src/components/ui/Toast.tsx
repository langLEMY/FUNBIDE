import { AlertTriangle, CheckCircle2, Info, X } from 'lucide-react'
import { useRef, useState, type PointerEvent } from 'react'
import type { ToastItem } from './ToastContext'
import './Toast.css'

const ICONO_POR_TIPO = {
  info: Info,
  exito: CheckCircle2,
  error: AlertTriangle,
} as const

const UMBRAL_DESCARTE_PX = 80

interface ToastStackProps {
  toasts: ToastItem[]
  onCerrar: (id: number) => void
}

/** Apila los toasts activos, más nuevo abajo — cada uno es su propio <Toast>
    con su propio gesto de swipe, así que descartar uno no afecta a los demás. */
export function ToastStack({ toasts, onCerrar }: ToastStackProps) {
  if (toasts.length === 0) return null

  return (
    <div className="ui-toast-stack" role="status" aria-live="polite">
      {toasts.map((toast) => (
        <Toast key={toast.id} toast={toast} onCerrar={() => onCerrar(toast.id)} />
      ))}
    </div>
  )
}

function Toast({ toast, onCerrar }: { toast: ToastItem; onCerrar: () => void }) {
  const [offsetX, setOffsetX] = useState(0)
  const [arrastrando, setArrastrando] = useState(false)
  const inicioRef = useRef(0)
  const Icono = ICONO_POR_TIPO[toast.tipo]

  const handlePointerDown = (event: PointerEvent<HTMLDivElement>) => {
    inicioRef.current = event.clientX
    setArrastrando(true)
    event.currentTarget.setPointerCapture(event.pointerId)
  }

  const handlePointerMove = (event: PointerEvent<HTMLDivElement>) => {
    if (!arrastrando) return
    setOffsetX(event.clientX - inicioRef.current)
  }

  const handlePointerUp = () => {
    setArrastrando(false)
    if (Math.abs(offsetX) > UMBRAL_DESCARTE_PX) {
      onCerrar()
      return
    }
    setOffsetX(0)
  }

  return (
    <div
      className={`ui-toast ui-toast-${toast.tipo}`}
      style={{ transform: `translateX(${offsetX}px)`, opacity: 1 - Math.min(Math.abs(offsetX) / 200, 0.7) }}
      onPointerDown={handlePointerDown}
      onPointerMove={handlePointerMove}
      onPointerUp={handlePointerUp}
      onPointerCancel={handlePointerUp}
    >
      <Icono size={17} className="ui-toast-icono" aria-hidden="true" />
      <p className="ui-toast-mensaje">{toast.mensaje}</p>
      <button type="button" className="ui-toast-cerrar" onClick={onCerrar} aria-label="Cerrar aviso">
        <X size={14} aria-hidden="true" />
      </button>
    </div>
  )
}
