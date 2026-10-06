# Windows 11 Desktop Performance & Low-Interference Mode (Win98/2000 Aesthetic)

> **Mantener Windows 11 moderno y funcional, pero hacer que el desktop sea lo más estático, simple y poco intrusivo posible cuando se necesita máxima capacidad de respuesta del sistema.**

[![GitHub Release](https://img.shields.io/github/v/release/rapabru/theme-98)](https://github.com/rapabru/theme-98/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Una herramienta de precisión para Windows 11 diseñada bajo los principios de **latencia mínima, reversibilidad estricta y reducción medible de interferencias del sistema operativo**, encapsulada en una interfaz clásica inspirada en **Windows 95/98/2000**.

### ⬇️ [Descargar la última versión (Releases)](https://github.com/rapabru/theme-98/releases/latest)
- **Standalone x64 (`DesktopPerformance-v1.0.0-win-x64-standalone.zip`):** No requiere instalar runtimes ni dependencias. Descargar, descomprimir y ejecutar directamente en Windows 11.
- **Portable Edition (`DesktopPerformance-v1.0.0-portable.zip`):** Paquete ligero de ~600 KB (requiere .NET 9).

---

## 🎯 Filosofía y Objetivos Técnicos

El propósito de esta aplicación **NO es "ahorrar RAM" ni aplicar placebos agresivos**. Su objetivo es suprimir la contención silenciosa del sistema que degrada el rendimiento en tareas críticas de latencia (gaming competitivo, producción de audio en tiempo real, renderizado intensivo):

- **Reducción de DWM Workload:** Eliminar la composición innecesaria, shaders de desenfoque (Mica/Acrylic) y animaciones de ventana.
- **Mitigación de CPU Wakeups:** Reducir temporizadores de alta frecuencia en segundo plano que despiertan núcleos de estados C6/C7.
- **Aislamiento Multi-Monitor:** Evitar que ventanas secundarias aceleradas por hardware generen conflictos de reloj de refresco o desincronización de VRR/G-Sync con el monitor principal.
- **Supresión de I/O de Fondo:** Pausar ordenadamente indexadores (Windows Search) y motores de sincronización (OneDrive).
- **Cero Placebos:** Sin pseudo-limpiadores de memoria (`EmptyWorkingSet`), sin romper Windows Defender, sin scripts de "debloat" destructivos.
- **Huella de la Propia Aplicación:** Menos de 10 MB de RAM, 0% de uso de CPU en reposo y cero aceleración por GPU.

---

## 🏛️ Arquitectura del Sistema

```
+-------------------------------------------------------------+
|                     CAPA DE INTERFAZ                        |
|       Win32 Classic GUI (Estética Windows 98/2000)          |
|      - Controles nativos (User32/Gdi32, Cero GPU usage)     |
|      - Menús clásicos, relieve 3D, consumo < 10 MB RAM      |
+-------------------------------------------------------------+
                              |
+-------------------------------------------------------------+
|                     CAPA DE CONTROL                         |
|                   Engine & Orchestrator                     |
|      - Profile Manager (Normal, Low-Interference, Max)      |
|      - Snapshot & Rollback Engine (Transaccional en JSON)   |
|      - Process & Service Supervisor (SCM & Win32 APIs)      |
|      - Display Topology Inspector (DXGI / Win32 Enum)       |
+-------------------------------------------------------------+
                              |
+-------------------------------------------------------------+
|                   CAPA DE INSTRUMENTACIÓN                   |
|                   Telemetry & Diagnostics                   |
|      - Performance Counters (PDH API: Context Switches/s)   |
|      - GPU Engine Polling (DWM DirectComposition, 3D)       |
|      - Process Enumerator (ToolHelp32 / Native NT)          |
|      - Benchmark Engine (Before vs After Verification)      |
+-------------------------------------------------------------+
```

---

## 🚦 Clasificación de Intervenciones

Todas las optimizaciones del proyecto siguen una clasificación técnica verificada:

| Categoría | Modificaciones Incluidas | Nivel de Riesgo |
|---|---|---|
| **SAFE** | Transparencia OFF, Animaciones OFF, Wallpaper estático, Ocultar Widgets, Pausar Windows Search Indexer, Prioridad `PROCESS_MODE_BACKGROUND_BEGIN`. | Ninguno. Reversible y soportado por APIs oficiales de Microsoft. |
| **LOW RISK** | Pausar OneDrive/Dropbox, desactivar aceleración de GPU en apps de chat secundarias, aislar afinidad de CPU para procesos de fondo. | Bajo. Reversible al restaurar el perfil. |
| **MEDIUM RISK** | Supresión temporal de servicios de periféricos/RGB (iCUE, Razer Synapse) que bloquean timer resolutions. | Medio. El hardware opera con sus perfiles de memoria onboard. |
| **DO NOT TOUCH** | Matar DWM (`dwm.exe`), "RAM cleaners" (`EmptyWorkingSet`), desactivar Defender, tocar BCD/HPET/Core Parking global. | **Prohibido.** Inestable, destructivo o contraproducente. |

---

## 📊 Medición y Benchmarking

El sistema incorpora un motor de telemetría de rendimiento basado en la API nativa **PDH (Performance Data Helper)** de Windows:

- **Context Switches/sec:** Mide la contención del planificador del procesador y la alternancia de hilos.
- **GPU Engine (DWM DirectComposition):** Evalúa el porcentaje real de GPU dedicado al compositor de ventanas.
- **Disk I/O Bytes/sec:** Supervisa la actividad de almacenamiento del indexador y procesos en segundo plano.
- **CPU Time %:** Carga global de procesamiento.

> **Principio de Honestidad:** Si una modificación no produce un cambio medible superior al 2%, el sistema lo reportará con total transparencia: *"No measurable improvement detected"*.

---

## 🗂️ Estructura del Repositorio

- [`FASE_1_INVESTIGACION_Y_ESPECIFICACION.md`](FASE_1_INVESTIGACION_Y_ESPECIFICACION.md): Documento maestro de investigación técnica, taxonomía de interferencias, mitos de multi-monitor y especificación del MVP.
- `src/`: *(Fase 2)* Código fuente en C++20 Win32 nativo.
- `include/`: *(Fase 2)* Cabeceras de orquestación, telemetría y UI.
- `docs/`: Especificaciones complementarias y esquemas de snapshot.

---

## 🛡️ Principios Inviolables de Desarrollo

1. **Seguridad antes que rendimiento.**
2. **Reversibilidad antes que agresividad (Rollback atómico garantizado).**
3. **Medición antes que afirmaciones (Telemetry-driven).**
4. **Cero placebos (Sin RAM cleaners, sin tweaks obsoletos de registro).**
5. **No matar procesos indiscriminadamente ni romper componentes de Windows.**
6. **La herramienta debe consumir menos de 10 MB de RAM y 0% de CPU en reposo.**

---

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Consulta el archivo [LICENSE](LICENSE) para más detalles.
