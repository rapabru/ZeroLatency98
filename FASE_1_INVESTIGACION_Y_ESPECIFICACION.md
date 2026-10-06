# FASE 1 — INVESTIGACIÓN TÉCNICA Y ESPECIFICACIÓN
## Proyecto: Desktop Performance & Low-Interference Mode (Windows 11)
**Estética:** Windows 95/98/2000 | **Plataforma Objetivo:** Windows 11 (22H2 / 23H2 / 24H2, WDDM 3.x)

---

## 1. ANÁLISIS PROFUNDO DE WINDOWS 11 DESKTOP Y SUBSISTEMAS

### 1.1. Desktop Window Manager (DWM) y Composición
- **Arquitectura:** En Windows 11, DWM (`dwm.exe`) es un componente no separable del subsistema de usuario (`user32.dll` / `win32kbase.sys` / `win32kfull.sys`). A diferencia de Windows 7 donde Aero podía desactivarse mediante `DwmEnableComposition(DWM_EC_DISABLECOMPOSITION)`, en Windows 8, 10 y 11 cualquier intento de terminar DWM provoca su reinicio inmediato por el `winlogon` o un pantallazo negro/BSOD.
- **Modelos de Presentación (Presentation Models):**
  - **Composed (Legacy Blt / Windowed Blt):** Cada ventana dibuja en su propio buffer off-screen; DWM lee todas las superficies y las compone mediante Direct3D/DirectComposition en el swapchain del desktop. Genera una copia intermedia de memoria, latencia de 1 a 2 frames y carga de GPU/VRAM bandwidth.
  - **DirectFlip:** Si una ventana cubre la pantalla o coincide exactamente con el viewport sin superposiciones opacas, DWM pasa la superficie de la aplicación directamente al scanout del controlador gráfico.
  - **Independent Flip (iFlip):** Implementado en WDDM 2.0+ y refinado en Windows 11 ("Optimizations for windowed games"). La aplicación envía sus buffers directamente al hardware de escaneo sin intervención activa del renderloop de DWM, reduciendo la latencia al nivel de Exclusive Fullscreen (FSE) manteniendo la capacidad de cambiar de ventana al instante.
- **DWM Workload en el Desktop:** DWM procesa efectos de sombreado, redondeo de esquinas (DirectComposition visual trees), efectos de blur (Mica y Acrylic material que samplean el fondo del desktop y lo procesan con compute shaders o gaussian blurs). Si la ventana está estática, DirectComposition retiene las texturas y el consumo de GPU de DWM baja a ~0.1%. Sin embargo, si hay elementos animados (loaders, widgets, barras de tareas dinámicas), DWM despierta en cada ciclo de v-blank para recomponer.

### 1.2. Shell de Windows 11 (Explorer, Taskbar, Start Menu, XAML Islands)
- **Desacoplamiento moderno:** Históricamente la barra de tareas y el Start Menu eran ventanas Win32 nativas creadas por `explorer.exe`. En Windows 11, la barra de tareas y el menú de inicio están desacoplados:
  - `explorer.exe`: Sigue siendo el host del shell, pero delega UI a islas XAML (`ShellAppRuntime.dll`, `Taskbar.dll`).
  - `StartMenuExperienceHost.exe`: Proceso aislado basado en UWP/WinUI para el menú de inicio.
  - `SearchHost.exe`: Proceso aislado para la búsqueda indexada integrada.
  - `Widgets.exe`: Host de WebView2 para el panel de Widgets.
- **Impacto:** Estos hosts mantienen procesos en suspensión o standby (`PROCESS_MODE_BACKGROUND_BEGIN`). Cuando se despiertan por telemetría o actualización de feeds, compiten por ciclos de CPU y reservas de memoria de GPU compartida.

### 1.3. Widgets, Copilot y Servicios Conectados
- **Widgets (`Widgets.exe` / `msedgewebview2.exe`):** Carga instancias embebidas de Microsoft Edge Chromium. Aun minimizado, ejecuta polling por HTTP a MSN, procesa feeds de noticias y consume entre 150 MB y 400 MB de RAM y múltiples hilos en espera de temporizadores de red.
- **Copilot (`Windows.UI.Core.TextInputHost.exe` / WebView2 / Edge):** Se integra a nivel de sistema. Añade hooks de teclado, hotkeys globales (`Win + C`) y precalienta instancias de navegador.
- **Desactivación limpia:** Se gestiona mediante políticas locales de grupo (`GPO` / Registro):
  - `HKLM\SOFTWARE\Policies\Microsoft\Dwm`: `DisallowShaking`, etc.
  - `HKLM\SOFTWARE\Policies\Microsoft\Windows\Widgets`: `AllowContentDeliveryNetwork` y `AllowPinnedEdgePages`.
  - `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced`: `TaskbarDa = 0` (oculta widgets y previene el autoinicio de su WebView2).

### 1.4. Windows Search e Indexing (`SearchIndexer.exe`)
- **Funcionamiento:** Monitorea el USN Journal de volúmenes NTFS. Monitorea cambios en disco y crea un índice B-tree en `C:\ProgramData\Microsoft\Search\Data\Applications\Windows\Windows.edb`.
- **Interferencia:** En tareas de alto I/O (compilación, extracción de assets, carga de texturas de juegos), `SearchIndexer` intercepta operaciones de archivos. Aunque Windows tiene un backoff automático cuando detecta actividad de usuario, este backoff tiene un retardo de 500ms a 2 segundos y con frecuencia no detecta cargas de GPU intensivas que no mueven el mouse.
- **Control nativo:** Puede ser pausado mediante la API COM `ISearchCatalogManager` o mediante el Service Control Manager (`net pause "wsearch"` o `ChangeServiceConfig` a Manual/Paused).

### 1.5. Notificaciones, Animaciones, Efectos Visuales y Wallpapers
- **Notificaciones (`ActionCenter.exe` / `NotificationController`):** Cada toast notification genera una interrupción de foco, despierta el compositor para fade-in/fade-out y activa llamadas de audio WASAPI si hay sonido configurado.
- **Efectos Visuales (UserPreferencesMask):**
  - Animaciones de ventana: Provocan escalado de texturas en Direct3D por parte de DWM durante ~200-300ms por apertura/cierre.
  - Transparencia y Acrílico: Requieren muestreo y blit multipass de texturas de fondo.
  - Wallpaper dinámico o Slideshow: Si el fondo rota, Explorer invalida el background brush del desktop, forzando un repaint completo de la superficie raíz de DWM. Un wallpaper estático de color sólido o bitmap sin escalar reduce el overhead de repintado del root desktop a cero.

---

### 1.6. EL PROBLEMA MULTI-MONITOR: MITO VS. REALIDAD TÉCNICA

