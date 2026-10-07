using System.Diagnostics;
using DesktopPerformance.Models;
using DesktopPerformance.Native;

namespace DesktopPerformance.Services;

public class MultiMonitorAnalysis
{
    public int TotalMonitors { get; set; }
    public bool HasAsymmetricRefreshRates { get; set; }
    public int MaxRefreshRateHz { get; set; }
    public int MinRefreshRateHz { get; set; }
    public bool IsVrrAtRisk { get; set; }
    public string VrrRiskReason { get; set; } = string.Empty;
    public List<DisplayInfo> Displays { get; set; } = new();
    public List<string> SecondaryMonitorRunningApps { get; set; } = new();
}

public class MultiMonitorOptimizer
{
    private readonly HardwareInspector _hardwareInspector;

    public MultiMonitorOptimizer(HardwareInspector hardwareInspector)
    {
        _hardwareInspector = hardwareInspector;
    }

    public MultiMonitorAnalysis AnalyzeDisplays()
    {
        var hw = _hardwareInspector.CaptureCurrentHardwareStatus();
        var analysis = new MultiMonitorAnalysis
        {
            TotalMonitors = hw.Displays.Count,
            Displays = hw.Displays
        };

        if (hw.Displays.Count > 1)
        {
            var hzList = hw.Displays.Select(d => d.RefreshRateHz).Distinct().ToList();
            analysis.MaxRefreshRateHz = hzList.Max();
            analysis.MinRefreshRateHz = hzList.Min();
            analysis.HasAsymmetricRefreshRates = hzList.Count > 1;

            if (analysis.HasAsymmetricRefreshRates)
            {
                analysis.IsVrrAtRisk = true;
                analysis.VrrRiskReason = $"Monitores combinan {analysis.MinRefreshRateHz} Hz con {analysis.MaxRefreshRateHz} Hz. " +
                                         "Si se renderiza video o animación por hardware en la pantalla secundaria, " +
                                         "el compositor de DWM y el driver de la GPU pueden experimentar contención de v-blank.";
            }
            else
            {
                analysis.IsVrrAtRisk = false;
                analysis.VrrRiskReason = "Todas las pantallas operan con la misma tasa de refresco; riesgo de desincronización mínimo.";
            }
        }
        else
        {
            analysis.MaxRefreshRateHz = hw.Displays.FirstOrDefault()?.RefreshRateHz ?? 60;
            analysis.MinRefreshRateHz = analysis.MaxRefreshRateHz;
            analysis.HasAsymmetricRefreshRates = false;
            analysis.IsVrrAtRisk = false;
            analysis.VrrRiskReason = "Un único monitor conectado; el compositor opera sin contención multi-display.";
        }

        // Identify secondary monitor potential offenders
        var activeProcesses = Process.GetProcesses();
        var knownOffenders = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "chrome", "msedge", "Discord", "Spotify", "steamwebhelper" };

        foreach (var p in activeProcesses)
        {
            if (knownOffenders.Contains(p.ProcessName) && p.MainWindowHandle != IntPtr.Zero)
            {
                analysis.SecondaryMonitorRunningApps.Add($"{p.ProcessName} (Window: {p.MainWindowTitle})");
            }
            p.Dispose();
        }

        return analysis;
    }

    public void ApplySecondaryDisplayStaticBackground(bool apply)
    {
        // When true, broadcasts desktop update to ensure no animated desktop engine runs
        try
        {
            Win32Native.SendMessageTimeout(
                (IntPtr)Win32Native.HWND_BROADCAST,
                Win32Native.WM_SETTINGCHANGE,
                IntPtr.Zero,
                "DeskPattern",
                Win32Native.SMTO_ABORTIFHUNG,
                500,
                out _);
        }
        catch { }
    }
}
