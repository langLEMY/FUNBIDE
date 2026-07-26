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
        WindowState = FormWindowState.Maximized;

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
            var entorno = await CoreWebView2Environment.CreateAsync(userDataFolder: carpetaDatos);
            await _webView.EnsureCoreWebView2Async(entorno);

            _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            _webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;

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