| Escenario Multi-Monitor | Mito Popular | Realidad Técnica en Windows 11 (WDDM 3.x) | Causa Raíz / Mecanismo | Impacto Real |
|---|---|---|---|---|
| **Distintos Refresh Rates (ej. 144/240 Hz + 60 Hz)** | "Windows bloquea el monitor rápido a 60 Hz si hay algo moviéndose en el de 60 Hz". | **Parcialmente superado, pero persiste bajo ciertas condiciones.** Pre-Win10 2004 (WDDM 2.7), DWM tenía un único reloj de tick. En Win11, DWM tiene `Per-Monitor V-Sync Clocks`. Sin embargo, si una app en el monitor de 60 Hz usa aceleración por hardware con un swapchain desalineado o bloqueo de V-Sync a nivel de driver, puede causar microstutter en el de 240 Hz si la GPU satura el scheduler de hardware (HAGS). | Desalineación de colas en el GPU Command Processor y contención de buffers si la GPU entra en estados intermedios de P-State. | **Medio-Alto:** Provoca frame drop e inconsistencia en frametimes del monitor principal. |
| **Video en Pantalla Secundaria (YouTube / Twitch a 60 fps)** | "El reproductor de video come 30% de CPU y bugea el juego". | **La CPU casi no interviene, pero la GPU satura el decodificador y el memory clock.** Los navegadores usan Chromium GPU Rasterization y decodificación NVDEC/VCN. Al renderizar video, la GPU activa la cola de decodificación y de video processing (VPP), aumentando la temperatura y a veces forzando a la GPU a alternar clocks de memoria. | GPU Video Decoding Queue + DWM Desktop Composition para la ventana del browser (si no está en iFlip). | **Alto en latencia:** Causa picos de frametime (99th percentile y P0.1) en la pantalla principal. |
| **Diferentes Resoluciones y DPI (ej. 4K + 1080p)** | "La GPU sufre renderizando dos resoluciones simultáneas". | **Mito.** La GPU maneja mallas de renderizado para cualquier resolución sin penalización de scheduling. | El único costo es VRAM para el framebuffer. | **Insignificante.** |
| **DPI Mixto (ej. 150% y 100%)** | "Baja los FPS del juego principal". | **Falso para juegos a pantalla completa; Real para ventanas Win32 que cruzan monitores.** El escalado DPI virtual (DWM scaling bitmap) solo se aplica a ventanas que no son Per-Monitor DPI Aware V2. | DWM realiza un resampling bilineal de la ventana si es legacy. No afecta al juego principal que corre en su propia superficie. | **Bajo.** |
| **HDR Mixto (HDR en Monitor 1, SDR en Monitor 2)** | "Tener HDR activo en un monitor degrada el rendimiento de SDR". | **Verdadero para DWM, neutro para iFlip.** DWM debe utilizar un swapchain FP16 (scRGB / HDR10) para componer el monitor HDR y tonemapping para SDR. El espacio de color FP16 consume el doble de ancho de banda de memoria que el estándar BGRA8_UNORM. | DWM Color Pipeline: pasa de buffers de 32 bits a 64 bits por pixel en el compositor del monitor HDR. | **Bajo-Medio:** Solo relevante en GPUs con ancho de banda VRAM saturado. |
| **VRR / G-Sync / FreeSync en Multi-Monitor** | "G-Sync no funciona en multimonitor si hay ventanas en el otro". | **Verdadero si el foco no es exclusivo o si hay aceleración por hardware en segundo plano.** Si la pantalla secundaria tiene animación continua, el driver de NVIDIA/AMD puede dudar sobre qué superficie sincronizar en el scanout si G-Sync está configurado en modo "Windowed & Fullscreen". | Conflicto de V-Sync trigger entre el driver de la GPU y los eventos de presentación de DWM. | **Crítico:** Puede desactivar VRR o causar flickering severo de frecuencias. |
| **Ventanas Animadas / Overlays (Discord, Spotify, Steam)** | "Tener Discord abierto en el monitor 2 no afecta en nada". | **Falso.** Los overlays y clientes Electron dibujan mediante hardware acceleration continuamente a 60 fps. Cada frame gatilla un repintado de DWM, impidiendo que el monitor secundario duerma sus scanouts. | Timer wakeups constantes (60 ticks/s) + context switching en la GPU. | **Alto:** Impacto directo en latencia de interrupción y P0.1 frametimes. |

---

## 2. TAXONOMÍA TÉCNICA DE "DESKTOP INTERFERENCE"

Definición conceptual y formal para el proyecto:
> **Desktop Interference:** *Cualquier actividad de procesamiento, sincronización, conmutación de contexto, renderizado o E/S ejecutada por subsistemas del sistema operativo o software en segundo plano que compita con el hilo de ejecución principal o la cola de renderizado de una aplicación crítica, degradando la previsibilidad de los tiempos de entrega de cuadros (frame pacing), incrementando la latencia de entrada (input-to-display latency) o provocando transiciones de estado de energía del procesador (C-states/P-states) innecesarias.*

### 2.1. CPU Interference
1. **CPU Wakeups:** Hilos de fondo que expiran temporizadores (Timer Resolution APIs como `timeSetEvent`, `Sleep(1)`, `WaitableTimer`). Despiertan núcleos de CPU de estados de ahorro de energía profundos (C6/C7/C8), induciendo latencia de re-activación (exit latency de hasta 100-200 microsegundos) y jitter térmico.
2. **Context Switching:** Frecuencia con la que el planificador del kernel (`ntoskrnl.exe`) interrumpe el hilo de la aplicación activa para atender tareas en segundo plano. Provoca invalidación de caché L1/L2/L3 (Cache Thrashing).
3. **Background Services & Scheduled Tasks:** Tareas como diagnósticos de CEIP (`Microsoft-Windows-Customer-Experience-Improvement-Program`), indexadores y mantenedores automáticos (`taskeng.exe`).

### 2.2. GPU Interference
1. **DWM Blit & Compositor Workload:** Procesamiento de efectos visuales (Mica, sombras de ventanas, bordes redondeados). Si no se usa iFlip, DWM añade un paso de copia de memoria en GPU.
2. **Hardware-Accelerated Background Apps:** Aplicaciones basadas en Electron (Discord, Spotify, Slack, Steam Web Helper) que renderizan en background usando la GPU, ocupando colas de 3D Graphics y Video Processing.
3. **Overlays:** Overlays gráficos (Discord Overlay, GeForce Experience, Game Bar) que inyectan DLLs en el proceso de renderizado (`Present()` hooking), agregando latencia de cómputo antes del swapchain.

### 2.3. I/O Interference
1. **Disk I/O Shards:** Operaciones de escritura de logs, telemetría y bases de datos locales (SQLite de navegadores, indexación NTFS, dumps de telemetría de Windows). En unidades NVMe el impacto en throughput es mínimo, pero genera colas de I/O a nivel de controlador que compiten con cargas de texturas y shaders de juegos.

### 2.4. Network Interference
1. **Sync Engines:** OneDrive, Google Drive, Dropbox analizando hashes de archivos locales y manteniendo sockets abiertos con keep-alives constantes.
2. **Telemetry & Update Traffic:** Descarga en segundo plano de componentes de Microsoft Store y Windows Update (BITS - Background Intelligent Transfer Service).

### 2.5. Visual Interference
1. **Efectos dinámicos en pantalla secundaria:** Animaciones de navegadores, banners, GIFs o videos que fuerzan a DWM a mantener una tasa de refresco constante en el display secundario, evitando que la GPU entre en un estado de reloj estable y forzando switches de frecuencia VRAM.

---

## 3. CLASIFICACIÓN DE OPTIMIZACIONES

Esta matriz clasifica rigurosamente las intervenciones según evidencia técnica, reversibilidad y riesgo sistémico.

