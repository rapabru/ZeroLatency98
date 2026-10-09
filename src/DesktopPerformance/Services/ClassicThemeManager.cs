using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using Microsoft.Win32;
using DesktopPerformance.Native;

namespace DesktopPerformance.Services;

public enum ClassicThemeVariant
{
    Windows95Classic,
    Windows98Plus,
    Windows2000Pro,
    Windows98HighContrastFlat
}

public class ClassicThemeDefinition
{
    public ClassicThemeVariant Variant { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Color BackgroundColor { get; set; }
    public Color ButtonFaceColor { get; set; }
    public Color ActiveTitleColor { get; set; }
    public Color GradientTitleColor { get; set; }
    public Color InactiveTitleColor { get; set; }
    public Color WindowColor { get; set; }
    public Color WindowTextColor { get; set; }
    public Color TitleTextColor { get; set; }
    public Color HilightColor { get; set; }
    public Color HilightTextColor { get; set; }
    public bool HighContrast { get; set; }
}

public class CompanionToolInfo
{
    public string Name { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
    public bool IsInstalled { get; set; }
    public string ExecutablePath { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ClassicThemeManager
{
    private readonly VisualEffectsManager _visualEffects;
    private readonly string _themesDirectory;
    private readonly string _backupThemePath;

    public bool IsClassicThemeActive { get; private set; }
    public ClassicThemeVariant? ActiveVariant { get; private set; }

    public event Action<ClassicThemeVariant>? OnThemeApplied;
    public event Action? OnThemeRestored;

    public static readonly Dictionary<ClassicThemeVariant, ClassicThemeDefinition> Definitions = new()
    {
        [ClassicThemeVariant.Windows95Classic] = new ClassicThemeDefinition
        {
            Variant = ClassicThemeVariant.Windows95Classic,
            DisplayName = "Windows 95 Classic",
            Description = "Iconic teal desktop (#008080), stone-gray 3D controls (#C0C0C0), and solid navy blue title bars (#000080).",
            BackgroundColor = Color.FromArgb(0, 128, 128),
            ButtonFaceColor = Color.FromArgb(192, 192, 192),
            ActiveTitleColor = Color.FromArgb(0, 0, 128),
            GradientTitleColor = Color.FromArgb(0, 0, 128),
            InactiveTitleColor = Color.FromArgb(128, 128, 128),
            WindowColor = Color.White,
            WindowTextColor = Color.Black,
            TitleTextColor = Color.White,
            HilightColor = Color.FromArgb(0, 0, 128),
            HilightTextColor = Color.White,
            HighContrast = true
        },
        [ClassicThemeVariant.Windows98Plus] = new ClassicThemeDefinition
        {
            Variant = ClassicThemeVariant.Windows98Plus,
            DisplayName = "Windows 98 Plus! (SE)",
            Description = "Classic Windows 98 Second Edition styling with two-tone horizontal caption gradient (#000080 to #1084D0) and teal background.",
            BackgroundColor = Color.FromArgb(0, 128, 128),
            ButtonFaceColor = Color.FromArgb(192, 192, 192),
            ActiveTitleColor = Color.FromArgb(0, 0, 128),
            GradientTitleColor = Color.FromArgb(16, 132, 208),
            InactiveTitleColor = Color.FromArgb(128, 128, 128),
            WindowColor = Color.White,
            WindowTextColor = Color.Black,
            TitleTextColor = Color.White,
            HilightColor = Color.FromArgb(0, 0, 128),
            HilightTextColor = Color.White,
            HighContrast = true
        },
        [ClassicThemeVariant.Windows2000Pro] = new ClassicThemeDefinition
        {
            Variant = ClassicThemeVariant.Windows2000Pro,
            DisplayName = "Windows 2000 Professional",
            Description = "Corporate slate palette (#D4D0C8) with deep royal blue caption gradient (#0A246A to #A6CAF0) and crisp 3D borders.",
            BackgroundColor = Color.FromArgb(58, 110, 165),
            ButtonFaceColor = Color.FromArgb(212, 208, 200),
            ActiveTitleColor = Color.FromArgb(10, 36, 106),
            GradientTitleColor = Color.FromArgb(166, 202, 240),
            InactiveTitleColor = Color.FromArgb(128, 128, 128),
            WindowColor = Color.White,
            WindowTextColor = Color.Black,
            TitleTextColor = Color.White,
            HilightColor = Color.FromArgb(10, 36, 106),
            HilightTextColor = Color.White,
            HighContrast = true
        },
        [ClassicThemeVariant.Windows98HighContrastFlat] = new ClassicThemeDefinition
        {
            Variant = ClassicThemeVariant.Windows98HighContrastFlat,
            DisplayName = "Win98 High-Contrast Flat (OLED / Max FPS)",
            Description = "Pure black desktop (#000000) for zero OLED power draw, high-contrast stone-gray controls, and lowest possible DWM overhead.",
            BackgroundColor = Color.Black,
            ButtonFaceColor = Color.FromArgb(32, 32, 32),
            ActiveTitleColor = Color.FromArgb(0, 128, 128),
            GradientTitleColor = Color.FromArgb(0, 128, 128),
            InactiveTitleColor = Color.FromArgb(64, 64, 64),
            WindowColor = Color.FromArgb(16, 16, 16),
            WindowTextColor = Color.White,
            TitleTextColor = Color.White,
            HilightColor = Color.FromArgb(0, 128, 128),
            HilightTextColor = Color.White,
            HighContrast = true
        }
    };

    public ClassicThemeManager(VisualEffectsManager visualEffects)
    {
        _visualEffects = visualEffects;

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string folder = Path.Combine(appData, "DesktopPerformance98");
        _themesDirectory = Path.Combine(folder, "Themes");
        _backupThemePath = Path.Combine(folder, "backup_theme.theme");

        Directory.CreateDirectory(_themesDirectory);
    }

    public string GenerateThemeContent(ClassicThemeVariant variant)
    {
        var def = Definitions[variant];
        var sb = new StringBuilder();

        sb.AppendLine("; Desktop Performance Mode - Windows 95/98 Classic Theme");
        sb.AppendLine("; Generated safely without system file patching or injection");
        sb.AppendLine("[Theme]");
        sb.AppendLine($"DisplayName={def.DisplayName}");
        sb.AppendLine();
        sb.AppendLine("[Control Panel\\Desktop]");
        sb.AppendLine("Wallpaper=");
        sb.AppendLine("TileWallpaper=0");
        sb.AppendLine("WallpaperStyle=0");
        sb.AppendLine("Pattern=");
        sb.AppendLine();
        sb.AppendLine("[Control Panel\\Colors]");
        sb.AppendLine($"Background={def.BackgroundColor.R} {def.BackgroundColor.G} {def.BackgroundColor.B}");
        sb.AppendLine($"ButtonFace={def.ButtonFaceColor.R} {def.ButtonFaceColor.G} {def.ButtonFaceColor.B}");
        sb.AppendLine($"ButtonHilight=255 255 255");
        sb.AppendLine($"ButtonLight=223 223 223");
        sb.AppendLine($"ButtonShadow=128 128 128");
        sb.AppendLine($"ButtonDkShadow=0 0 0");
        sb.AppendLine($"ButtonText={def.WindowTextColor.R} {def.WindowTextColor.G} {def.WindowTextColor.B}");
        sb.AppendLine($"ActiveTitle={def.ActiveTitleColor.R} {def.ActiveTitleColor.G} {def.ActiveTitleColor.B}");
        sb.AppendLine($"GradientActiveTitle={def.GradientTitleColor.R} {def.GradientTitleColor.G} {def.GradientTitleColor.B}");
        sb.AppendLine($"TitleText={def.TitleTextColor.R} {def.TitleTextColor.G} {def.TitleTextColor.B}");
        sb.AppendLine($"InactiveTitle={def.InactiveTitleColor.R} {def.InactiveTitleColor.G} {def.InactiveTitleColor.B}");
        sb.AppendLine($"GradientInactiveTitle=181 181 181");
        sb.AppendLine($"InactiveTitleText=192 192 192");
        sb.AppendLine($"Window={def.WindowColor.R} {def.WindowColor.G} {def.WindowColor.B}");
        sb.AppendLine($"WindowText={def.WindowTextColor.R} {def.WindowTextColor.G} {def.WindowTextColor.B}");
        sb.AppendLine($"WindowFrame=0 0 0");
        sb.AppendLine($"Menu={def.ButtonFaceColor.R} {def.ButtonFaceColor.G} {def.ButtonFaceColor.B}");
        sb.AppendLine($"MenuBar={def.ButtonFaceColor.R} {def.ButtonFaceColor.G} {def.ButtonFaceColor.B}");
        sb.AppendLine($"MenuText={def.WindowTextColor.R} {def.WindowTextColor.G} {def.WindowTextColor.B}");
        sb.AppendLine($"MenuHilight={def.HilightColor.R} {def.HilightColor.G} {def.HilightColor.B}");
        sb.AppendLine($"Hilight={def.HilightColor.R} {def.HilightColor.G} {def.HilightColor.B}");
        sb.AppendLine($"HilightText={def.HilightTextColor.R} {def.HilightTextColor.G} {def.HilightTextColor.B}");
        sb.AppendLine($"ActiveBorder={def.ButtonFaceColor.R} {def.ButtonFaceColor.G} {def.ButtonFaceColor.B}");
        sb.AppendLine($"InactiveBorder={def.ButtonFaceColor.R} {def.ButtonFaceColor.G} {def.ButtonFaceColor.B}");
        sb.AppendLine($"AppWorkspace=128 128 128");
        sb.AppendLine($"Scrollbar={def.ButtonFaceColor.R} {def.ButtonFaceColor.G} {def.ButtonFaceColor.B}");
        sb.AppendLine($"GrayText=128 128 128");
        sb.AppendLine($"HotTrackingColor={def.HilightColor.R} {def.HilightColor.G} {def.HilightColor.B}");
        sb.AppendLine("InfoText=0 0 0");
        sb.AppendLine("InfoWindow=255 255 225");
        sb.AppendLine();
        sb.AppendLine("[VisualStyles]");
        sb.AppendLine("Path=");
        sb.AppendLine("ColorStyle=NormalColor");
        sb.AppendLine("Size=NormalSize");
        if (def.HighContrast)
        {
            sb.AppendLine("HighContrast=1");
        }
        sb.AppendLine();

        return sb.ToString();
    }

    public string GetThemeFilePath(ClassicThemeVariant variant)
    {
        string filename = variant switch
        {
            ClassicThemeVariant.Windows95Classic => "Windows95Classic.theme",
            ClassicThemeVariant.Windows98Plus => "Windows98Plus.theme",
            ClassicThemeVariant.Windows2000Pro => "Windows2000Pro.theme",
            ClassicThemeVariant.Windows98HighContrastFlat => "Win98FlatOled.theme",
            _ => "Classic98.theme"
        };
        return Path.Combine(_themesDirectory, filename);
    }

    public string SaveThemeFile(ClassicThemeVariant variant)
    {
        string path = GetThemeFilePath(variant);
        string content = GenerateThemeContent(variant);
        File.WriteAllText(path, content, Encoding.Unicode);
        return path;
    }

    public void BackupCurrentTheme()
    {
        try
        {
            if (File.Exists(_backupThemePath)) return; // Backup already exists

            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes");
            var currentTheme = key?.GetValue("CurrentTheme") as string;
            if (!string.IsNullOrWhiteSpace(currentTheme) && File.Exists(currentTheme))
            {
                File.Copy(currentTheme, _backupThemePath, true);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to backup current theme: {ex.Message}");
        }
    }

    public bool ApplyClassicTheme(ClassicThemeVariant variant, bool launchThemeFile = true)
    {
        try
        {
            BackupCurrentTheme();

            var def = Definitions[variant];

            // 1. Generate & save .theme file
            string themePath = SaveThemeFile(variant);

            // 2. Set in-memory system colors via Win32 SetSysColors for immediate UI update
            ApplySysColors(def);

            // 3. Clear desktop wallpaper to solid color
            Win32Native.SystemParametersInfo(
                Win32Native.SPI_SETDESKWALLPAPER,
                0,
                string.Empty,
                Win32Native.SPIF_UPDATEINIFILE | Win32Native.SPIF_SENDCHANGE);

            // 4. Update registry background color
            using (var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Colors"))
            {
                if (key != null)
                {
                    key.SetValue("Background", $"{def.BackgroundColor.R} {def.BackgroundColor.G} {def.BackgroundColor.B}");
                    key.SetValue("ButtonFace", $"{def.ButtonFaceColor.R} {def.ButtonFaceColor.G} {def.ButtonFaceColor.B}");
                    key.SetValue("ActiveTitle", $"{def.ActiveTitleColor.R} {def.ActiveTitleColor.G} {def.ActiveTitleColor.B}");
                }
            }

            // 5. Disable DWM heavy visual effects (transparency, animations, window shadows)
            _visualEffects.ApplyTransparency(false);
            _visualEffects.ApplyWindowAnimations(false);
            _visualEffects.BroadcastSettingChange("Colors");

            // 6. Launch native Windows theme engine if requested
            if (launchThemeFile && File.Exists(themePath))
            {
                LaunchThemeFile(themePath);
            }

            IsClassicThemeActive = true;
            ActiveVariant = variant;
            OnThemeApplied?.Invoke(variant);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error applying classic theme: {ex.Message}");
            return false;
        }
    }

    public bool RestoreModernTheme()
    {
        try
        {
            string themeToRestore = string.Empty;
            if (File.Exists(_backupThemePath))
            {
                themeToRestore = _backupThemePath;
            }
            else
            {
                string windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                string aero = Path.Combine(windir, "resources", "Themes", "aero.theme");
                string dark = Path.Combine(windir, "resources", "Themes", "dark.theme");
                if (File.Exists(aero)) themeToRestore = aero;
                else if (File.Exists(dark)) themeToRestore = dark;
            }

            if (!string.IsNullOrWhiteSpace(themeToRestore) && File.Exists(themeToRestore))
            {
                LaunchThemeFile(themeToRestore);
            }

            // Restore standard visual effects
            _visualEffects.ApplyTransparency(true);
            _visualEffects.ApplyWindowAnimations(true);
            _visualEffects.BroadcastSettingChange("Colors");

            IsClassicThemeActive = false;
            ActiveVariant = null;
            OnThemeRestored?.Invoke();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error restoring modern theme: {ex.Message}");
            return false;
        }
    }

    public static void LaunchThemeFile(string themeFilePath)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "rundll32.exe",
                Arguments = $"themecpl.dll,OpenThemeAction \"{themeFilePath}\"",
                UseShellExecute = true,
                CreateNoWindow = true
            };
            Process.Start(psi);
        }
        catch
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = themeFilePath,
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }

    private static void ApplySysColors(ClassicThemeDefinition def)
    {
        int[] elements = new int[]
        {
            Win32Native.COLOR_BACKGROUND,
            Win32Native.COLOR_BTNFACE,
            Win32Native.COLOR_ACTIVECAPTION,
            Win32Native.COLOR_GRADIENTACTIVECAPTION,
            Win32Native.COLOR_INACTIVECAPTION,
            Win32Native.COLOR_WINDOW,
            Win32Native.COLOR_WINDOWTEXT,
            Win32Native.COLOR_CAPTIONTEXT,
            Win32Native.COLOR_BTNSHADOW,
            Win32Native.COLOR_BTNHIGHLIGHT,
            Win32Native.COLOR_MENU,
            Win32Native.COLOR_MENUTEXT,
            Win32Native.COLOR_HIGHLIGHT,
            Win32Native.COLOR_HIGHLIGHTTEXT,
            Win32Native.COLOR_ACTIVEBORDER,
            Win32Native.COLOR_INACTIVEBORDER,
            Win32Native.COLOR_APPWORKSPACE,
            Win32Native.COLOR_SCROLLBAR
        };

        uint[] colors = new uint[]
        {
            ToRgb(def.BackgroundColor),
            ToRgb(def.ButtonFaceColor),
            ToRgb(def.ActiveTitleColor),
            ToRgb(def.GradientTitleColor),
            ToRgb(def.InactiveTitleColor),
            ToRgb(def.WindowColor),
            ToRgb(def.WindowTextColor),
            ToRgb(def.TitleTextColor),
            ToRgb(Color.FromArgb(128, 128, 128)),
            ToRgb(Color.FromArgb(255, 255, 255)),
            ToRgb(def.ButtonFaceColor),
            ToRgb(def.WindowTextColor),
            ToRgb(def.HilightColor),
            ToRgb(def.HilightTextColor),
            ToRgb(def.ButtonFaceColor),
            ToRgb(def.ButtonFaceColor),
            ToRgb(Color.FromArgb(128, 128, 128)),
            ToRgb(def.ButtonFaceColor)
        };

        Win32Native.SetSysColors(elements.Length, elements, colors);
    }

    private static uint ToRgb(Color color)
    {
        return (uint)(color.R | (color.G << 8) | (color.B << 16));
    }

    public CompanionToolInfo GetRetroBarInfo()
    {
        bool isRunning = Process.GetProcessesByName("RetroBar").Length > 0;
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        string[] possiblePaths = new[]
        {
            Path.Combine(appData, "DesktopPerformance98", "RetroBar", "RetroBar.exe"),
            Path.Combine(appData, "Programs", "RetroBar", "RetroBar.exe"),
            Path.Combine(progFiles, "RetroBar", "RetroBar.exe"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RetroBar", "RetroBar.exe")
        };

        string foundPath = possiblePaths.FirstOrDefault(File.Exists) ?? string.Empty;

        return new CompanionToolInfo
        {
            Name = "RetroBar",
            IsRunning = isRunning,
            IsInstalled = !string.IsNullOrEmpty(foundPath),
            ExecutablePath = foundPath,
            DownloadUrl = "https://github.com/dremin/RetroBar/releases",
            Description = "Replaces the modern taskbar with authentic pixel-perfect Windows 95/98/2000 classic taskbar. Zero file modifications."
        };
    }

    public CompanionToolInfo GetOpenShellInfo()
    {
        bool isRunning = Process.GetProcessesByName("StartMenu").Length > 0;
        string progFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        string[] possiblePaths = new[]
        {
            Path.Combine(progFiles, "Open-Shell", "StartMenu.exe"),
            Path.Combine(progFiles, "Classic Shell", "StartMenu.exe")
        };

        string foundPath = possiblePaths.FirstOrDefault(File.Exists) ?? string.Empty;

        return new CompanionToolInfo
        {
            Name = "Open-Shell",
            IsRunning = isRunning,
            IsInstalled = !string.IsNullOrEmpty(foundPath),
            ExecutablePath = foundPath,
            DownloadUrl = "https://github.com/Open-Shell/Open-Shell-Menu/releases",
            Description = "Classic Windows 95/98/2000 Start Menu with two-column layout and banner. Native Win32, 0% GPU load."
        };
    }

    public bool LaunchCompanionTool(string exePath)
    {
        try
        {
            if (File.Exists(exePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true
                });
                return true;
            }
        }
        catch { }
        return false;
    }

    public bool StopCompanionTool(string toolName)
    {
        bool stopped = false;
        string procName = toolName.Equals("Open-Shell", StringComparison.OrdinalIgnoreCase) ? "StartMenu" : toolName;
        foreach (var p in Process.GetProcessesByName(procName))
        {
            try
            {
                p.Kill();
                p.WaitForExit(2000);
                stopped = true;
            }
            catch { }
        }
        return stopped;
    }

    public async Task<(bool Success, string Message)> DownloadAndInstallRetroBarAsync(IProgress<int>? progress = null, Action<string>? statusCallback = null)
    {
        try
        {
            statusCallback?.Invoke("Conectando con GitHub Releases para obtener RetroBar...");
            string downloadUrl = "https://github.com/dremin/RetroBar/releases/latest/download/RetroBar.Portable.zip";

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string targetDir = Path.Combine(appData, "DesktopPerformance98", "RetroBar");
            string tempZip = Path.Combine(appData, "DesktopPerformance98", "RetroBar_temp.zip");

            Directory.CreateDirectory(Path.GetDirectoryName(tempZip)!);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ZeroLatency98-CompanionDownloader");
                using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 17_600_000L;
                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(tempZip, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                byte[] buffer = new byte[81920];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;
                    int percent = (int)((totalRead * 100) / totalBytes);
                    progress?.Report(percent);
                    statusCallback?.Invoke($"Descargando RetroBar Portable... {percent}% ({totalRead / (1024.0 * 1024.0):F1} MB)");
                }
            }

            statusCallback?.Invoke("Extrayendo archivos de RetroBar...");

            // If RetroBar was already running, stop it before extracting
            StopCompanionTool("RetroBar");

            if (Directory.Exists(targetDir))
            {
                try { Directory.Delete(targetDir, true); } catch { }
            }
            Directory.CreateDirectory(targetDir);

            ZipFile.ExtractToDirectory(tempZip, targetDir, overwriteFiles: true);

            try { File.Delete(tempZip); } catch { }

            string exePath = Path.Combine(targetDir, "RetroBar.exe");
            if (File.Exists(exePath))
            {
                statusCallback?.Invoke("¡RetroBar integrado exitosamente!");
                return (true, exePath);
            }

            return (false, "No se encontró el ejecutable RetroBar.exe en el paquete descargado.");
        }
        catch (Exception ex)
        {
            statusCallback?.Invoke($"Error al descargar/integrar RetroBar: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> DownloadOpenShellInstallerAsync(IProgress<int>? progress = null, Action<string>? statusCallback = null)
    {
        try
        {
            statusCallback?.Invoke("Conectando con GitHub Releases para instalador de Open-Shell...");
            string downloadUrl = "https://github.com/Open-Shell/Open-Shell-Menu/releases/download/v4.4.198/OpenShellSetup_4_4_198.exe";

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string installerPath = Path.Combine(appData, "DesktopPerformance98", "OpenShellSetup.exe");

            Directory.CreateDirectory(Path.GetDirectoryName(installerPath)!);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ZeroLatency98-CompanionDownloader");
                using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 10_000_000L;
                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(installerPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                byte[] buffer = new byte[81920];
                long totalRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;
                    int percent = (int)((totalRead * 100) / totalBytes);
                    progress?.Report(percent);
                    statusCallback?.Invoke($"Descargando OpenShellSetup.exe... {percent}% ({totalRead / (1024.0 * 1024.0):F1} MB)");
                }
            }

            statusCallback?.Invoke("Iniciando instalador oficial de Open-Shell...");
            Process.Start(new ProcessStartInfo
            {
                FileName = installerPath,
                UseShellExecute = true
            });

            return (true, installerPath);
        }
        catch (Exception ex)
        {
            statusCallback?.Invoke($"Error al descargar instalador de Open-Shell: {ex.Message}");
            return (false, ex.Message);
        }
    }
}
