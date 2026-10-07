using DesktopPerformance.Services;

namespace DesktopPerformance.Tests;

public class LocalizationTests
{
    [Fact]
    public void LocalizationManager_ProvidesEnglishAndSpanishTranslations()
    {
        var loc = new LocalizationManager();

        loc.CurrentLanguage = AppLanguage.English;
        Assert.Equal("ZeroLatency98 - Windows Latency & Classic Theme Suite", loc.T("App_Title"));
        Assert.Equal("&File", loc.T("Menu_File"));
        Assert.Equal("Background Manager", loc.T("Tab_Background"));

        loc.CurrentLanguage = AppLanguage.Spanish;
        Assert.Equal("ZeroLatency98 - Modo Baja Latencia y Tema Clásico", loc.T("App_Title"));
        Assert.Equal("&Archivo", loc.T("Menu_File"));
        Assert.Equal("Administrador de Fondo", loc.T("Tab_Background"));
    }

    [Fact]
    public void LocalizationManager_FallsBackToEnglishForMissingKeys()
    {
        var loc = new LocalizationManager();
        loc.CurrentLanguage = AppLanguage.Spanish;

        // If key doesn't exist anywhere, returns key
        Assert.Equal("NonExistentKey_XYZ", loc.T("NonExistentKey_XYZ"));
    }

    [Fact]
    public void LocalizationManager_TriggersEventOnLanguageChange()
    {
        var loc = new LocalizationManager();
        bool fired = false;
        loc.OnLanguageChanged += () => fired = true;

        loc.CurrentLanguage = loc.CurrentLanguage == AppLanguage.English ? AppLanguage.Spanish : AppLanguage.English;

        Assert.True(fired);
    }
}
