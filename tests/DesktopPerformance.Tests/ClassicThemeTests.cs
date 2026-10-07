using DesktopPerformance.Services;
using Xunit;

namespace DesktopPerformance.Tests;

public class ClassicThemeTests
{
    private readonly ClassicThemeManager _manager;

    public ClassicThemeTests()
    {
        var visualEffects = new VisualEffectsManager();
        _manager = new ClassicThemeManager(visualEffects);
    }

    [Fact]
    public void Definitions_AllVariantsAreDefined()
    {
        Assert.True(ClassicThemeManager.Definitions.ContainsKey(ClassicThemeVariant.Windows95Classic));
        Assert.True(ClassicThemeManager.Definitions.ContainsKey(ClassicThemeVariant.Windows98Plus));
        Assert.True(ClassicThemeManager.Definitions.ContainsKey(ClassicThemeVariant.Windows2000Pro));
        Assert.True(ClassicThemeManager.Definitions.ContainsKey(ClassicThemeVariant.Windows98HighContrastFlat));
    }

    [Fact]
    public void GenerateThemeContent_Windows95_ContainsIconicTealAndNavyColors()
    {
        string content = _manager.GenerateThemeContent(ClassicThemeVariant.Windows95Classic);

        Assert.Contains("[Theme]", content);
        Assert.Contains("[Control Panel\\Desktop]", content);
        Assert.Contains("[Control Panel\\Colors]", content);
        Assert.Contains("[VisualStyles]", content);

        // Win95 Teal desktop is RGB 0 128 128
        Assert.Contains("Background=0 128 128", content);
        // Win95 Navy active title is RGB 0 0 128
        Assert.Contains("ActiveTitle=0 0 128", content);
        // Win95 Stone-gray button face is RGB 192 192 192
        Assert.Contains("ButtonFace=192 192 192", content);
    }

    [Fact]
    public void GenerateThemeContent_Windows98Plus_ContainsCaptionGradient()
    {
        string content = _manager.GenerateThemeContent(ClassicThemeVariant.Windows98Plus);

        Assert.Contains("Background=0 128 128", content);
        Assert.Contains("ActiveTitle=0 0 128", content);
        // Win98 gradient caption end is RGB 16 132 208
        Assert.Contains("GradientActiveTitle=16 132 208", content);
    }

    [Fact]
    public void GenerateThemeContent_HighContrastFlat_ContainsBlackBackground()
    {
        string content = _manager.GenerateThemeContent(ClassicThemeVariant.Windows98HighContrastFlat);

        // Zero-nit OLED black background is RGB 0 0 0
        Assert.Contains("Background=0 0 0", content);
        Assert.Contains("HighContrast=1", content);
    }

    [Fact]
    public void SaveThemeFile_CreatesThemeFileOnDisk()
    {
        string path = _manager.SaveThemeFile(ClassicThemeVariant.Windows95Classic);

        Assert.True(File.Exists(path));
        string text = File.ReadAllText(path);
        Assert.Contains("DisplayName=Windows 95 Classic", text);
    }

    [Fact]
    public void CompanionTools_ProvideValidGitHubUrls()
    {
        var retroBar = _manager.GetRetroBarInfo();
        var openShell = _manager.GetOpenShellInfo();

        Assert.Equal("RetroBar", retroBar.Name);
        Assert.StartsWith("https://github.com/dremin/RetroBar", retroBar.DownloadUrl);

        Assert.Equal("Open-Shell", openShell.Name);
        Assert.StartsWith("https://github.com/Open-Shell/Open-Shell-Menu", openShell.DownloadUrl);
    }
}
