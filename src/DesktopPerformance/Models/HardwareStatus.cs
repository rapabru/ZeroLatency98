namespace DesktopPerformance.Models;

public class DisplayInfo
{
    public string DeviceName { get; set; } = string.Empty;
    public string MonitorName { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int RefreshRateHz { get; set; }
    public int BitsPerPixel { get; set; }
    public bool IsPrimary { get; set; }

    public override string ToString()
    {
        string primaryTag = IsPrimary ? " [PRIMARY]" : "";
        return $"{MonitorName} ({DeviceName}): {Width}x{Height} @ {RefreshRateHz} Hz, {BitsPerPixel}-bit{primaryTag}";
    }
}

public class HardwareStatus
{
    public string CpuName { get; set; } = "Unknown CPU";
    public int LogicalCores { get; set; }
    public double CpuLoadPercentage { get; set; }

    public ulong TotalPhysicalMemoryMB { get; set; }
    public ulong AvailablePhysicalMemoryMB { get; set; }
    public double MemoryUsagePercentage => TotalPhysicalMemoryMB > 0 
        ? Math.Round((1.0 - ((double)AvailablePhysicalMemoryMB / TotalPhysicalMemoryMB)) * 100.0, 1) 
        : 0;

    public string GpuAdapterName { get; set; } = "Unknown GPU";
    public List<DisplayInfo> Displays { get; set; } = new();

    public int TotalProcessesCount { get; set; }
    public int TotalThreadsCount { get; set; }
}
