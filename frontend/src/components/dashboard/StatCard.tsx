import type { ReactNode } from 'react'
import { IconoNav, type NombreIconoNav } from '../layout/IconoNav'
import './StatCard.css'

interface StatCardProps {
  etiqueta: string
  valor: string
  colorSerie: string
  icono?: NombreIconoNav
  sparkline?: ReactNode
}

export function StatCard({ etiqueta, valor, colorSerie, icono, sparkline }: StatCardProps) {
  if (!icono) {
    return (
      <div className="stat-card">
        <span className="stat-card-marca" style={{ background: colorSerie }} />
        <div>
          <p className="stat-card-etiqueta text-secondary">{etiqueta}</p>
          <p className="stat-card-valor">{valor}</p>
        </div>
      </div>
    )
  }

  return (
    <div className="stat-card stat-card-con-icono">
      <div className="stat-card-encabezado">
        <span className="stat-card-icono" style={{ background: `${colorSerie}22`, color: colorSerie }}>
          <IconoNav nombre={icono} />
        </span>
        <p className="stat-card-etiqueta text-secondary">{etiqueta}</p>
      </div>
      <p className="stat-card-valor">{valor}</p>
      {sparkline && <div className="stat-card-sparkline">{sparkline}</div>}
    </div>
  )
}
