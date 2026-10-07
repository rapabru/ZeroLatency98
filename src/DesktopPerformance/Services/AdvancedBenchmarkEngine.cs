using System.Diagnostics;
using DesktopPerformance.Models;
using DesktopPerformance.Native;

namespace DesktopPerformance.Services;

public class AdvancedBenchmarkMetric : BenchmarkMetric
{
    public double CurrentTimerResolutionMs { get; set; }
    public double MinTimerResolutionMs { get; set; }
    public double MaxTimerResolutionMs { get; set; }
    public double EstimatedP1JitterMs { get; set; }
    public double EstimatedP01JitterMs { get; set; }
}

public class TrilateralBenchmarkReport
{
    public AdvancedBenchmarkMetric NormalBaseline { get; set; } = new();
    public AdvancedBenchmarkMetric LowInterference { get; set; } = new();
    public AdvancedBenchmarkMetric MaxResponse { get; set; } = new();

    public double ContextSwitchesReductionPercent => NormalBaseline.ContextSwitchesPerSecond > 0
        ? Math.Round(((MaxResponse.ContextSwitchesPerSecond - NormalBaseline.ContextSwitchesPerSecond) / NormalBaseline.ContextSwitchesPerSecond) * 100.0, 1)
        : 0;

    public double DwmMemoryReductionPercent => NormalBaseline.DwmWorkingSetMB > 0
        ? Math.Round(((MaxResponse.DwmWorkingSetMB - NormalBaseline.DwmWorkingSetMB) / NormalBaseline.DwmWorkingSetMB) * 100.0, 1)
        : 0;

    public string SummaryText
    {
        get
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            if (ContextSwitchesReductionPercent <= -2.0)
            {
                return $"Max Response achieved a {Math.Abs(ContextSwitchesReductionPercent).ToString("F1", inv)}% reduction in context switches compared to Normal baseline.";
            }
            return "No measurable difference exceeding 2.0% detected across profiles.";
        }
    }
}

public class AdvancedBenchmarkEngine
{
    private readonly BenchmarkEngine _baseEngine;

    public AdvancedBenchmarkEngine(BenchmarkEngine baseEngine)
    {
        _baseEngine = baseEngine;
    }

    public async Task<AdvancedBenchmarkMetric> SampleAdvancedAsync(int sampleDurationMs = 3000, IProgress<string>? progress = null)
    {
        var baseMetric = await _baseEngine.SampleAsync(sampleDurationMs, progress);
        var advanced = new AdvancedBenchmarkMetric
        {
            Timestamp = baseMetric.Timestamp,
            CpuLoadPercentage = baseMetric.CpuLoadPercentage,
            ContextSwitchesPerSecond = baseMetric.ContextSwitchesPerSecond,
            DiskBytesPerSecond = baseMetric.DiskBytesPerSecond,
            DwmWorkingSetMB = baseMetric.DwmWorkingSetMB,
            ActiveProcessCount = baseMetric.ActiveProcessCount
        };

        // Query Native NT Timer Resolution
        try
        {
            if (Win32Native.NtQueryTimerResolution(out uint minRes, out uint maxRes, out uint curRes) == 0)
            {
                // Values are in 100-nanosecond units (1 ms = 10,000 units)
                advanced.CurrentTimerResolutionMs = Math.Round(curRes / 10000.0, 3);
                advanced.MaxTimerResolutionMs = Math.Round(minRes / 10000.0, 3); // min resolution = maximum interval
                advanced.MinTimerResolutionMs = Math.Round(maxRes / 10000.0, 3); // max resolution = minimum interval
            }
        }
        catch
        {
            advanced.CurrentTimerResolutionMs = 1.0;
        }

        // Calculate synthetic P1 / P0.1 jitter estimation based on context switches per core
        int cores = Math.Max(1, Environment.ProcessorCount);
        double csPerCore = advanced.ContextSwitchesPerSecond / cores;
        // P1 jitter estimation formula based on queue wait time
        advanced.EstimatedP1JitterMs = Math.Round((csPerCore / 1000.0) * 0.12, 2);
        advanced.EstimatedP01JitterMs = Math.Round(advanced.EstimatedP1JitterMs * 1.85, 2);

        return advanced;
    }
}
