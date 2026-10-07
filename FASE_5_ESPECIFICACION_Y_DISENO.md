# FASE 5 — ESPECIFICACIÓN Y DISEÑO: TEMA CLÁSICO WINDOWS 95/98 DEL SISTEMA
## Proyecto: Desktop Performance & Low-Interference Mode (Windows 11)

Esta fase introduce el **Motor de Tema Clásico y Modo Cero Sobrecarga (System-Wide Classic Theme & Zero-Overhead Engine)**, permitiendo aplicar la estética visual clásica de Windows 95, Windows 98 y Windows 2000 en todo el sistema operativo Windows 10 y Windows 11 de forma nativa, segura, reversible y orientada a la reducción medible de latencia y carga de DWM.

---

## 1. Justificación Técnica y Filosofía Zero-Placebo

### 1.1. El Problema de la Composición Moderna en Windows 10/11
- En Windows 10 y 11, el Gestor de Ventanas del Escritorio (DWM) utiliza composición basada en DirectX con shaders de desenfoque continuo (Mica, Acrylic, Fluent Design), sombras proyectadas en tiempo real, efectos de desvanecimiento y bordes redondeados con suavizado por GPU.
- En escenarios de gaming competitivo, producción de audio de baja latencia o hardware modesto, esta composición:
  - Genera contención innecesaria de VRAM y ciclos de cómputo en la GPU.
  - Provoca micro-stuttering o alternancia de reloj en configuraciones de múltiples monitores con distintas frecuencias de actualización.
  - Despierta núcleos de la CPU para animar elementos no esenciales de la interfaz.

### 1.2. Por qué los Parches Tradicionales son Peligrosos y Están Prohibidos
- Herramientas obsoletas de épocas pasadas modificaban o reemplazaban binarios críticos como `uxtheme.dll` o inyectaban código no verificado en `dwm.exe`.
- Esto provoca pantallazos negros, fallos al reiniciar tras actualizaciones acumulativas de Windows y problemas de compatibilidad con Secure Boot.
- **Principio Inviolable:** En este proyecto jamás se modifican archivos de sistema ni se inyecta código invasivo.

---

## 2. Solución Técnica de Fase 5 (100% Segura, Nativa y Reversible)

La Fase 5 implementa una arquitectura en 5 capas complementarias:

### 2.1. Generador Dinámico de Archivos `.theme` Clásicos
Windows 10 y 11 cuentan con un subsistema nativo para el procesamiento de archivos `.theme`. Fase 5 genera dinámicamente archivos de tema optimizados en `%LOCALAPPDATA%\DesktopPerformance98\Themes\`:
1. **Windows 95 Classic:**
   - Escritorio: Color sólido Verde Azulado / Teal `#008080` (RGB `0, 128, 128`).
   - Controles y Bordes 3D: Gris piedra clásico `#C0C0C0` (RGB `192, 192, 192`).
   - Barra de título activa: Azul marino profundo `#000080` (RGB `0, 0, 128`) con texto blanco nítido.
   - Barra inactiva: Gris `#808080` (RGB `128, 128, 128`).
2. **Windows 98 Second Edition (Plus!):**
   - Incorpora el clásico gradiente horizontal de dos tonos en la barra de título: `#000080` a `#1084D0`.
   - Paleta clásica con bordes biselados de alto contraste.
3. **Windows 2000 Professional:**
   - Paleta corporativa refinada: Gris pizarra `#D4D0C8`, títulos `#0A246A` y bordes estáticos.
4. **Windows 98 Flat / Zero-Nit OLED (Máximo Rendimiento):**
   - Fondo negro puro `#000000` para consumo cero en pantallas OLED y máxima visibilidad.

### 2.2. Inyección Inmediata en Memoria con `SetSysColors`
- Para que todas las ventanas Win32, herramientas del sistema (Administrador de Tareas, Panel de Control, Bloc de Notas, diálogos clásicos) adopten los colores al instante sin requerir cierre de sesión, se invoca la API Win32 nativa `SetSysColors`.
- Se configuran los 30+ registros de color de la interfaz (Background, BtnFace, ActiveCaption, Window, Menu, Hilight, 3DShadow, etc.).

### 2.3. Configuración Atómica de Fondo Sólido Teal (`SPI_SETDESKWALLPAPER`)
- Elimina cualquier fondo animado o imagen pesada de alta resolución de la memoria de la GPU.
- Aplica el color plano exacto `#008080` mediante `SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, "", SPIF_UPDATEINIFILE | SPIF_SENDCHANGE)`.

### 2.4. Supresión Integral de Efectos DWM y Shaders Innecesarios
Al activar el Modo Clásico, se apagan en bloque:
- Transparencia y efectos de acrílico/desenfoque.
- Sombras bajo las ventanas y bajo los menús emergentes (`SPI_SETDROPSHADOW`).
- Sombras bajo el puntero del ratón.
- Animaciones de ventanas al minimizar/maximizar (`SPI_SETANIMATION`).
- Animaciones de la barra de tareas.

### 2.5. Snapshot y Rollback Atómico a Windows 11 Estándar (1 Clic)
- Antes de aplicar cualquier modificación, el motor captura una instantánea del tema y los colores actuales del usuario (`%LOCALAPPDATA%\DesktopPerformance98\backup_theme.theme`).
- Con un solo clic en **"Restaurar Tema de Windows 11"**, el sistema reestablece los estilos visuales originales, el fondo de pantalla y los efectos DWM predeterminados.

### 2.6. Asistente Opcional para Shell Retro Completo (RetroBar & Open-Shell)
- Para usuarios que además desean reemplazar visualmente la barra de tareas y el menú de inicio modernos sin alterar Windows:
  - Detección automática de **RetroBar** y **Open-Shell**.
  - Acceso directo para ejecutar/descargar herramientas portátiles recomendadas con configuraciones predefinidas de Windows 95 y 98.

---

## 3. Arquitectura del Código en C# / .NET 9

- **`ClassicThemeManager.cs`:**
  - `GenerateThemeFile(ClassicThemeVariant variant)`: Genera el archivo `.theme` con la especificación completa de colores, métricas y fuentes clásicas.
  - `ApplyClassicTheme(ClassicThemeVariant variant)`: Aplica el archivo `.theme`, ejecuta `SetSysColors`, elimina el wallpaper para fondo sólido y desactiva sombras/animaciones DWM.
  - `RestoreModernTheme()`: Revierte al tema original de Windows 11.
  - `BackupCurrentTheme()`: Crea el snapshot de respaldo antes de modificar nada.
- **Integración en UI (`MainForm.cs`):**
  - Nueva sección dedicada o expansión de la pestaña de Shell Retro con:
    - Selector visual de temas clásicos del sistema (Win95, Win98, Win2000, High-Contrast Flat).
    - Botón **"Aplicar Tema Clásico al Sistema (Baja Carga)"**.
    - Botón **"Restaurar Tema Original de Windows 11"**.
    - Monitor en vivo de estado del tema del sistema y reducción de sobrecosto DWM.
  - Soporte multilingüe completo (Inglés / Español).
- **Pruebas Unitarias (`ClassicThemeTests.cs`):**
  - Verificación de generación de archivos `.theme`.
  - Verificación de consistencia de paletas RGB.
  - Verificación de creación de copias de seguridad e idempotencia de restauración.
