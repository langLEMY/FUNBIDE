<#
.SYNOPSIS
    Arranca el backend de FUNBIDE en modo servidor: escucha en todas las
    interfaces de red (0.0.0.0), no solo localhost.

.DESCRIPTION
    FUNBIDE.exe (el lanzador con ventana nativa) siempre arranca su propio
    backend escuchando solo en 127.0.0.1 - a proposito, pensado para el modo
    de un solo puesto (ver launcher/Program.cs). Este script es el que hay que
    correr en la maquina que va a actuar de SERVIDOR para que las demas PCs de
    la red local puedan conectarse con un navegador comun.

    Requiere que esta carpeta este al lado de publish\ (la misma estructura
    que genera scripts/publicar-portable.ps1). No abre ninguna ventana: queda
    corriendo en segundo plano, pensado para dejarlo como tarea programada de
    Windows (ver LEEME-INSTALACION-SERVIDOR.txt, seccion 6).

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File instalacion\iniciar-servidor.ps1
#>

$ErrorActionPreference = "Stop"
$puerto = 5090
$raiz = Split-Path -Parent $PSScriptRoot
$apiExe = Join-Path $raiz "publish\FUNBIDE.API.exe"

if (-not (Test-Path $apiExe)) {
    Write-Host "No se encontro $apiExe." -ForegroundColor Red
    Write-Host "Esta carpeta 'instalacion' tiene que estar al lado de 'publish\' (ver LEEME-INSTALACION-SERVIDOR.txt)." -ForegroundColor Red
    exit 1
}

function Test-Backend {
    try {
        $r = Invoke-WebRequest -Uri "http://127.0.0.1:$puerto/health" -TimeoutSec 1 -UseBasicParsing
        return $r.StatusCode -eq 200
    } catch {
        return $false
    }
}

if (Test-Backend) {
    Write-Host "FUNBIDE ya esta corriendo en el puerto $puerto." -ForegroundColor Yellow
    exit 0
}

$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $apiExe
$psi.WorkingDirectory = Join-Path $raiz "publish"
$psi.WindowStyle = [System.Diagnostics.ProcessWindowStyle]::Hidden
$psi.CreateNoWindow = $true
$psi.UseShellExecute = $false
# "Local": mismo nombre de entorno que usa FUNBIDE.exe, carga publish\appsettings.Local.json
# (la conexion real a Supabase, la misma para todas las maquinas).
$psi.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Local"
# La diferencia clave con el modo de un solo puesto: 0.0.0.0 en lugar de 127.0.0.1,
# para aceptar conexiones desde otras PCs de la red local, no solo desde esta maquina.
$psi.EnvironmentVariables["ASPNETCORE_URLS"] = "http://0.0.0.0:$puerto"
[System.Diagnostics.Process]::Start($psi) | Out-Null

Write-Host "Arrancando FUNBIDE en modo servidor (puerto $puerto, todas las interfaces)..." -ForegroundColor Cyan
$listo = $false
for ($i = 0; $i -lt 60; $i++) {
    Start-Sleep -Milliseconds 500
    if (Test-Backend) { $listo = $true; break }
}

if ($listo) {
    $ipLocal = (Get-NetIPAddress -AddressFamily IPv4 |
        Where-Object { $_.InterfaceAlias -notmatch "Loopback" -and $_.IPAddress -notlike "169.254.*" } |
        Select-Object -First 1).IPAddress
    Write-Host "FUNBIDE listo. Las otras PCs se conectan con un navegador a: http://$ipLocal`:$puerto" -ForegroundColor Green
} else {
    Write-Host "FUNBIDE no respondio a tiempo. Revisa la conexion a internet (hace falta para Supabase) o si el proceso quedo bloqueado." -ForegroundColor Red
    exit 1
}
