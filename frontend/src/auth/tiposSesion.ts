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
  signOut(): Promise<ResultadoAuth>
  resetPasswordForEmail(correo: string, opciones?: { redirectTo?: string }): Promise<ResultadoAuth>
  updateUser(atributos: { password: string }): Promise<ResultadoAuth>
}

export interface ClienteAutenticacion {
  auth: ClienteAuth
}