| Optimización | Mecanismo | Beneficio esperado | Riesgo | Reversible | Medible | Clasificación |
|---|---|---|---|---|---|---|
| **Desactivar Transparencia y Efectos Acrílicos/Mica** | `HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize` -> `EnableTransparency = 0` | Reduce shaders de composición en DWM; elimina muestreo de fondo y blurs en GPU. | Ninguno. Totalmente soportado por API de Windows. | Sí (Instantáneo vía registro / API) | Sí (GPU Engine: DWM 3D Usage baja a 0%) | **SAFE** |
| **Desactivar Animaciones del Shell** | `SystemParametersInfo(SPI_SETANIMATION)` + `UserPreferencesMask` | Cero tiempo de espera visual (apertura/cierre instantáneo); DWM no genera frames intermedios de transformación. | Ninguno. API estándar de Win32. | Sí (Instantáneo) | Sí (Elimina repintados intermedios en ETW) | **SAFE** |
| **Establecer Wallpaper Estático / Color Plano** | `SystemParametersInfo(SPI_SETDESKWALLPAPER)` | Elimina invalidación de texturas de escritorio por slideshows; reduce VRAM de DWM dedicada a background. | Ninguno. API nativa. | Sí (Instantáneo) | Sí (VRAM de `dwm.exe`) | **SAFE** |
| **Desactivar Widgets de Barra de Tareas** | Registro: `TaskbarDa = 0` en `Explorer\Advanced` | Detiene la inicialización de `Widgets.exe` y las instancias de WebView2 en segundo plano. | Ninguno. Política oficial. | Sí (Requiere reinicio de Explorer o toggling de UI) | Sí (Eliminación de 2-4 procesos `msedgewebview2.exe`) | **SAFE** |
| **Desactivar Cortana / Copilot** | GPO: `TurnOffWindowsCopilot = 1` | Previene hotkeys globales y reservas de procesos WebView2. | Ninguno. Política oficial de Windows. | Sí | Sí (Procesos en Task Manager) | **SAFE** |
| **Pausar Windows Search Indexer** | Service Control Manager (`ControlService` con `SERVICE_CONTROL_PAUSE` o stop temporal) | Cero I/O en segundo plano e interrupciones en NTFS USN journal. | Mínimo: Las búsquedas en Explorer serán más lentas mientras esté pausado. | Sí (`SERVICE_CONTROL_CONTINUE` o reinicio del servicio) | Sí (Disk Read/Write de `SearchIndexer.exe` a 0 B/s) | **SAFE** |
| **Activar Focus Assist / Do Not Disturb** | API `WpnApps` / Registro Notification Manager | Bloquea toasts emergentes, sonsonetes y desvíos de foco; evita que el compositor despierte por notificaciones. | Ninguno: No se pierden, van al centro de actividades. | Sí (Toggle nativo) | Sí (Zero interrupciones de eventos de ventana) | **SAFE** |
| **Pausar Sincronizadores Cloud (OneDrive, Dropbox)** | API oficial de pausa (ej. OneDrive command-line `/shutdown` o SIGSTOP de hilos de sync) | Detiene hashing de disco y tráfico de red en background. | Bajo: Los archivos no se subirán hasta reanudar. No corrompe archivos. | Sí (Relanzar ejecutable o resumir proceso) | Sí (I/O de disco y sockets TCP de `OneDrive.exe`) | **LOW RISK** |
| **Suspender Procesos de Navegadores en Fondo** | `NtSuspendProcess` en procesos huérfanos de Chrome/Edge que corren con `--type=utility` o background mode | Libera hilos de temporizador y ciclos de CPU; libera memoria VRAM residual. | Bajo: Pestañas secundarias o extensiones no recibirán push en tiempo real. | Sí (`NtResumeProcess`) | Sí (CPU Usage y Wakeups de navegadores a 0) | **LOW RISK** |
| **Desactivar Aceleración por Hardware en Apps Secundarias (Discord, Spotify)** | Modificación de `settings.json` o parámetros `--disable-gpu` en ejecutables secundarios | Impide que clientes secundarios compartan el command buffer de la GPU con el juego. | Bajo: Las apps de chat usarán renderizado por CPU para su UI básica. | Sí (Reversible reescribiendo config) | Sí (VRAM asignada y GPU Engine allocation) | **LOW RISK** |
| **Fijar Afinidad de CPU para Procesos Secundarios** | `SetProcessAffinityMask` para mandar apps de fondo a núcleos de menor rendimiento (E-Cores) o núcleos secundarios | Aísla los núcleos de renderizado principal (Cores 0-7 o V-Cache cores) para la tarea crítica. | Medio: Si la aplicación requiere throughput, puede ralentizarse mientras corre en fondo. | Sí (`SetProcessAffinityMask` a máscara completa) | Sí (Distribución de carga por núcleo en Performance Counters) | **LOW RISK** |
| **Ajustar Prioridad de I/O y CPU de Background** | `SetPriorityClass(PROCESS_MODE_BACKGROUND_BEGIN)` en procesos no críticos | El kernel de Windows automáticamente desprioriza I/O y memoria para estos procesos cuando el juego compite. | Muy bajo: Diseñado específicamente por Microsoft para este propósito. | Sí (`PROCESS_MODE_BACKGROUND_END`) | Sí (I/O Priority visible en Process Hacker / System Informer) | **SAFE** |
| **Desactivar Xbox Game Bar / Overlays** | Registro: `AppCaptureEnabled = 0`, `GameDVR_Enabled = 0` | Elimina hooks de presentación Direct3D en el juego; previene grabación pasiva de video. | Bajo: Se pierde la barra de captura y grabación de clips al vuelo. | Sí (Reescribiendo llaves de registro) | Sí (Elimina latencia de inyección de DLLs en Present) | **LOW RISK** |
| **Deshabilitar Temporizadores de Alta Resolución Residuales** | Inspección y cierre de procesos que fuerzan `timeBeginPeriod(1)` innecesariamente | Permite al procesador entrar en C-states más profundos cuando la tarea no demanda timing sub-milisegundo. | Medio: Aplicaciones mal programadas que dependen de `Sleep()` fijo pueden ir a diferente velocidad. | Sí | Sí (`powercfg /energy` y Timer Resolution tools) | **MEDIUM RISK** |
| **Deshabilitar SysMain (Superfetch)** | Detener servicio `SysMain` | Evita que Windows precaliente memoria en stand-by. | Medio: En PCs con SSDs NVMe rápidos el impacto es bajo, pero en sistemas con uso variado de apps puede empeorar tiempos de apertura de software. | Sí (`StartService`) | Sí (Contador de standby memory list) | **MEDIUM RISK** |
| **Desactivar Windows Defender / Real-time Protection** | Modificación de registro / Tamper Protection bypass | Reduce spikes de CPU al abrir ejecutables. | **ALTO RIESGO / NO RECOMENDADO:** Abre brechas de seguridad críticas. Windows 11 bloquea esto con Tamper Protection y genera eventos de error constantes. | No fácilmente sin deshabilitar seguridad de kernel. | Sí (CPU de `MsMpEng.exe`) | **HIGH RISK / DO NOT TOUCH** |
| **Matar o Deshabilitar DWM (`dwm.exe`)** | `taskkill /f /im dwm.exe` o inyección DLL para suspenderlo | Intento de recrear el comportamiento de WinXP/Win7 Classic Theme. | **CATASTRÓFICO:** Rompe el renderizado completo de Windows 11. Provoca crash de sesión, pantalla en negro y bucle de reinicio del shell. | No | Inusable | **DO NOT TOUCH** |
| **Limpieza Forzada de RAM ("RAM Cleaners" / `EmptyWorkingSet`)** | Llamar a `SetProcessWorkingSetSize` o `EmptyWorkingSet` en todos los procesos | Apariencia de "más memoria libre". | **CONTRA-PRODUCENTE (PLACEBO/PERJUDICIAL):** Fuerza a Windows a purgar páginas de RAM hacia el Pagefile en disco. Cuando los procesos necesitan los datos, generan millones de Hard Page Faults, congelando el sistema con I/O masivo de disco. | Reversible automáticamente cuando el sistema vuelve a paginar | Medible negativamente (Aumento crítico de Page Faults/sec) | **DO NOT TOUCH** |
| **Deshabilitar Core Parking / Ajustes Globales de BCD / HPET** | Modificar timers de BIOS (`useplatformclock`) o desactivar C-States globales | Falso mito de "menos latencia de reloj". | **ALTO RIESGO / INESTABLE:** En procesadores híbridos (Intel 12th/13th/14th Gen) o AMD Ryzen (7800X3D / 9900X), romper el planificador de energía descalibra el Thread Director, sobrecalienta la CPU y degrada el rendimiento mono-núcleo y boost clocks. | Requiere comandos BCDEDIT y reinicios | Frecuencias fijas pero con degradación de microarquitectura | **DO NOT TOUCH** |

---

## 4. ANÁLISIS DE SERVICIOS Y APLICACIONES EN SEGUNDO PLANO

Análisis minucioso de componentes candidatos para intervención, identificando su función, métodos de control seguro y protocolos de restauración.

