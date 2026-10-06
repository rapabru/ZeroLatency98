using System.Diagnostics;
using System.Text.Json;
using DesktopPerformance.Models;

namespace DesktopPerformance.Services;

public class SnapshotManager
{
    private readonly string _snapshotFilePath;

    public SnapshotManager(string? customPath = null)
    {
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            _snapshotFilePath = customPath;
        }
        else
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dir = Path.Combine(appData, "DesktopPerformance98");
            Directory.CreateDirectory(dir);
            _snapshotFilePath = Path.Combine(dir, "snapshot_baseline.json");
        }
    }

    public string SnapshotFilePath => _snapshotFilePath;

    public bool HasActiveSnapshot()
    {
        return File.Exists(_snapshotFilePath);
    }

    public void SaveSnapshot(SystemSnapshot snapshot)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(snapshot, options);
            File.WriteAllText(_snapshotFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error writing snapshot file: {ex.Message}");
            throw;
        }
    }

    public SystemSnapshot? LoadSnapshot()
    {
        try
        {
            if (!File.Exists(_snapshotFilePath))
                return null;

            string json = File.ReadAllText(_snapshotFilePath);
            return JsonSerializer.Deserialize<SystemSnapshot>(json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error reading snapshot file: {ex.Message}");
            return null;
        }
    }

    public void ClearSnapshot()
    {
        try
        {
            if (File.Exists(_snapshotFilePath))
            {
                File.Delete(_snapshotFilePath);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error clearing snapshot: {ex.Message}");
        }
    }
}
