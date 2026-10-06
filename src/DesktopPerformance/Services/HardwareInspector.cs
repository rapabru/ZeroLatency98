using System.Diagnostics;
using Microsoft.Win32;
using DesktopPerformance.Models;
using DesktopPerformance.Native;

namespace DesktopPerformance.Services;

public class HardwareInspector
{
    public HardwareStatus CaptureCurrentHardwareStatus()
    {
        var status = new HardwareStatus();

        // 1. CPU Name and Cores
        status.LogicalCores = Environment.ProcessorCount;
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            if (key != null)
            {
                var name = key.GetValue("ProcessorNameString") as string;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    status.CpuName = name.Trim();
                }
            }
        }
        catch
        {
            status.CpuName = $"{Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "x64 Processor"} ({status.LogicalCores} Threads)";
        }

        // 2. RAM Information
        var memStatus = new Win32Native.MEMORYSTATUSEX();
        memStatus.dwLength = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32Native.MEMORYSTATUSEX));
        if (Win32Native.GlobalMemoryStatusEx(ref memStatus))
        {
            status.TotalPhysicalMemoryMB = memStatus.ullTotalPhys / (1024 * 1024);
            status.AvailablePhysicalMemoryMB = memStatus.ullAvailPhys / (1024 * 1024);
        }

        // 3. GPU Adapter Name
        try
        {
            using var videoKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000");
            if (videoKey != null)
            {
                var desc = videoKey.GetValue("DriverDesc") as string;
                if (!string.IsNullOrWhiteSpace(desc))
                {
                    status.GpuAdapterName = desc.Trim();
                }
            }
        }
        catch
        {
            status.GpuAdapterName = "Default Graphics Adapter";
        }

        // 4. Displays & Monitors via Win32 EnumDisplayDevices / Settings
        status.Displays = DetectDisplays();

        // 5. Total Process and Thread Counts
        try
        {
            var processes = Process.GetProcesses();
            status.TotalProcessesCount = processes.Length;
            int totalThreads = 0;
            foreach (var p in processes)
            {
                try { totalThreads += p.Threads.Count; } catch { }
                p.Dispose();
            }
            status.TotalThreadsCount = totalThreads;
        }
        catch
        {
            status.TotalProcessesCount = 0;
            status.TotalThreadsCount = 0;
        }

        return status;
    }

    private List<DisplayInfo> DetectDisplays()
    {
        var list = new List<DisplayInfo>();
        uint devNum = 0;

        while (true)
        {
            var displayDevice = new Win32Native.DISPLAY_DEVICEA();
            displayDevice.cb = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32Native.DISPLAY_DEVICEA));

            if (!Win32Native.EnumDisplayDevicesA(null, devNum, ref displayDevice, 0))
                break;

            // Check if device is attached to desktop
            const int DISPLAY_DEVICE_ATTACHED_TO_DESKTOP = 0x00000001;
            const int DISPLAY_DEVICE_PRIMARY_DEVICE = 0x00000004;

            if ((displayDevice.StateFlags & DISPLAY_DEVICE_ATTACHED_TO_DESKTOP) != 0)
            {
                var devMode = new Win32Native.DEVMODEA();
                devMode.dmSize = (short)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32Native.DEVMODEA));

                if (Win32Native.EnumDisplaySettingsExA(displayDevice.DeviceName, Win32Native.ENUM_CURRENT_SETTINGS, ref devMode, 0))
                {
                    list.Add(new DisplayInfo
                    {
                        DeviceName = displayDevice.DeviceName,
                        MonitorName = string.IsNullOrWhiteSpace(displayDevice.DeviceString) ? $"Display {devNum + 1}" : displayDevice.DeviceString.Trim(),
                        Width = devMode.dmPelsWidth,
                        Height = devMode.dmPelsHeight,
                        RefreshRateHz = devMode.dmDisplayFrequency,
                        BitsPerPixel = devMode.dmBitsPerPel,
                        IsPrimary = (displayDevice.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE) != 0
                    });
                }
            }

            devNum++;
        }

        // Fallback to Screen.AllScreens if Win32 enumeration didn't return any
        if (list.Count == 0)
        {
            foreach (var screen in Screen.AllScreens)
            {
                list.Add(new DisplayInfo
                {
                    DeviceName = screen.DeviceName,
                    MonitorName = screen.Primary ? "Primary Display" : "Secondary Display",
                    Width = screen.Bounds.Width,
                    Height = screen.Bounds.Height,
                    RefreshRateHz = 60, // Fallback default
                    BitsPerPixel = screen.BitsPerPixel,
                    IsPrimary = screen.Primary
                });
            }
        }

        return list;
    }
}