### 4.1. Sincronizadores en la Nube
#### OneDrive
- **Función:** Sincronización continua de carpetas personales y documentos con Azure Cloud.
- **Proceso / Servicio:** `OneDrive.exe` (proceso de usuario). No es un servicio del sistema.
- **Startup / Tasks:** Carpeta `Startup` o registro `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, más tareas programadas `OneDrive Standalone Update Task`.
- **Dependencias:** Si se usa Files-On-Demand (`clfs.sys`, cloud filter driver), desacopla punteros a archivos deshidratados.
- **Qué sucede si se pausa:** Cesa la monitorización del FileSystemWatcher y la subida de datos. No hay impacto sistémico. Si el usuario intenta abrir un archivo que no está descargado localmente, obtendrá un timeout o error de I/O.
- **Riesgo:** Bajo.
- **Cómo pausarlo:** Vía CLI oficial: `OneDrive.exe /shutdown`. Alternativamente, suspender el proceso con `NtSuspendProcess` o pausar sincronización mediante su interfaz/API.
- **Cómo restaurarlo:** Ejecutar `%LOCALAPPDATA%\Microsoft\OneDrive\OneDrive.exe /background`.

#### Google Drive / Dropbox
- **Función:** Virtualización de disco y sincronización de archivos.
- **Proceso / Servicio:** `GoogleDriveFS.exe` (servicio `GoogleDriveFS`), `Dropbox.exe` (servicios `DropboxUpdate`).
- **Qué sucede si se pausa:** Google Drive monta una unidad virtual (ej. `G:`). Si se termina el proceso por la fuerza, la unidad desaparece y cualquier aplicación con rutas abiertas hacia esa unidad fallará con `ERROR_FILE_NOT_FOUND`.
- **Riesgo:** Medio si hay rutas en la unidad virtual; Bajo si se suspenden solo los hilos de sync.
- **Procedimiento Seguro:** Pausar la sincronización mediante IPC o suspender solo el proceso secundario de sync; **no desmontar el volumen virtual**.

### 4.2. Servicios en Segundo Plano de Adobe
- **Función:** Verificación de licencias, sincronización de fuentes Typekit, gestión de librerías CC y actualización de productos.
- **Procesos / Servicios:**
  - Servicios: `AdobeARMservice` (Update Service), `AGMService`, `AGSService` (Adobe Genuine Software Integrity).
  - Procesos de usuario: `Creative Cloud UI Helper.exe`, `Adobe Desktop Service.exe`, `CCXProcess.exe`, `CoreSync.exe`.
- **Startup / Tasks:** Múltiples tareas en el Programador de Tareas (`Task Scheduler`) bajo `\Adobe\`.
- **Dependencias:** Ningún componente de Windows depende de Adobe. Las suites abiertas (Photoshop, Premiere) validan tokens locales temporalmente cacheados.
- **Qué sucede si se pausa:** Cesa el polling a servidores de Adobe, se liberan hilos de red y escaneo de fuentes. Si Photoshop no está abierto, el impacto es totalmente transparente. Si Photoshop se abre durante la pausa, puede tardar más en cargar librerías o dar advertencia de fuentes desincronizadas.
- **Cómo pausarlo:** Detener servicios con SCM (`ControlService(hService, SERVICE_CONTROL_STOP)`), suspender o cerrar procesos auxiliares de usuario.
- **Cómo restaurarlo:** Iniciar servicios con SCM (`StartService`) y relanzar `Adobe Desktop Service.exe`.

### 4.3. Launchers y Clientes de Juegos (Steam, Epic, Battle.net)
- **Función:** Distribución digital, overlay, chat, descargas de parches y DRM.
- **Procesos / Servicios:** `steam.exe` (más múltiples `steamwebhelper.exe`), `EpicGamesLauncher.exe`, `Battle.net.exe`.
- **Dependencias:** Juegos que utilizan DRM de Steamworks requieren que el proceso `steam.exe` esté activo y responda en la pipe local `\\.\pipe\SteamIPC`.
- **Qué sucede si se pausa:** Si se mata o suspende `steam.exe` mientras un juego con Steamworks está activo, el juego detectará desconexión de API y puede cerrarse al instante.
- **Procedimiento Seguro:** **NUNCA cerrar el proceso principal de Steam si un juego de Steam está corriendo.** Lo que SÍ se puede optimizar es:
  - Establecer los procesos `steamwebhelper.exe` (que son instancias de Chromium consumiendo GPU y memoria) en `PROCESS_MODE_BACKGROUND_BEGIN` y restringir su afinidad a núcleos secundarios.
  - Pausar descargas automáticas durante el juego (ajuste nativo de Steam).

### 4.4. Utilidades de Periféricos y Software RGB (iCUE, Razer Synapse, Armoury Crate)
- **Función:** Control de iluminación, macros de teclado/mouse, perfiles de DPI, pantallas OLED en hardware.
- **Procesos / Servicios:**
  - iCUE: `iCUE.exe`, `Corsair.Service.exe`, `Corsair.Service.CpuIdRemote.exe`.
  - Razer: `Razer Synapse Service.exe`, `RazerCentralService.exe`.
  - ASUS: `ArmouryCrate.Service.exe`, `AsusCertService.exe`, `LightingService.exe`.
- **Comportamiento Crítico Identificado:** Son los **peores ofensores de Desktop Interference en sistemas modernos**.
  - Mantienen un polling USB constante (hasta 1000 Hz) a través de controladores HID.
  - Fuerzan el Timer Resolution del sistema a 1.0 ms o 0.5 ms de forma ininterrumpida.
  - Ejecutan consultas WMI periódicas para medir temperaturas del hardware, lo que induce bloqueos breves de kernel en el bus ACPI/SMBus.
- **Qué sucede si se pausa:** El hardware de teclado y mouse pasa a usar su memoria onboard (perfiles guardados en el chip). La iluminación puede quedar estática o volver al patrón por defecto de fábrica. Los macros complejos dependientes de software dejan de funcionar temporalmente.
- **Riesgo:** Bajo en hardware moderno con memoria onboard; Medio si el usuario depende de remapaqueos complejos por software.
- **Cómo restaurarlo:** Reinicio de los servicios correspondientes mediante el SCM.

### 4.5. Procesos en Segundo Plano de Navegadores (Chrome, Edge, Brave)
- **Función:** Mantener procesos vivos tras cerrar la ventana para recibir notificaciones web, precalentar el motor de renderizado y sincronizar extensiones.
- **Procesos:** `chrome.exe`, `msedge.exe` con flags `--type=crashpad-handler`, `--type=utility`, `--extension-process`.
- **Qué sucede si se pausa:** Si el navegador principal no está en pantalla, no hay pérdida de datos. Si se reanuda, restaura su sesión normalmente.
- **Procedimiento Seguro:** Enviar señal de terminación limpia (`WM_CLOSE`) a ventanas ocultas o suspender con `NtSuspendProcess`.

### 4.6. Windows Search Indexer (`wsearch`)
- **Proceso / Servicio:** `SearchIndexer.exe` / Servicio `WSearch`.
- **Dependencias:** El menú de inicio y Outlook dependen de este servicio para entregar resultados instantáneos.
- **Qué sucede si se pausa:** Las búsquedas dentro del shell siguen funcionando pero caen a un modo de escaneo no indexado más lento. Outlook no arroja resultados de búsqueda recientes.
- **Riesgo:** Cero riesgo de corrupción de datos. El catálogo EDB se cierra limpiamente al pausar el servicio.
- **Restauración:** `net start wsearch` o reanudación vía SCM.

---

## 5. REVISIÓN Y ANÁLISIS DE HERRAMIENTAS EXISTENTES

Evaluación del ecosistema para determinar qué tomar como referencia técnica y qué descartar.

| Herramienta | Tipo de Arquitectura | Mecanismo Central | Fortalezas Técnicas | Limitaciones / Riesgos | Veredicto para Nuestro Proyecto |
|---|---|---|---|---|---|
| **Microsoft PowerToys** | C# / C++ (WinUI 3 / Win32) | Utilidades de usuario desacopladas, hooks de teclado globales de bajo nivel. | Integración nativa con APIs de Microsoft, código limpio y abierto, respeta la seguridad del sistema. | Es pesada para ser una suite de herramientas (usa WinUI 3 y WebView2 intensivamente); su propio overhead de background es no despreciable. | **Referencia de arquitectura:** Estudiar su wrapper de hooks de Windows y APIs de gestión de ventanas. |
| **ExplorerPatcher** | C / C++ (DLL Injection) | Inyecta código en `explorer.exe` (`dxgi.dll` local hijack o runtime injection) para forzar el uso de la barra de tareas de Windows 10/Classic. | Modificación visual profunda y directa sin reemplazar el shell entero. | **Fragilidad extrema:** Con cada actualización acumulativa mensual de Windows 11 (`explorer.exe` cambia offsets internos), suele romper el login y causar bucles de reinicio de Explorer. | **Descartado para MVP:** Demasiado agresivo y riesgoso. Viola nuestro principio de estabilidad y seguridad. |
| **Windhawk** | C++ (Kernel/Userland Mod Engine) | Motor de inyección de mods en procesos del sistema mediante drivers y hooking de funciones en memoria. | Extremadamente modular, código abierto, no modifica archivos en disco (todo en RAM). | Requiere privilegios elevados y genera alarmas en soluciones de seguridad; su complejidad añade una capa de abstracción pesada. | **Referencia:** Estudiar qué funciones Win32 de DWM hookea para ajustar estilos visuales. |
| **StartAllBack / Open-Shell** | C++ (Com Inproc Server) | Reemplaza la barra de tareas y el menú de inicio renderizando controles Win32 nativos independientes de XAML. | Eficiencia extrema de CPU y memoria; emulación visual perfecta de Windows 98/2000. | StartAllBack es software propietario de pago. Open-Shell se enfoca solo en el menú inicio y no en optimización de rendimiento. | **Inspiración conceptual:** La estética retro debe lograrse mediante controles nativos ligeros, sin inyecciones destructivas. |
| **Winaero Tweaker** | C# (.NET Framework) | Modificación masiva de llaves de registro y políticas de grupo. | Cobertura masiva de opciones de configuración de Windows. | Muchas modificaciones quedan huérfanas sin rollback claro; mezcla optimizaciones útiles con placebos; no mide el impacto de lo que cambia. | **Advertencia:** Demuestra qué NO hacer: no aplicar tweaks a ciegas sin medición ni rollback automático garantizado. |
| **Process Lasso** | C++ nativo | Driver de kernel (`processgovernor.exe`) que ajusta afinidades dinámicas, I/O priorities y "ProBalance". | El estándar de oro en mitigación de contención de CPU; ultra-ligero (<10 MB RAM, 0% CPU); altamente testeado. | Interfaz compleja para el usuario común; requiere driver para ciertas funciones avanzadas. | **Excelente referencia:** Tomar su filosofía de ajuste de prioridades (`PROCESS_MODE_BACKGROUND_BEGIN`) y afinidad de cores. |
| **System Informer (ex-Process Hacker)** | C nativo | Interfaz completa sobre la Native API de Windows (`ntdll.dll`). | Capacidad quirúrgica para inspeccionar y pausar hilos, tokens, descriptores y servicios. | Herramienta de diagnóstico, no de automatización de perfiles. | **Referencia de APIs:** Adoptar su uso de `NtSuspendProcess`, `NtResumeProcess` y enumeración de threads. |
| **PresentMon / CapFrameX** | C++ / ETW Consumer | Lee eventos del kernel de gráficos (`Microsoft-Windows-DxgKrnl`) mediante ETW en tiempo real. | Capacidad indiscutible para medir frame pacing, latencia de GPU y desacoplamiento de DWM (iFlip vs Composed). | PresentMon es una herramienta de instrumentación pura. | **Piedra angular para Fase 3:** Utilizar su librería de captura para verificar el impacto real de las optimizaciones. |

---

## 6. SISTEMAS DE MEDICIÓN Y TELEMETRÍA DE RENDIMIENTO

Para satisfacer el principio: *"Medición antes que afirmaciones. No inventar mejoras."*, se evalúan las siguientes interfaces del sistema operativo:

### 6.1. Event Tracing for Windows (ETW)
- **Providers Clave:**
  - `Microsoft-Windows-Kernel-Processor-Power`: Mide transiciones de C-States, cambios de frecuencia del procesador y razones de wakeups de núcleos.
  - `Microsoft-Windows-Kernel-Process` / `Scheduler`: Registra context switches y eventos de creación de hilos.
  - `Microsoft-Windows-DxgKrnl`: Proporciona telemetría exacta de cada llamada a `Present()`, el tiempo que pasa en la cola del driver, el modelo de presentación alcanzado (Composed, DirectFlip, iFlip) y frames descartados (*dropped/stutter frames*).
- **Viabilidad:** Requiere sesión de trace elevada (Administrador). Para el MVP, abrir una sesión ETW completa puede ser pesado; para el MVP se utilizarán Performance Counters y APIs Win32 directas, reservando ETW para el benchmarking avanzado de Fase 3.

### 6.2. Windows Performance Counters (PDH API - `Pdh.dll`)
- **Métricas Confiables en Tiempo Real:**
  - `\Processor Information(_Total)\% Processor Time`: Carga global de CPU.
  - `\Processor Information(_Total)\Context Switches/sec`: Indicador directo de contención de hilos.
  - `\Processor Information(_Total)\Interrupts/sec`: Interrupciones de hardware e I/O.
  - `\GPU Engine(*)\Utilization Percentage`: Uso discriminado por motor de GPU (3D, Video Decode, Copy, DirectComposition).
  - `\Memory\Available MBytes`: Memoria física libre.
  - `\PhysicalDisk(_Total)\Disk Reads/sec` y `Disk Writes/sec`: Actividad real de almacenamiento.
- **Overhead:** Menos de 0.05% de CPU al consultar a intervalos de 1 a 2 segundos mediante la API nativa de PDH (`PdhOpenQuery`, `PdhAddCounter`, `PdhCollectQueryData`).

### 6.3. DXGI / Windows Graphics Diagnostics
- **Consultas Directas:**
  - `IDXGIFactory6::EnumAdapterByGpuPreference`: Enumeración exacta de adaptadores gráficos y sus estados de memoria local dedicada (VRAM) vs memoria compartida.
  - `QueryDisplayConfig`: Información precisa de topología multi-monitor: resolución nativa, tasa de refresco actual (micro-hercios), formato de color (RGB 8-bit, 10-bit, HDR) y estado de sincronización.

### 6.4. Service Control Manager (SCM) & ToolHelp32 API
- **Supervisión de Estados:**
  - `OpenSCManager`, `EnumServicesStatusEx`: Consulta atómica del estado de servicios en menos de 5 milisegundos sin invocar PowerShell.
  - `CreateToolhelp32Snapshot` o `NtQuerySystemInformation`: Enumeración instantánea de procesos y consumo de recursos sin el overhead de WMI.

---

## 7. ARQUITECTURA Y COMPARACIÓN DEL STACK TECNOLÓGICO

La herramienta cuyo objetivo es reducir la interferencia **no puede convertirse ella misma en un vector de interferencia**.

### Matriz Comparativa

| Criterio | C# / .NET 8 (WinForms / WPF) | C# / .NET 8 (WinUI 3) | C++ Nativo (Win32 API) | Rust (Win32 / windows-rs) | Web-based (Tauri / Electron) |
|---|---|---|---|---|---|
| **Consumo de Memoria en Reposo** | ~35 - 65 MB | ~90 - 150 MB | **< 8 - 15 MB** | **< 6 - 12 MB** | 80 MB (Tauri) / >200 MB (Electron) |
| **CPU Wakeups en Reposo** | Ocasionales (GC background pauses) | Frecuentes (XAML animation loop) | **0.0% (Bloqueo puro en `GetMessage`)** | **0.0% (Bloqueo puro en `GetMessage`)** | Continuos (Chromium IPC y V8 event loop) |
| **DWM Composition Overhead** | Bajo en WinForms; Medio en WPF (DirectX 9 pipeline) | Alto (Usa WinUI/DirectComposition intensivamente) | **Cero (Renderizado GDI/GDI+ tradicional o Direct2D ligero)** | **Cero (Win32 o Direct2D ligero)** | Alto (Requiere compositor de GPU propio) |
| **Acceso a Win32 / Native NT APIs** | Mediante P/Invoke (`DllImport`). Rápido de programar pero agrega marshalling. | Mediante C#/WinRT y P/Invoke. | **Nativo y directo (Header files de Windows SDK).** | **Nativo mediante bindings directos (`windows` crate).** | Complejo (Requiere backend IPC en Rust/Node). |
| **Facilidad para Estética Retro Win98** | Media (WinForms lo soporta nativo con estilos clásicos). | Muy difícil (WinUI 3 está diseñado para Fluent Design, bordes redondeados y Mica). | **Perfecta y trivial:** Las APIs clásicas de Win32 (`USER32`/`GDI32`) tienen los controles estándar de Windows (botones con bordes 3D, barras de progreso clásicas, Menus con relieve). | **Excelente:** Control total sobre mensajes `WM_PAINT` o controles nativos Win32. | Requiere emular mediante CSS (pesado, no nativo). |
| **Tiempo de Arranque / Huella de Distribución** | ~500ms / Single-file exe ~60MB (self-contained) | ~1.5s / Runtime de Windows App SDK necesario | **Instantáneo (<50ms) / Exe de ~1-2 MB sin dependencias** | **Instantáneo (<50ms) / Exe de ~2-4 MB sin dependencias** | Lento (>1.5s) / Requiere WebView2 o Chromium |
| **Seguridad de Memoria** | Managed (Garbage Collector) | Managed (Garbage Collector) | Manual (Requiere disciplina RAII) | **Garantizada en tiempo de compilación** | Dependiente de runtime |

### Diagnóstico y Selección
- **Descartados Inmediatamente:**
  - **Electron / Tauri:** Inaceptables. Crear una aplicación para optimizar la GPU montando un navegador web de fondo viola el propósito técnico fundamental del proyecto.
  - **WinUI 3:** Forzaría la activación de DirectComposition, XAML y componentes que queremos reducir en el sistema.
- **Finalistas:**
  1. **C++20 Nativo (Win32 / Direct2D):** Máximo control, cero dependencias, mínima huella de memoria (<10 MB), estética retro completamente nativa (usando estilos clásicos de Windows o GDI/Direct2D personalizado sin compositores pesados).
  2. **C# / .NET 8 (AOT compilado - WinForms):** Rápido de iterar, excelente soporte para GUI clásica retro nativa, pero la recolección de basura y el runtime agregan huella en memoria innecesaria.
  3. **Rust (`windows-rs`):** Excelente rendimiento y seguridad, pero el diseño de GUI nativa Win32 en Rust suele tener mayor fricción de desarrollo que en C++.
- **Veredicto:** **C++20 con Win32 API pura y wrappers RAII limpios** (o alternativamente **C# .NET 8 con NativeAOT** si se prioriza velocidad de entrega en el MVP manteniendo la GUI en WinForms clásico con `Application.SetVisualStyleState(0)` para conseguir el aspecto exacto de Windows 98/2000 sin consumir GPU).
- **Para este proyecto se recomienda formalmente:** **C++20 Win32 Nativo** para el core de optimización y motor de servicios, permitiendo una integración de latencia cero, binario único de ~1.5 MB, sin dependencias externas y con consumo de memoria menor a 10 MB.

---

## 8. SÍNTESIS TÉCNICA FORMAL: LOS 16 PUNTOS CLAVE

A continuación se da respuesta exhaustiva y estructurada a los 16 requerimientos establecidos:

### 1. Investigación Técnica (Resumen Consolidado)
Windows 11 es un sistema operativo con una arquitectura de escritorio profundamente desacoplada de la interfaz Win32 tradicional. La composición mediante DWM es obligatoria e indivisible. El shell (`explorer.exe`) ha externalizado sus partes hacia procesos UWP/WinUI y hosts de WebView2 (`msedgewebview2.exe`). La contención no se origina principalmente por falta de RAM, sino por **competencia de scheduling de hilos (CPU wakeups), contención en las colas de la GPU (especialmente en multi-monitor con aceleración por hardware activa) y ciclos de E/S de fondo (indexadores y servicios de telemetría).**

### 2. Hallazgos Importantes
- **DWM no se puede apagar:** Cualquier herramienta o tweak que prometa "apagar DWM en Windows 11" es falso o produce inestabilidad catastrófica. La única vía técnicamente válida es **minimizar el trabajo que DWM debe realizar** y garantizar que la aplicación activa alcance el modo **Independent Flip (iFlip)**.
- **El verdadero culpable del Stutter en Multi-Monitor:** No es una limitación de hardware para manejar dos pantallas, sino la **mezcla de una aplicación en iFlip (juego en pantalla 1) con una aplicación que renderiza por hardware en composición de ventana normal (navegador con video o Discord en pantalla 2)**. Esto satura el GPU scheduler y desestabiliza el sincronismo de V-Sync y VRR.
- **Timer Resolution Global:** En Windows 11 moderno, Microsoft cambió las reglas de `timeBeginPeriod`. Ya no afecta a todo el sistema globalmente como en Windows 7, salvo que la ventana tenga el foco. Sin embargo, procesos de periféricos (RGB) continúan manteniendo hilos de alta frecuencia que impiden a los núcleos entrar en estados C6/C7.

### 3. Qué Realmente Puede Reducir "Desktop Interference"
1. **Desactivar la transparencia y los materiales acrílicos/Mica de DWM:** Reduce instantáneamente el cómputo de shaders en el compositor.
2. **Desactivar animaciones del shell:** Elimina la generación de fotogramas de transición y hace que el entorno responda con inmediatez absoluta.
3. **Pausar sincronizadores de archivos (OneDrive, Dropbox) durante la sesión de trabajo:** Cero operaciones de hashing de disco y cero transferencias de red no programadas.
4. **Pausar el Windows Search Indexer (`WSearch`):** Elimina la competencia por el USN Journal y lecturas de disco.
5. **Configurar procesos secundarios en Modo Background:** Usar la API oficial de Windows `SetPriorityClass(hProcess, PROCESS_MODE_BACKGROUND_BEGIN)`, forzando al kernel a despriorizar automáticamente su memoria, su CPU y su cuota de I/O de disco.
6. **Aislar afinidades de CPU:** Reservar los núcleos de mayor rendimiento (y con mayor caché L3, por ejemplo en CPUs AMD con 3D V-Cache o núcleos P de Intel) exclusivamente para la aplicación activa, confinando Discord/navegadores a los núcleos secundarios.
7. **Detener o suspender el panel de Widgets:** Elimina 2 a 4 instancias de WebView2 activas en memoria.

### 4. Qué No Vale la Pena Tocar
- **Resoluciones mixtas y escalado DPI en monitores independientes:** Si la aplicación principal corre a pantalla completa, la diferencia de escala entre pantallas no introduce penalización de GPU apreciable.
- **Servicios de red esenciales del kernel (`Dnscache`, `Dhcp`, `LanmanWorkstation`):** Detenerlos ahorra cero ciclos medibles y rompe la conectividad de red local.
- **Prefetch / Superfetch (`SysMain`) en SSDs modernos:** Con unidades NVMe que leen a >3500 MB/s, el impacto de SysMain en I/O es transitorio (ocurre solo en el arranque) y mantener las librerías en memoria ayuda a reabrir utilidades rápidamente.
- **Fuentes tipográficas o servicios de localización:** El ahorro de recursos es infinitesimal y no medible.

### 5. Qué es Placebo (Mitos de la Comunidad de "Tweaking")
- **"RAM Cleaners" (`EmptyWorkingSet`):** **El mayor placebo y daño técnico.** Forzar el vaciado del Working Set traslada páginas de memoria activa al archivo de paginación (`pagefile.sys`). En cuanto el sistema necesita esas páginas, sufre una tormenta de *Hard Page Faults*, provocando micro-congelamientos severos en el sistema.
- **Desactivar el Pagefile:** Causa fallos inmediatos de asignación en juegos que reservan memoria virtual contigua (`VirtualAlloc` con `MEM_COMMIT`), incluso teniendo 32 GB o 64 GB de RAM libre.
- **Tweak de registro `SystemResponsiveness = 0`:** Solo aplica al programador de red multimedia heredado de Windows Vista/7; en el kernel moderno de Windows 11 no tiene efecto medible en el despacho de hilos de juegos.
- **Comandos BCDEDIT como `useplatformclock true` o deshabilitar HPET a ciegas:** Desincroniza el timer invariant TSC de la CPU, degradando el rendimiento del scheduler y produciendo stuttering.
- **Desactivar telemetría básica rompiendo servicios del sistema:** Provoca que Windows Update y los componentes de integridad entren en reintentos infinitos en segundo plano, consumiendo **más** CPU de la que intentaban ahorrar.

### 6. Modificaciones que son Seguras (SAFE)
- Modificación de valores de configuración visual documentados (`SystemParametersInfo` para animaciones, wallpapers, fuentes de iconos).
- Modificación de llaves de usuario para transparencia (`EnableTransparency = 0`).
- Ocultar Widgets y Cortana mediante políticas oficiales.
- Pausar el servicio `WSearch` usando el Service Control Manager oficial.
- Pausar aplicaciones en segundo plano invocando sus propios argumentos de línea de comandos de suspensión (`/shutdown`) o mediante `SetPriorityClass` a modo background.
- Establecer planes de energía estándar de Windows a "High Performance" o ajustar `Power Throttling` para procesos conocidos.
- Activar el modo "No molestar" (Focus Assist) para suprimir notificaciones.

### 7. Modificaciones que son Peligrosas (HIGH RISK / DO NOT TOUCH)
- Intentar matar, inyectar DLLs no verificadas o deshabilitar `dwm.exe`.
- Deshabilitar Windows Defender manipulando el registro o desactivando Tamper Protection.
- Deshabilitar servicios de infraestructura crítica: `DcomLaunch`, `RpcSs`, `EventLog`, `PlugPlay`, `CryptSvc`, `BrokerInfrastructure`.
- Eliminar o modificar componentes de Windows mediante scripts destructivos tipo "Debloat" que borran paquetes de AppX a la fuerza, rompiendo dependencias futuras de Windows Update.
- Modificar parámetros de bajo nivel de PCI-Express o temporizadores de BIOS vía software.

### 8. Arquitectura Recomendada del Sistema
El sistema se estructurará en tres capas estrictamente desacopladas:

```
+-------------------------------------------------------------+
|                     CAPA DE INTERFAZ                        |
|       Win32 Classic GUI (Estética Windows 98/2000)          |
|      - Controles nativos (User32/Gdi32, Cero GPU usage)     |
|      - Menús clásicos, Iconos retro 16x16 / 32x32           |
|      - Responsive, consumo de memoria < 10 MB               |
+-------------------------------------------------------------+
                              |
