import { z } from 'zod'

// Mismo mensaje genérico que tenía el chequeo manual anterior en LoginPage.tsx
// (no distinguir "falta el usuario" de "falta la contraseña" es intencional acá).
const MENSAJE_REQUERIDO = 'Ingresa tu usuario y tu contraseña.'

export const esquemaLogin = z.object({
  nombreUsuario: z.string().trim().min(1, MENSAJE_REQUERIDO),
  contrasena: z.string().min(1, MENSAJE_REQUERIDO),
})

export type DatosLogin = z.infer<typeof esquemaLogin>
