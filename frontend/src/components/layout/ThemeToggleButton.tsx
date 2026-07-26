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

  return (
    <button
      type="button"
      className={`tema-toggle ${className}`}
      onClick={alternarTema}
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
