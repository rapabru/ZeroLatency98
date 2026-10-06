## Desktop Performance & Low-Interference Mode v1.0.0 (MVP)

First official release of **Desktop Performance & Low-Interference Mode** for Windows 11 with authentic **Windows 98/2000 Retro Aesthetics**.

### 🌟 Key Features
- **Hardware & Multi-Monitor Topology Inspector:** Detects CPU, RAM, GPU, active displays with exact resolution and refresh rates (Hz).
- **Windows 11 Visual Effects Controller:** Instant toggling of Mica/Acrylic transparency, shell window animations, and Taskbar Widgets.
- **Three Core Profiles:**
  - `NORMAL`: Windows default baseline.
  - `LOW INTERFERENCE`: Transparency OFF, Animations OFF, Widgets OFF, Windows Search (`wsearch`) paused, Cloud Sync (OneDrive) paused.
  - `MAX RESPONSE`: Low Interference + background process priority demotion (`PROCESS_MODE_BACKGROUND_BEGIN`) and CPU core isolation.
- **Transactional Snapshots & 1-Click Restore:** Guaranteed atomic rollback via `snapshot_baseline.json` through the `[ RESTORE TO NORMAL (UNDO ALL) ]` button.
- **Background Manager & Permanent Whitelist:** Safe process management with permanent protection for Steam, OBS Studio, and audio/graphics drivers.
- **PDH Benchmark Engine (Anti-Placebo Rule):** Live BEFORE vs AFTER performance counter comparisons (Context Switches/s, CPU %, Disk I/O bytes/s, DWM memory). Reports honestly if variations are within noise margin (< 2%).
- **Authentic Windows 98/2000 UI:** 100% native GDI rendering, classic 3D beveled borders, Tahoma font, 0% GPU composition overhead, < 15 MB RAM in idle.

### 📦 Assets Included
1. **`DesktopPerformance-v1.0.0-win-x64-standalone.zip`:** Self-contained executable, runs out of the box on any Windows 11 machine without installing .NET runtime.
2. **`DesktopPerformance-v1.0.0-portable.zip`:** Ultra-lightweight portable package (600 KB), requires .NET 9 Desktop Runtime.
