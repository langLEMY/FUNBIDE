<#
.SYNOPSIS
    Genera el paquete portable de FUNBIDE (login y datos reales de Supabase) listo para
    copiar a otra maquina: FUNBIDE.exe + carpeta publish\, sin instalador ni wizard.

.DESCRIPTION
    Compila el frontend en modo real (con las credenciales de Supabase de frontend\.env.production),
    publica el backend como un unico .exe autocontenido, copia el frontend dentro de wwwroot,
    publica el launcher de escritorio, copia la configuracion real de conexion (la misma
    para todas las maquinas, ya que todas comparten la misma base en la nube), y deja todo
    empaquetado en dist-portable\ (+ un .zip) - una carpeta aparte del repo.

    A diferencia de publicar-offline.ps1 (modo Auth:Provider=Local, base propia por maquina,
    sin secretos en el paquete), este script SI copia un secreto real (la contrasena de la
    base de Supabase) desde publish\appsettings.Local.json en esta maquina. El zip resultante
    hay que tratarlo como archivo sensible: no subir a ningun lado publico.

.EXAMPLE
    pwsh scripts/publicar-portable.ps1
#>

$ErrorActionPreference = "Stop"

function Invoke-Paso {
    param([string]$Descripcion, [scriptblock]$Accion)
    Write-Host "==> $Descripcion" -ForegroundColor Cyan
    & $Accion
    if ($LASTEXITCODE -ne 0) {
        throw "Fallo: $Descripcion (codigo $LASTEXITCODE)"
    }
}

$raiz = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Set-Location $raiz

$configReal = Join-Path $raiz "publish\appsettings.Local.json"
if (-not (Test-Path $configReal)) {
    throw "Falta $configReal (la configuracion real con la cadena de conexion a Supabase). Este script copia ese archivo tal cual esta en esta maquina - crealo primero."
}

$paqueteDir = Join-Path $raiz "dist-portable"
$publishDir = Join-Path $paqueteDir "publish"
$zipPath = Join-Path $raiz "dist-portable.zip"

if (Test-Path $paqueteDir) {
    Remove-Item $paqueteDir -Recurse -Force
}
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

Invoke-Paso "Compilando frontend en modo real (Supabase, ver frontend\.env.production)" {
    Push-Location (Join-Path $raiz "frontend")
    try { npm run build } finally { Pop-Location }
}

Invoke-Paso "Publicando backend autocontenido (FUNBIDE.API.exe, win-x64)" {
    dotnet publish (Join-Path $raiz "src\FUNBIDE.API") -c Release -o $publishDir `
        -r win-x64 --self-contained true `
        -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
}

# Igual que publicar-offline.ps1: "dotnet publish" copia cualquier appsettings.*.json del
# proyecto, incluido appsettings.Development.json si existe en esta maquina de desarrollo
# (tiene la password real tambien, pero es la copia de *este* dev, no la que se distribuye).
# Se borra y se reemplaza por la config real de abajo, la misma para todas las maquinas.
Get-ChildItem $publishDir -Filter "appsettings.*.json" |
    Where-Object { $_.Name -ne "appsettings.json" } |
    Remove-Item -Force

Copy-Item $configReal (Join-Path $publishDir "appsettings.Local.json") -Force

Write-Host "==> Copiando el build del frontend a publish\wwwroot" -ForegroundColor Cyan
$wwwroot = Join-Path $publishDir "wwwroot"
New-Item -ItemType Directory -Force -Path $wwwroot | Out-Null
Copy-Item (Join-Path $raiz "frontend\dist\*") $wwwroot -Recurse -Force

Invoke-Paso "Publicando el launcher (FUNBIDE.exe)" {
    dotnet publish (Join-Path $raiz "launcher") -c Release -o $paqueteDir
}

# Simbolos de debug y XML de documentacion de paquetes NuGet (WebView2): no le
# sirven a quien instala, solo ensucian el paquete.
Get-ChildItem $paqueteDir -Filter "*.pdb" | Remove-Item -Force
Get-ChildItem $paqueteDir -Filter "*.xml" | Remove-Item -Force

# Texto en ASCII puro a proposito (ver publicar-offline.ps1: PowerShell 5.1 puede corromper
# tildes al leer este .ps1 sin BOM UTF-8 antes de llegar a Set-Content).
$leeme = @'
FUNBIDE - version portable
===========================

Que es esto: FUNBIDE.exe + la carpeta publish\ ya listos para usar. No hace
falta instalar nada - ni PostgreSQL, ni .NET, ni Node. Los datos y logins son
los reales (Supabase en la nube), asi que hace falta conexion a internet para
que la app arranque.

IMPORTANTE - archivo sensible:
  publish\appsettings.Local.json trae adentro la contrasena real de la base
  de datos de produccion. No subir este .zip a ningun lugar publico (Drive
  compartido, WhatsApp, etc.) - entregarlo por USB o un link privado.

Pasos en la PC destino:
  1. Copiar esta carpeta completa (o descomprimir el .zip) a un disco local
     (por ejemplo C:\FUNBIDE). No hace falta que sea Archivos de Programa.
  2. Si la PC no tiene el runtime "Microsoft Edge WebView2", FUNBIDE.exe lo
     va a avisar la primera vez - se descarga en segundos desde
     https://go.microsoft.com/fwlink/p/?LinkId=2124703 (Windows 11 ya lo trae).
  3. Abrir FUNBIDE.exe. Va a pedir el login real de siempre.

Para actualizar mas adelante: volver a correr scripts/publicar-portable.ps1
en la maquina de desarrollo y repartir el .zip nuevo (reemplaza todo el
contenido de la carpeta de instalacion en destino).
'@
Set-Content -Path (Join-Path $paqueteDir "LEEME.txt") -Value $leeme -Encoding utf8

Invoke-Paso "Comprimiendo dist-portable.zip" {
    Compress-Archive -Path (Join-Path $paqueteDir "*") -DestinationPath $zipPath -Force
    $global:LASTEXITCODE = 0
}

Write-Host ""
Write-Host "Paquete listo:" -ForegroundColor Green
Write-Host "  Carpeta: $paqueteDir"
Write-Host "  Zip:     $zipPath  <- esto es lo unico que hay que llevar a la USB/PC destino"
Write-Host ""
Write-Host "Recorda: el zip lleva la contrasena real de la base adentro. Tratalo como archivo sensible." -ForegroundColor Yellow
