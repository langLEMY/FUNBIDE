<#
.SYNOPSIS
    Genera el paquete offline/USB de FUNBIDE (Auth:Provider=Local).

.DESCRIPTION
    Compila el frontend en modo local (VITE_AUTH_MODE=local), publica el backend y lo
    copia dentro de wwwroot, y publica el launcher de escritorio — todo en el layout que
    launcher/Program.cs espera (FUNBIDE.exe en la raíz del repo + publish/ al lado).

.EXAMPLE
    pwsh scripts/publicar-offline.ps1

.NOTES
    Después de correrlo:
      1. Copiar publish/appsettings.Local.json.example a publish/appsettings.Local.json
         y completar los CHANGE_ME (connection string del Postgres local, claves
         generadas — el propio .example trae el comando para generarlas).
      2. Copiar FUNBIDE.exe, funbide.ico y la carpeta publish/ a la USB.
      3. En la PC destino: crear la base 'funbide' en el PostgreSQL local antes de
         abrir FUNBIDE.exe (el esquema se migra solo al arrancar).
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

Invoke-Paso "Compilando frontend en modo offline (VITE_AUTH_MODE=local)" {
    Push-Location (Join-Path $raiz "frontend")
    try { npm run build:offline } finally { Pop-Location }
}

$publishDir = Join-Path $raiz "publish"
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

Invoke-Paso "Publicando backend (FUNBIDE.API)" {
    dotnet publish (Join-Path $raiz "src\FUNBIDE.API") -c Release -o $publishDir
}

Write-Host "==> Copiando el build del frontend a publish\wwwroot" -ForegroundColor Cyan
$wwwroot = Join-Path $publishDir "wwwroot"
New-Item -ItemType Directory -Force -Path $wwwroot | Out-Null
Copy-Item (Join-Path $raiz "frontend\dist\*") $wwwroot -Recurse -Force

Invoke-Paso "Publicando el launcher (FUNBIDE.exe)" {
    dotnet publish (Join-Path $raiz "launcher") -c Release -o $raiz
}

Copy-Item (Join-Path $raiz "src\FUNBIDE.API\appsettings.Local.json.example") `
    (Join-Path $publishDir "appsettings.Local.json.example") -Force

Write-Host ""
Write-Host "Paquete listo en '$raiz' (FUNBIDE.exe + publish\)." -ForegroundColor Green
Write-Host "Antes de copiarlo a la USB:" -ForegroundColor Green
Write-Host "  1. Copia publish\appsettings.Local.json.example a publish\appsettings.Local.json"
Write-Host "     y completa los CHANGE_ME (connection string del Postgres local, claves generadas)."
Write-Host "  2. Copia FUNBIDE.exe, funbide.ico y la carpeta publish\ a la USB."
Write-Host "  3. En la PC destino: crea la base 'funbide' en el PostgreSQL local antes de abrir FUNBIDE.exe."
