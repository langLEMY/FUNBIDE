const CLAVE = 'funbide-session-id'

/**
 * Id aleatorio estable por dispositivo (no por pestaña), persistido en localStorage. Lo usa
 * el latido de presencia (ver AuthContext) para que Admin pueda contar cuántos dispositivos
 * distintos están usando la app ahora mismo — no es el JWT, así que sobrevive a que el
 * token se renueve o expire.
 */
export function obtenerSessionId(): string {
  let id = localStorage.getItem(CLAVE)
  if (!id) {
    id = crypto.randomUUID()
    localStorage.setItem(CLAVE, id)
  }
  return id
}
