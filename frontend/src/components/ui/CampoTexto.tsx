import type { InputHTMLAttributes, ReactNode } from 'react'
import type { UseFormRegisterReturn } from 'react-hook-form'
import './CampoTexto.css'

interface CampoTextoProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'name' | 'onChange' | 'onBlur'> {
  etiqueta: string
  /** Resultado de register('campo') de react-hook-form. */
  registro: UseFormRegisterReturn
  obligatorio?: boolean
  error?: string
  ayuda?: ReactNode
  /** Formatea el valor en vivo (mientras se escribe) antes de que react-hook-form lo
      registre — ver utils/mascaras.ts. */
  mascara?: (valor: string) => string
}

/**
 * Input con label consistente en toda la app: asterisco rojo si es obligatorio,
 * mensaje de error debajo, y soporte opcional de máscara en vivo (cédula, teléfono,
 * montos). Pensado para usarse junto a react-hook-form: `registro` es directamente
 * lo que devuelve `register('campo')`.
 */
export function CampoTexto({ etiqueta, registro, obligatorio, error, ayuda, mascara, id, ...resto }: CampoTextoProps) {
  const idCampo = id ?? registro.name

  return (
    <div className="ui-campo-texto">
      <label htmlFor={idCampo} className="ui-campo-texto-label">
        {etiqueta}
        {obligatorio && (
          <span className="ui-campo-texto-obligatorio" aria-hidden="true">
            *
          </span>
        )}
        {ayuda}
      </label>
      <input
        id={idCampo}
        className="ui-campo-texto-input"
        aria-required={obligatorio || undefined}
        aria-invalid={Boolean(error) || undefined}
        {...resto}
        {...registro}
        onChange={(evento) => {
          if (mascara) {
            evento.target.value = mascara(evento.target.value)
          }
          void registro.onChange(evento)
        }}
      />
      {error && <p className="ui-campo-texto-error">{error}</p>}
    </div>
  )
}
