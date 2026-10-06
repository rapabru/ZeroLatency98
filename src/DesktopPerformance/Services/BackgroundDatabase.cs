using DesktopPerformance.Models;

namespace DesktopPerformance.Services;

public class BackgroundDatabase
{
    private static readonly List<ProcessDatabaseEntry> Database = new()
    {
        // SAFE
        new ProcessDatabaseEntry
        {
            ProcessName = "OneDrive",
            DisplayName = "Microsoft OneDrive",
            Vendor = "Microsoft",
            Category = "Cloud Sync",
            Classification = SafetyClassification.Safe,
            ImpactDescription = "Continuous disk I/O and HTTP polling to sync cloud folders.",
            WhatHappensIfPaused = "Stops file uploads/downloads. Files-on-Demand placeholders cannot be hydrated until resumed.",
            HowToRestore = "Relaunch %LOCALAPPDATA%\\Microsoft\\OneDrive\\OneDrive.exe /background."
        },
        new ProcessDatabaseEntry
        {
            ProcessName = "Dropbox",
            DisplayName = "Dropbox Client",
            Vendor = "Dropbox Inc.",
            Category = "Cloud Sync",
            Classification = SafetyClassification.Safe,
            ImpactDescription = "Disk scanning and network keep-alives.",
            WhatHappensIfPaused = "Pauses sync queue. Local files remain intact.",
            HowToRestore = "Resume sync from tray icon or relaunch Dropbox.exe."
        },
        new ProcessDatabaseEntry
        {
            ProcessName = "SearchIndexer",
            DisplayName = "Windows Search Indexer (WSearch)",
            Vendor = "Microsoft",
            Category = "Windows System",
            Classification = SafetyClassification.Safe,
            ImpactDescription = "Intercepts file modifications in NTFS USN journal to update search catalog.",
            WhatHappensIfPaused = "Zero background disk I/O. Searches in Explorer fall back to direct file scan.",
            HowToRestore = "Resume service WSearch via Service Control Manager."
        },
        new ProcessDatabaseEntry
        {
            ProcessName = "Widgets",
            DisplayName = "Windows 11 Taskbar Widgets",
            Vendor = "Microsoft",
            Category = "Windows Shell",
            Classification = SafetyClassification.Safe,
            ImpactDescription = "WebView2 Edge Chromium host polling MSN news feeds in background.",
            WhatHappensIfPaused = "Frees 200-400 MB of RAM and terminates background network requests.",
            HowToRestore = "Re-enable TaskbarDa in registry or Settings > Personalization > Taskbar."
        },

        // LOW RISK
        new ProcessDatabaseEntry
        {
            ProcessName = "Discord",
            DisplayName = "Discord Client",
            Vendor = "Discord Inc.",
            Category = "Chat / Overlay",
            Classification = SafetyClassification.LowRisk,
            ImpactDescription = "Hardware-accelerated Electron renderer; draws continuously at 60 fps; CPU timer wakeups.",
            WhatHappensIfPaused = "Voice and chat stop receiving messages until resumed.",
            HowToRestore = "NtResumeProcess or relaunch Discord."
        },
        new ProcessDatabaseEntry
        {
            ProcessName = "Spotify",
            DisplayName = "Spotify Desktop",
            Vendor = "Spotify AB",
            Category = "Media",
            Classification = SafetyClassification.LowRisk,
            ImpactDescription = "Chromium embedded framework; audio buffer rendering and cache indexing.",
            WhatHappensIfPaused = "Audio playback pauses.",
            HowToRestore = "Resume process."
        },
        new ProcessDatabaseEntry
        {
            ProcessName = "steamwebhelper",
            DisplayName = "Steam Web Helper",
            Vendor = "Valve Corporation",
            Category = "Game Launcher",
            Classification = SafetyClassification.LowRisk,
            ImpactDescription = "Chromium renderers for Steam UI/Store. Multiple instances consuming GPU and RAM.",
            WhatHappensIfPaused = "Steam store/friends list UI pauses. Game execution is unaffected.",
            HowToRestore = "Resume process."
        },
        new ProcessDatabaseEntry
        {
            ProcessName = "CCXProcess",
            DisplayName = "Adobe Creative Cloud Experience",
            Vendor = "Adobe Systems",
            Category = "Software Suite",
            Classification = SafetyClassification.LowRisk,
            ImpactDescription = "Node.js host for Adobe cloud fonts and libraries.",
            WhatHappensIfPaused = "Typekit fonts and cloud libraries do not refresh in real time.",
            HowToRestore = "Relaunch Adobe Desktop Service."
        },
        new ProcessDatabaseEntry
        {
            ProcessName = "chrome",
            DisplayName = "Google Chrome (Background)",
            Vendor = "Google LLC",
            Category = "Web Browser",
            Classification = SafetyClassification.LowRisk,
            ImpactDescription = "Background extensions and notification listeners after closing browser window.",
            WhatHappensIfPaused = "Web push notifications are deferred until browser opens.",
            HowToRestore = "Relaunch Google Chrome."
        },

        // MEDIUM RISK
        new ProcessDatabaseEntry
        {
            ProcessName = "Corsair.Service",
            DisplayName = "Corsair iCUE Service",
            Vendor = "Corsair",
            Category = "RGB / Peripherals",
            Classification = SafetyClassification.MediumRisk,
            ImpactDescription = "1000 Hz USB polling and 1.0ms timer resolution lock. Induces CPU C-state wakeups.",
            WhatHappensIfPaused = "RGB illumination switches to onboard hardware profile; software macros pause.",
            HowToRestore = "Start service Corsair.Service."
        },
        new ProcessDatabaseEntry
        {
            ProcessName = "Razer Synapse Service",
            DisplayName = "Razer Synapse Service",
            Vendor = "Razer Inc.",
            Category = "RGB / Peripherals",
            Classification = SafetyClassification.MediumRisk,
            ImpactDescription = "Continuous driver polling and Chroma SDK interprocess communication.",
            WhatHappensIfPaused = "Mouse/keyboard DPI reverts to onboard defaults.",
            HowToRestore = "Start Razer Synapse Service."
        },
        new ProcessDatabaseEntry
        {
            ProcessName = "ArmouryCrate.Service",
            DisplayName = "ASUS Armoury Crate Service",
            Vendor = "ASUSTeK",
            Category = "Hardware Control",
            Classification = SafetyClassification.MediumRisk,
            ImpactDescription = "Frequent WMI queries to motherboard sensor buses causing kernel latency.",
            WhatHappensIfPaused = "Fan curves revert to BIOS hardware defaults.",
            HowToRestore = "Start ArmouryCrate.Service."
        },

        // HIGH RISK
        new ProcessDatabaseEntry
        {
            ProcessName = "SysMain",
            DisplayName = "SysMain (Superfetch)",
            Vendor = "Microsoft",
            Category = "Windows Memory",
            Classification = SafetyClassification.HighRisk,
            ImpactDescription = "Preloads frequently used application pages into standby memory.",
            WhatHappensIfPaused = "May increase launch times of regular apps on mechanical HDDs or slow SSDs.",
            HowToRestore = "Start service SysMain."
        },

        // DO NOT TOUCH
        new ProcessDatabaseEntry
        {
            ProcessName = "steam",
            DisplayName = "Steam Core Client",
            Vendor = "Valve Corporation",
            Category = "Gaming DRM",
            Classification = SafetyClassification.DoNotTouch,
            ImpactDescription = "Main Steam client providing Steamworks IPC pipes to games.",
            WhatHappensIfPaused = "Any active Steam game crashes immediately due to lost DRM IPC heartbeat.",
            HowToRestore = "Do NOT pause or terminate."
        },
        new ProcessDatabaseEntry
        {
            ProcessName = "obs64",
            DisplayName = "OBS Studio 64-bit",
            Vendor = "OBS Project",
            Category = "Streaming",
            Classification = SafetyClassification.DoNotTouch,
            ImpactDescription = "Live video encoder and compositor for broadcasting.",
            WhatHappensIfPaused = "Stream or recording drops frames or terminates abruptly.",
            HowToRestore = "Whitelisted permanently."
        },
        new ProcessDatabaseEntry
        {
            ProcessName = "dwm",
            DisplayName = "Desktop Window Manager",
            Vendor = "Microsoft",
            Category = "Windows Core",
            Classification = SafetyClassification.DoNotTouch,
            ImpactDescription = "Core DirectComposition compositor for all Windows 11 surfaces.",
            WhatHappensIfPaused = "System crashes with black screen, logon termination or BSOD.",
            HowToRestore = "Integral part of the OS. Never touch."
        },
        new ProcessDatabaseEntry
        {
            ProcessName = "audiodg",
            DisplayName = "Windows Audio Device Graph",
            Vendor = "Microsoft",
            Category = "Audio Subsystem",
            Classification = SafetyClassification.DoNotTouch,
            ImpactDescription = "Low-latency user-mode audio mixing engine for WASAPI/DirectSound.",
            WhatHappensIfPaused = "Complete system audio mute and possible audio buffer deadlock.",
            HowToRestore = "Never touch."
        },
        new ProcessDatabaseEntry
        {
            ProcessName = "nvcontainer",
            DisplayName = "NVIDIA Driver Container",
            Vendor = "NVIDIA Corporation",
            Category = "Display Driver",
            Classification = SafetyClassification.DoNotTouch,
            ImpactDescription = "Display driver user-mode communication server for G-Sync and display modes.",
            WhatHappensIfPaused = "Loss of display settings, potential display driver reset.",
            HowToRestore = "Never touch."
        }
    };

    public IReadOnlyList<ProcessDatabaseEntry> GetAllEntries() => Database;

    public ProcessDatabaseEntry? FindByProcessName(string processName)
    {
        return Database.FirstOrDefault(e => e.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));
    }
}
