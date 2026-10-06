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

    [Fact]
    public void DesktopBusyAnalyzer_ReturnsValidDiagnosticFindings()
    {
        var hwInspector = new HardwareInspector();
        var analyzer = new DesktopBusyAnalyzer(hwInspector);

        var findings = analyzer.AnalyzeCurrentInterference();
        Assert.NotNull(findings);
        Assert.NotEmpty(findings);

        foreach (var f in findings)
        {
            Assert.False(string.IsNullOrWhiteSpace(f.Category));
            Assert.False(string.IsNullOrWhiteSpace(f.HumanExplanation));
        }
    }

    [Fact]
    public void SmartProfileManager_FindsMatchingProfileForGameTrigger()
    {
        string tempProfilesPath = Path.Combine(Path.GetTempPath(), $"smart_profiles_test_{Guid.NewGuid():N}.json");
        try
        {
            var manager = new SmartProfileManager(tempProfilesPath);
            Assert.NotEmpty(manager.Profiles);

            // CS2 trigger test
            var cs2Profile = manager.FindProfileForProcess("cs2.exe");
            Assert.NotNull(cs2Profile);
            Assert.Equal("CS2 / Competitive Gaming", cs2Profile.Name);
            Assert.True(cs2Profile.IsolateCpuCores);
            Assert.True(cs2Profile.DisableTransparency);

            // Case insensitive and without .exe
            var cs2WithoutExe = manager.FindProfileForProcess("cs2");
            Assert.NotNull(cs2WithoutExe);
            Assert.Equal(cs2Profile.Id, cs2WithoutExe.Id);
        }
        finally
        {
            if (File.Exists(tempProfilesPath)) File.Delete(tempProfilesPath);
        }
    }

    [Fact]
    public void BackgroundDatabase_ContainsComprehensiveClassifiedEntries()
    {
        var db = new BackgroundDatabase();
        var entries = db.GetAllEntries();

        Assert.NotEmpty(entries);
        Assert.Contains(entries, e => e.ProcessName == "OneDrive" && e.Classification == SafetyClassification.Safe);
        Assert.Contains(entries, e => e.ProcessName == "Discord" && e.Classification == SafetyClassification.LowRisk);
        Assert.Contains(entries, e => e.ProcessName == "steam" && e.Classification == SafetyClassification.DoNotTouch);
        Assert.Contains(entries, e => e.ProcessName == "dwm" && e.Classification == SafetyClassification.DoNotTouch);
        Assert.Contains(entries, e => e.ProcessName == "Corsair.Service" && e.Classification == SafetyClassification.MediumRisk);
    }

    [Fact]
    public void ExportManager_ExportsAndImportsProfilesAndBenchmark()
    {
        var exporter = new ExportManager();
        string tempBenchPath = Path.Combine(Path.GetTempPath(), $"bench_export_{Guid.NewGuid():N}.json");
        string tempProfilesPath = Path.Combine(Path.GetTempPath(), $"profiles_export_{Guid.NewGuid():N}.json");

        try
        {
            var comparison = new BenchmarkComparison
            {
                Before = new BenchmarkMetric { CpuLoadPercentage = 10, ContextSwitchesPerSecond = 5000 },
                After = new BenchmarkMetric { CpuLoadPercentage = 5, ContextSwitchesPerSecond = 2000 }
            };

            bool benchOk = exporter.ExportBenchmarkSession(comparison, tempBenchPath);
            Assert.True(benchOk);
            Assert.True(File.Exists(tempBenchPath));

            var profiles = new List<SmartProfile>
            {
                new SmartProfile { Name = "Test Profile", TriggerProcessName = "test.exe" }
            };

            bool profOk = exporter.ExportProfiles(profiles, tempProfilesPath);
            Assert.True(profOk);
            Assert.True(File.Exists(tempProfilesPath));

            var imported = exporter.ImportProfiles(tempProfilesPath);
            Assert.NotNull(imported);
            Assert.Single(imported);
            Assert.Equal("Test Profile", imported[0].Name);
        }
        finally
        {
            if (File.Exists(tempBenchPath)) File.Delete(tempBenchPath);
            if (File.Exists(tempProfilesPath)) File.Delete(tempProfilesPath);
        }
    }
}
