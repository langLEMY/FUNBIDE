<#
.SYNOPSIS
    Instala FUNBIDE como aplicacion de escritorio en esta PC, sin instalador ni
    asistente: genera el paquete offline, lo deja conectado a la base real de la
    fundacion (rol acotado funbide_app, sin privilegios de esquema) y crea un
    acceso directo en el Escritorio.

.DESCRIPTION
    Reemplaza al viejo instalador de Inno Setup (que pedia los datos de conexion a
    Postgres en un asistente). Ahora:
      1. Corre publicar-offline.ps1 (compila frontend + backend autocontenido + launcher
         a dist-offline\).
      2. Escribe dist-offline\publish\appsettings.Local.json ya completo: apunta al
         Postgres real de Supabase con el rol funbide_app (sin permisos de esquema,
         solo lectura/escritura de filas — ver la migracion que lo crea), con
         Database:AplicarMigracionesAlIniciar=false (esa PC nunca corre migraciones:
         el esquema lo migra el despliegue real en Railway).
      3. Copia dist-offline\ a una carpeta estable fuera del repo
         (%LOCALAPPDATA%\Programs\FUNBIDE), para que un "git clean" o un rebuild del
         repo no se lleve puesta la instalacion.
      4. Crea un acceso directo "FUNBIDE" en el Escritorio, apuntando a FUNBIDE.exe
         ahi adentro.

.PARAMETER DbPassword
    Contrasena del rol funbide_app en Supabase. Requerido. No se guarda en ningun
    archivo del repo (appsettings.Local.json esta en .gitignore).

.EXAMPLE
    pwsh scripts/instalar-escritorio.ps1 -DbPassword "..."
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$DbPassword
)

$ErrorActionPreference = "Stop"

$raiz = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Set-Location $raiz

Write-Host "==> Generando el paquete offline (frontend + backend + launcher)" -ForegroundColor Cyan
& (Join-Path $raiz "scripts\publicar-offline.ps1")
if ($LASTEXITCODE -ne 0) {
    throw "publicar-offline.ps1 fallo (codigo $LASTEXITCODE)"
}

$publishDir = Join-Path $raiz "dist-offline\publish"

function New-Base64Key {
    $b = New-Object byte[] 32
    (New-Object Security.Cryptography.RNGCryptoServiceProvider).GetBytes($b)
    [Convert]::ToBase64String($b)
}

Write-Host "==> Escribiendo appsettings.Local.json (conectado a la base real, sin preguntar nada)" -ForegroundColor Cyan
$config = [ordered]@{
    Logging = @{
        LogLevel = @{
            Default                     = "Information"
            "Microsoft.AspNetCore"      = "Warning"
            "Microsoft.EntityFrameworkCore" = "Warning"
        }
    }
    ConnectionStrings = @{
        FunbideDatabase = "Host=aws-0-us-east-1.pooler.supabase.com;Port=5432;Database=postgres;Username=funbide_app.ynrxsqkvkkhpkqgeawpt;Password=$DbPassword;SSL Mode=Require;Trust Server Certificate=true"
    }
    Auth = @{
        Provider = "Local"
        Local    = @{
            SigningKeyBase64 = New-Base64Key
            DuracionToken    = "12:00:00"
        }
    }
    Storage = @{
        Local = @{
            DirectorioBase  = "almacenamiento-local"
            FirmaKeyBase64  = New-Base64Key
        }
    }
    # Esta PC nunca migra el esquema ni corre en carrera contra otras instalaciones de
    # personal apuntando a la misma base: el esquema real lo migra el despliegue de
    # Railway, y el rol funbide_app no tiene privilegios de DDL de todas formas.
    Database = @{
        AplicarMigracionesAlIniciar = $false
    }
    Backup = @{
        Habilitado = $false
    }
    Cors = @{
        OrigenesPermitidos = @()
    }
}
$config | ConvertTo-Json -Depth 6 | Set-Content -Path (Join-Path $publishDir "appsettings.Local.json") -Encoding utf8

$destino = Join-Path $env:LOCALAPPDATA "Programs\FUNBIDE"
Write-Host "==> Copiando la aplicacion a $destino" -ForegroundColor Cyan
if (Test-Path $destino) {
    Remove-Item $destino -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $destino | Out-Null
Copy-Item (Join-Path $raiz "dist-offline\*") $destino -Recurse -Force

Write-Host "==> Creando el acceso directo en el Escritorio" -ForegroundColor Cyan
$shell = New-Object -ComObject WScript.Shell
$acceso = $shell.CreateShortcut((Join-Path ([Environment]::GetFolderPath("Desktop")) "FUNBIDE.lnk"))
$acceso.TargetPath = Join-Path $destino "FUNBIDE.exe"
$acceso.WorkingDirectory = $destino
$iconoOrigen = Join-Path $raiz "launcher\funbide.ico"
if (Test-Path $iconoOrigen) {
    $iconoDestino = Join-Path $destino "funbide.ico"
    Copy-Item $iconoOrigen $iconoDestino -Force
    $acceso.IconLocation = $iconoDestino
}
$acceso.Save()

Write-Host ""
Write-Host "Listo." -ForegroundColor Green
Write-Host "  Aplicacion: $destino"
Write-Host "  Acceso directo: $([Environment]::GetFolderPath('Desktop'))\FUNBIDE.lnk"
Write-Host "  Abrila desde el acceso directo del Escritorio."
