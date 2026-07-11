<#
.SYNOPSIS
    Genera el paquete offline/USB de FUNBIDE (Auth:Provider=Local) listo para copiar a
    otra máquina, sin necesitar el repositorio ni el SDK de .NET/Node instalados ahí.

.DESCRIPTION
    Compila el frontend en modo local (VITE_AUTH_MODE=local), publica el backend como un
    único .exe autocontenido (incluye el runtime de .NET — la PC destino no necesita
    instalar nada aparte de PostgreSQL), copia el frontend dentro de wwwroot, publica el
    launcher de escritorio, y deja todo empaquetado en dist-offline\ (+ un .zip) — una
    carpeta aparte del repo, no la mezcla con el código fuente.

.EXAMPLE
    pwsh scripts/publicar-offline.ps1

.NOTES
    Después de correrlo:
      1. Copiar publish\appsettings.Local.json.example a publish\appsettings.Local.json
         (dentro de dist-offline\) y completar los CHANGE_ME (connection string del
         Postgres local, claves generadas — el propio .example trae el comando para
         generarlas).
      2. Llevar dist-offline.zip a la USB / a la PC destino. Ese .zip es autocontenido:
         no hace falta .NET, Node ni el repositorio ahí — solo PostgreSQL instalado.
      3. En la PC destino: crear la base 'funbide' en el PostgreSQL local, descomprimir
         el .zip, y abrir FUNBIDE.exe.
#>

$ErrorActionPreference = "Stop"

function Invoke-Paso {
    param([string]$Descripcion, [scriptblock]$Accion)
    Write-Host "==> $Descripcion" -ForegroundColor Cyan
    & $Accion
    if ($LASTEXITCODE -ne 0) {
        throw "Falló: $Descripcion (código $LASTEXITCODE)"
    }
}

$raiz = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Set-Location $raiz

$paqueteDir = Join-Path $raiz "dist-offline"
$publishDir = Join-Path $paqueteDir "publish"
$zipPath = Join-Path $raiz "dist-offline.zip"

if (Test-Path $paqueteDir) {
    Remove-Item $paqueteDir -Recurse -Force
}
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

Invoke-Paso "Compilando frontend en modo offline (VITE_AUTH_MODE=local)" {
    Push-Location (Join-Path $raiz "frontend")
    try { npm run build:offline } finally { Pop-Location }
}

Invoke-Paso "Publicando backend autocontenido (FUNBIDE.API.exe, win-x64, sin depender de que el runtime de .NET este instalado en destino)" {
    dotnet publish (Join-Path $raiz "src\FUNBIDE.API") -c Release -o $publishDir `
        -r win-x64 --self-contained true `
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
}

# "dotnet publish" copia cualquier appsettings.*.json que encuentre en el proyecto,
# incluido appsettings.Development.json — que en la máquina de un desarrollador puede
# tener secretos reales (cadena de conexión a la base cloud). Nunca debe viajar en un
# paquete que se entrega a un cliente: se borra todo excepto el appsettings.json base
# (solo placeholders, ya está en git tal cual).
Get-ChildItem $publishDir -Filter "appsettings.*.json" |
    Where-Object { $_.Name -ne "appsettings.json" } |
    Remove-Item -Force

Write-Host "==> Copiando el build del frontend a publish\wwwroot" -ForegroundColor Cyan
$wwwroot = Join-Path $publishDir "wwwroot"
New-Item -ItemType Directory -Force -Path $wwwroot | Out-Null
Copy-Item (Join-Path $raiz "frontend\dist\*") $wwwroot -Recurse -Force

Invoke-Paso "Publicando el launcher (FUNBIDE.exe)" {
    dotnet publish (Join-Path $raiz "launcher") -c Release -o $paqueteDir
}

Copy-Item (Join-Path $raiz "src\FUNBIDE.API\appsettings.Local.json.example") `
    (Join-Path $publishDir "appsettings.Local.json.example") -Force

# Texto en ASCII puro a propósito (sin tildes/ni/em-dash): Windows PowerShell 5.1 (el
# que trae Windows por defecto) lee este .ps1 con la codepage del sistema si el archivo
# no tiene BOM, y corrompe cualquier caracter no-ASCII embebido en un literal antes de
# llegar a Set-Content — visto en la práctica (tildes convertidas en "Ã³" etc.). Evitarlo
# de raíz es más confiable que depender de que el .ps1 conserve un BOM UTF-8.
$leeme = @'
FUNBIDE - instalacion offline/USB
==================================

Requisito unico en la PC destino: PostgreSQL instalado y corriendo. No hace
falta instalar .NET, Node ni nada mas - FUNBIDE.exe y publish\FUNBIDE.API.exe
ya traen todo lo que necesitan.

Pasos:
  1. Crear una base de datos vacia llamada "funbide" en el PostgreSQL local
     (y un usuario con permisos sobre ella).
  2. Copiar publish\appsettings.Local.json.example a publish\appsettings.Local.json
     y completar ahi los CHANGE_ME: la cadena de conexion a esa base, y dos
     claves generadas (el propio archivo trae el comando de PowerShell para
     generarlas).
  3. Abrir FUNBIDE.exe. La primera vez crea el esquema solo y una cuenta
     administradora ("Lemy") con una contrasena temporal que se muestra en
     pantalla una unica vez - cambiala apenas entres, desde tu perfil.
  4. Las siguientes veces, FUNBIDE.exe simplemente abre la app en el navegador.
'@
Set-Content -Path (Join-Path $paqueteDir "LEEME.txt") -Value $leeme -Encoding utf8

Invoke-Paso "Comprimiendo dist-offline.zip" {
    Compress-Archive -Path (Join-Path $paqueteDir "*") -DestinationPath $zipPath -Force
    $global:LASTEXITCODE = 0
}

Write-Host ""
Write-Host "Paquete listo:" -ForegroundColor Green
Write-Host "  Carpeta: $paqueteDir"
Write-Host "  Zip:     $zipPath  <- esto es lo unico que hay que llevar a la USB/PC destino"
Write-Host ""
Write-Host "Antes de instalar en destino:" -ForegroundColor Yellow
Write-Host "  1. Completar publish\appsettings.Local.json (ver LEEME.txt dentro del paquete)."
Write-Host "  2. Confirmar que la PC destino tiene PostgreSQL instalado."
