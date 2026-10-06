using System.Diagnostics;
using DesktopPerformance.Models;

namespace DesktopPerformance.Services;

public class BenchmarkEngine
{
    private PerformanceCounter? _cpuCounter;
    private PerformanceCounter? _contextSwitchesCounter;
    private PerformanceCounter? _diskBytesCounter;

    public BenchmarkEngine()
    {
        InitializeCounters();
    }

    private void InitializeCounters()
    {
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
            _cpuCounter.NextValue(); // First call primes the counter
        }
        catch { _cpuCounter = null; }

        try
        {
            _contextSwitchesCounter = new PerformanceCounter("System", "Context Switches/sec", true);
            _contextSwitchesCounter.NextValue();
        }
        catch { _contextSwitchesCounter = null; }

        try
        {
            _diskBytesCounter = new PerformanceCounter("PhysicalDisk", "Disk Bytes/sec", "_Total", true);
            _diskBytesCounter.NextValue();
        }
        catch { _diskBytesCounter = null; }
    }

    public async Task<BenchmarkMetric> SampleAsync(int sampleDurationMs = 3000, IProgress<string>? progress = null)
    {
        progress?.Report("Sampling background activity...");

        int iterations = Math.Max(1, sampleDurationMs / 500);
        double totalCpu = 0;
        double totalCs = 0;
        double totalDisk = 0;
        int validSamples = 0;

        for (int i = 0; i < iterations; i++)
        {
            await Task.Delay(500);

            try
            {
                if (_cpuCounter != null) totalCpu += _cpuCounter.NextValue();
                if (_contextSwitchesCounter != null) totalCs += _contextSwitchesCounter.NextValue();
                if (_diskBytesCounter != null) totalDisk += _diskBytesCounter.NextValue();
                validSamples++;
            }
            catch { }
        }

        double dwmMem = 0;
        try
        {
            var dwm = Process.GetProcessesByName("dwm").FirstOrDefault();
            if (dwm != null)
            {
                dwmMem = dwm.WorkingSet64 / (1024.0 * 1024.0);
                dwm.Dispose();
            }
        }
        catch { }

        int procCount = 0;
        try
        {
            procCount = Process.GetProcesses().Length;
        }
        catch { }

        return new BenchmarkMetric
        {
            Timestamp = DateTime.UtcNow,
            CpuLoadPercentage = validSamples > 0 ? Math.Round(totalCpu / validSamples, 1) : 0,
            ContextSwitchesPerSecond = validSamples > 0 ? Math.Round(totalCs / validSamples, 0) : 0,
            DiskBytesPerSecond = validSamples > 0 ? Math.Round(totalDisk / validSamples, 0) : 0,
            DwmWorkingSetMB = Math.Round(dwmMem, 1),
            ActiveProcessCount = procCount
        };
    }

    public BenchmarkComparison Compare(BenchmarkMetric before, BenchmarkMetric after)
    {
        return new BenchmarkComparison
        {
            Before = before,
            After = after
        };
    }
}
