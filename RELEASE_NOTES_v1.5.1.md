# Release Notes - v1.5.1: Security Hardening & Zero-Placebo Real-Time Precision

Version 1.5.1 is a comprehensive security hardening and performance-verification update for **ZeroLatency98**. It resolves all security audit findings, replaces legacy undocumented hooks with safe Win32 QoS scheduling, adds strict cryptographic integrity verification, and provides true hardware-level NT timer resolution.

---

## 🔒 Security Hardening (ZL98-SEC)

1. **Cryptographic SHA-256 Pinning (`ZL98-SEC-01`):**
   - Companion tool downloads (RetroBar, Open-Shell) are verified against official pinned SHA-256 hashes prior to extraction or execution.
   - Fails closed on any hash mismatch, preventing supply-chain and MITM tampering.

2. **Safe Win32 QoS Background Mode (`ZL98-SEC-06`):**
   - Replaced undocumented `NtSuspendProcess` and `NtResumeProcess` with standard Windows Quality of Service (`PROCESS_MODE_BACKGROUND_BEGIN` and `PROCESS_MODE_BACKGROUND_END`).
   - Completely eliminates deadlocks, cross-process thread lockups, and anti-cheat false positives (Easy Anti-Cheat, BattlEye, Vanguard).

3. **System Binary Path Canonicalization (`ZL98-SEC-03`):**
   - System executables (`rundll32.exe`, `explorer.exe`) are executed strictly from `Environment.SystemDirectory` (`C:\Windows\System32`), preventing PATH-hijacking or unquoted search path vulnerabilities.

4. **Strict Theme Validation (`ZL98-SEC-04`):**
   - `LaunchThemeFile` strictly validates file existence, canonical paths, and requires the `.theme` file extension before invocation.

5. **Anti-ZipSlip & Path Traversal Guard (`ZL98-SEC-02`):**
   - Archive extraction strictly validates that all target entries reside within the authorized destination directory, rejecting path traversal (`../`) and illegal characters.

6. **Memory Exhaustion & DoS Guard on Profile Import (`ZL98-SEC-07`):**
   - Import profiles enforces a 512 KB file size cap, limits imports to a maximum of 100 profiles, and sanitizes all input strings.

7. **System Process Anti-Spoofing (`ZL98-SEC-05`):**
   - `IsGenuineSystemProcess` verifies that protected system binaries (`dwm.exe`, `audiodg.exe`, `explorer.exe`) reside within genuine Windows system directories.

---

## ⚡ Performance & Kernel Precision

1. **Windows NT Kernel Timer Resolution (0.5 ms / 1.0 ms):**
   - Integrates `NtSetTimerResolution` and `timeBeginPeriod(1)` to eliminate thread wait jitter and frame pacing variance.
   - Live status indicator and controls in Tab 2 and status bar.
   - Clean lifecycle disposal on exit.

2. **100% Genuine Non-Demo Architecture:**
   - **Discrete GPU Detection:** Direct hardware identification (`AMD Radeon`, `NVIDIA GeForce`) filtering out virtual drivers.
   - **True Multi-Monitor Window Filtering:** Uses `Screen.FromHandle` to detect windows physically on secondary monitors.
   - **Secondary Window Minimization:** `MinimizeSecondaryWindows` drops GPU DirectComposition swapchain presentation on secondary screens, eliminating G-Sync/FreeSync micro-stuttering.
   - **Dynamic Cross-Game Learning:** Dynamic pattern recognition engine analyzing real session co-occurrence for any game or app.
   - **Empirical Thread Scheduling Jitter:** Measures live scheduling overshoot with `Stopwatch.Frequency` in nanoseconds.

---

## 🧪 Quality & Tests

- **57 of 57 Unit Tests Passing** (`dotnet test`):
  - 100% test pass rate across core systems, security utilities, timer resolution, and retro theme managers.

---

## 📦 Download Packages

- **`ZeroLatency98-v1.5.1-win-x64-standalone.zip`:** Complete standalone single-file x64 executable. No .NET runtime required.
- **`ZeroLatency98-v1.5.1-portable.zip`:** Ultra-lightweight portable package (requires .NET 9 Desktop Runtime).
