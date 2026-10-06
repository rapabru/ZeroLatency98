namespace DesktopPerformance.Models;

public enum SafetyClassification
{
    Safe,
    LowRisk,
    MediumRisk,
    HighRisk,
    DoNotTouch
}

public class ProcessDatabaseEntry
{
    public string ProcessName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "Cloud Sync", "Chat/Overlay", "RGB/Telemetry", "Core Windows"
    public SafetyClassification Classification { get; set; }
    public string ImpactDescription { get; set; } = string.Empty;
    public string WhatHappensIfPaused { get; set; } = string.Empty;
    public string HowToRestore { get; set; } = string.Empty;
}

public class SmartProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TriggerProcessName { get; set; } = string.Empty; // e.g. "cs2.exe", "obs64.exe"
    
    // Visual Flags
    public bool DisableTransparency { get; set; } = true;
    public bool DisableAnimations { get; set; } = true;
    public bool DisableWidgets { get; set; } = true;

    // Service Rules
    public bool PauseWindowsSearch { get; set; } = true;
    public bool PauseRgbServices { get; set; } = false;

    // Process Rules
    public bool PauseCloudSync { get; set; } = true;
    public bool LowerBackgroundAppsPriority { get; set; } = true;
    public bool IsolateCpuCores { get; set; } = false;

    // Power
    public bool ForceHighPerformancePower { get; set; } = false;

    public List<string> CustomWhitelistedProcesses { get; set; } = new();
}

public enum AutoGameDetectionMode
{
    Auto,
    Manual,
    Disabled
}
