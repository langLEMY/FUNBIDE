import { ThemeToggleButton } from './ThemeToggleButton'
import { ProfileMenu } from './ProfileMenu'
import './Topbar.css'

interface TopbarProps {
  titulo: string
}

export function Topbar({ titulo }: TopbarProps) {
  return (
    <header className="topbar">
      <div className="topbar-titulo-grupo">
        {/* Mismo lenguaje visual "ventana" que .login-ventana-barra en LoginPage
            (puntos estilo macOS): FUNBIDE se abre como app de escritorio (ver
            launcher/), esto extiende esa identidad a la ventana principal, no
            solo a la pantalla de login. */}
        <span className="topbar-ventana-puntos" aria-hidden="true">
          <span className="topbar-punto topbar-punto-rojo" />
          <span className="topbar-punto topbar-punto-amarillo" />
          <span className="topbar-punto topbar-punto-verde" />
        </span>
        <h1 className="topbar-titulo">{titulo}</h1>
      </div>
      <div className="topbar-acciones">
        <ThemeToggleButton className="topbar-tema-boton" />
        <ProfileMenu />
      </div>
    </header>
  )
}
