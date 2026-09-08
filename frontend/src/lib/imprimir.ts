/**
 * Imprime la ventana actual. Dentro del launcher de escritorio (WebView2) le avisa
 * al host nativo por postMessage para que imprima directo a la impresora predeterminada
 * de Windows sin ningún diálogo (ver launcher/FormPrincipal.cs, ImprimirDirectoAsync) —
 * pensado para la impresora térmica de recibos de Caja. Fuera del launcher (navegador
 * normal, `npm run dev`) no existe `window.chrome.webview`, así que cae al diálogo de
 * impresión estándar del navegador.
 */
export function imprimirVentana(): void {
  const webview = (window as unknown as { chrome?: { webview?: { postMessage: (mensaje: string) => void } } }).chrome
    ?.webview

  if (webview) {
    webview.postMessage('imprimir-directo')
    return
  }

  window.print()
}
