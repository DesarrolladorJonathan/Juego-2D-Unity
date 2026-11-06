# Brasas del Bosque

Juego de plataformas 2D con un solo nivel. Explora el bosque, recoge monedas y derrota al jefe para llegar al último faro.

## Jugar

1. Haz doble clic en `Jugar.cmd`.
2. Pulsa **Enter** para comenzar.
3. Usa **Alt + F4** para salir.

Si falta el ejecutable, sigue los pasos de instalación y compilación de abajo.

| Acción | Tecla |
| --- | --- |
| Moverse | A / D o flechas |
| Saltar / doble salto | Espacio |
| Dash | X + dirección |
| Atacar | Z o clic izquierdo |
| Atacar arriba / abajo | Flecha arriba / abajo + Z |
| Pausa | Escape |
| Volver al último faro | R durante la partida |
| Silenciar | M |

Los faros guardan tu punto de retorno y recuperan tu vida. Cada 20 monedas recuperas un punto de vida. El progreso no se guarda al cerrar el juego.

## Instalar y abrir el proyecto

1. Instala **Unity Hub** y activa una licencia válida.
2. Instala [Unity 2020.3.0f1](https://unity.com/releases/editor/whats-new/2020.3.0f1).
3. Agrega la carpeta del repositorio a Unity Hub y ábrela con esa versión.
4. Espera a que Unity importe los archivos.
5. Abre `Assets/Scenes/BrasasDelBosque.unity` y pulsa **Play**.

Si Unity ya está instalado en la ubicación habitual, también puedes usar `Abrir-Juego.cmd`.

## Modificar el juego

Puedes mover plataformas, enemigos y monedas desde la jerarquía de la escena. Guarda con **Ctrl + S**.

| Quiero cambiar… | Archivo |
| --- | --- |
| Movimiento, salto, ataques y vida | `Assets/Script/PlayerController.cs` |
| Comportamiento de enemigos y jefe | `Assets/Script/Stage/StageEnemy.cs` |
| Reglas, interfaz y puntos de control | `Assets/Script/Stage/StageDirector.cs` |
| Diseño inicial del mapa | `Assets/Editor/StageBuild.cs` |

Si modificas el diseño en `StageBuild.cs`, usa **Brasas → Crear o reconstruir el stage**. Esto reemplaza los cambios manuales de la escena.

## Crear el ejecutable

1. Guarda la escena en Unity.
2. Selecciona **Brasas → Compilar Windows**.
3. Al terminar, abre `Jugar.cmd`.

El juego se genera en `.tools/Build-Brasas-Final/`. Para compartirlo, copia **toda esa carpeta**.

Git conserva el código, las escenas y los recursos. Las instalaciones locales, cachés y ejecutables se excluyen con `.gitignore`, por lo que debes compilar el juego después de clonar el repositorio.
