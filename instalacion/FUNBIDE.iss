; Instalador profesional de FUNBIDE (escritorio Windows).
;
; No lo compiles a mano con datos a medio generar: usa
; scripts\generar-instalador.ps1 -DbPassword "..." desde la raiz del repo, que
; primero arma dist-offline\ (frontend + backend + launcher, ya conectado al
; Postgres real de Supabase con el rol acotado funbide_app) y despues llama a
; ISCC.exe sobre este archivo. Este .iss por si solo asume que dist-offline\ ya
; existe y esta completo.
;
; Requiere Inno Setup 6 (https://jrsoftware.org/isinfo.php).

; La version se lee de version.txt en la raiz del repo (fuente unica compartida con
; launcher/FUNBIDE.Launcher.csproj, que la lee via MSBuild) -- antes cada uno tenia su
; propio numero hardcodeado y se desincronizaban. Ver launcher/Actualizacion/ServicioActualizacion.cs.
#define MyAppName "FUNBIDE"
#define MyAppVersion Trim(FileRead(FileOpen("..\version.txt")))
#define MyAppPublisher "FUNBIDE"
#define MyAppExeName "FUNBIDE.exe"
#define SourceDir "..\dist-offline"

[Setup]
; Mismo AppId que la 1.0: Inno Setup lo trata como una actualizacion en el mismo
; lugar (reemplaza los archivos e icono existentes) en vez de instalar aparte.
AppId={{7C9E6A3F-2B7B-4B29-9E9B-9C6C6F0F6B3A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\FUNBIDE
DefaultGroupName=FUNBIDE
DisableProgramGroupPage=yes
OutputBaseFilename=FUNBIDE-{#MyAppVersion}-Setup-x64
OutputDir=Output
SetupIconFile=..\launcher\funbide.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; Instala en el perfil del usuario actual (localappdata), no en Archivos de
; Programa del sistema: no requiere permisos de administrador, igual que el
; flujo anterior (instalar-escritorio.ps1) que copiaba a %LOCALAPPDATA%.
PrivilegesRequired=lowest
WizardStyle=modern

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\FUNBIDE"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Desinstalar FUNBIDE"; Filename: "{uninstallexe}"
Name: "{userdesktop}\FUNBIDE"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Crear un acceso directo en el Escritorio"; GroupDescription: "Accesos directos:"; Flags: checkedonce

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Abrir FUNBIDE"; Flags: nowait postinstall skipifsilent

[Code]
// Aviso temprano si falta el runtime de WebView2 (Evergreen). No lo instala
// solo (evita depender de un plugin de descarga externo a Inno Setup) — el
// propio FUNBIDE.exe ya avisa con un enlace de descarga si falta al abrirlo,
// esto solo adelanta el aviso antes de terminar la instalacion.
function EstaWebView2Instalado(): Boolean;
var
  Version: String;
begin
  Result :=
    RegQueryStringValue(HKLM64, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version) or
    RegQueryStringValue(HKLM64, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version) or
    RegQueryStringValue(HKCU, 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', Version);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep = ssPostInstall) and not EstaWebView2Instalado() then
  begin
    MsgBox(
      'FUNBIDE necesita el runtime "Microsoft Edge WebView2" para funcionar y no se ' + #13#10 +
      'detecto instalado en esta PC. Windows 11 ya lo trae; en Windows 10 puede hacer ' + #13#10 +
      'falta instalarlo aparte desde:' + #13#10#13#10 +
      'https://go.microsoft.com/fwlink/p/?LinkId=2124703' + #13#10#13#10 +
      'FUNBIDE tambien te avisara de esto la primera vez que lo abras.',
      mbInformation, MB_OK);
  end;
end;
