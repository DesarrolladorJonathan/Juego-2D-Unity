# Brasas del Bosque

Un stage de plataformas y combate 2D: recorre el poblado, sube a las copas del bosque, cruza el claro roto y derrota al Guardian de la Brasa para abrir el ultimo faro.

## Jugar en Windows

Haz doble clic en **`Jugar.cmd`** para abrir la version compilada en pantalla completa. El ejecutable y sus archivos estan en `.tools/Build-Brasas-Final/`; conserva toda esa carpeta para copiar el juego a otro equipo. `.tools/` es local y no se incluye en Git.

Pulsa **Enter** en la portada para comenzar. **Alt + F4** cierra el juego.

| Accion | Controles |
| --- | --- |
| Caminar | A / D o flechas |
| Saltar y doble salto | Espacio o C; soltar antes reduce la altura |
| Dash | X o Shift; WASD / flechas eligen la direccion, sin direccion usa hacia donde miras |
| Atacar | Z, J o clic izquierdo; encadena tres ataques para un golpe fuerte |
| Ataque vertical | Arriba / abajo + ataque |
| Rebote sobre enemigos | Abajo + ataque en el aire |
| Devolver brasas del jefe | Golpearlas con la espada |
| Pausa / continuar | Escape |
| Volver al ultimo faro | R durante la partida |
| Reiniciar el stage | R en pausa o en la pantalla de victoria |
| Silenciar / activar sonido | M |

## Contenido del stage

- Un recorrido de 162 unidades con seis islas y 17 plataformas; rutas altas opcionales.
- Slimes, esqueletos y murcielagos: patrulla, persecucion, contacto y ataques anunciados.
- Un jefe con cargas y proyectiles que se pueden devolver. Al reaparecer, recupera su vida.
- Cinco puntos de vida, invulnerabilidad breve al recibir dano y durante el dash.
- Cuatro faros que guardan el punto de retorno y recuperan la vida al activarlos.
- Monedas, recompensas por derrotar enemigos y tres reliquias opcionales. Cada 20 monedas recuperas un punto de vida.
- Los coleccionables y enemigos derrotados se conservan al caer; reiniciar el stage restablece todo.
- Camara suave con parallax, estelas de espada, particulas, indicadores, musica y efectos sintetizados.
- Portada, pausa, estadisticas y victoria. No hay guardado entre sesiones.

## Abrir y editar en Unity

Usa **Unity 2020.3.0f1** con una licencia activa. `Abrir-Juego.cmd` busca esa version en `.tools/Unity-2020.3.0f1` o en la instalacion habitual de Unity Hub.

La escena jugable es **`Assets/Scenes/BrasasDelBosque.unity`**, habilitada en Build Settings. El escenario, los enemigos y los objetos estan guardados como objetos editables en la jerarquia. `SampleScene.unity` conserva el escenario original del tutorial como referencia; el controlador de jugador compartido fue actualizado.

Desde el menu **Brasas** del editor:

- **Crear o reconstruir el stage** regenera `BrasasDelBosque.unity` desde el diseno en `StageBuild.cs`. Sobrescribe los cambios manuales de esa escena, por lo que debes guardarlos aparte antes de usarlo.
- **Compilar Windows** valida la escena y produce `.tools/Build-Brasas-Final/BrasasDelBosque.exe`.

La version exacta del editor se puede descargar desde [Unity](https://unity.com/releases/editor/whats-new/2020.3.0f1). Si el gestor rechaza todos los paquetes por `failed entitlement check`, activa una licencia valida en Unity Hub y reinicia el editor.

## Codigo principal

- `Assets/Script/PlayerController.cs`: movimiento con aceleracion, coyote time, salto almacenado, doble salto, dash, ataques direccionales y salud.
- `Assets/Script/Stage/StageDirector.cs`: partida, HUD, puntos de control, coleccionables, sonido y victoria.
- `Assets/Script/Stage/StageEnemy.cs`: enemigos y guardian.
- `Assets/Script/Stage/StageCamera.cs`: seguimiento y fondos en parallax.
- `Assets/Script/Stage/StagePickup.cs`, `StageProjectile.cs`, `StageSlash.cs`, `StageParticle.cs`: objetos, proyectiles y efectos.
- `Assets/Editor/StageBuild.cs`: generacion de escena, validacion y compilacion.
- `Assets/Editor/StagePlayVerification.cs`: pruebas de comportamiento en Play Mode.

## Verificacion

La verificacion en Play Mode comprueba la introduccion, salto fisico, doble salto, dash y recarga, dano, invulnerabilidad, reaparicion con coleccionables, impacto real de espada, muerte de enemigos, reinicio del jefe, apertura del portal y victoria. Las pruebas usan una sesion aislada del editor.

Para ejecutarlas con el editor cerrado, desde la raiz del repositorio:

```powershell
& '.\.tools\Unity-2020.3.0f1\Editor\Unity.exe' -batchmode -projectPath $PWD.Path -executeMethod StagePlayVerification.Run -logFile "$PWD\.tools\stage-verify.log"
```

El editor sale con codigo 0 si pasan o 1 ante un fallo. No agregues `-quit`: la prueba sale por si misma al finalizar Play Mode. Resultado resumido: `.tools/stage-verification.txt`.

El primer proyecto incluia `Library` y otros archivos generados en Git. No es necesario borrarlos para ejecutar el stage; Unity reconstruye su cache cuando corresponde.
