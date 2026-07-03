import './StatCard.css'

interface StatCardProps {
  etiqueta: string
  valor: string
  colorSerie: string
}

export function StatCard({ etiqueta, valor, colorSerie }: StatCardProps) {
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
