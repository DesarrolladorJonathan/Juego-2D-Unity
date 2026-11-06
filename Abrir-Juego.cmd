@echo off
setlocal
set "UNITY_EDITOR=%~dp0.tools\Unity-2020.3.0f1\Editor\Unity.exe"
if exist "%UNITY_EDITOR%" goto abrir
set "UNITY_EDITOR=%ProgramFiles%\Unity\Hub\Editor\2020.3.0f1\Editor\Unity.exe"
if exist "%UNITY_EDITOR%" goto abrir
echo No se encontro Unity 2020.3.0f1. Consulta README.md para instalarlo.
pause
exit /b 1

:abrir
start "" "%UNITY_EDITOR%" -projectPath "%~dp0." -openfile "%~dp0Assets\Scenes\BrasasDelBosque.unity"

