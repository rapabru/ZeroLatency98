# Release Notes - v1.4.0: Phase 5 - System-Wide Windows 95/98 Classic Theme & Zero-Overhead Engine

Desktop Performance & Low-Interference Mode for Windows 11 — Version 1.4.0 introduces **Phase 5**, allowing users to apply the authentic **Windows 95/98/2000 Classic Theme across the entire Windows 10/11 operating system** safely, natively, and reversibly to minimize DWM GPU composition overhead.

---

## 🌟 What's New in v1.4.0

### 1. 🪟 Phase 5: System-Wide Classic Theme Engine
- **Dynamic `.theme` Generation:** Creates native, valid Windows `.theme` files without any file patching or DLL injection:
  - **Windows 95 Classic:** Teal desktop (`#008080`), stone-gray 3D buttons (`#C0C0C0`), solid navy title bars (`#000080`).
  - **Windows 98 Plus! (SE):** Classic horizontal caption gradient (`#000080` to `#1084D0`) and teal desktop.
  - **Windows 2000 Professional:** Corporate slate palette (`#D4D0C8`) with deep blue gradient.
  - **Win98 High-Contrast Flat (OLED / Max FPS):** 0-nit black background for maximum OLED battery savings and lowest compositor load.
- **In-Memory `SetSysColors` API:** Applies 25+ classic UI color indices in memory instantly via Win32 User32 APIs, updating open dialogs, File Explorer elements, and classic windows without requiring logoff.
- **Solid Teal Desktop Background:** Automatically switches to solid teal (`#008080`) or black via `SystemParametersInfo(SPI_SETDESKWALLPAPER)` to purge heavy wallpaper textures from VRAM.
- **DWM Blur & Shadow Suppression:** Shuts down transparency, Mica/Acrylic blur shaders, drop shadows, and window animations to free up GPU cycles and prevent frame pacing jitter.
- **Atomic 1-Click Rollback:** Backs up your existing Windows 11 theme (`backup_theme.theme`) before applying, and restores modern Windows 11 default themes and visual effects with one click.

### 2. 🕹️ Companion Retro Shell Ecosystem Integration
- Built-in scanner and one-click launcher for safe, non-invasive open-source retro shell tools:
  - **RetroBar:** Native Win95/98 classic taskbar (detects Running / Installed / Download links).
  - **Open-Shell:** Classic two-column Start Menu with 0% GPU composition.

### 3. 🌐 Full Multilingual Support (English & Spanish)
- All new Phase 5 UI controls, labels, tooltips, dialogs, and notices are fully localized in both English and Spanish in real-time.

### 4. 🧪 Test Suite & Reliability
- Added 6 new unit tests in `ClassicThemeTests.cs` verifying `.theme` generation, RGB palette definitions, and companion tool metadata.
- Entire test suite: **27/27 unit tests passing**.

---

## 📦 Download Packages

- **`DesktopPerformance-v1.4.0-win-x64-standalone.zip`:** Complete, standalone single-file x64 executable. No .NET runtime installation required.
- **`DesktopPerformance-v1.4.0-portable.zip`:** Lightweight portable edition (requires .NET 9 Desktop Runtime).
