using System.Diagnostics;
using DesktopPerformance.Models;
using DesktopPerformance.Native;

namespace DesktopPerformance.Services;

public class ProcessSupervisor
{
    // Permanent Whitelist: NEVER touch under any circumstance
    private static readonly HashSet<string> PermanentWhitelist = new(StringComparer.OrdinalIgnoreCase)
    {
        "audiodg",          // Windows Audio Device Graph Isolation
        "nvcontainer",      // NVIDIA Display Driver Container
        "nvspcaps64",       // NVIDIA Shadowplay
        "amdrsserv",        // AMD Radeon Software Host
        "amdow",            // AMD Overlay
        "steam",            // Steam client core (touching this breaks active games with Steamworks DRM)
        "obs64",            // OBS Studio 64-bit
        "obs32",            // OBS Studio 32-bit
        "dwm",              // Desktop Window Manager (MUST NEVER BE SUSPENDED)
        "csrss",            // Client Server Runtime Process
        "lsass",            // Local Security Authority
        "smss",             // Session Manager
        "winlogon",         // Windows Logon
        "services"          // Service Control Manager
    };

    private static readonly (string Name, string ProcessName, string Description, bool IsWhitelisted)[] TargetApps = new[]
    {
        ("OneDrive", "OneDrive", "Microsoft cloud sync; performs continuous disk indexing and network traffic.", false),
        ("Dropbox", "Dropbox", "Cloud file synchronization agent.", false),
        ("Discord", "Discord", "Hardware-accelerated Electron chat client; high GPU composition and wakeups.", false),
        ("Spotify", "Spotify", "Hardware-accelerated music player and background streaming helper.", false),
        ("Steam Web Helper", "steamwebhelper", "Chromium web renderer for Steam store/chat; safe to lower priority.", false),
        ("Epic Games", "EpicGamesLauncher", "Epic Games Store background client.", false),
        ("Adobe CC Process", "CCXProcess", "Adobe Creative Cloud background sync and font manager.", false),
        ("Adobe Core Sync", "CoreSync", "Adobe Creative Cloud file synchronization engine.", false),
        ("Google Chrome (Background)", "chrome", "Chrome background helper processes running when window is closed.", false),
        ("Microsoft Edge (Background)", "msedge", "Edge background helper processes and startup boost.", false),
        ("OBS Studio", "obs64", "Broadcasting & streaming software (PROTECTED).", true),
        ("Steam Game Client", "steam", "Main Steam runtime and DRM engine (PROTECTED).", true)
    };

    public List<ManagedAppInfo> ScanManagedApps()
    {
        var result = new List<ManagedAppInfo>();
        var runningProcesses = Process.GetProcesses();
        var processGroup = runningProcesses.GroupBy(p => p.ProcessName, StringComparer.OrdinalIgnoreCase)
                                           .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var target in TargetApps)
        {
            var appInfo = new ManagedAppInfo
            {
                DisplayName = target.Name,
                ProcessName = target.ProcessName,
                Description = target.Description,
                IsWhitelisted = target.IsWhitelisted || PermanentWhitelist.Contains(target.ProcessName),
                Policy = (target.IsWhitelisted || PermanentWhitelist.Contains(target.ProcessName)) 
                    ? AppActionPolicy.NeverTouch 
                    : AppActionPolicy.Default
            };

            if (processGroup.TryGetValue(target.ProcessName, out var procs))
            {
                appInfo.RunningPids = procs.Select(p => p.Id).ToList();
                appInfo.CurrentState = appInfo.IsWhitelisted ? ProcessExecutionState.Whitelisted : ProcessExecutionState.RunningNormal;
                appInfo.Details = $"{procs.Count} instance(s) running (PIDs: {string.Join(", ", appInfo.RunningPids.Take(3))}{(procs.Count > 3 ? "..." : "")})";
            }
            else
            {
                appInfo.CurrentState = ProcessExecutionState.NotRunning;
                appInfo.Details = "Not active";
            }

            result.Add(appInfo);
        }

        // Cleanup process handles
        foreach (var p in runningProcesses)
        {
            p.Dispose();
        }