+-------------------------------------------------------------+
|                     CAPA DE CONTROL                         |
|                   Engine & Orchestrator                     |
|      - Profile Manager (Normal, Low-Interference, Max)      |
|      - Snapshot & Rollback Engine (Transaccional)           |
|      - Process & Service Supervisor (SCM & Win32 APIs)      |
|      - Display Topology Inspector (DXGI / Win32 Enum)       |
+-------------------------------------------------------------+
                              |
+-------------------------------------------------------------+
|                   CAPA DE INSTRUMENTACIÓN                   |
|                   Telemetry & Diagnostics                   |
|      - Performance Counters (PDH API: CPU, Context Switch)  |
|      - GPU Engine Polling (DWM, 3D, Video)                  |
|      - Process Enumerator (ToolHelp32 / Native NT)          |
|      - State Recorder (Snapshot JSON persistente)           |
+-------------------------------------------------------------+
```

### 9. Stack Tecnológico Recomendado
- **Lenguaje:** **C++20** (compilador MSVC 2022 o Clang-cl).
- **Subsystem:** Win32 Native (`/SUBSYSTEM:WINDOWS`).
- **Librerías del Sistema:**
  - `User32.lib`, `Gdi32.lib`, `Shell32.lib` (GUI clásica y gestión de ventanas sin dependencias externas).
  - `Advapi32.lib` (Service Control Manager, Registry).
  - `Pdh.lib` (Performance Data Helper para métricas precisas sin costo de CPU).
  - `Dxgi.lib` (Detección de topología de monitores y memoria de adaptadores).
  - `Ntdll.lib` (Llamadas opcionales seguras para gestión atómica de prioridad de procesos).
- **Serialización:** Formato JSON plano (usando librería cabecera ligera como `nlohmann/json` o un parser simple sin dependencias pesadas) para snapshots y perfiles.
- **Justificación:** Un único binario portable (`.exe`), tamaño menor a 2 MB, ejecución instantánea, sin instalación de runtimes (.NET o WebView2), cero consumo de GPU y menos de 10 MB de memoria RAM.

### 10. Sistema de Perfiles Propuesto
El motor operará en base a tres perfiles fundamentales y mutuamente excluyentes:
1. **NORMAL (Default / Baseline):**
   - Estado tal cual el usuario configuró su Windows 11.
   - La aplicación no aplica ninguna restricción ni modificación activa.
   - Sirve como ancla de referencia para comparar.
2. **LOW INTERFERENCE (Uso general optimizado / Trabajo / Multitarea silenciosa):**
   - *Visual:* Transparencia desactivada, animaciones del sistema desactivadas, wallpaper fijado a imagen estática sin rotación.
   - *Shell:* Widgets y noticias desactivados; Focus Assist / No molestar activado.
   - *I/O:* Windows Search Indexer pausado temporalmente.
   - *Procesos de fondo:* Aplicaciones cloud (OneDrive, Dropbox) puestas en pausa de sincronización. Procesos de navegadores huérfanos colocados en `PROCESS_MODE_BACKGROUND_BEGIN`.
   - *Monitores:* No modifica resoluciones ni frecuencias, pero suspende repintados innecesarios en pantallas inactivas.
3. **MAX RESPONSE (Sesión de latencia crítica / Gaming competitivo / Audio en tiempo real):**
   - Todas las medidas de *Low Interference*, más:
   - *Afinidad de CPU:* Procesos de fondo (Discord, Spotify, launchers) restringidos a núcleos secundarios o núcleos de eficiencia (E-Cores), dejando los núcleos primarios 100% aislados para la tarea en primer plano.
   - *Aceleración de GPU en apps secundarias:* Forzado a minimizar o suspender temporalmente el renderizado de ventanas secundarias no visibles.
   - *Servicios de terceros:* Pausado temporal de servicios de software periférico no esenciales (RGB, telemetría de suites de periféricos).
   - *Power Plan:* Forzado a esquema de alto rendimiento para prevenir aparcamiento de núcleos durante la sesión.

### 11. Sistema de Rollback y Snapshot (Transaccionalidad)
- **Principio Fundamental:** Ninguna configuración del sistema se altera sin haber persistido previamente el estado original exacto en un archivo de snapshot atómico (`snapshot_baseline.json`).
- **Estructura del Snapshot:**
  - Valores originales del registro (claves exactas de transparencia, efectos visuales, políticas de widgets).
  - Lista de servicios modificados y su estado previo (`SERVICE_RUNNING`, `SERVICE_PAUSED`, `SERVICE_STOPPED`) junto con su modo de inicio (`SERVICE_AUTO_START`, `SERVICE_DEMAND_START`).
  - Lista de procesos intervenidos (prioridad previa y máscara de afinidad previa de 64 bits).
  - Identificador de plan de energía activo (`GUID`).
  - Ruta y modo del wallpaper original.
- **Protocolo de Restauración ("RESTORE NORMAL"):**
  - Un único botón visible permanentemente en la interfaz.
  - Al presionarse (o automáticamente al cerrar la aplicación si se activa la opción "Restore on Exit"), el motor lee el archivo de snapshot y revierte cada cambio en orden cronológico inverso.
  - Verificación post-rollback: El sistema confirma que cada servicio y valor de registro ha retornado a su estado inicial y elimina el archivo de snapshot pendiente.
  - Protección contra fallos inesperados (Crashes / Cortes de luz): Al iniciar la aplicación, si detecta un snapshot huérfano de una sesión anterior no restaurada, alertará al usuario y ofrecerá la restauración inmediata con un clic.

### 12. Estrategia Multi-Monitor
- **Diagnóstico y Visualización:**
  - Detección precisa de cada monitor conectado mediante `EnumDisplayMonitors` y `QueryDisplayConfig`.
  - Reporte de frecuencia de refresco nativa, resolución, escala DPI, espacio de color (SDR/HDR) y estado de VRR.
- **Mitigación Activa de Interferencia:**
  - **Eliminación de dinamismo secundario:** En los modos *Low Interference* y *Max Response*, se evita cualquier animación de shell en pantallas no primarias.
  - **Manejo de ventanas no enfocadas:** Minimizar o suspender el renderloop de navegadores o reproductores en pantallas secundarias cuando se detecta una tarea prioritaria en pantalla completa, evitando que la GPU divida sus recursos de composición y previniendo el conflicto de VRR/G-Sync.

### 13. Estrategia de Benchmarking ("Medir, no Asumir")
- **Flujo de Medición:**
  1. **FASE 1 (BEFORE):** Durante una ventana de 5 segundos, la aplicación toma muestras en tiempo real usando PDH API:
     - Promedio y desviación estándar de CPU %.
     - Cantidad de Context Switches por segundo.
     - Actividad de GPU en el motor 3D y en el motor DirectComposition de DWM.
     - Lectura/Escritura de disco (KB/s).
     - Conteo de procesos activos y subprocesos en ejecución.
  2. **APPLY PROFILE:** Se ejecuta la transición de estado.
  3. **FASE 2 (AFTER):** Durante otros 5 segundos con el usuario en reposo, se toman exactamente las mismas métricas bajo idénticas condiciones.
  4. **REPORTE COMPARATIVO:**
     - Se calcula el delta porcentual y absoluto:
       - $\Delta$ Context Switches/sec (indicador clave de reducción de contención).
       - $\Delta$ GPU DirectComposition utilization (evidencia de menor carga de DWM).
       - $\Delta$ Disk I/O bytes (evidencia de mitigación de indexación/sync).
     - Si la diferencia está dentro del margen de error (< 2%), la interfaz declara explícitamente:
       > *"No measurable background improvement detected."*

### 14. Riesgos y Mitigaciones
- **Riesgo 1: Cierre anómalo del sistema con servicios pausados.**
  - *Mitigación:* Los snapshots se graban en disco en formato JSON antes de aplicar cualquier acción. Al arrancar, el programa detecta si quedó un perfil activo y ofrece restaurar el sistema al estado original de fábrica.
- **Riesgo 2: Falsos positivos de software antivirus.**
  - *Mitigación:* No se utilizarán técnicas de inyección de código (DLL injection) ni hooks de kernel. Todas las operaciones se realizan exclusivamente mediante las APIs documentadas de Win32, SCM y Registro de Windows.
- **Riesgo 3: Bloqueo de aplicaciones dependientes de servicios pausados (ej. OneDrive desincronizado temporalmente).**
  - *Mitigación:* Notificación visual clara en la UI que enumera exactamente qué servicios están en pausa y lista de exclusión ("Whitelist") inviolable.

### 15. Limitaciones Técnicas Infranqueables de Windows 11
1. **DWM es indestructible:** No existe una forma soportada ni estable de ejecutar Windows 11 en modo "GDI puro" o sin compositor como en Windows XP o 2000. DWM siempre estará presente; nuestro objetivo es llevar su costo de renderizado a ~0% manteniéndolo en reposo absoluto.
2. **Windows Defender Tamper Protection:** La protección en tiempo real de Microsoft Defender no se puede desactivar programáticamente desde aplicaciones de usuario estándar sin intervención manual del usuario en la interfaz de Windows Security, debido a las restricciones de integridad de seguridad de Windows 11.
3. **Comportamiento interno de drivers propietarios:** Las discrepancias entre drivers de NVIDIA/AMD respecto a cómo manejan monitores de diferentes Hz con aceleración de hardware no se pueden sobreescribir desde el espacio de usuario; la única solución eficaz es mitigar las causas que gatillan ese comportamiento (desactivar aceleración de GPU en apps secundarias).

---

## 16. DEFINICIÓN EXACTA DEL MVP (FASE 2 SCOPE)

Para garantizar un producto robusto, seguro y verificable, el MVP estará acotado estrictamente a las siguientes especificaciones:

### Alcance Funcional del MVP

```
+--------------------------------------------------------------------------+
|  [#] DESKTOP PERFORMANCE - LOW-INTERFERENCE MODE (Win98 Style)      [_][X]|
+--------------------------------------------------------------------------+
|  File  Profiles  Diagnostics  Help                                       |
+--------------------------------------------------------------------------+
|  SYSTEM TOPOLOGY:                                                        |
|  CPU: AMD Ryzen 7 7800X3D (8C/16T) | RAM: 32 GB (Available: 24.2 GB)    |
|  Monitors Detected: 2                                                    |
|    - Display 1 (Primary): 2560x1440 @ 240 Hz [HDR: Yes, VRR: Active]     |
|    - Display 2: 1920x1080 @ 60 Hz [HDR: No, SDR Standard]               |
+--------------------------------------------------------------------------+
|  PROFILE SELECTOR:                                                       |
|  (o) NORMAL (Default Windows)                                            |
|  ( ) LOW INTERFERENCE                                                    |
|  ( ) MAX RESPONSE                                                        |
|                                                                          |
|  [ APPLY SELECTED PROFILE ]            [ RESTORE TO NORMAL (UNDO ALL) ]  |
+--------------------------------------------------------------------------+
|  MANAGED BACKGROUND APPS (Detected):                                     |
|  [X] OneDrive (Sync Agent)             -> Action: Pause on Low/Max       |
|  [X] Discord (Hardware Accel Active)   -> Action: Lower Priority/Affinity|
|  [X] Windows Search Indexer            -> Action: Pause Service          |
|  [ ] Steam                             -> Action: NEVER TOUCH (Whitelisted|
+--------------------------------------------------------------------------+
|  BENCHMARK VERIFICATION:                                                 |
|  [ RUN BEFORE/AFTER BENCHMARK ]                                          |
|  Metrics:                                                                |
|    - Context Switches/sec: 14,200 -> 3,100 (-78.1%)                      |
|    - DWM GPU Utilization:    4.2% ->  0.1% (-97.6%)                      |
|    - Active Process Count:    245 ->   228 (-17 processes)              |
+--------------------------------------------------------------------------+
|  STATUS: System state captured in snapshot_baseline.json                 |
+--------------------------------------------------------------------------+
```

#### 1. Módulo de Diagnóstico y Detección de Estado
- Lectura de telemetría de hardware en tiempo real: Modelo de CPU, núcleos lógicos, utilización total %, memoria RAM disponible.
- Detección precisa de monitores: Resolución, tasa de refresco (Hz), estado de HDR.
- Estado actual de efectos visuales: Transparencia (Activada/Desactivada), Animaciones (Activadas/Desactivadas).
- Listado de aplicaciones y servicios supervisados conocidos que se encuentren en ejecución.

#### 2. Módulo de Snapshot y Rollback ("Restore Normal")
- Guardado atómico en disco (`snapshot_baseline.json`) de todas las variables antes de aplicar cualquier cambio.
- Botón **RESTORE TO NORMAL** operativo en todo momento.
- Capacidad de revertir el sistema al estado original tras cerrar y volver a abrir la aplicación.

#### 3. Motor de Perfiles
- **NORMAL:** Retorna el sistema al snapshot original.
- **LOW INTERFERENCE:**
  - Desactiva transparencia del sistema.
  - Desactiva animaciones de ventanas.
  - Pausa temporalmente el servicio `wsearch`.
  - Desactiva los Widgets de Windows en la barra de tareas.
  - Pausa sincronizadores de archivos detectados (OneDrive).
- **MAX RESPONSE:**
  - Aplica todo lo de *Low Interference*.
  - Coloca procesos de fondo seleccionados (Discord, Chrome/Edge huérfanos) en `PROCESS_MODE_BACKGROUND_BEGIN` y ajusta afinidad a núcleos secundarios.
  - Activa el esquema de energía de alto rendimiento.

#### 4. Background Manager Básico
- Lista con casillas de verificación para aplicaciones y servicios conocidos: OneDrive, Discord, Chrome en fondo, Adobe helpers, Windows Search.
- Lista de exclusión permanente ("Never Touch Whitelist") preconfigurada con: Audio de Windows, controladores gráficos, Steam, OBS Studio.

#### 5. Interfaz Visual Retro Windows 98/2000
- Ventana clásica con marco 3D biselado, barra de títulos tradicional, tipografía estándar (MS Sans Serif / Tahoma / Segoe UI clásica), botones rectangulares con sombra invertida al presionar.
- Consumo estricto de recursos de la aplicación: **Cero aceleración por GPU, renderizado Win32 estándar, menos de 10 MB de RAM y 0.0% de CPU en reposo.**

#### 6. Módulo de Verificación y Benchmark Básico
- Rutina integrada que captura 5 segundos de telemetría previa y 5 segundos de telemetría posterior al aplicar un perfil.
- Presentación de métricas objetivas (Context Switches/s, uso de GPU por DWM, I/O de disco).
- Si no hay cambio cuantificable, reporte honesto: *"No measurable improvement detected"*.

#### 7. Restricciones Absolutas del MVP (Seguridad y Estabilidad)
- **NO** requiere driver de kernel.
- **NO** modifica el Registro sin backup transaccional.
- **NO** altera Windows Defender ni Windows Update.
- **NO** utiliza limpiadores de RAM perjudiciales (`EmptyWorkingSet`).
- **NO** intenta apagar o inyectar código en `dwm.exe`.
- **NO** toca la configuración de arranque (BCD) ni temporizadores de BIOS.
