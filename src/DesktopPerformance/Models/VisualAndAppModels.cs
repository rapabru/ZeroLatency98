namespace DesktopPerformance.Models;

public class VisualState
{
    public bool TransparencyEnabled { get; set; }
    public bool WindowAnimationsEnabled { get; set; }
    public bool TaskbarWidgetsEnabled { get; set; }
    public string WallpaperPath { get; set; } = string.Empty;
}

public class ManagedAppInfo
{
    public string DisplayName { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AppActionPolicy Policy { get; set; } = AppActionPolicy.Default;
    public bool IsWhitelisted { get; set; }
    public ProcessExecutionState CurrentState { get; set; } = ProcessExecutionState.NotRunning;
    public List<int> RunningPids { get; set; } = new();
    public string Details { get; set; } = string.Empty;
}

public class ServiceInfo
{
    public string ServiceName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Unknown";
    public string StartType { get; set; } = "Unknown";
    public bool IsActive => Status.Equals("Running", StringComparison.OrdinalIgnoreCase);
}
