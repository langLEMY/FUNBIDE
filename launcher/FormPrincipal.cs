using System.Diagnostics;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace FUNBIDE.Launcher;

/// <summary>
/// Ventana nativa que hospeda la SPA en un WebView2 en vez de abrir el navegador
/// del sistema, para que FUNBIDE se sienta como un programa instalado (ícono y
/// entrada propios en la barra de tareas, sin barra de direcciones) y no como una
/// pestaña de Chrome/Edge apuntando a localhost.
/// </summary>
public sealed class FormPrincipal : Form
{
    private readonly Process? _procesoBackend;
    private readonly string _url;
    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill };

    public FormPrincipal(string url, Process? procesoBackend)
    {
        _url = url;
        _procesoBackend = procesoBackend;

        Text = "FUNBIDE";
        Width = 1366;
        Height = 850;
        MinimumSize = new Size(1024, 700);
        StartPosition = FormStartPosition.CenterScreen;
        // Maximizar recién en Shown (no acá): si la ventana ya nace Maximized, WebView2
        // a veces crea su controller con los bounds de un frame intermedio y queda
        // pintando negro sólido para siempre — encontrado reproduciendo el bug real,
        // confirmado que la SPA en sí carga bien (misma URL abierta en un navegador
        // normal funciona perfecto). Nacer en tamaño normal y maximizar después le da
        // al control un resize real del que agarrarse.
        Shown += (_, _) => WindowState = FormWindowState.Maximized;

        // El .ico no se copia como archivo suelto al publicar (PublishSingleFile);
        // se extrae del recurso ya embebido en el propio .exe vía <ApplicationIcon>.
        var iconoEmbebido = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        if (iconoEmbebido is not null)
        {
            Icon = iconoEmbebido;
        }

        Controls.Add(_webView);
        Load += FormPrincipal_LoadAsync;
        FormClosing += FormPrincipal_FormClosing;
    }

    private async void FormPrincipal_LoadAsync(object? sender, EventArgs e)
    {
        try
        {
            // Perfil propio (no el de Edge del usuario) para que las cookies/localStorage
            // de la sesión de FUNBIDE no se mezclen con el navegador normal de la persona.
            var carpetaDatos = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FUNBIDE", "WebView2");
            // --disable-gpu: sin esto, en máquinas/VMs con aceleración de GPU floja o sin
            // drivers correctos (muy común en escritorios remotos/VMs), el WebView2 renderiza
            // una pantalla negra sólida en vez de caer a renderizado por software — la SPA
            // carga bien (confirmado abriéndola en un navegador normal contra el mismo backend
            // local), el problema es solo la composición gráfica del control WebView2 en sí.
            var opciones = new CoreWebView2EnvironmentOptions { AdditionalBrowserArguments = "--disable-gpu" };
            var entorno = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null, userDataFolder: carpetaDatos, options: opciones);
            await _webView.EnsureCoreWebView2Async(entorno);

            _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            _webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;

            // Puente para imprimir directo a la impresora térmica de recibos, sin el
            // diálogo de impresión de Windows: la SPA (CobrosPage) no puede hablarle a
            // una impresora — le pide a este host nativo vía postMessage, y acá sí
            // podemos usar CoreWebView2.PrintAsync en modo silencioso. Ver
            // frontend/src/lib/imprimir.ts para el lado del mensaje.
            _webView.CoreWebView2.WebMessageReceived += async (_, args) =>
            {
                if (args.TryGetWebMessageAsString() == "imprimir-directo")
                {
                    await ImprimirDirectoAsync();
                }
            };

            _webView.Source = new Uri(_url);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            MessageBox.Show(
                "Falta el componente \"Microsoft Edge WebView2 Runtime\", necesario para mostrar FUNBIDE.\n\n" +
                "Windows 11 lo trae instalado de fábrica; si no está, descargalo desde " +
                "https://go.microsoft.com/fwlink/p/?LinkId=2124703 e instalalo, luego volvé a abrir FUNBIDE.",
                "FUNBIDE",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            Close();
        }
        catch (Exception ex)
        {
            // Antes esta rama no existía: un fallo acá (p. ej. no se pudo crear el entorno
            // de WebView2) quedaba completamente silencioso para quien lo usa — la ventana
            // se abría vacía/negra sin ninguna pista de qué pasó. AppDomain.UnhandledException
            // (ver Program.cs) no lo agarra porque esto es un async void ya dentro de su
            // propio try.
            MessageBox.Show(
                "FUNBIDE no pudo iniciar la vista:\n\n" + ex,
                "FUNBIDE — error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Imprime la página actual (el comprobante de Cobros) directo a la impresora
    /// predeterminada de Windows, sin mostrar ningún diálogo — pensado para la
    /// impresora térmica de recibos de Caja. No se fuerza ningún tamaño de papel
    /// (MediaSize/PageWidth/PageHeight): se deja el que ya tenga configurado por
    /// defecto el driver de esa impresora, para no arriesgar un valor incorrecto
    /// para el modelo real que use la fundación.
    /// </summary>
    private async Task ImprimirDirectoAsync()
    {
        try
        {
            var configuracion = _webView.CoreWebView2.Environment.CreatePrintSettings();
            configuracion.ShouldPrintBackgrounds = true;
            configuracion.ShouldPrintHeaderAndFooter = false;
            // Márgenes chicos (el default de WebView2 es ~1cm por lado): un recibo
            // térmico angosto no puede darse el lujo de perder 2cm de ancho en blanco.
            configuracion.MarginTop = 0.08;
            configuracion.MarginBottom = 0.08;
            configuracion.MarginLeft = 0.08;
            configuracion.MarginRight = 0.08;
            // PrinterName vacío = imprime a la impresora predeterminada del sistema.

            var resultado = await _webView.CoreWebView2.PrintAsync(configuracion);
            if (resultado != CoreWebView2PrintStatus.Succeeded)
            {
                MessageBox.Show(
                    $"No se pudo imprimir ({resultado}). Revisa que la impresora predeterminada de Windows esté encendida y conectada.",
                    "FUNBIDE — impresión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No se pudo imprimir: {ex.Message}",
                "FUNBIDE — impresión",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void FormPrincipal_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // Solo apagamos el backend si este launcher lo arrancó (proceso propio, no
        // uno que ya estuviera corriendo de antes en el puerto).
        if (_procesoBackend is { HasExited: false })
        {
            try
            {
                _procesoBackend.Kill(entireProcessTree: true);
            }
            catch
            {
                // Mejor esfuerzo: si ya terminó o no se puede matar, no bloquea el cierre.
            }
        }
    }
}
