## Desktop Performance & Low-Interference Mode v1.1.0 (Phase 3 Update)

Official Phase 3 release featuring the **Diagnostic Engine**, **Auto-Game Hook**, **Smart Workload Profiles**, and **Process Safety Database**.

### 🌟 New Features in v1.1.0
1. **Diagnostic Engine: "Why Is My Desktop Busy?"**
   - Automatically scans background GPU acceleration, active disk indexing, cloud sync loops, and peripheral RGB polling.
   - Explains findings in clear, human language with actionable recommendations.
   - Diagnoses multi-monitor mixed refresh rate contention (e.g. 240 Hz + 60 Hz).

2. **Automatic Game / Workload Detection (`ProcessWatcher`):**
   - Background event watcher (< 0.01% CPU) monitoring registered processes (e.g., `cs2.exe`, `obs64.exe`, `Adobe Premiere Pro.exe`).
   - Automatically captures snapshot and activates the matching Smart Profile on launch.
   - Automatically restores baseline `NORMAL` state when the process closes.
   - Modes: `AUTO`, `MANUAL`, `DISABLED`.

3. **Smart Profiles Library:**
   - **CS2 / Competitive Gaming:** Full CPU isolation, DWM blurs OFF, secondary apps moved to secondary cores.
   - **Streaming & OBS:** Preserves OBS Studio and Discord voice connections with zero frame drops.
   - **Video Editing & 3D:** Maximizes RAM allocation while retaining NTFS indexing for media assets.
   - **General Work & Silence:** Distraction-free desktop with zero animations.

4. **Process & Service Safety Database:**
   - Searchable local catalog of background processes with technical classifications: `SAFE`, `LOW_RISK`, `MEDIUM_RISK`, `HIGH_RISK`, `DO_NOT_TOUCH`.
   - Explains technical impact, what happens if paused, and restoration protocols.

5. **Portable JSON Export/Import:**
   - Export benchmark comparison reports with exact percentage deltas.
   - Export and import customized Smart Profiles in standard JSON.

### 📦 Assets Included
1. **`DesktopPerformance-v1.1.0-win-x64-standalone.zip`:** Self-contained executable, runs on any Windows 11 PC without installing .NET runtime.
2. **`DesktopPerformance-v1.1.0-portable.zip`:** Lightweight portable package (~650 KB), requires .NET 9.
