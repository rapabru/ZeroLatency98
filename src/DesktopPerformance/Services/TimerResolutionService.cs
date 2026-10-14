using System.Diagnostics;
using DesktopPerformance.Native;

namespace DesktopPerformance.Services;

/// <summary>
/// Diagnostic and state payload for system timer resolution.
/// </summary>
public class TimerResolutionInfo
{
    /// <summary>
    /// Current system timer resolution in milliseconds (e.g. 0.500 ms, 1.000 ms, 15.625 ms).
    /// </summary>
    public double CurrentResolutionMs { get; set; }

    /// <summary>
    /// Minimum resolution supported by hardware/kernel in milliseconds (the longest interval, usually ~15.625 ms).
    /// </summary>
    public double MinResolutionMs { get; set; }

    /// <summary>
    /// Maximum resolution supported by hardware/kernel in milliseconds (the shortest interval / highest precision, usually ~0.500 ms).
    /// </summary>
    public double MaxResolutionMs { get; set; }

    /// <summary>
    /// Whether this application currently has a custom high-precision resolution active.
    /// </summary>
    public bool IsCustomActive { get; set; }

    /// <summary>
    /// Target requested resolution in milliseconds if custom is active.
    /// </summary>
    public double TargetResolutionMs { get; set; }
}

/// <summary>
/// Provides fine-grained control over Windows NT timer resolution and multimedia timer periods.
/// Allows setting 0.5 ms / 1.0 ms precision to eliminate scheduling jitter and input latency.
/// </summary>
public class TimerResolutionService : IDisposable
{
    private bool _isCustomActive = false;
    private uint _activeNtResolutionUnits = 0;
    private uint _activeWinmmPeriodMs = 0;
    private double _targetResolutionMs = 0;
    private readonly object _lock = new();

    public bool IsCustomActive => _isCustomActive;
    public double TargetResolutionMs => _targetResolutionMs;

    public event Action<TimerResolutionInfo>? OnResolutionChanged;

    /// <summary>
    /// Queries the current Windows NT timer resolution and hardware capabilities.
    /// </summary>
    public TimerResolutionInfo QueryCurrentResolution()
    {
        lock (_lock)
        {
            var info = new TimerResolutionInfo
            {
                IsCustomActive = _isCustomActive,
                TargetResolutionMs = _targetResolutionMs
            };

            try
            {
                if (Win32Native.NtQueryTimerResolution(out uint minRes, out uint maxRes, out uint curRes) == 0)
                {
                    // NT timer units are 100-nanoseconds (1 ms = 10,000 units)
                    info.CurrentResolutionMs = Math.Round(curRes / 10000.0, 3);
                    info.MinResolutionMs = Math.Round(minRes / 10000.0, 3); // min resolution = maximum interval (15.625 ms)
                    info.MaxResolutionMs = Math.Round(maxRes / 10000.0, 3); // max resolution = minimum interval (0.500 ms)
                }
                else
                {
                    info.CurrentResolutionMs = 15.625;
                    info.MinResolutionMs = 15.625;
                    info.MaxResolutionMs = 0.5;
                }
            }
            catch
            {
                info.CurrentResolutionMs = 15.625;
                info.MinResolutionMs = 15.625;
                info.MaxResolutionMs = 0.5;
            }

            return info;
        }
    }

    /// <summary>
    /// Sets the Windows NT timer resolution to the specified target in milliseconds (e.g. 0.5 ms or 1.0 ms).
    /// </summary>
    public bool SetResolution(double targetMs, out double actualMs)
    {
        lock (_lock)
        {
            actualMs = targetMs;

            // Query limits first
            uint minRes = 156250;
            uint maxRes = 5000;
            try
            {
                Win32Native.NtQueryTimerResolution(out minRes, out maxRes, out _);
            }
            catch { }

            // Target in 100-ns units
            uint desiredUnits = (uint)Math.Clamp((long)Math.Round(targetMs * 10000.0), (long)maxRes, (long)minRes);

            // Clean up previous period if active
            if (_isCustomActive)
            {
                ReleaseCustomResolution();
            }

            try
            {
                // 1. Request via NT Kernel
                int status = Win32Native.NtSetTimerResolution(desiredUnits, true, out uint currentUnits);
                bool success = (status == 0);

                // 2. Also align WinMM multimedia timer subsystem
                uint periodMs = (uint)Math.Max(1, Math.Round(targetMs));
                Win32Native.TimeBeginPeriod(periodMs);

                _isCustomActive = true;
                _activeNtResolutionUnits = desiredUnits;
                _activeWinmmPeriodMs = periodMs;
                _targetResolutionMs = targetMs;

                // Query actual resulting resolution
                if (Win32Native.NtQueryTimerResolution(out _, out _, out uint resultingUnits) == 0)
                {
                    actualMs = Math.Round(resultingUnits / 10000.0, 3);
                }
                else
                {
                    actualMs = targetMs;
                }

                OnResolutionChanged?.Invoke(QueryCurrentResolution());
                return success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to set timer resolution: {ex.Message}");
                actualMs = targetMs;
                return false;
            }
        }
    }

    /// <summary>
    /// Sets the timer resolution to the maximum hardware precision supported (typically 0.500 ms).
    /// </summary>
    public bool SetMaximumResolution(out double actualMs)
    {
        var info = QueryCurrentResolution();
        double maxPrecision = info.MaxResolutionMs > 0 ? info.MaxResolutionMs : 0.5;
        return SetResolution(maxPrecision, out actualMs);
    }

    /// <summary>
    /// Restores the timer resolution to the default Windows clock rate (~15.625 ms).
    /// </summary>
    public bool RestoreDefault(out double currentMs)
    {
        lock (_lock)
        {
            ReleaseCustomResolution();
            var info = QueryCurrentResolution();
            currentMs = info.CurrentResolutionMs;
            OnResolutionChanged?.Invoke(info);
            return true;
        }
    }

    private void ReleaseCustomResolution()
    {
        if (_isCustomActive)
        {
            try
            {
                if (_activeNtResolutionUnits > 0)
                {
                    Win32Native.NtSetTimerResolution(_activeNtResolutionUnits, false, out _);
                }

                if (_activeWinmmPeriodMs > 0)
                {
                    Win32Native.TimeEndPeriod(_activeWinmmPeriodMs);
                }
            }
            catch { }
            finally
            {
                _isCustomActive = false;
                _activeNtResolutionUnits = 0;
                _activeWinmmPeriodMs = 0;
                _targetResolutionMs = 0;
            }
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            ReleaseCustomResolution();
        }
    }
}
