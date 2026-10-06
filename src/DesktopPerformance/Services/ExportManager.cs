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

            string json = File.ReadAllText(sourcePath);
            return JsonSerializer.Deserialize<List<SmartProfile>>(json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to import profiles: {ex.Message}");
            return null;
        }
    }
}
