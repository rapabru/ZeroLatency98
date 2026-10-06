using DesktopPerformance.Models;
using DesktopPerformance.Services;

namespace DesktopPerformance.Tests;

public class CoreTests
{
    [Fact]
    public void HardwareInspector_CapturesRealSystemData()
    {
        var inspector = new HardwareInspector();
        var status = inspector.CaptureCurrentHardwareStatus();

        Assert.NotNull(status);
        Assert.False(string.IsNullOrWhiteSpace(status.CpuName));
        Assert.True(status.LogicalCores > 0, "Logical cores must be > 0");
        Assert.True(status.TotalPhysicalMemoryMB > 0, "Physical RAM must be > 0");
        Assert.True(status.AvailablePhysicalMemoryMB > 0, "Available RAM must be > 0");
        Assert.NotNull(status.Displays);
        Assert.NotEmpty(status.Displays);

        var primary = status.Displays.FirstOrDefault(d => d.IsPrimary);
        Assert.NotNull(primary);
        Assert.True(primary.Width > 0);
        Assert.True(primary.Height > 0);
        Assert.True(primary.RefreshRateHz > 0);
    }

    [Fact]
    public void VisualEffectsManager_CapturesVisualState()
    {
        var visualManager = new VisualEffectsManager();
        var state = visualManager.CaptureCurrentVisualState();

        Assert.NotNull(state);
        // Booleans are initialized
        Assert.NotNull(state.WallpaperPath);
    }

    [Fact]
    public void ProcessSupervisor_EnforcesPermanentWhitelist()
    {
        var supervisor = new ProcessSupervisor();

        // Critical processes must be permanently whitelisted
        Assert.True(supervisor.IsWhitelisted("audiodg"));
        Assert.True(supervisor.IsWhitelisted("steam"));
        Assert.True(supervisor.IsWhitelisted("obs64"));
        Assert.True(supervisor.IsWhitelisted("dwm"));
        Assert.True(supervisor.IsWhitelisted("csrss"));
        Assert.True(supervisor.IsWhitelisted("lsass"));

        // Regular background apps must not be whitelisted
        Assert.False(supervisor.IsWhitelisted("OneDrive"));
        Assert.False(supervisor.IsWhitelisted("Discord"));
        Assert.False(supervisor.IsWhitelisted("Spotify"));

        // Attempting to suspend whitelisted process must return false immediately
        Assert.False(supervisor.SuspendProcess("dwm"));
        Assert.False(supervisor.SuspendProcess("steam"));
        Assert.False(supervisor.SuspendProcess("audiodg"));
    }

    [Fact]
    public void SnapshotManager_PerformsTransactionalSerialization()
    {
        string tempSnapshotPath = Path.Combine(Path.GetTempPath(), $"snapshot_test_{Guid.NewGuid():N}.json");
        var snapshotManager = new SnapshotManager(tempSnapshotPath);

        try
        {
            Assert.False(snapshotManager.HasActiveSnapshot());

            var originalSnapshot = new SystemSnapshot
            {
                CreatedAt = DateTime.UtcNow,
                AppliedProfile = ProfileType.LowInterference,
                PriorTransparency = true,
                PriorWindowAnimations = true,
                PriorTaskbarWidgets = true,
                PriorWallpaperPath = @"C:\Windows\Web\Wallpaper\Theme1\img0.jpg"
            };
            originalSnapshot.ServicesPriorStatus["WSearch"] = "Running";
            originalSnapshot.ProcessesPriorPriority["Discord"] = 32; // Normal
            originalSnapshot.ProcessesPriorAffinity["Discord"] = 0xFF;

            // Save
            snapshotManager.SaveSnapshot(originalSnapshot);
            Assert.True(snapshotManager.HasActiveSnapshot());
            Assert.True(File.Exists(tempSnapshotPath));

            // Reload & Verify
            var loaded = snapshotManager.LoadSnapshot();
            Assert.NotNull(loaded);
            Assert.Equal(ProfileType.LowInterference, loaded.AppliedProfile);
            Assert.True(loaded.PriorTransparency);
            Assert.True(loaded.PriorWindowAnimations);
            Assert.Equal(@"C:\Windows\Web\Wallpaper\Theme1\img0.jpg", loaded.PriorWallpaperPath);
            Assert.Equal("Running", loaded.ServicesPriorStatus["WSearch"]);
            Assert.Equal(32, loaded.ProcessesPriorPriority["Discord"]);
            Assert.Equal(0xFF, loaded.ProcessesPriorAffinity["Discord"]);

            // Clear
            snapshotManager.ClearSnapshot();
            Assert.False(snapshotManager.HasActiveSnapshot());
            Assert.False(File.Exists(tempSnapshotPath));
        }
        finally
        {
            if (File.Exists(tempSnapshotPath))
            {
                File.Delete(tempSnapshotPath);
            }
        }
    }

    [Fact]
    public void BenchmarkComparison_EnforcesHonestMetricsThreshold()
    {
        var engine = new BenchmarkEngine();

        // Scenario 1: Variations within noise margin (< 2.0%)
        var before1 = new BenchmarkMetric
        {
            CpuLoadPercentage = 10.0,
            ContextSwitchesPerSecond = 10000,
            DiskBytesPerSecond = 50000,
            DwmWorkingSetMB = 100
        };
        var after1 = new BenchmarkMetric
        {
            CpuLoadPercentage = 9.9, // -1.0%
            ContextSwitchesPerSecond = 9900, // -1.0%
            DiskBytesPerSecond = 49500, // -1.0%
            DwmWorkingSetMB = 99 // -1.0%
        };

        var comp1 = engine.Compare(before1, after1);
        Assert.False(comp1.IsImprovementMeasurable, "Noise below 2.0% must NOT be reported as improvement");
        Assert.Contains("No measurable improvement detected", comp1.SummaryText);

        // Scenario 2: True measurable improvement (>= 2.0% reduction)
        var before2 = new BenchmarkMetric
        {
            CpuLoadPercentage = 15.0,
            ContextSwitchesPerSecond = 20000,
            DiskBytesPerSecond = 100000,
            DwmWorkingSetMB = 120
        };
        var after2 = new BenchmarkMetric
        {
            CpuLoadPercentage = 12.0, // -20%
            ContextSwitchesPerSecond = 5000, // -75%
            DiskBytesPerSecond = 10000, // -90%
            DwmWorkingSetMB = 90 // -25%
        };

        var comp2 = engine.Compare(before2, after2);
        Assert.True(comp2.IsImprovementMeasurable);
        Assert.Contains("Measurable reduction detected", comp2.SummaryText);
        Assert.Contains("Context switches reduced by 75.0%", comp2.SummaryText);
    }
}
