@echo off
setlocal
set "GAME=%~dp0.tools\Build-Brasas-Final\BrasasDelBosque.exe"
if not exist "%GAME%" (
    echo No se encontro el ejecutable. Consulta README.md para compilarlo desde Unity.
    pause
    exit /b 1
)
start "" "%GAME%" -screen-fullscreen 1 -window-mode borderless
