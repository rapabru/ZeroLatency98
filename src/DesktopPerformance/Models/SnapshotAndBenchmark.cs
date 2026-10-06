namespace DesktopPerformance.Models;

public class SystemSnapshot
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ProfileType AppliedProfile { get; set; } = ProfileType.Normal;

    // Visual configurations
    public bool PriorTransparency { get; set; } = true;
    public bool PriorWindowAnimations { get; set; } = true;
    public bool PriorTaskbarWidgets { get; set; } = true;
    public string PriorWallpaperPath { get; set; } = string.Empty;

    // Services modified: ServiceName -> prior Status (e.g., "Running")
    public Dictionary<string, string> ServicesPriorStatus { get; set; } = new();

    // Process modifications: ProcessName -> prior PriorityClass (int)
    public Dictionary<string, int> ProcessesPriorPriority { get; set; } = new();

    // Process affinities: ProcessName -> prior AffinityMask (long)
    public Dictionary<string, long> ProcessesPriorAffinity { get; set; } = new();

    // Paused processes names
    public List<string> SuspendedProcesses { get; set; } = new();
}

public class BenchmarkMetric
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double CpuLoadPercentage { get; set; }
    public double ContextSwitchesPerSecond { get; set; }
    public double DiskBytesPerSecond { get; set; }
    public double DwmWorkingSetMB { get; set; }
    public int ActiveProcessCount { get; set; }
}

public class BenchmarkComparison
{
    public BenchmarkMetric Before { get; set; } = new();
    public BenchmarkMetric After { get; set; } = new();

    public double DeltaContextSwitchesPercent => Before.ContextSwitchesPerSecond > 0 
        ? Math.Round(((After.ContextSwitchesPerSecond - Before.ContextSwitchesPerSecond) / Before.ContextSwitchesPerSecond) * 100.0, 1) 
        : 0;

    public double DeltaCpuPercent => Before.CpuLoadPercentage > 0
        ? Math.Round(((After.CpuLoadPercentage - Before.CpuLoadPercentage) / Before.CpuLoadPercentage) * 100.0, 1)
        : 0;

    public double DeltaDiskPercent => Before.DiskBytesPerSecond > 0
        ? Math.Round(((After.DiskBytesPerSecond - Before.DiskBytesPerSecond) / Before.DiskBytesPerSecond) * 100.0, 1)
        : 0;

    public double DeltaDwmMemoryPercent => Before.DwmWorkingSetMB > 0
        ? Math.Round(((After.DwmWorkingSetMB - Before.DwmWorkingSetMB) / Before.DwmWorkingSetMB) * 100.0, 1)
        : 0;

    // A change is measurable if there is an improvement (reduction in context switches, disk I/O, or CPU) of at least 2.0%
    public bool IsImprovementMeasurable => 
        DeltaContextSwitchesPercent <= -2.0 || 
        DeltaDiskPercent <= -2.0 || 
        DeltaCpuPercent <= -2.0 ||
        DeltaDwmMemoryPercent <= -2.0;

    public string SummaryText
    {
        get
        {
            if (!IsImprovementMeasurable)
            {
                return "No measurable improvement detected (variations within standard system noise margin < 2.0%).";
            }

            var inv = System.Globalization.CultureInfo.InvariantCulture;
            var improvements = new List<string>();
            if (DeltaContextSwitchesPercent <= -2.0)
                improvements.Add($"Context switches reduced by {Math.Abs(DeltaContextSwitchesPercent).ToString("F1", inv)}%");
            if (DeltaDiskPercent <= -2.0)
                improvements.Add($"Disk I/O reduced by {Math.Abs(DeltaDiskPercent).ToString("F1", inv)}%");
            if (DeltaCpuPercent <= -2.0)
                improvements.Add($"CPU background load reduced by {Math.Abs(DeltaCpuPercent).ToString("F1", inv)}%");
            if (DeltaDwmMemoryPercent <= -2.0)
                improvements.Add($"DWM memory footprint reduced by {Math.Abs(DeltaDwmMemoryPercent).ToString("F1", inv)}%");

            return "Measurable reduction detected: " + string.Join(", ", improvements) + ".";
        }
    }
}
