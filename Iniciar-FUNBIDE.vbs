' Lanza FUNBIDE sin mostrar ninguna ventana (ni de consola ni de PowerShell).
Set shell = CreateObject("WScript.Shell")
scriptDir = CreateObject("Scripting.FileSystemObject").GetParentFolderName(WScript.ScriptFullName)
ps1 = """" & scriptDir & "\Iniciar-FUNBIDE-Silencioso.ps1"""
shell.Run "powershell.exe -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File " & ps1, 0, False
