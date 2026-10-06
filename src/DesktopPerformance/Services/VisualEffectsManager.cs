using System.Diagnostics;
using System.Text;
using Microsoft.Win32;
using DesktopPerformance.Models;
using DesktopPerformance.Native;

namespace DesktopPerformance.Services;

public class VisualEffectsManager
{
    private const string PersonalizeSubKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string ExplorerAdvancedSubKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
    private const string DesktopSubKey = @"Control Panel\Desktop";

    public VisualState CaptureCurrentVisualState()
    {
        var state = new VisualState();

        // 1. Transparency
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(PersonalizeSubKey);
            if (key != null)
            {
                var val = key.GetValue("EnableTransparency");
                state.TransparencyEnabled = val is int intVal ? intVal == 1 : true;
            }
        }
        catch
        {
            state.TransparencyEnabled = true;
        }

        // 2. Window Animations via SystemParametersInfo
        try
        {
            var animInfo = new Win32Native.ANIMATIONINFO();
            animInfo.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32Native.ANIMATIONINFO));
            if (Win32Native.SystemParametersInfo(Win32Native.SPI_GETANIMATION, animInfo.cbSize, ref animInfo, 0))
            {
                state.WindowAnimationsEnabled = animInfo.iMinAnimate != 0;
            }
            else
            {
                state.WindowAnimationsEnabled = true;
            }
        }
        catch
        {
            state.WindowAnimationsEnabled = true;
        }

        // 3. Taskbar Widgets
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(ExplorerAdvancedSubKey);
            if (key != null)
            {
                var val = key.GetValue("TaskbarDa");
                state.TaskbarWidgetsEnabled = val is int intVal ? intVal == 1 : true;
            }
        }
        catch
        {
            state.TaskbarWidgetsEnabled = true;
        }

        // 4. Wallpaper
        try
        {
            var sb = new StringBuilder(260);
            if (Win32Native.SystemParametersInfo(Win32Native.SPI_GETDESKWALLPAPER, (uint)sb.Capacity, sb, 0))
            {
                state.WallpaperPath = sb.ToString();
            }
            else
            {
                using var key = Registry.CurrentUser.OpenSubKey(DesktopSubKey);
                state.WallpaperPath = (key?.GetValue("Wallpaper") as string) ?? string.Empty;
            }
        }
        catch
        {
            state.WallpaperPath = string.Empty;
        }

        return state;
    }

    public void ApplyTransparency(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(PersonalizeSubKey);
            key?.SetValue("EnableTransparency", enable ? 1 : 0, RegistryValueKind.DWord);
            BroadcastSettingChange("ImmersiveColorSet");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to set transparency: {ex.Message}");
        }
    }

    public void ApplyWindowAnimations(bool enable)
    {
        try
        {
            var animInfo = new Win32Native.ANIMATIONINFO();
            animInfo.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32Native.ANIMATIONINFO));
            animInfo.iMinAnimate = enable ? 1 : 0;
            Win32Native.SystemParametersInfo(
                Win32Native.SPI_SETANIMATION,
                animInfo.cbSize,
                ref animInfo,
                Win32Native.SPIF_UPDATEINIFILE | Win32Native.SPIF_SENDCHANGE);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to set animations: {ex.Message}");
        }
    }

    public void ApplyTaskbarWidgets(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(ExplorerAdvancedSubKey);
            key?.SetValue("TaskbarDa", enable ? 1 : 0, RegistryValueKind.DWord);
            BroadcastSettingChange("TraySettings");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to set taskbar widgets: {ex.Message}");
        }
    }

    public void BroadcastSettingChange(string section)
    {
        try
        {
            Win32Native.SendMessageTimeout(
                (IntPtr)Win32Native.HWND_BROADCAST,
                Win32Native.WM_SETTINGCHANGE,
                IntPtr.Zero,
                section,
                Win32Native.SMTO_ABORTIFHUNG,
                500,
                out _);
        }
        catch { }
    }
}
