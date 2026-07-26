# Acceso directo de escritorio: si el backend ya esta corriendo solo abre el navegador
# (rapido). Si no, lo arranca desde el .dll ya compilado (evita el chequeo de
# "esta actualizado" de `dotnet run`, que agrega ~1-2s) y espera a /health antes de abrir.

$raiz = Split-Path -Parent $PSScriptRoot
$backendDir = Join-Path $raiz 'src\FUNBIDE.API'
$dll = Join-Path $backendDir 'bin\Debug\net9.0\FUNBIDE.API.dll'
$url = 'http://localhost:5080'

function Test-Backend {
    try {
        $resp = Invoke-WebRequest -Uri "$url/health" -TimeoutSec 1 -UseBasicParsing
        return $resp.StatusCode -eq 200
    } catch {
        return $false
    }
}

if (-not (Test-Backend)) {
    if (-not (Test-Path $dll)) {
        Add-Type -AssemblyName System.Windows.Forms
        [System.Windows.Forms.MessageBox]::Show(
            "No se encontro FUNBIDE.API.dll compilado.`nCorre 'dotnet build' en $backendDir primero.",
            'FUNBIDE') | Out-Null
        exit 1
    }

    # Al correr el .dll directo (no via `dotnet run`) no se lee launchSettings.json,
    # asi que el puerto hay que fijarlo a mano o Kestrel cae al 5000 por defecto.
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:ASPNETCORE_URLS = $url
    Start-Process -FilePath 'dotnet' -ArgumentList "`"$dll`"" -WorkingDirectory $backendDir -WindowStyle Hidden

    $listo = $false
    for ($i = 0; $i -lt 60; $i++) {
        Start-Sleep -Milliseconds 250
        if (Test-Backend) { $listo = $true; break }
    }

    if (-not $listo) {
        Add-Type -AssemblyName System.Windows.Forms
        [System.Windows.Forms.MessageBox]::Show(
            "FUNBIDE no respondio a tiempo. Revisa que PostgreSQL/la conexion a internet esten disponibles.",
            'FUNBIDE') | Out-Null
        exit 1
    }
}

Start-Process $url
