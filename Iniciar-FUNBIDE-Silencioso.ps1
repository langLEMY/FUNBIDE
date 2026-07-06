# Levanta FUNBIDE (backend + frontend, ya compilados en un solo proceso) sin
# mostrar ninguna ventana de consola, y abre el navegador cuando esta listo.

$root = $PSScriptRoot
$exe = Join-Path $root "publish\FUNBIDE.API.exe"
$logPath = Join-Path $env:TEMP "funbide-app.log"
$puerto = 5090
$urlEscucha = "http://0.0.0.0:$puerto"
$url = "http://localhost:$puerto"

if (-not (Test-Path $exe)) {
    Add-Type -AssemblyName PresentationFramework
    [System.Windows.MessageBox]::Show("No se encontro $exe. Hay que publicar el backend primero.", "FUNBIDE")
    exit 1
}

# Si ya esta corriendo (ventana previa sin cerrar), solo abre el navegador.
$yaCorriendo = Get-NetTCPConnection -LocalPort $puerto -State Listen -ErrorAction SilentlyContinue
if (-not $yaCorriendo) {
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $exe
    $psi.WorkingDirectory = Join-Path $root "publish"
    $psi.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden
    $psi.CreateNoWindow = $true
    $psi.UseShellExecute = $false
    $psi.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development"
    $psi.EnvironmentVariables["ASPNETCORE_URLS"] = $urlEscucha
    [System.Diagnostics.Process]::Start($psi) | Out-Null

    # Espera a que responda antes de abrir el navegador.
    $listo = $false
    for ($i = 0; $i -lt 40; $i++) {
        try {
            Invoke-RestMethod -Uri "$url/health" -TimeoutSec 1 -ErrorAction Stop | Out-Null
            $listo = $true
            break
        } catch {
            Start-Sleep -Milliseconds 500
        }
    }
}

Start-Process $url
