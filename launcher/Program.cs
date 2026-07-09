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
        $"No se encontró {apiExe}.\nHay que publicar el backend primero (dotnet publish src/FUNBIDE.API -c Release -o publish).",
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
    psi.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
    psi.EnvironmentVariables["ASPNETCORE_URLS"] = $"http://0.0.0.0:{Puerto}";
    Process.Start(psi);

    if (!await EsperarListoAsync(url))
    {
        MessageBox.Show(
            "FUNBIDE tardó demasiado en arrancar. Intenta abrir " + url + " manualmente en unos segundos.",
            "FUNBIDE",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }
}

Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
return;

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
