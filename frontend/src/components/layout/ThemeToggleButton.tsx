import type { MouseEvent } from 'react'
import { useTheme } from '../../theme/ThemeContext'
import './ThemeToggleButton.css'

interface ThemeToggleButtonProps {
  className: string
}

/**
 * Botón de sol/luna reutilizado en Topbar y LoginPage. Los dos íconos están
 * siempre montados y apilados (position: absolute) para poder hacer un
 * crossfade con transición CSS al alternar — antes cada lugar montaba/
 * desmontaba un <svg> distinto según el tema, así que el ícono saliente
 * desaparecía de golpe en vez de desvanecerse.
 */
export function ThemeToggleButton({ className }: ThemeToggleButtonProps) {
  const { tema, alternarTema } = useTheme()

  const handleClick = (event: MouseEvent<HTMLButtonElement>) => {
    const doc = document as Document & { startViewTransition?: (callback: () => void) => unknown }
    const prefiereMenosMovimiento = window.matchMedia('(prefers-reduced-motion: reduce)').matches

    // Barrido circular desde el punto exacto del click (View Transitions API,
    // soportada en Chromium/WebView2) en vez de que todas las variables de
    // color salten de golpe. Sin soporte (o con prefers-reduced-motion), cae
    // al cambio instantáneo de siempre -- sigue siendo "suave" porque cada
    // propiedad individual ya transiciona por la regla global de theme.css.
    if (!doc.startViewTransition || prefiereMenosMovimiento) {
      alternarTema()
      return
    }

    const x = event.clientX
    const y = event.clientY
    const radio = Math.hypot(Math.max(x, window.innerWidth - x), Math.max(y, window.innerHeight - y))
    document.documentElement.style.setProperty('--tema-barrido-x', `${x}px`)
    document.documentElement.style.setProperty('--tema-barrido-y', `${y}px`)
    document.documentElement.style.setProperty('--tema-barrido-radio', `${radio}px`)

    doc.startViewTransition(() => {
      alternarTema()
    })
  }

  return (
    <button
      type="button"
      className={`tema-toggle ${className}`}
      onClick={handleClick}
      aria-label={tema === 'oscuro' ? 'Cambiar a tema claro' : 'Cambiar a tema oscuro'}
      aria-pressed={tema === 'claro'}
    >
      <svg
        className={`tema-toggle-icono${tema === 'oscuro' ? ' visible' : ''}`}
        width="18"
        height="18"
        viewBox="0 0 24 24"
        fill="currentColor"
        aria-hidden="true"
      >
        <path d="M12 3a9 9 0 1 0 9 9 7 7 0 0 1-9-9z" />
      </svg>
      <svg
        className={`tema-toggle-icono${tema === 'claro' ? ' visible' : ''}`}
        width="18"
        height="18"
        viewBox="0 0 24 24"
        fill="none"
        aria-hidden="true"
      >
        <circle cx="12" cy="12" r="4" fill="currentColor" />
        <circle cx="12" cy="12" r="7.5" stroke="currentColor" strokeWidth="1.5" />
      </svg>
    </button>
  )
}
