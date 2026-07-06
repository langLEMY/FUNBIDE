import { useTheme } from '../../theme/ThemeContext'
import { ProfileMenu } from './ProfileMenu'
import './Topbar.css'

interface TopbarProps {
  titulo: string
}

export function Topbar({ titulo }: TopbarProps) {
  const { tema, alternarTema } = useTheme()

  return (
    <header className="topbar">
      <h1 className="topbar-titulo">{titulo}</h1>
      <div className="topbar-acciones">
        <button
          type="button"
          className="topbar-tema-boton"
          onClick={alternarTema}
          aria-label={tema === 'oscuro' ? 'Cambiar a tema claro' : 'Cambiar a tema oscuro'}
          aria-pressed={tema === 'claro'}
        >
          {tema === 'oscuro' ? (
            <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
              <path d="M12 3a9 9 0 1 0 9 9 7 7 0 0 1-9-9z" />
            </svg>
          ) : (
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true">
              <circle cx="12" cy="12" r="4" fill="currentColor" />
              <circle cx="12" cy="12" r="7.5" stroke="currentColor" strokeWidth="1.5" />
            </svg>
          )}
        </button>
        <ProfileMenu />
      </div>
    </header>
  )
}
