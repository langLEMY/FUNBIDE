<#
.SYNOPSIS
    Genera el instalador profesional de escritorio de FUNBIDE
    (instalacion\Output\FUNBIDE-Setup-x64.exe): wizard real con Inno Setup, acceso
    directo, entrada en "Agregar o quitar programas" y desinstalador — reemplaza el
    flujo anterior de instalar-escritorio.ps1 (copiar carpeta + crear acceso directo
    a mano).

.DESCRIPTION
    1. Corre publicar-offline.ps1 (compila frontend en modo Auth:Provider=Local +
       backend autocontenido + launcher, a dist-offline\).
    2. Escribe dist-offline\publish\appsettings.Local.json apuntando al Postgres real
       de Supabase con el rol acotado "funbide_app" (sin privilegios de esquema, ver
       instalacion\funbide_app_setup.sql para crear ese rol la primera vez), con
       Database:AplicarMigracionesAlIniciar=false — esta PC nunca migra el esquema,
       eso lo hace el despliegue de servidor.
    3. Compila instalacion\FUNBIDE.iss con Inno Setup (ISCC.exe) y deja el .exe final
       en instalacion\Output\.
    4. Genera el hash SHA256 del .exe (mismo nombre + ".sha256") -- el auto-update del
       launcher (ver launcher\Actualizacion\ServicioActualizacion.cs) lo descarga y lo
       compara antes de ejecutar nada. Subir AMBOS archivos como assets del Release.

    El instalador resultante NO lleva ninguna clave maestra de Supabase (ni la
    contrasena del rol admin, ni la ServiceRoleKey) — solo la contrasena del rol
    restringido funbide_app, que unicamente puede leer/escribir filas dentro del
    esquema funbide.

.PARAMETER DbPassword
    Contrasena del rol funbide_app en Supabase. Requerido. No se guarda en ningun
    archivo del repo (appsettings.Local.json esta en .gitignore).

.EXAMPLE
    pwsh scripts/generar-instalador.ps1 -DbPassword "..."
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$DbPassword
)

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

Invoke-Paso "Generando el paquete offline (frontend + backend + launcher)" {
    & (Join-Path $raiz "scripts\publicar-offline.ps1")
}

$publishDir = Join-Path $raiz "dist-offline\publish"

function New-Base64Key {
    $b = New-Object byte[] 32
    (New-Object Security.Cryptography.RNGCryptoServiceProvider).GetBytes($b)
    [Convert]::ToBase64String($b)
}

Write-Host "==> Escribiendo appsettings.Local.json (conectado a la base real, rol funbide_app)" -ForegroundColor Cyan
$config = [ordered]@{
    Logging           = @{
        LogLevel = @{
            Default                         = "Information"
            "Microsoft.AspNetCore"          = "Warning"
            "Microsoft.EntityFrameworkCore" = "Warning"
        }
    }
    ConnectionStrings = @{
        FunbideDatabase = "Host=aws-0-us-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=funbide_app.ynrxsqkvkkhpkqgeawpt;Password=$DbPassword;SSL Mode=Require;Trust Server Certificate=true"
    }
    Auth              = @{
        Provider = "Local"
        Local    = @{
            SigningKeyBase64 = New-Base64Key
            DuracionToken    = "12:00:00"
        }
    }
    Storage           = @{
        Local = @{
            DirectorioBase = "almacenamiento-local"
            FirmaKeyBase64 = New-Base64Key
        }
    }
    # Esta PC nunca migra el esquema ni corre en carrera contra otras instalaciones de
    # personal apuntando a la misma base: el esquema real lo migra el despliegue del
    # servidor, y el rol funbide_app no tiene privilegios de DDL de todas formas.
    Database          = @{
        AplicarMigracionesAlIniciar = $false
    }
    Backup            = @{
        Habilitado = $false
    }
    Cors              = @{
        OrigenesPermitidos = @()
    }
}
$config | ConvertTo-Json -Depth 6 | Set-Content -Path (Join-Path $publishDir "appsettings.Local.json") -Encoding utf8

$iscc = Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $iscc)) {
    throw "No se encontro ISCC.exe (Inno Setup) en '$iscc'. Instalalo con: winget install JRSoftware.InnoSetup"
}

Invoke-Paso "Compilando el instalador con Inno Setup" {
    & $iscc (Join-Path $raiz "instalacion\FUNBIDE.iss")
}

# El nombre del .exe incluye la version (ver instalacion\FUNBIDE.iss, OutputBaseFilename),
# leida de version.txt -- se busca en vez de hardcodearlo para no desincronizarse.
$versionTexto = (Get-Content (Join-Path $raiz "version.txt") -Raw).Trim()
$outputDir = Join-Path $raiz "instalacion\Output"
$rutaInstalador = Join-Path $outputDir "FUNBIDE-$versionTexto-Setup-x64.exe"

if (-not (Test-Path $rutaInstalador)) {
    throw "No se encontro el instalador esperado en '$rutaInstalador' -- revisa que version.txt y OutputBaseFilename en FUNBIDE.iss coincidan."
}

# Hash SHA256 publicado junto al .exe: el launcher lo descarga y lo compara ANTES de
# ejecutar nada (ver launcher/Actualizacion/ServicioActualizacion.cs,
# VerificarHash.Coincide) -- sin este archivo, el auto-update no ofrece la version nueva.
# Hay que subir AMBOS archivos como assets del mismo GitHub Release, con estos mismos
# nombres (el .sha256 tiene que llamarse exactamente "<nombre-del-exe>.sha256").
Write-Host "==> Generando hash SHA256 del instalador" -ForegroundColor Cyan
$hash = (Get-FileHash -Path $rutaInstalador -Algorithm SHA256).Hash
$rutaHash = "$rutaInstalador.sha256"
Set-Content -Path $rutaHash -Value $hash -Encoding ascii -NoNewline

Write-Host ""
Write-Host "Instalador listo:" -ForegroundColor Green
Write-Host "  $rutaInstalador"
Write-Host "  $rutaHash"
Write-Host ""
Write-Host "Para que el auto-update (dentro de la app) encuentre esta version, subi AMBOS" -ForegroundColor Yellow
Write-Host "archivos como assets del GitHub Release con el tag v$versionTexto." -ForegroundColor Yellow
Write-Host ""
Write-Host "Ese .exe SI se puede repartir a las PCs de la fundacion: no lleva ninguna" -ForegroundColor Yellow
Write-Host "clave maestra, solo la contrasena del rol acotado funbide_app." -ForegroundColor Yellow
