/**
 * Notifica a App.tsx que el backend respondió 503 "Sistema en mantenimiento" (ver
 * MantenimientoMiddleware) para que muestre la pantalla de bloqueo de pantalla completa
 * en vez de dejar que cada página maneje el error por separado. Mismo patrón pub-sub que
 * los `escuchadores` de localAuthClient.ts.
 */
type Escuchador = (mensaje: string | null | undefined) => void

const escuchadores = new Set<Escuchador>()

export function avisarMantenimientoActivo(mensaje: string | null) {
  escuchadores.forEach((callback) => callback(mensaje))
}

/** Saca la pantalla de bloqueo (ej. al volver a /login tras cerrar sesión manualmente). */
export function limpiarMantenimiento() {
  escuchadores.forEach((callback) => callback(undefined))
}

export function suscribirseAMantenimiento(callback: Escuchador): () => void {
  escuchadores.add(callback)
  return () => escuchadores.delete(callback)
}
