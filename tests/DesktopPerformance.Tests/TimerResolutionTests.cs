using DesktopPerformance.Services;
using Xunit;

namespace DesktopPerformance.Tests;

public class TimerResolutionTests
{
    [Fact]
    public void QueryCurrentResolution_ReturnsPlausibleValues()
    {
        using var service = new TimerResolutionService();
        var info = service.QueryCurrentResolution();

        Assert.True(info.CurrentResolutionMs > 0, "Current resolution must be positive");
        Assert.True(info.MinResolutionMs >= info.MaxResolutionMs, "Min resolution (interval) should be >= Max resolution (precision)");
        Assert.True(info.MaxResolutionMs <= 1.5, "Maximum hardware resolution should be 1.0ms or finer on Windows NT");
        Assert.True(info.MinResolutionMs >= 5.0, "Minimum interval should typically be 15.625ms");
    }

    [Fact]
    public void SetResolution_AppliesResolutionAndTracksState()
    {
        using var service = new TimerResolutionService();
        bool eventFired = false;
        double eventVal = 0;
        service.OnResolutionChanged += (info) =>
        {
            eventFired = true;
            eventVal = info.CurrentResolutionMs;
        };

        try
        {
            bool ok = service.SetResolution(1.0, out double actualMs);
            Assert.True(ok, "SetResolution(1.0) should succeed on Windows NT");
            Assert.True(actualMs <= 1.05, $"Expected resolution <= 1.05ms, but got {actualMs}ms");
            Assert.True(service.IsCustomActive);
            Assert.True(eventFired);
            Assert.True(eventVal <= 1.05);
        }
        finally
        {
            service.RestoreDefault(out _);
        }
    }

    [Fact]
    public void SetMaximumResolution_AppliesTightestResolution()
    {
        using var service = new TimerResolutionService();

        try
        {
            bool ok = service.SetMaximumResolution(out double actualMs);
            Assert.True(ok, "SetMaximumResolution should succeed");
            Assert.True(actualMs <= 1.05, $"Expected tight resolution <= 1.05ms, but got {actualMs}ms");
            Assert.True(service.IsCustomActive);
        }
        finally
        {
            service.RestoreDefault(out _);
        }
    }

    [Fact]
    public void RestoreDefault_ReleasesCustomResolution()
    {
        using var service = new TimerResolutionService();
        service.SetMaximumResolution(out _);
        Assert.True(service.IsCustomActive);

        bool restored = service.RestoreDefault(out double currentMs);
        Assert.True(restored);
        Assert.False(service.IsCustomActive);
        Assert.True(currentMs > 0);
    }

    [Fact]
    public void Dispose_AutomaticallyRestoresDefault()
    {
        var service = new TimerResolutionService();
        service.SetMaximumResolution(out _);
        Assert.True(service.IsCustomActive);

        service.Dispose();
        Assert.False(service.IsCustomActive);
    }

    [Fact]
    public void LocalizationManager_ContainsTimerKeys()
    {
        var loc = new LocalizationManager();

        loc.CurrentLanguage = AppLanguage.English;
        Assert.Contains("Timer Resolution", loc.T("Timer_Group"));
        Assert.Contains("0.5 ms", loc.T("Timer_SetMaxBtn"));

        loc.CurrentLanguage = AppLanguage.Spanish;
        Assert.Contains("Temporizador", loc.T("Timer_Group"));
        Assert.Contains("0.5 ms", loc.T("Timer_SetMaxBtn"));
    }
}
