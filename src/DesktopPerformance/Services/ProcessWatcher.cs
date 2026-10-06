using System.Diagnostics;
using DesktopPerformance.Models;

namespace DesktopPerformance.Services;

public class ProcessWatcher : IDisposable
{
    private readonly SmartProfileManager _profileManager;
    private readonly ProfileEngine _profileEngine;
    private System.Threading.Timer? _timer;
    private readonly HashSet<string> _activeDetectedGames = new(StringComparer.OrdinalIgnoreCase);
    private bool _isChecking = false;

    public AutoGameDetectionMode Mode { get; set; } = AutoGameDetectionMode.Auto;

    public event Action<string, SmartProfile>? OnGameStarted;
    public event Action<string>? OnGameExited;
    public event Action<string, SmartProfile>? OnGameDetectedManual;

    public ProcessWatcher(SmartProfileManager profileManager, ProfileEngine profileEngine)
    {
        _profileManager = profileManager;
        _profileEngine = profileEngine;
    }

    public void Start(int intervalMs = 2000)
    {
        _timer?.Dispose();
        _timer = new System.Threading.Timer(CheckProcessesCallback, null, 1000, intervalMs);
    }

    public void Stop()
    {
        _timer?.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private async void CheckProcessesCallback(object? state)
    {
        if (_isChecking || Mode == AutoGameDetectionMode.Disabled)
            return;

        _isChecking = true;
        try
        {
            var profilesWithTriggers = _profileManager.Profiles
                .Where(p => !string.IsNullOrWhiteSpace(p.TriggerProcessName))
                .ToList();

            if (profilesWithTriggers.Count == 0)
                return;

            foreach (var profile in profilesWithTriggers)
            {
                string procName = profile.TriggerProcessName;
                if (procName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    procName = procName.Substring(0, procName.Length - 4);

                var running = Process.GetProcessesByName(procName);
                bool isCurrentlyRunning = running.Length > 0;

                // Dispose process handles
                foreach (var p in running) p.Dispose();

                if (isCurrentlyRunning)
                {
                    if (!_activeDetectedGames.Contains(procName))
                    {
                        // Game just started!
                        _activeDetectedGames.Add(procName);

                        if (Mode == AutoGameDetectionMode.Auto)
                        {
                            await _profileEngine.ApplyProfileAsync(ProfileType.MaxResponse);
                            OnGameStarted?.Invoke(procName, profile);
                        }
                        else if (Mode == AutoGameDetectionMode.Manual)
                        {
                            OnGameDetectedManual?.Invoke(procName, profile);
                        }
                    }
                }
                else
                {
                    if (_activeDetectedGames.Contains(procName))
                    {
                        // Game just exited!
                        _activeDetectedGames.Remove(procName);

                        if (Mode == AutoGameDetectionMode.Auto)
                        {
                            await _profileEngine.RestoreToNormalAsync();
                            OnGameExited?.Invoke(procName);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ProcessWatcher check error: {ex.Message}");
        }
        finally
        {
            _isChecking = false;
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
