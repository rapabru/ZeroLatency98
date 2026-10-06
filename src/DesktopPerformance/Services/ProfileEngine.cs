using System.Diagnostics;
using DesktopPerformance.Models;

namespace DesktopPerformance.Services;

public class ProfileEngine
{
    private readonly VisualEffectsManager _visualEffects;
    private readonly ServiceSupervisor _services;
    private readonly ProcessSupervisor _processes;
    private readonly SnapshotManager _snapshotManager;

    public ProfileEngine(
        VisualEffectsManager visualEffects,
        ServiceSupervisor services,
        ProcessSupervisor processes,
        SnapshotManager snapshotManager)
    {
        _visualEffects = visualEffects;
        _services = services;
        _processes = processes;
        _snapshotManager = snapshotManager;
    }

    public ProfileType CurrentProfile { get; private set; } = ProfileType.Normal;

    public bool HasPendingRestore => _snapshotManager.HasActiveSnapshot();

    public void Initialize()
    {
        var snapshot = _snapshotManager.LoadSnapshot();
        if (snapshot != null)
        {
            CurrentProfile = snapshot.AppliedProfile;
        }
        else
        {
            CurrentProfile = ProfileType.Normal;
        }
    }

    public async Task<(bool Success, List<string> Log)> ApplyProfileAsync(ProfileType targetProfile)
    {
        var log = new List<string>();

        if (targetProfile == ProfileType.Normal)
        {
            return await RestoreToNormalAsync();
        }

        // STEP 1: If no baseline snapshot exists, create one BEFORE touching anything
        if (!_snapshotManager.HasActiveSnapshot())
        {
            log.Add("[SNAPSHOT] Capturing baseline system state before applying profile...");
            var currentVisuals = _visualEffects.CaptureCurrentVisualState();
            var baseline = new SystemSnapshot
            {
                CreatedAt = DateTime.UtcNow,
                AppliedProfile = targetProfile,
                PriorTransparency = currentVisuals.TransparencyEnabled,
                PriorWindowAnimations = currentVisuals.WindowAnimationsEnabled,
                PriorTaskbarWidgets = currentVisuals.TaskbarWidgetsEnabled,
                PriorWallpaperPath = currentVisuals.WallpaperPath
            };

            // Capture services
            var activeServices = _services.GetManagedServices();
            foreach (var s in activeServices)
            {
                baseline.ServicesPriorStatus[s.ServiceName] = s.Status;
            }

            // Capture managed app processes
            var activeApps = _processes.ScanManagedApps();
            foreach (var app in activeApps)
            {
                if (app.IsWhitelisted || app.CurrentState == ProcessExecutionState.NotRunning)
                    continue;

                var procs = Process.GetProcessesByName(app.ProcessName);
                if (procs.Length > 0)
                {
                    try
                    {
                        baseline.ProcessesPriorPriority[app.ProcessName] = (int)procs[0].PriorityClass;
                        baseline.ProcessesPriorAffinity[app.ProcessName] = (long)procs[0].ProcessorAffinity;
                    }
                    catch { }
                }
            }

            _snapshotManager.SaveSnapshot(baseline);
            log.Add($"[SNAPSHOT] Baseline successfully saved to: {_snapshotManager.SnapshotFilePath}");
        }

        // STEP 2: Apply Visual Changes (SAFE)
        log.Add("[VISUAL] Disabling DWM transparency and acrylic blurs...");
        _visualEffects.ApplyTransparency(false);

        log.Add("[VISUAL] Disabling shell window animations...");
        _visualEffects.ApplyWindowAnimations(false);

        log.Add("[VISUAL] Hiding Taskbar Widgets & preventing WebView2 background autostart...");
        _visualEffects.ApplyTaskbarWidgets(false);

        // STEP 3: Manage Windows Services
        log.Add("[SERVICES] Pausing Windows Search Indexer (WSearch)...");
        if (_services.PauseOrStopService("WSearch", out string svcErr))
        {
            log.Add("[SERVICES] WSearch service paused successfully.");
        }
        else
        {
            log.Add($"[SERVICES] Note on WSearch: {svcErr}");
        }

        if (targetProfile == ProfileType.MaxResponse)
        {
            // In MaxResponse, attempt to pause peripheral RGB services if present
            foreach (var svcName in new[] { "Corsair.Service", "Razer Synapse Service", "ArmouryCrate.Service" })
            {
                if (_services.PauseOrStopService(svcName, out string rgbErr))
                {
                    log.Add($"[SERVICES] Paused RGB/telemetry service: {svcName}");
                }
            }
        }

        // STEP 4: Manage Background Processes
        bool isolateCores = (targetProfile == ProfileType.MaxResponse);
        var apps = _processes.ScanManagedApps();

        foreach (var app in apps)
        {
            if (app.IsWhitelisted || app.CurrentState == ProcessExecutionState.NotRunning)
                continue;

            // Cloud sync: OneDrive, Dropbox
            if (app.ProcessName.Equals("OneDrive", StringComparison.OrdinalIgnoreCase) ||
                app.ProcessName.Equals("Dropbox", StringComparison.OrdinalIgnoreCase))
            {
                if (_processes.SuspendProcess(app.ProcessName))
                {
                    log.Add($"[PROCESS] Suspended sync engine: {app.DisplayName}");
                }
            }
            else
            {
                // Lower priority to PROCESS_MODE_BACKGROUND_BEGIN and isolate CPU cores
                if (_processes.SetBackgroundModeAndAffinity(app.ProcessName, isolateCores, out _, out _))
                {
                    string affText = isolateCores ? " + isolated to secondary CPU cores" : "";
                    log.Add($"[PROCESS] Set background priority{affText}: {app.DisplayName}");
                }
            }
        }

        CurrentProfile = targetProfile;
        log.Add($"[SUCCESS] Profile '{targetProfile}' applied successfully.");
        return (true, log);
    }

