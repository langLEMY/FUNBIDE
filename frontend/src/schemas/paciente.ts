import { z } from 'zod'

export const esquemaCrearPaciente = z.object({
  nombre: z.string().trim().min(1, 'El nombre es obligatorio.'),
  apellido: z.string().trim().min(1, 'El apellido es obligatorio.'),
  cedula: z.string().trim().min(1, 'La cédula es obligatoria.'),
  telefono: z.string().trim().optional(),
})

export type DatosCrearPaciente = z.infer<typeof esquemaCrearPaciente>
