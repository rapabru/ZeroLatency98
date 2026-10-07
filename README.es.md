# Windows 11 Desktop Performance & Low-Interference Mode (Estética Win98/2000)

> **Mantener Windows 11 moderno y funcional, pero hacer que el desktop sea lo más estático, simple y poco intrusivo posible cuando se necesita máxima capacidad de respuesta del sistema.**

[![GitHub Release](https://img.shields.io/github/v/release/rapabru/theme-98)](https://github.com/rapabru/theme-98/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Tests](https://img.shields.io/badge/Tests-21%2F21%20Passing-brightgreen.svg)]()
[![Platform: Windows 11](https://img.shields.io/badge/Platform-Windows%2011%20x64-blue.svg)]()

🌐 **[Read this documentation in English (README.md)](README.md)**

Una herramienta de precisión para Windows 11 diseñada bajo los principios de **latencia mínima, reversibilidad estricta y reducción medible de interferencias del sistema operativo**, encapsulada en una interfaz clásica y ligera inspirada en **Windows 95/98/2000**.

---

### ⬇️ [Descargar la última versión (Releases)](https://github.com/rapabru/theme-98/releases/latest)

- **Standalone x64 (`DesktopPerformance-v1.3.0-win-x64-standalone.zip`):** Un único archivo ejecutable autónomo. No requiere instalar runtimes ni dependencias. Descargar, descomprimir y ejecutar directamente en Windows 11 x64.
- **Portátil Ligero (`DesktopPerformance-v1.3.0-portable.zip`):** Paquete ultraligero (requiere .NET 9 Desktop Runtime instalado).

---

## 📸 Galería de Interfaz

### Administrador de Procesos de Fondo y Ordenamiento por Columnas
La interfaz permite pausar, reanudar y asignar prioridad de fondo a procesos pesados con ordenamiento interactivo en todas sus columnas (`▲` / `▼`):

![Administrador de Fondo](docs/screenshots/01_background_manager.png)
*Vista en ejecución: Detección y ordenamiento interactivo de procesos con estadísticas de consumo, threads y memoria en tiempo real.*

---

### Módulos del Sistema (7 Pestañas Especializadas)

| Pestaña | Captura de Pantalla | Descripción |
|---|---|---|
| **1. Topología del Sistema** | ![Topología](docs/screenshots/tab_es_0_system_topology.png) | Inspección de monitores, resolución, tasa de refresco (Hz), DPI y estado de composición DWM. |
| **2. Perfiles y Optimización** | ![Perfiles](docs/screenshots/tab_es_1_profiles_optimization.png) | Conmutación instantánea entre perfiles *Normal*, *Baja Interferencia* y *Máxima Respuesta*. |
| **3. Administrador de Fondo** | ![Administrador](docs/screenshots/tab_es_2_background_manager.png) | Control granular de aplicaciones secundarias (Chrome, Discord, Steam, Spotify, OneDrive). |
| **4. Verificación y Benchmark** | ![Benchmark](docs/screenshots/tab_es_3_verification_benchmark.png) | Medición Trilateral automatizada (Antes vs Después vs Delta) con contadores de rendimiento PDH. |
| **5. Diagnósticos y Perfiles Inteligentes** | ![Diagnósticos](docs/screenshots/tab_es_4_diagnostics_smart_profiles.png) | Detección de procesos intrusivos ("¿Por qué está ocupado el sistema?") y auto-activación por juegos. |
| **6. Estudio Multi-Monitor & VRR** | ![Multi-Monitor](docs/screenshots/tab_es_5_multi_monitor_vrr.png) | Prevención de desincronización de G-Sync/FreeSync y microstuttering por aceleración en pantallas secundarias. |
| **7. Shell Retro y Temas** | ![Temas Retro](docs/screenshots/tab_es_6_retro_shell_themes.png) | Temas retro de baja carga gráfica (Classic 98, Charcoal 2000, Teal Workbench, Amber Terminal, High Contrast). |

---

## 🌐 Soporte Multilingüe Integrado (Español / Inglés)

El programa incluye soporte completo para **Inglés y Español** en un único ejecutable:
- Se cambia instantáneamente en caliente desde la barra de menú: **Idioma / Language** ➔ **English (US)** / **Español**.
- Detecta automáticamente el idioma de tu sistema operativo al iniciar por primera vez.
- Guarda tu preferencia en `%LOCALAPPDATA%\DesktopPerformance98\language.txt`.
- No requiere reiniciar la aplicación ni descargar paquetes adicionales.

---

## 🎯 Filosofía y Principios Técnicos

El propósito de esta aplicación **NO es "ahorrar RAM" ni aplicar placebos agresivos**. Su objetivo es suprimir la contención silenciosa del sistema operativo que degrada la estabilidad de fotogramas y la latencia en tareas de alta exigencia (gaming competitivo, producción de audio digital DAW, streaming y renderizado):

- **Reducción de DWM Workload:** Desactiva efectos de transparencia (Mica/Acrylic) y animaciones innecesarias de ventanas.
- **Mitigación de CPU Wakeups:** Reduce la actividad de temporizadores y cambios de contexto innecesarios en segundo plano.
- **Aislamiento Multi-Monitor:** Evita que navegadores y reproductores en monitores secundarios degraden el refresco variable (VRR/G-Sync) del monitor principal.
- **Supresión de I/O de Fondo:** Pausa ordenadamente indexadores (Windows Search) y sincronizadores (OneDrive).
- **Cero Placebos:** Sin pseudo-limpiadores de memoria (`EmptyWorkingSet`), sin alterar Windows Defender, sin scripts de "debloat" destructivos.
- **Huella Mínima de la Herramienta:** Menos de 10 MB de memoria RAM, 0% de uso de CPU en reposo y cero consumo de GPU (GDI puro).

---

## 🚦 Clasificación de Intervenciones y Seguridad

| Categoría | Modificaciones Incluidas | Nivel de Riesgo |
|---|---|---|
| **SAFE** | Transparencia OFF, Animaciones OFF, Wallpaper estático, Ocultar Widgets, Pausar Windows Search Indexer, Prioridad `PROCESS_MODE_BACKGROUND_BEGIN`. | Ninguno. Totalmente reversible y soportado por APIs nativas de Microsoft. |
| **LOW RISK** | Pausar OneDrive/Dropbox, aislar afinidad de CPU para procesos de fondo, suspender apps de chat secundarias. | Bajo. Totalmente reversible al restaurar el perfil o al salir de la aplicación. |
| **MEDIUM RISK** | Supresión temporal de suites RGB/periféricos pesadas que bloquean temporizadores del kernel. | Medio. El hardware opera con sus perfiles de memoria onboard. |
| **DO NOT TOUCH** | Matar `dwm.exe`, "RAM cleaners" destructivos, desactivar Defender, tocar BCD/HPET/Core Parking global. | **Prohibido.** Inestable o contraproducente. |

---

## 📊 Medición y Verificación Objetiva

La aplicación incluye un motor de telemetría de rendimiento basado en la API nativa **PDH (Performance Data Helper)** de Windows:

- **Context Switches/sec:** Mide la contención del planificador del procesador y la alternancia de hilos.
- **GPU Engine (DWM DirectComposition):** Evalúa el porcentaje real de GPU dedicado al compositor de ventanas.
- **Disk I/O Bytes/sec:** Supervisa la actividad de almacenamiento del indexador y procesos en segundo plano.
- **CPU Time %:** Carga global de procesamiento.

> **Principio de Honestidad:** Si una optimización produce un delta inferior al 2%, el sistema lo reportará con total transparencia: *"No measurable improvement detected"*.

---

## 🚀 Inicio Rápido y Compilación

### Ejecución Directa
1. Descarga el archivo zip desde [Releases](https://github.com/rapabru/theme-98/releases/latest).
2. Extrae el contenido en cualquier carpeta.
3. Ejecuta `DesktopPerformance.exe`.

### Compilación desde el Código Fuente
Requiere [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0):

```powershell
# Clonar el repositorio
git clone https://github.com/rapabru/theme-98.git
cd "theme 98"

# Ejecutar las pruebas unitarias
dotnet test

# Ejecutar en modo desarrollo
.\run.bat

# Publicar ejecutable standalone x64
dotnet publish src/DesktopPerformance -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

---

## 🛡️ Principios Inviolables de Desarrollo

1. **Seguridad antes que rendimiento.**
2. **Reversibilidad antes que agresividad (Rollback atómico transaccional garantizado).**
3. **Medición antes que afirmaciones (Telemetry-driven con verificación PDH).**
4. **Cero placebos (Sin vaciado falso de RAM ni tweaks destructivos de registro).**
5. **No romper componentes de Windows ni antivirus.**
6. **La propia herramienta consume < 10 MB de RAM y 0% de CPU en reposo.**

---

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Consulta el archivo [LICENSE](LICENSE) para más detalles.
