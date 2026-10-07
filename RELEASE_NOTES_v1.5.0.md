# Release Notes - v1.5.0: Rebranded to ZeroLatency98

Welcome to **ZeroLatency98**! Version 1.5.0 formally updates the application name and identity to **ZeroLatency98**, reflecting its core mission: eliminating system latency and DWM interference on Windows 10/11 through a classic Windows 98/2000 aesthetic and zero-overhead architecture.

---

## 🌟 What's New in v1.5.0

### 1. 🏷️ Rebranding to ZeroLatency98
- **New Project Identity:** The application is now officially titled **ZeroLatency98: Windows Latency & Classic Theme Suite** (`ZeroLatency98.exe`).
- **Clear Alignment:** The name directly expresses what the application achieves:
  - **ZeroLatency:** Direct suppression of DWM DirectComposition shaders, CPU context switches, secondary display stutter, and timer resolution contention for gaming and real-time audio.
  - **98:** Pure Windows 95/98/2000 retro aesthetic, native Win32 controls (< 10 MB RAM, 0% CPU idle), and native System-Wide Classic Theme enabler.
- **Updated Binaries & Scripts:**
  - Standalone and Portable builds now output `ZeroLatency98.exe`.
  - `run.bat` automatically launches `ZeroLatency98.exe`.
- **Multilingual Names:**
  - English: `ZeroLatency98 - Windows Latency & Classic Theme Suite`
  - Spanish: `ZeroLatency98 - Modo Baja Latencia y Tema Clásico`

### 2. 🛡️ Complete Feature Set
- **Phase 1 & 2:** High-Performance Engine, atomic snapshots, process supervisor (`NtSuspendProcess`), Eco Priority.
- **Phase 3:** Telemetry-driven Trilateral Benchmark (PDH counters) and *"Why is my desktop busy?"* Diagnostic Engine.
- **Phase 4:** VRR & Multi-Monitor Studio (secondary display desync mitigation) and Smart Profiles with auto-game watcher.
- **Phase 5:** System-Wide Classic Theme Engine (Windows 95, 98 Plus!, 2000 Pro, Flat High-Contrast OLED) with 1-click restore.

### 3. 🧪 Quality & Tests
- Full test suite passes: **27/27 unit tests passing**.

---

## 📦 Download Packages

- **`ZeroLatency98-v1.5.0-win-x64-standalone.zip`:** Complete, standalone single-file x64 executable. No .NET runtime installation required.
- **`ZeroLatency98-v1.5.0-portable.zip`:** Lightweight portable edition (requires .NET 9 Desktop Runtime).
