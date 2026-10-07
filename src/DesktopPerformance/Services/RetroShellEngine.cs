using DesktopPerformance.Native;

namespace DesktopPerformance.Services;

public enum RetroThemePreset
{
    Windows95Classic,
    Windows98Plus,
    Windows2000Pro,
    HighContrastOledBlack
}

public class RetroThemeDetails
{
    public RetroThemePreset Preset { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Color BackgroundColor { get; set; }
    public Color ButtonFaceColor { get; set; }
    public Color ActiveCaptionColor { get; set; }
    public Color TextColor { get; set; }
}

public class RetroShellEngine
{
    private static readonly Dictionary<RetroThemePreset, RetroThemeDetails> Themes = new()
    {
        [RetroThemePreset.Windows95Classic] = new RetroThemeDetails
        {
            Preset = RetroThemePreset.Windows95Classic,
            DisplayName = "Windows 95 Classic",
            Description = "Iconic teal desktop (#008080) with stone-gray 3D controls and navy title bars.",
            BackgroundColor = Color.FromArgb(0, 128, 128),
            ButtonFaceColor = Color.FromArgb(192, 192, 192),
            ActiveCaptionColor = Color.FromArgb(0, 0, 128),
            TextColor = Color.Black
        },
        [RetroThemePreset.Windows98Plus] = new RetroThemeDetails
        {
            Preset = RetroThemePreset.Windows98Plus,
            DisplayName = "Windows 98 Plus!",
            Description = "Classic Windows 98 Second Edition styling with two-tone horizontal caption gradients.",
            BackgroundColor = Color.FromArgb(58, 110, 165),
            ButtonFaceColor = Color.FromArgb(192, 192, 192),
            ActiveCaptionColor = Color.FromArgb(16, 132, 208),
            TextColor = Color.Black
        },
        [RetroThemePreset.Windows2000Pro] = new RetroThemeDetails
        {
            Preset = RetroThemePreset.Windows2000Pro,
            DisplayName = "Windows 2000 Professional",
            Description = "Refined corporate slate palette with crisp typography and subtle 3D borders.",
            BackgroundColor = Color.FromArgb(58, 110, 165),
            ButtonFaceColor = Color.FromArgb(212, 208, 200),
            ActiveCaptionColor = Color.FromArgb(10, 36, 106),
            TextColor = Color.Black
        },
        [RetroThemePreset.HighContrastOledBlack] = new RetroThemeDetails
        {
            Preset = RetroThemePreset.HighContrastOledBlack,
            DisplayName = "High Contrast Performance (OLED 0-Nit)",
            Description = "Pure black background (#000000) for true zero-power pixels on OLED monitors.",
            BackgroundColor = Color.Black,
            ButtonFaceColor = Color.FromArgb(32, 32, 32),
            ActiveCaptionColor = Color.FromArgb(64, 64, 64),
            TextColor = Color.White
        }
    };

    public IReadOnlyCollection<RetroThemeDetails> AvailableThemes => Themes.Values;

    public RetroThemeDetails GetTheme(RetroThemePreset preset) => Themes[preset];

    public bool ApplySysColorsTheme(RetroThemePreset preset)
    {
        var theme = Themes[preset];
        int[] elements = new int[]
        {
            Win32Native.COLOR_BACKGROUND,
            Win32Native.COLOR_BTNFACE,
            Win32Native.COLOR_ACTIVECAPTION
        };

        uint[] colors = new uint[]
        {
            ToRgb(theme.BackgroundColor),
            ToRgb(theme.ButtonFaceColor),
            ToRgb(theme.ActiveCaptionColor)
        };

        return Win32Native.SetSysColors(elements.Length, elements, colors);
    }

    private static uint ToRgb(Color color)
    {
        return (uint)(color.R | (color.G << 8) | (color.B << 16));
    }
}
