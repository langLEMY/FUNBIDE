import { ChevronRight } from 'lucide-react'
import type { MouseEvent } from 'react'
import { Link } from 'react-router-dom'
import './Breadcrumbs.css'

export interface MigaPan {
  etiqueta: string
  /** Sin `to`: es la miga actual (no clickeable), siempre la última. */
  to?: string
  /** Reemplaza la navegación directa de `to` — p. ej. para chequear cambios
      sin guardar antes de salir (ver PacienteHistorialPage). Debe navegar
      por su cuenta si corresponde; recibe el evento para poder cancelarlo. */
  onClick?: (event: MouseEvent) => void
}

interface BreadcrumbsProps {
  migas: MigaPan[]
}

/** Ruta de navegación para vistas anidadas (p. ej. Pacientes → Historial). */
export function Breadcrumbs({ migas }: BreadcrumbsProps) {
  return (
    <nav className="ui-breadcrumbs" aria-label="Ruta de navegación">
      {migas.map((miga, indice) => (
        <span key={indice} className="ui-breadcrumbs-item">
          {indice > 0 && <ChevronRight size={13} className="ui-breadcrumbs-separador" aria-hidden="true" />}
          {miga.to ? (
            <Link
              to={miga.to}
              className="ui-breadcrumbs-link"
              onClick={(event) => {
                if (!miga.onClick) return
                event.preventDefault()
                miga.onClick(event)
              }}
            >
              {miga.etiqueta}
            </Link>
          ) : (
            <span className="ui-breadcrumbs-actual" aria-current="page">
              {miga.etiqueta}
            </span>
          )}
        </span>
      ))}
    </nav>
  )
}
