import { ProfileMenu } from './ProfileMenu'
import './Topbar.css'

interface TopbarProps {
  titulo: string
}

export function Topbar({ titulo }: TopbarProps) {
  return (
    <header className="topbar">
      <h1 className="topbar-titulo">{titulo}</h1>
      <ProfileMenu />
    </header>
  )
}