        return result;
    }

    public bool IsWhitelisted(string processName)
    {
        return PermanentWhitelist.Contains(processName);
    }

    public bool SetBackgroundModeAndAffinity(string processName, bool isolateCores, out int priorPriority, out long priorAffinity)
    {
        priorPriority = (int)ProcessPriorityClass.Normal;
        priorAffinity = -1;

        if (IsWhitelisted(processName))
            return false;

        var procs = Process.GetProcessesByName(processName);
        if (procs.Length == 0)
            return false;

        bool anySuccess = false;
        int totalCores = Environment.ProcessorCount;

        // Calculate secondary core mask: if 8 cores (0xFF), secondary cores = upper 4 cores (0xF0)
        long secondaryCoreMask = -1;
        if (isolateCores && totalCores >= 4)
        {
            int halfCores = totalCores / 2;
            long mask = 0;
            for (int i = halfCores; i < totalCores; i++)
            {
                mask |= (1L << i);
            }
            secondaryCoreMask = mask;
        }

        foreach (var p in procs)
        {
            try
            {
                priorPriority = (int)p.PriorityClass;
                priorAffinity = (long)p.ProcessorAffinity;

                IntPtr hProcess = Win32Native.OpenProcess(
                    Win32Native.PROCESS_SET_INFORMATION | Win32Native.PROCESS_QUERY_INFORMATION, 
                    false, 
                    p.Id);

                if (hProcess != IntPtr.Zero)
                {
                    try
                    {
                        // Set Windows Background Mode (automatically lowers I/O and CPU priority)
                        Win32Native.SetPriorityClass(hProcess, Win32Native.PROCESS_MODE_BACKGROUND_BEGIN);

                        // Restrict CPU affinity to secondary cores if requested
                        if (secondaryCoreMask > 0)
                        {
                            Win32Native.SetProcessAffinityMask(hProcess, (UIntPtr)secondaryCoreMask);
                        }

                        anySuccess = true;
                    }
                    finally
                    {
                        Win32Native.CloseHandle(hProcess);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adjusting process {processName} (PID {p.Id}): {ex.Message}");
            }
            finally
            {
                p.Dispose();
            }
        }

        return anySuccess;
    }

    public bool RestoreNormalModeAndAffinity(string processName, int priorPriority, long priorAffinity)
    {
        var procs = Process.GetProcessesByName(processName);
        if (procs.Length == 0)
            return false;

        bool anySuccess = false;
        foreach (var p in procs)
        {
            try
            {
                IntPtr hProcess = Win32Native.OpenProcess(
                    Win32Native.PROCESS_SET_INFORMATION | Win32Native.PROCESS_QUERY_INFORMATION, 
                    false, 
                    p.Id);

                if (hProcess != IntPtr.Zero)
                {
                    try
                    {
                        // End background mode
                        Win32Native.SetPriorityClass(hProcess, Win32Native.PROCESS_MODE_BACKGROUND_END);

                        if (priorPriority > 0)
                        {
                            p.PriorityClass = (ProcessPriorityClass)priorPriority;
                        }

                        if (priorAffinity > 0)
                        {
                            p.ProcessorAffinity = (IntPtr)priorAffinity;
                        }

                        anySuccess = true;
                    }
                    finally
                    {
                        Win32Native.CloseHandle(hProcess);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error restoring process {processName} (PID {p.Id}): {ex.Message}");
            }
            finally
            {
                p.Dispose();
            }
        }

        return anySuccess;
    }

    public bool SuspendProcess(string processName)
    {
        if (IsWhitelisted(processName))
            return false;

        var procs = Process.GetProcessesByName(processName);
        if (procs.Length == 0)
            return false;

        bool anySuccess = false;
        foreach (var p in procs)
        {
            try
            {
                IntPtr hProcess = Win32Native.OpenProcess(Win32Native.PROCESS_SUSPEND_RESUME, false, p.Id);
                if (hProcess != IntPtr.Zero)
                {
                    try
                    {
                        int ntStatus = Win32Native.NtSuspendProcess(hProcess);
                        if (ntStatus == 0) anySuccess = true;
                    }
                    finally
                    {
                        Win32Native.CloseHandle(hProcess);
                    }
                }
            }
            catch { }
            finally { p.Dispose(); }
        }

        return anySuccess;
    }

    public bool ResumeProcess(string processName)
    {
        var procs = Process.GetProcessesByName(processName);
        if (procs.Length == 0)
            return false;

        bool anySuccess = false;
        foreach (var p in procs)
        {
            try
            {
                IntPtr hProcess = Win32Native.OpenProcess(Win32Native.PROCESS_SUSPEND_RESUME, false, p.Id);
                if (hProcess != IntPtr.Zero)
                {
                    try
                    {
                        int ntStatus = Win32Native.NtResumeProcess(hProcess);
                        if (ntStatus == 0) anySuccess = true;
                    }
                    finally
                    {
                        Win32Native.CloseHandle(hProcess);
                    }
                }
            }
            catch { }
            finally { p.Dispose(); }
        }

        return anySuccess;
    }
}
