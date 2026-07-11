using System.Diagnostics;
using System.Net.Sockets;
using System.Windows.Forms;

const int Puerto = 5090;
var url = $"http://localhost:{Puerto}";

var exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
var raiz = Path.GetDirectoryName(exePath) ?? Directory.GetCurrentDirectory();
var apiExe = Path.Combine(raiz, "publish", "FUNBIDE.API.exe");

if (!File.Exists(apiExe))
{
    MessageBox.Show(
        $"No se encontró {apiExe}.\nHay que generar el paquete offline primero (scripts/publicar-offline.ps1 desde la raíz del repo).",
        "FUNBIDE",
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
    return;
}

if (!PuertoEnUso(Puerto))
{
    var psi = new ProcessStartInfo
    {
        FileName = apiExe,
        WorkingDirectory = Path.Combine(raiz, "publish"),
        WindowStyle = ProcessWindowStyle.Hidden,
        CreateNoWindow = true,
        UseShellExecute = false,
    };
    // "Local": instalación offline/USB de un solo puesto (Auth:Provider=Local, ver
    // appsettings.Local.json.example) — nunca "Development", que carga
    // appsettings.Development.json (apunta a la base de Supabase en la nube).
    psi.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Local";
    // Solo loopback: es una instalación de un solo puesto, no hay razón para exponer
    // HTTP plano (con bearer tokens viajando en claro) al resto de la red local.
    psi.EnvironmentVariables["ASPNETCORE_URLS"] = $"http://127.0.0.1:{Puerto}";
    Process.Start(psi);

    if (!await EsperarListoAsync(url))
    {
        MessageBox.Show(
            "FUNBIDE tardó demasiado en arrancar. Intenta abrir " + url + " manualmente en unos segundos.",
            "FUNBIDE",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    MostrarCredencialesInicialesSiExisten(raiz);
}

Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
return;

static void MostrarCredencialesInicialesSiExisten(string raiz)
{
    // Lo escribe SembradorUsuarioInicialLocalService la primera vez que arranca con la
    // base vacía (ver src/FUNBIDE.Infrastructure/Security). Se muestra una única vez.
    var rutaCredenciales = Path.Combine(raiz, "publish", "credenciales-iniciales.txt");
    if (!File.Exists(rutaCredenciales))
    {
        return;
    }

    try
    {
        MessageBox.Show(
            File.ReadAllText(rutaCredenciales),
            "FUNBIDE — cuenta inicial",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        File.Delete(rutaCredenciales);
    }
    catch
    {
        // Mejor esfuerzo: si no se puede leer/borrar (permisos, archivo en uso), no
        // bloquea el arranque — se vuelve a intentar mostrar la próxima vez.
    }
}

static bool PuertoEnUso(int puerto)
{
    try
    {
        using var cliente = new TcpClient();
        var resultado = cliente.BeginConnect("127.0.0.1", puerto, null, null);
        var conectado = resultado.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(300));
        if (conectado && cliente.Connected)
        {
            cliente.EndConnect(resultado);
            return true;
        }
        return false;
    }
    catch
    {
        return false;
    }
}

static async Task<bool> EsperarListoAsync(string url)
{
    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(1) };
    for (var i = 0; i < 40; i++)
    {
        try
        {
            var respuesta = await http.GetAsync($"{url}/health");
            if (respuesta.IsSuccessStatusCode)
            {
                return true;
            }
        }
        catch
        {
            // Todavía no está listo, se reintenta.
        }

        await Task.Delay(500);
    }

    return false;
}
