import { ChevronLeft, ChevronRight } from 'lucide-react'
import './Paginacion.css'

interface PaginacionProps {
  paginaActual: number
  totalPaginas: number
  onCambiarPagina: (pagina: number) => void
}

export function Paginacion({ paginaActual, totalPaginas, onCambiarPagina }: PaginacionProps) {
  if (totalPaginas <= 1) {
    return null
  }

  return (
    <div className="ui-paginacion">
      <button
        type="button"
        className="ui-paginacion-boton"
        onClick={() => onCambiarPagina(paginaActual - 1)}
        disabled={paginaActual <= 1}
        aria-label="Página anterior"
      >
        <ChevronLeft size={16} aria-hidden="true" />
      </button>

      <span className="ui-paginacion-texto">
        Página {paginaActual} de {totalPaginas}
      </span>

      <button
        type="button"
        className="ui-paginacion-boton"
        onClick={() => onCambiarPagina(paginaActual + 1)}
        disabled={paginaActual >= totalPaginas}
        aria-label="Página siguiente"
      >
        <ChevronRight size={16} aria-hidden="true" />
      </button>
    </div>
  )
}
