## Desktop Performance & Low-Interference Mode v1.2.1 (Hotfix)

### 🐛 Bug Fixes & Visual Polish
- **Fixed Top-Right Title Bar Button Corruption:** Resolved the issue in Windows 11 where the native Minimize, Maximize, and Close caption buttons were double-rendering and displaying corrupted overlapping rectangles.
- **Removed Redundant Inner Banner:** Eliminated the extra fake title bar beneath the menu bar, eliminating vertical tick mark rendering artifacts and restoring the authentic single-titlebar window hierarchy of Windows 98/2000.
- **Fixed Mnemonic Text Rendering:** Buttons now use `TextRenderer` with proper accelerator support (no literal `&` characters printed on buttons or group box headers).
- **Streamlined Top Section:** Clean sunken summary panel showing hardware topology directly beneath the classic menu bar.

### 📦 Assets Included
1. **`DesktopPerformance-v1.2.1-win-x64-standalone.zip`:** Self-contained executable, runs on any Windows 11 PC out of the box without installing .NET.
2. **`DesktopPerformance-v1.2.1-portable.zip`:** Lightweight portable package (~700 KB), requires .NET 9.
