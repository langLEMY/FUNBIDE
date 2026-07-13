/**
 * Subconjunto mínimo del cliente de auth de Supabase que AuthContext.tsx y
 * RestablecerContrasenaPage.tsx realmente usan. Permite que supabaseClient.ts exporte
 * el cliente real de Supabase o el shim local (localAuthClient.ts) sin que el resto
 * del código sepa cuál de los dos está detrás.
 */
export interface SesionMinima {
  access_token: string
}

export interface ResultadoAuth {
  error: Error | null
}

export interface ClienteAuth {
  getSession(): Promise<{ data: { session: SesionMinima | null } }>
  onAuthStateChange(
    callback: (event: string, session: SesionMinima | null) => void,
  ): { data: { subscription: { unsubscribe(): void } } }
  signInWithPassword(credenciales: { email: string; password: string }): Promise<ResultadoAuth>
  /**
   * `scope: 'global'` cierra la sesión en todos los dispositivos conectados con esta
   * cuenta (Supabase revoca todos los refresh tokens del lado de GoTrue). El shim
   * local (ver localAuthClient.ts) lo ignora: no hay multi-dispositivo real en una
   * instalación offline/USB de un solo puesto.
   */
  signOut(opciones?: { scope?: 'global' }): Promise<ResultadoAuth>
  resetPasswordForEmail(correo: string, opciones?: { redirectTo?: string }): Promise<ResultadoAuth>
  updateUser(atributos: { password: string }): Promise<ResultadoAuth>
}

export interface ClienteAutenticacion {
  auth: ClienteAuth
}
