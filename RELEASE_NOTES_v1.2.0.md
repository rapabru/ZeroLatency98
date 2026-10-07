## Desktop Performance & Low-Interference Mode v1.2.0 (Full Suite)

Comprehensive Phase 3 release adding **Multi-Monitor Studio & VRR Analysis**, **Advanced Trilateral Benchmarking**, **Classic Retro Shell Themes**, and the **Local Pattern Learner**.

### 🌟 New Features in v1.2.0
1. **Multi-Monitor Studio & VRR Desync Analysis (Tab 6):**
   - Detects asymmetric refresh rate setups (e.g. 240 Hz + 60 Hz).
   - Identifies background hardware-accelerated windows on secondary monitors (Discord, Chrome, Spotify) that induce v-blank contention.
   - Provides 1-click option to enforce static desktop backgrounds on secondary monitors.

2. **Advanced Trilateral Benchmarking (Tab 4):**
   - Trilateral live comparison: `NORMAL` vs `LOW INTERFERENCE` vs `MAX RESPONSE`.
   - Native NT Timer Resolution measurement via `NtQueryTimerResolution` in milliseconds (0.5 ms vs 1.0 ms vs 15.6 ms).
   - P1 (99th percentile) and P0.1 (99.9th percentile) queue wait jitter estimation.

3. **Retro Shell & Visual Themes (Tab 7):**
   - Authentic classic visual theme presets via native `SetSysColors`:
     - *Windows 95 Classic* (Teal `#008080`, silver 3D buttons `#C0C0C0`).
     - *Windows 98 Plus!* (Classic caption gradients).
     - *Windows 2000 Professional* (Corporate slate aesthetic).
     - *High Contrast Performance OLED* (Pure `#000000` black for 0-nit power savings on OLED displays).
   - Architectural guide and safety ratings for companion shell tools (Open-Shell vs Windhawk vs ExplorerPatcher).

4. **Local Profile Learning Engine:**
   - Observes and correlates user application habits (e.g. running Discord with CS2).
   - Generates local, non-intrusive optimization suggestions.
   - 100% private, stored in `%LOCALAPPDATA%\DesktopPerformance98\user_patterns.json` with zero external telemetry.

5. **Integrated 7-Tab Classic Win98 Interface:**
   - 1. System Topology & Monitors
   - 2. Profiles & Optimization
   - 3. Background Manager & Whitelist
   - 4. Verification & Benchmark
   - 5. Diagnostics & Smart Profiles
   - 6. Multi-Monitor Studio & VRR
   - 7. Retro Shell & Themes

### 📦 Assets Included
1. **`DesktopPerformance-v1.2.0-win-x64-standalone.zip`:** Self-contained executable, runs on any Windows 11 PC out of the box.
2. **`DesktopPerformance-v1.2.0-portable.zip`:** Lightweight portable package (~700 KB), requires .NET 9.
