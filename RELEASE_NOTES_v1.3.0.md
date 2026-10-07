# Release Notes - v1.3.0: Multilingual Support (EN/ES) & Screenshot Showcase

Desktop Performance & Low-Interference Mode for Windows 11 — Version 1.3.0 brings full **English and Spanish localization** directly built into the application, alongside comprehensive screenshot documentation in the repository.

---

## 🌟 What's New in v1.3.0

### 1. 🌐 Dynamic In-App Multilingual Engine (English & Spanish)
- **Single Unified Executable:** Seamless localization toggle between English and Spanish without requiring separate builds or application restarts.
- **Menu Bar Switcher:** Added `Language` / `Idioma` menu allowing instant toggling with visual radio-check indicators.
- **Zero-Restart Live Re-rendering:** All 7 tab pages, buttons, group boxes, dialogs, status strip messages, and list views are immediately updated upon language change.
- **Smart OS Detection:** Automatically detects system language via `CultureInfo.CurrentUICulture` on first launch.
- **Persistent Preferences:** Stores user language preference to `%LOCALAPPDATA%\DesktopPerformance98\language.txt`.
- **Header Sort Preservation:** Column headers dynamically localize while maintaining active sorting glyph indicators (`▲` / `▼`).

### 2. 📸 Interface Documentation & Screenshots
- Added high-resolution screenshots for all 7 application modules:
  - System Topology & Displays
  - Profiles & Optimization
  - Background Process Manager (with interactive sorting)
  - Verification & Trilateral Benchmark
  - Diagnostics & Smart Profiles (Game Watcher)
  - Multi-Monitor Studio & VRR Protection
  - Retro Shell & Classic Themes
- Updated `README.md` in comprehensive English and preserved `README.es.md` in Spanish.

### 3. 🧪 Quality & Test Suite
- Added localization test suite in `tests/DesktopPerformance.Tests/LocalizationTests.cs`.
- 21/21 unit tests passing.

---

## 📦 Download Packages

- **`DesktopPerformance-v1.3.0-win-x64-standalone.zip`:** Complete, standalone single-file x64 executable. No .NET runtime installation required.
- **`DesktopPerformance-v1.3.0-portable.zip`:** Lightweight portable edition (requires .NET 9 Desktop Runtime).
