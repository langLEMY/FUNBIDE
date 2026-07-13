export function exportarCsv(nombreArchivo: string, filas: Record<string, string | number>[]): void {
  if (filas.length === 0) {
    return
  }

  const columnas = Object.keys(filas[0])
  const escapar = (valor: string | number) => {
    const texto = String(valor)
    // Un string con ceros a la izquierda (p. ej. un código "000123") se ve como número al
    // abrirlo en Excel, que lo reinterpreta y pierde los ceros. La sintaxis ="..." fuerza
    // a Excel a tratarlo como texto literal en vez de numérico.
    if (typeof valor === 'string' && /^0\d+$/.test(valor)) {
      return `="${valor}"`
    }
    return /[",\n]/.test(texto) ? `"${texto.replace(/"/g, '""')}"` : texto
  }

  const lineas = [
    columnas.join(','),
    ...filas.map((fila) => columnas.map((columna) => escapar(fila[columna])).join(',')),
  ]

  const blob = new Blob([lineas.join('\n')], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const enlace = document.createElement('a')
  enlace.href = url
  enlace.download = nombreArchivo
  enlace.click()
  URL.revokeObjectURL(url)
}
