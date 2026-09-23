import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr'
import { useEffect, useRef } from 'react'
import { supabase } from './supabaseClient'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL as string

export interface EventoCaja {
  tipo: 'cobro' | 'movimiento' | 'turno-cerrado'
  descripcion: string
  monto: number
  esIngreso: boolean
  registradoEn: string
}

/**
 * Conecta al hub de caja en tiempo real (ver CajaHub en el backend) y llama a `onEvento`
 * cada vez que se registra un cobro/movimiento o se cierra un turno — reemplaza el sondeo
 * de 20s de CajaPage por un empujón instantáneo. Solo los admins conectados reciben algo
 * (el hub los filtra del lado del servidor); para cualquier otro rol la conexión igual se
 * abre pero simplemente nunca llega un evento. Reconecta sola con backoff si se corta.
 */
export function useCajaTiempoReal(onEvento: (evento: EventoCaja) => void): void {
  const onEventoRef = useRef(onEvento)
  onEventoRef.current = onEvento

  useEffect(() => {
    if (!apiBaseUrl) return

    let cancelado = false
    let conexion: HubConnection | null = null

    const conectar = async () => {
      const nuevaConexion = new HubConnectionBuilder()
        .withUrl(`${apiBaseUrl}/hubs/caja`, {
          accessTokenFactory: async () => {
            const { data } = await supabase.auth.getSession()
            return data.session?.access_token ?? ''
          },
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Warning)
        .build()

      nuevaConexion.on('evento-caja', (evento: EventoCaja) => onEventoRef.current(evento))

      try {
        await nuevaConexion.start()
      } catch {
        // Sin conexión en tiempo real, CajaPage sigue funcionando con su sondeo de 20s
        // de respaldo -- no hace falta reintentar a mano acá.
        return
      }

      if (cancelado) {
        await nuevaConexion.stop()
        return
      }
      conexion = nuevaConexion
    }

    void conectar()

    return () => {
      cancelado = true
      void conexion?.stop()
    }
  }, [])
}
