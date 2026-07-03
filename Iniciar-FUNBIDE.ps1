# Levanta el backend (.NET API) y el frontend (Vite) de FUNBIDE, cada uno en su
# propia ventana de PowerShell, y abre el navegador cuando el frontend esta listo.

$root = $PSScriptRoot
$apiPath = Join-Path $root "src\FUNBIDE.API"
$frontendPath = Join-Path $root "frontend"

Write-Host "Iniciando backend (API) en http://localhost:5080..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$apiPath'; dotnet run --launch-profile http"

Start-Sleep -Seconds 5

Write-Host "Iniciando frontend en http://localhost:5173..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$frontendPath'; npm run dev"

Start-Sleep -Seconds 4

Write-Host "Abriendo el navegador..." -ForegroundColor Cyan
Start-Process "http://localhost:5173"

Write-Host ""
Write-Host "FUNBIDE esta iniciando. Deja abiertas las dos ventanas de PowerShell" -ForegroundColor Yellow
Write-Host "que se abrieron (backend y frontend) mientras trabajas." -ForegroundColor Yellow
