using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Windows.Forms;
using FUNBIDE.Launcher;
using FUNBIDE.Launcher.Actualizacion;

namespace FUNBIDE.Launcher;

internal static class Program
{
    private const int Puerto = 5090;

    // Cuánto se espera, como máximo, la respuesta de GitHub antes de seguir arrancando
    // sin más — ver BuscarActualizacionAsync. Instalaciones "Local" offline/USB (ver
    // ASPNETCORE_ENVIRONMENT=Local más abajo) nunca van a tener red: para esas, este es
    // exactamente el tiempo de más que tarda en abrir FUNBIDE, siempre.
    private static readonly TimeSpan TimeoutChequeoActualizacion = TimeSpan.FromSeconds(2.5);

    // Hilo STA único de punta a punta: WebView2 inicializa su propio apartamento COM
    // más adelante (en FormPrincipal) y choca (RPC_E_CHANGED_MODE) si el hilo no fue
    // STA desde el arranque. Por eso el entry point es síncrono ([STAThread] no es
    // válido en un Main async) y las esperas usan GetAwaiter().GetResult().
    [STAThread]
    private static void Main()
    {
        MatarWebView2HuerfanosAsync().GetAwaiter().GetResult();

        // No bloqueante en el sentido de "nunca interrumpe": corre acá, antes de levantar
        // la ventana, con un timeout corto — si no hay red o GitHub no contesta a tiempo,
        // sigue el arranque normal sin ningún diálogo (ver ServicioActualizacion, que
        // nunca lanza). El aviso en sí (si hay algo nuevo) recién se muestra, ignorable,
        // una vez que FormPrincipal ya está abierto — ver FormPrincipal.Shown.
        // Timeout del HttpClient en 10 minutos (no el default de 100s): se reusa esta
        // misma instancia para el chequeo corto (con su propio CancellationToken de 2.5s,
        // más restrictivo, ver BuscarActualizacionAsync) y para la descarga del instalador
        // (con hasta 5 minutos, ver FormActualizacionDisponible) — el Timeout del cliente
        // no puede ser menor que eso o cortaría la descarga sin importar el token pasado.
        var servicioActualizacion = new ServicioActualizacion(new HttpClient { Timeout = TimeSpan.FromMinutes(10) });
        var infoActualizacion = BuscarActualizacionAsync(servicioActualizacion).GetAwaiter().GetResult();

        var url = $"http://127.0.0.1:{Puerto}";

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

        Process? procesoBackend = null;
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
            psi.EnvironmentVariables["ASPNETCORE_URLS"] = url;
            procesoBackend = Process.Start(psi);

            if (!EsperarListoAsync(url).GetAwaiter().GetResult())
            {
                MessageBox.Show(
                    "FUNBIDE tardó demasiado en arrancar. Cerralo y volvé a intentar en unos segundos.",
                    "FUNBIDE",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            MostrarCredencialesInicialesSiExisten(raiz);
        }

        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => RegistrarCrash(e.Exception, raiz);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => RegistrarCrash(e.ExceptionObject as Exception, raiz);
        Application.Run(new FormPrincipal(url, procesoBackend, servicioActualizacion, infoActualizacion));
    }

    private static async Task<InfoActualizacion?> BuscarActualizacionAsync(ServicioActualizacion servicioActualizacion)
    {
        try
        {
            var versionLocal = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);
            using var cancelacion = new CancellationTokenSource(TimeoutChequeoActualizacion);
            return await servicioActualizacion.BuscarActualizacionAsync(versionLocal, cancelacion.Token);
        }
        catch
        {
            // ServicioActualizacion.BuscarActualizacionAsync ya atrapa todo lo suyo, pero
            // esto cubre igual cualquier cosa rara (p. ej. el propio timeout cancelando la
            // tarea) — jamás debe impedir que FUNBIDE arranque.
            return null;
        }
    }

    private static void RegistrarCrash(Exception? ex, string raiz)
    {
        var mensaje = ex?.ToString() ?? "Excepción desconocida (sin objeto Exception).";
        try
        {
            File.WriteAllText(Path.Combine(raiz, "crash.log"), $"{DateTimeOffset.Now:O}\n{mensaje}");
        }
        catch
        {
            // Si ni siquiera se puede escribir el log, igual mostramos el cuadro de abajo.
        }

        MessageBox.Show(
            "FUNBIDE encontró un error inesperado y se va a cerrar:\n\n" + mensaje,
            "FUNBIDE — error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private static void MostrarCredencialesInicialesSiExisten(string raiz)
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

    /// <summary>
    /// Si FUNBIDE se cerró de forma abrupta (Kill de este mismo launcher, o un crash),
    /// el/los proceso(s) hijo msedgewebview2.exe del perfil de FUNBIDE a veces quedan
    /// vivos como huérfanos. El próximo arranque reutiliza ese mismo proceso de browser
    /// (comparten perfil por carpetaDatos) — y si ese proceso quedó en un estado roto de
    /// renderizado, la ventana nueva hereda la pantalla negra para siempre, aunque el
    /// código del launcher esté perfecto. Encontrado reproduciendo el bug en vivo: matar
    /// estos huérfanos antes de arrancar es lo único que garantiza un browser process
    /// limpio en cada apertura. Solo se tocan procesos cuya línea de comandos referencia
    /// la carpeta de perfil de FUNBIDE — nunca msedgewebview2.exe de Edge/Teams/otras apps.
    /// </summary>
    private static async Task MatarWebView2HuerfanosAsync()
    {
        // Ojo: no comparar contra la ruta completa de LocalApplicationData. Windows puede
        // reportar la línea de comandos de otro proceso en formato de nombre corto 8.3
        // (p. ej. "RAYFER~1" en vez de "RAYFER FABIAN"), así que un match por ruta completa
        // falla en silencio y no mata nada. "FUNBIDE" y "WebView2" tienen 8 caracteres o
        // menos, así que ese tramo final nunca se acorta — es la parte estable para matchear.
        var marcaCarpetaDatos = Path.Combine("FUNBIDE", "WebView2");

        try
        {
            using var buscador = new System.Management.ManagementObjectSearcher(
                "SELECT ProcessId, CommandLine FROM Win32_Process WHERE Name = 'msedgewebview2.exe'");
            using var resultados = buscador.Get();

            foreach (var objeto in resultados)
            {
                using var proceso = objeto;
                var lineaComandos = proceso["CommandLine"] as string;
                if (string.IsNullOrEmpty(lineaComandos) ||
                    !lineaComandos.Contains(marcaCarpetaDatos, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var pid = (uint)proceso["ProcessId"];
                try
                {
                    Process.GetProcessById((int)pid).Kill(entireProcessTree: true);
                }
                catch
                {
                    // Mejor esfuerzo: si ya terminó entre el WMI query y acá, seguir.
                }
            }
        }
        catch
        {
            // WMI puede fallar en instalaciones raras (servicio deshabilitado, etc.) — no
            // vale la pena bloquear el arranque de FUNBIDE por esto, solo perdemos la
            // limpieza preventiva de huérfanos de esta vez.
        }

        // Da tiempo a que Windows libere el handle del proceso/perfil antes de que
        // CoreWebView2Environment.CreateAsync intente crear uno nuevo sobre esa carpeta.
        await Task.Delay(300);
    }

    private static bool PuertoEnUso(int puerto)
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

    private static async Task<bool> EsperarListoAsync(string url)
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
}
