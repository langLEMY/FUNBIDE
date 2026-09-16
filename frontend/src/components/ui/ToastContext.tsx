import { createContext, useCallback, useContext, useMemo, useRef, useState, type ReactNode } from 'react'
import { ToastStack } from './Toast'

export type TipoToast = 'info' | 'exito' | 'error'

export interface ToastItem {
  id: number
  mensaje: string
  tipo: TipoToast
}

interface ToastContextValue {
  mostrarToast: (mensaje: string, tipo?: TipoToast) => void
}

const ToastContext = createContext<ToastContextValue | null>(null)

const DURACION_AUTO_CIERRE_MS = 4000

/**
 * Proveedor del sistema de toasts de toda la app — se monta una sola vez en
 * main.tsx (junto a ThemeProvider/AuthProvider), cualquier página los dispara
 * con useToast().mostrarToast(...). El apilamiento visual y el descarte por
 * swipe viven en <ToastStack>/<Toast> (Toast.tsx), acá solo se maneja la cola.
 */
export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([])
  const idRef = useRef(0)

  const cerrarToast = useCallback((id: number) => {
    setToasts((actual) => actual.filter((t) => t.id !== id))
  }, [])

  const mostrarToast = useCallback(
    (mensaje: string, tipo: TipoToast = 'info') => {
      const id = ++idRef.current
      setToasts((actual) => [...actual, { id, mensaje, tipo }])
      setTimeout(() => cerrarToast(id), DURACION_AUTO_CIERRE_MS)
    },
    [cerrarToast],
  )

  const value = useMemo(() => ({ mostrarToast }), [mostrarToast])

  return (
    <ToastContext.Provider value={value}>
      {children}
      <ToastStack toasts={toasts} onCerrar={cerrarToast} />
    </ToastContext.Provider>
  )
}

export function useToast(): ToastContextValue {
  const contexto = useContext(ToastContext)
  if (!contexto) {
    throw new Error('useToast debe usarse dentro de <ToastProvider>')
  }
  return contexto
}