    public async Task<(bool Success, List<string> Log)> RestoreToNormalAsync()
    {
        await Task.Yield();
        var log = new List<string>();
        log.Add("[RESTORE] Restoring system to original baseline...");

        var snapshot = _snapshotManager.LoadSnapshot();

        // 1. Restore Visual Effects
        bool restoreTransparency = snapshot?.PriorTransparency ?? true;
        bool restoreAnimations = snapshot?.PriorWindowAnimations ?? true;
        bool restoreWidgets = snapshot?.PriorTaskbarWidgets ?? true;

        log.Add($"[VISUAL] Restoring Transparency: {restoreTransparency}...");
        _visualEffects.ApplyTransparency(restoreTransparency);

        log.Add($"[VISUAL] Restoring Window Animations: {restoreAnimations}...");
        _visualEffects.ApplyWindowAnimations(restoreAnimations);

        log.Add($"[VISUAL] Restoring Taskbar Widgets: {restoreWidgets}...");
        _visualEffects.ApplyTaskbarWidgets(restoreWidgets);

        // 2. Restore Services
        log.Add("[SERVICES] Resuming Windows Search Indexer (WSearch)...");
        _services.ResumeOrStartService("WSearch", out _);

        if (snapshot != null)
        {
            foreach (var kvp in snapshot.ServicesPriorStatus)
            {
                if (kvp.Key != "WSearch" && kvp.Value.Equals("Running", StringComparison.OrdinalIgnoreCase))
                {
                    _services.ResumeOrStartService(kvp.Key, out _);
                    log.Add($"[SERVICES] Resumed service: {kvp.Key}");
                }
            }
        }

        // 3. Restore Processes
        var apps = _processes.ScanManagedApps();
        foreach (var app in apps)
        {
            if (app.IsWhitelisted)
                continue;

            // Resume suspended
            _processes.ResumeProcess(app.ProcessName);

            // Restore normal priority and affinity
            int priorPrio = (int)ProcessPriorityClass.Normal;
            long priorAff = -1;

            if (snapshot != null && snapshot.ProcessesPriorPriority.TryGetValue(app.ProcessName, out int p))
                priorPrio = p;
            if (snapshot != null && snapshot.ProcessesPriorAffinity.TryGetValue(app.ProcessName, out long a))
                priorAff = a;

            _processes.RestoreNormalModeAndAffinity(app.ProcessName, priorPrio, priorAff);
        }
        log.Add("[PROCESS] Restored normal priority and CPU affinity for all background processes.");

        // 4. Clear Snapshot
        _snapshotManager.ClearSnapshot();
        CurrentProfile = ProfileType.Normal;
        log.Add("[SUCCESS] System fully restored to Normal baseline.");

        return (true, log);
    }
}
