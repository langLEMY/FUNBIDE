import { useRef, useState, type ChangeEvent } from 'react'
import { api, ApiError } from '../lib/api'
import './ImportarExcel.css'

interface ImportarExcelProps<T> {
  endpoint: string
  onImportado: (resultado: T) => void
}

export function ImportarExcel<T>({ endpoint, onImportado }: ImportarExcelProps<T>) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [importando, setImportando] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleChange = async (event: ChangeEvent<HTMLInputElement>) => {
    const archivo = event.target.files?.[0]
    if (!archivo) return

    setError(null)
    setImportando(true)
    try {
      const formData = new FormData()
      formData.append('archivo', archivo)
      const resultado = await api.postForm<T>(endpoint, formData)
      onImportado(resultado)
    } catch (err) {
      setError(err instanceof ApiError ? (err.detalle ?? err.message) : 'No se pudo importar el archivo.')
    } finally {
      setImportando(false)
      if (inputRef.current) inputRef.current.value = ''
    }
  }

  return (
    <div className="importar-excel">
      <label className={`importar-excel-boton${importando ? ' deshabilitado' : ''}`}>
        {importando ? 'Importando…' : 'Importar desde Excel'}
        <input
          ref={inputRef}
          type="file"
          accept=".xls,.xlsx"
          onChange={(event) => void handleChange(event)}
          disabled={importando}
          hidden
        />
      </label>
      {error && <p className="importar-excel-error">{error}</p>}
    </div>
  )
}
