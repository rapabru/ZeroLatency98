# FASE 3 — ESPECIFICACIÓN Y DISEÑO CONCEPTUAL
## Proyecto: Desktop Performance & Low-Interference Mode (Windows 11)

Esta etapa amplía las capacidades del MVP hacia una suite modular avanzada de optimización, diagnóstico y automatización inteligente sin comprometer los principios de estabilidad, reversibilidad y bajo consumo de recursos.

---

## 1. MÓDULOS DE LA FASE 3

### 1.1. Diagnóstico Humano: "Why Is My Desktop Busy?"
- **Objetivo:** Responder a la pregunta del usuario sobre qué componente del sistema operativo o software secundario está compitiendo por recursos en este instante, expresándolo en lenguaje natural y claro.
- **Detecciones Clave:**
  - *GPU Contention:* Identificar qué ventana secundaria fuera de foco está consumiendo ancho de banda del motor 3D o Video Decode de la GPU (ej. pestañas de YouTube/Twitch en navegadores, clientes de Discord o Spotify).
  - *I/O Contention:* Identificar qué proceso está saturando el almacenamiento con lecturas o escrituras continuas (ej. indexación de Windows Search, escaneo de OneDrive, descargas de Steam o parches de Windows Update).
  - *Timer Wakeups & CPU Jitter:* Detectar aplicaciones que fuerzan temporizadores sub-milisegundo o mantienen hilos activos en bucles de polling (ej. suites RGB iCUE, Armoury Crate).
  - *Multi-Monitor Refresh Lock:* Detectar si hay un reproductor de video en un monitor secundario de 60 Hz mientras el monitor principal de 144/240 Hz experimenta caídas de presentación.

### 1.2. Detección Automática de Juegos y Procesos Críticos (`AutoGameDetector`)
- **Objetivo:** Transición desatendida entre el perfil `NORMAL` y un perfil optimizado específico al iniciar un juego, con restauración automática al cerrar.
- **Flujo Operativo:**
  1. El motor monitorea la creación de procesos mediante `ManagementEventWatcher` (WMI `__InstanceCreationEvent`) o polling ligero Win32 (intervalo de 1.5s, 0% CPU).
  2. Al detectar la ejecución de un proceso registrado (ej. `cs2.exe`, `valorant.exe`, `r5apex.exe`):
     - Guarda el snapshot del estado actual (`snapshot_baseline.json`).
     - Activa automáticamente el perfil asignado al juego.
     - Entra en estado de supervisión activa.
  3. Al terminar el proceso del juego:
     - Ejecuta la restauración atómica a `NORMAL`.
     - Registra el evento en el journal.
- **Modos de Operación:** `AUTO`, `MANUAL`, `DISABLED`.

### 1.3. Smart Profiles (Perfiles Específicos por Carga de Trabajo)
Biblioteca extensible de perfiles preconfigurados:
- **CS2 / Competitive Gaming:** Prioridad máxima en aislamiento de CPU, supresión total de animaciones y transparencia, confinamiento de Discord/Steam Web Helper a núcleos secundarios, esquema de energía High Performance.
- **Streaming & OBS:** Preserva OBS Studio y audio en máxima prioridad; aísla capturadoras y navegadores de chat a hilos secundarios sin suspender conexiones de red.
- **Video Editing / 3D Rendering (Premiere, Blender):** Mantiene servicios de Adobe e indexación activa en discos de almacenamiento, optimiza el Working Set de memoria y evita suspensión de aceleración de GPU.
- **General Work & Silence:** Minimiza el consumo de batería y ventiladores, desactivando efectos innecesarios sin alterar afinidades de CPU.

### 1.4. Base de Datos Local de Procesos y Servicios
Catálogo local estructurado en JSON con clasificación técnica rigurosa:
- Categorías: `SAFE`, `LOW_RISK`, `MEDIUM_RISK`, `HIGH_RISK`, `DO_NOT_TOUCH`.
- Metadatos: Nombre de proceso, servicio asociado, proveedor, explicación técnica de qué sucede si se pausa y cómo restaurarlo.

### 1.5. Sistema de Importación y Exportación Portable
- Exportación en formato JSON estándar:
  - Perfiles personalizados creados por el usuario.
  - Informes y comparativas de benchmarking BEFORE/AFTER.
  - Historial de diagnósticos de interferencia.
  - Snapshots de configuración.
