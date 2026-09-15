import { CalendarDays, ChevronLeft, ChevronRight } from 'lucide-react'
import { useEffect, useMemo, useRef, useState } from 'react'
import './SelectorRangoFechas.css'

interface SelectorRangoFechasProps {
  /** Fechas en formato ISO corto (yyyy-mm-dd), igual que un <input type="date"> nativo —
      así el componente es un reemplazo directo sin tocar el resto del estado de la página. */
  desde: string
  hasta: string
  onCambiar: (desde: string, hasta: string) => void
  /** Fecha máxima seleccionable (yyyy-mm-dd), p. ej. "hoy" en Finanzas/Auditoría. */
  max?: string
}

const DIAS_SEMANA = ['L', 'M', 'M', 'J', 'V', 'S', 'D']
const formateadorMes = new Intl.DateTimeFormat('es-DO', { month: 'long', year: 'numeric' })
const formateadorFechaCorta = new Intl.DateTimeFormat('es-DO', { dateStyle: 'short' })

function aFechaISO(fecha: Date): string {
  const y = fecha.getFullYear()
  const m = String(fecha.getMonth() + 1).padStart(2, '0')
  const d = String(fecha.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

function desdeFechaISO(iso: string): Date {
  const [y, m, d] = iso.split('-').map(Number)
  return new Date(y, m - 1, d)
}

/** Grilla de 6 semanas (42 días) empezando en lunes, incluyendo días del mes
    anterior/siguiente para completar la primera y última semana. */
function diasDeLaGrilla(anio: number, mes: number): Date[] {
  const primerDia = new Date(anio, mes, 1)
  // getDay(): 0=domingo..6=sábado. Se convierte a offset desde lunes (0=lunes..6=domingo).
  const offsetLunes = (primerDia.getDay() + 6) % 7
  const inicioGrilla = new Date(anio, mes, 1 - offsetLunes)
  return Array.from({ length: 42 }, (_, i) => new Date(inicioGrilla.getFullYear(), inicioGrilla.getMonth(), inicioGrilla.getDate() + i))
}

/**
 * Selector de rango de fechas con calendario propio: reemplaza el par de
 * <input type="date"> nativos (con su UI inconsistente entre navegadores) por
 * un único control — un botón que abre un popover con dos clicks para elegir
 * el rango (primer click = "desde", segundo click = "hasta").
 */
export function SelectorRangoFechas({ desde, hasta, onCambiar, max }: SelectorRangoFechasProps) {
  const [abierto, setAbierto] = useState(false)
  const [mesVisible, setMesVisible] = useState(() => {
    const d = desdeFechaISO(desde)
    return new Date(d.getFullYear(), d.getMonth(), 1)
  })
  const [seleccionandoHasta, setSeleccionandoHasta] = useState(false)
  const contenedorRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!abierto) return

    const handleClickAfuera = (event: MouseEvent) => {
      if (contenedorRef.current && !contenedorRef.current.contains(event.target as Node)) {
        setAbierto(false)
      }
    }
    const handleEsc = (event: globalThis.KeyboardEvent) => {
      if (event.key === 'Escape') setAbierto(false)
    }

    document.addEventListener('mousedown', handleClickAfuera)
    document.addEventListener('keydown', handleEsc)
    return () => {
      document.removeEventListener('mousedown', handleClickAfuera)
      document.removeEventListener('keydown', handleEsc)
    }
  }, [abierto])

  const dias = useMemo(() => diasDeLaGrilla(mesVisible.getFullYear(), mesVisible.getMonth()), [mesVisible])
  const fechaDesde = desdeFechaISO(desde)
  const fechaHasta = desdeFechaISO(hasta)
  const fechaMax = max ? desdeFechaISO(max) : null

  const handleClickDia = (dia: Date) => {
    if (fechaMax && dia > fechaMax) return

    if (!seleccionandoHasta) {
      const iso = aFechaISO(dia)
      onCambiar(iso, iso)
      setSeleccionandoHasta(true)
      return
    }

    if (dia < fechaDesde) {
      onCambiar(aFechaISO(dia), aFechaISO(fechaDesde))
    } else {
      onCambiar(aFechaISO(fechaDesde), aFechaISO(dia))
    }
    setSeleccionandoHasta(false)
    setAbierto(false)
  }

  return (
    <div className="ui-rango-fechas" ref={contenedorRef}>
      <button type="button" className="ui-rango-fechas-boton" onClick={() => setAbierto((v) => !v)}>
        <CalendarDays size={15} aria-hidden="true" />
        {formateadorFechaCorta.format(fechaDesde)} – {formateadorFechaCorta.format(fechaHasta)}
      </button>

      {abierto && (
        <div className="ui-rango-fechas-popover" role="dialog" aria-label="Elegir rango de fechas">
          <div className="ui-rango-fechas-cabecera">
            <button
              type="button"
              className="ui-rango-fechas-nav"
              onClick={() => setMesVisible((m) => new Date(m.getFullYear(), m.getMonth() - 1, 1))}
              aria-label="Mes anterior"
            >
              <ChevronLeft size={16} aria-hidden="true" />
            </button>
            <span className="ui-rango-fechas-mes">{formateadorMes.format(mesVisible)}</span>
            <button
              type="button"
              className="ui-rango-fechas-nav"
              onClick={() => setMesVisible((m) => new Date(m.getFullYear(), m.getMonth() + 1, 1))}
              aria-label="Mes siguiente"
            >
              <ChevronRight size={16} aria-hidden="true" />
            </button>
          </div>

          <div className="ui-rango-fechas-grilla">
            {DIAS_SEMANA.map((letra, indice) => (
              <span key={indice} className="ui-rango-fechas-diasemana">
                {letra}
              </span>
            ))}
            {dias.map((dia) => {
              const iso = aFechaISO(dia)
              const fueraDeMes = dia.getMonth() !== mesVisible.getMonth()
              const enRango = dia >= fechaDesde && dia <= fechaHasta
              const esExtremo = iso === desde || iso === hasta
              const deshabilitado = Boolean(fechaMax && dia > fechaMax)

              return (
                <button
                  key={iso}
                  type="button"
                  className={[
                    'ui-rango-fechas-dia',
                    fueraDeMes && 'ui-rango-fechas-dia-fuera',
                    enRango && 'ui-rango-fechas-dia-en-rango',
                    esExtremo && 'ui-rango-fechas-dia-extremo',
                  ]
                    .filter(Boolean)
                    .join(' ')}
                  disabled={deshabilitado}
                  onClick={() => handleClickDia(dia)}
                >
                  {dia.getDate()}
                </button>
              )
            })}
          </div>
        </div>
      )}
    </div>
  )
}
