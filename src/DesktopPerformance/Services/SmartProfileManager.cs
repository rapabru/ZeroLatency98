using System.Diagnostics;
using System.Text.Json;
using DesktopPerformance.Models;

namespace DesktopPerformance.Services;

public class SmartProfileManager
{
    private readonly string _profilesFilePath;
    private readonly List<SmartProfile> _profiles = new();

    public SmartProfileManager(string? customPath = null)
    {
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            _profilesFilePath = customPath;
        }
        else
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dir = Path.Combine(appData, "DesktopPerformance98");
            Directory.CreateDirectory(dir);
            _profilesFilePath = Path.Combine(dir, "smart_profiles.json");
        }

        LoadProfiles();
    }

    public IReadOnlyList<SmartProfile> Profiles => _profiles;

    public void LoadProfiles()
    {
        _profiles.Clear();

        if (File.Exists(_profilesFilePath))
        {
            try
            {
                string json = File.ReadAllText(_profilesFilePath);
                var loaded = JsonSerializer.Deserialize<List<SmartProfile>>(json);
                if (loaded != null && loaded.Count > 0)
                {
                    _profiles.AddRange(loaded);
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading smart profiles: {ex.Message}");
            }
        }

        // Initialize Default Built-in Profiles
        _profiles.AddRange(GetDefaultSmartProfiles());
        SaveProfiles();
    }

    public void SaveProfiles()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_profiles, options);
            File.WriteAllText(_profilesFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving smart profiles: {ex.Message}");
        }
    }

    public void AddOrUpdateProfile(SmartProfile profile)
    {
        var existing = _profiles.FirstOrDefault(p => p.Id == profile.Id);
        if (existing != null)
        {
            int index = _profiles.IndexOf(existing);
            _profiles[index] = profile;
        }
        else
        {
            _profiles.Add(profile);
        }
        SaveProfiles();
    }

    public bool DeleteProfile(string profileId)
    {
        int removed = _profiles.RemoveAll(p => p.Id == profileId);
        if (removed > 0)
        {
            SaveProfiles();
            return true;
        }
        return false;
    }

    public SmartProfile? FindProfileForProcess(string processName)
    {
        string cleanName = processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? processName.Substring(0, processName.Length - 4)
            : processName;

        return _profiles.FirstOrDefault(p =>
        {
            string trigger = p.TriggerProcessName;
            if (trigger.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                trigger = trigger.Substring(0, trigger.Length - 4);

            return trigger.Equals(cleanName, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static List<SmartProfile> GetDefaultSmartProfiles()
    {
        return new List<SmartProfile>
        {
            new SmartProfile
            {
                Id = "profile_cs2",
                Name = "CS2 / Competitive Gaming",
                Description = "Maximum CPU isolation and zero DWM background interference optimized for Counter-Strike 2.",
                TriggerProcessName = "cs2.exe",
                DisableTransparency = true,
                DisableAnimations = true,
                DisableWidgets = true,
                PauseWindowsSearch = true,
                PauseRgbServices = true,
                PauseCloudSync = true,
                LowerBackgroundAppsPriority = true,
                IsolateCpuCores = true,
                ForceHighPerformancePower = true,
                CustomWhitelistedProcesses = new List<string> { "steam", "cs2", "audiodg" }
            },
            new SmartProfile
            {
                Id = "profile_streaming",
                Name = "Streaming & OBS",
                Description = "Optimized for live broadcasting. Retains OBS and Discord voice while suppressing desktop blurs and indexer.",
                TriggerProcessName = "obs64.exe",
                DisableTransparency = true,
                DisableAnimations = true,
                DisableWidgets = true,
                PauseWindowsSearch = true,
                PauseRgbServices = false,
                PauseCloudSync = true,
                LowerBackgroundAppsPriority = false,
                IsolateCpuCores = false,
                ForceHighPerformancePower = true,
                CustomWhitelistedProcesses = new List<string> { "obs64", "Discord", "steam", "audiodg" }
            },
            new SmartProfile
            {
                Id = "profile_video_editing",
                Name = "Video Editing & 3D (Blender / Premiere)",
                Description = "Optimized for heavy rendering. Retains disk indexing for assets and maximizes RAM allocation.",
                TriggerProcessName = "Adobe Premiere Pro.exe",
                DisableTransparency = true,
                DisableAnimations = true,
                DisableWidgets = true,
                PauseWindowsSearch = false, // Keep search for assets
                PauseRgbServices = false,
                PauseCloudSync = false,
                LowerBackgroundAppsPriority = true,
                IsolateCpuCores = false,
                ForceHighPerformancePower = true
            },
            new SmartProfile
            {
                Id = "profile_general_work",
                Name = "General Work & Silence",
                Description = "Clean, distraction-free environment. Disables notifications and animations without altering affinities.",
                TriggerProcessName = "code.exe",
                DisableTransparency = true,
                DisableAnimations = true,
                DisableWidgets = true,
                PauseWindowsSearch = false,
                PauseRgbServices = false,
                PauseCloudSync = false,
                LowerBackgroundAppsPriority = false,
                IsolateCpuCores = false,
                ForceHighPerformancePower = false
            }
        };
    }
}
