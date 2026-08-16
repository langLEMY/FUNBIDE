/**
 * Supabase Auth devuelve sus mensajes de error en inglés y sin distinguir muchos
 * casos (credenciales inválidas vs. cuenta desactivada). Esto traduce los patrones
 * más comunes a español; cualquier mensaje no reconocido cae al fallback dado por
 * quien llama, en vez de mostrar el texto crudo de Supabase.
 */
export function traducirErrorAuth(mensaje: string | undefined, fallback: string): string {
  if (!mensaje) return fallback

  const normalizado = mensaje.toLowerCase()

  if (normalizado.includes('invalid login credentials')) {
    return 'Usuario o contraseña incorrectos.'
  }
  if (normalizado.includes('banned') || normalizado.includes('disabled')) {
    return 'Esta cuenta fue desactivada. Contacta a un administrador.'
  }
  if (normalizado.includes('email not confirmed')) {
    return 'Tu correo todavía no ha sido confirmado. Contacta a un administrador.'
  }
  if (normalizado.includes('rate limit') || normalizado.includes('too many requests')) {
    return 'Demasiados intentos. Espera unos minutos e inténtalo de nuevo.'
  }
  if (normalizado.includes('password should be at least')) {
    return 'La contraseña debe tener al menos 8 caracteres.'
  }
  if (normalizado.includes('user not found')) {
    return 'No existe una cuenta con ese usuario.'
  }
  if (normalizado.includes('network')) {
    return 'No se pudo conectar. Revisa tu conexión a internet.'
  }

  return fallback
}
