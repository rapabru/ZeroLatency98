using System.Runtime.InteropServices;

namespace DesktopPerformance.Native;

public static class Win32Native
{
    #region Constants
    public const uint SPI_GETANIMATION = 0x0048;
    public const uint SPI_SETANIMATION = 0x0049;
    public const uint SPI_GETDESKWALLPAPER = 0x0073;
    public const uint SPI_SETDESKWALLPAPER = 0x0014;

    public const uint SPIF_UPDATEINIFILE = 0x01;
    public const uint SPIF_SENDCHANGE = 0x02;

    public const int HWND_BROADCAST = 0xFFFF;
    public const uint WM_SETTINGCHANGE = 0x001A;
    public const uint SMTO_ABORTIFHUNG = 0x0002;

    public const uint PROCESS_SET_INFORMATION = 0x0200;
    public const uint PROCESS_QUERY_INFORMATION = 0x0400;
    public const uint PROCESS_SUSPEND_RESUME = 0x0800;

    // Process Priority Classes
    public const uint NORMAL_PRIORITY_CLASS = 0x00000020;
    public const uint IDLE_PRIORITY_CLASS = 0x00000040;
    public const uint HIGH_PRIORITY_CLASS = 0x00000080;
    public const uint ABOVE_NORMAL_PRIORITY_CLASS = 0x00008000;
    public const uint BELOW_NORMAL_PRIORITY_CLASS = 0x00004000;
    public const uint PROCESS_MODE_BACKGROUND_BEGIN = 0x00100000;
    public const uint PROCESS_MODE_BACKGROUND_END = 0x00200000;

    public const int ENUM_CURRENT_SETTINGS = -1;
    #endregion

    #region Structs
    [StructLayout(LayoutKind.Sequential)]
    public struct ANIMATIONINFO
    {
        public uint cbSize;
        public int iMinAnimate;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODEA
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DISPLAY_DEVICEA
    {
        public int cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        public int StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }
    #endregion

    #region User32
    public const int COLOR_SCROLLBAR = 0;
    public const int COLOR_BACKGROUND = 1;
    public const int COLOR_ACTIVECAPTION = 2;
    public const int COLOR_INACTIVECAPTION = 3;
    public const int COLOR_MENU = 4;
    public const int COLOR_WINDOW = 5;
    public const int COLOR_WINDOWFRAME = 6;
    public const int COLOR_MENUTEXT = 7;
    public const int COLOR_WINDOWTEXT = 8;
    public const int COLOR_CAPTIONTEXT = 9;
    public const int COLOR_ACTIVEBORDER = 10;
    public const int COLOR_INACTIVEBORDER = 11;
    public const int COLOR_APPWORKSPACE = 12;
    public const int COLOR_HIGHLIGHT = 13;
    public const int COLOR_HIGHLIGHTTEXT = 14;
    public const int COLOR_BTNFACE = 15;
    public const int COLOR_BTNSHADOW = 16;
    public const int COLOR_GRAYTEXT = 17;
    public const int COLOR_BTNTEXT = 18;
    public const int COLOR_INACTIVECAPTIONTEXT = 19;
    public const int COLOR_BTNHIGHLIGHT = 20;
    public const int COLOR_3DDKSHADOW = 21;
    public const int COLOR_3DLIGHT = 22;
    public const int COLOR_INFOTEXT = 23;
    public const int COLOR_INFOBK = 24;
    public const int COLOR_HOTLIGHT = 26;
    public const int COLOR_GRADIENTACTIVECAPTION = 27;
    public const int COLOR_GRADIENTINACTIVECAPTION = 28;
    public const int COLOR_MENUHILIGHT = 29;
    public const int COLOR_MENUBAR = 30;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetSysColors(int cElements, int[] lpaElements, uint[] lpaRgbValues);

    [DllImport("user32.dll")]
    public static extern uint GetSysColor(int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref ANIMATIONINFO pvParam, uint fWinIni);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, System.Text.StringBuilder pvParam, uint fWinIni);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        uint Msg,
        IntPtr wParam,
        string lParam,
        uint fuFlags,
        uint uTimeout,
        out IntPtr lpdwResult);

    [DllImport("user32.dll", CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumDisplayDevicesA(string? lpDevice, uint iDevNum, ref DISPLAY_DEVICEA lpDisplayDevice, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumDisplaySettingsExA(string? lpszDeviceName, int iModeNum, ref DEVMODEA lpDevMode, uint dwFlags);
    #endregion

    #region Kernel32
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetPriorityClass(IntPtr handle, uint priorityClass);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint GetPriorityClass(IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetProcessAffinityMask(IntPtr hProcess, UIntPtr dwProcessAffinityMask);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetProcessAffinityMask(IntPtr hProcess, out UIntPtr lpProcessAffinityMask, out UIntPtr lpSystemAffinityMask);
    #endregion

    #region Ntdll
    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern int NtSuspendProcess(IntPtr processHandle);

    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern int NtResumeProcess(IntPtr processHandle);

    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern int NtQueryTimerResolution(out uint MinimumResolution, out uint MaximumResolution, out uint CurrentResolution);
    #endregion
}
