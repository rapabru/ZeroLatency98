using System.Diagnostics;
using System.Text.Json;
using DesktopPerformance.Models;

namespace DesktopPerformance.Services;

public class ExportManager
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public bool ExportBenchmarkSession(BenchmarkComparison comparison, string destinationPath)
    {
        try
        {
            var data = new
            {
                ExportedAt = DateTime.UtcNow,
                Summary = comparison.SummaryText,
                MeasurableImprovement = comparison.IsImprovementMeasurable,
                DeltaContextSwitchesPercent = comparison.DeltaContextSwitchesPercent,
                DeltaCpuPercent = comparison.DeltaCpuPercent,
                DeltaDiskPercent = comparison.DeltaDiskPercent,
                DeltaDwmMemoryPercent = comparison.DeltaDwmMemoryPercent,
                Baseline = comparison.Before,
                Optimized = comparison.After
            };

            string json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(destinationPath, json);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to export benchmark: {ex.Message}");
            return false;
        }
    }

    public bool ExportProfiles(IEnumerable<SmartProfile> profiles, string destinationPath)
    {
        try
        {
            string json = JsonSerializer.Serialize(profiles, JsonOptions);
            File.WriteAllText(destinationPath, json);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to export profiles: {ex.Message}");
            return false;
        }
    }

    public List<SmartProfile>? ImportProfiles(string sourcePath)
    {
        try
        {
            if (!File.Exists(sourcePath))
                return null;

            var fileInfo = new FileInfo(sourcePath);
            // Enforce 512 KB maximum file size to prevent OutOfMemory DoS
            if (fileInfo.Length > 512 * 1024)
            {
                Debug.WriteLine("Rejected profile import: File size exceeds 512 KB limit.");
                return null;
            }

            string json = File.ReadAllText(sourcePath);
            var loaded = JsonSerializer.Deserialize<List<SmartProfile>>(json);
            if (loaded == null)
                return null;

            var validated = new List<SmartProfile>();
            foreach (var profile in loaded)
            {
                if (string.IsNullOrWhiteSpace(profile.Name))
                    continue;

                // Restrict and sanitize string fields
                if (profile.Name.Length > 64)
                    profile.Name = profile.Name.Substring(0, 64);

                if (!string.IsNullOrEmpty(profile.Description) && profile.Description.Length > 256)
                    profile.Description = profile.Description.Substring(0, 256);

                profile.TriggerProcessName = SecurityUtils.SanitizeProcessName(profile.TriggerProcessName);

                if (profile.CustomWhitelistedProcesses != null)
                {
                    profile.CustomWhitelistedProcesses = profile.CustomWhitelistedProcesses
                        .Select(SecurityUtils.SanitizeProcessName)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(50)
                        .ToList();
                }

                validated.Add(profile);
            }

            return validated;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to import profiles: {ex.Message}");
            return null;
        }
    }
}
