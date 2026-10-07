using System.Diagnostics;
using System.Text.Json;

namespace DesktopPerformance.Services;

public class ObservedSessionPattern
{
    public string TriggerApp { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public List<string> CoRunningBackgroundApps { get; set; } = new();
}

public class LocalLearningReport
{
    public string SuggestionTitle { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public double ConfidencePercentage { get; set; }
}

public class ProfileLearner
{
    private readonly string _storagePath;
    private readonly List<ObservedSessionPattern> _history = new();

    public ProfileLearner(string? customPath = null)
    {
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            _storagePath = customPath;
        }
        else
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dir = Path.Combine(appData, "DesktopPerformance98");
            Directory.CreateDirectory(dir);
            _storagePath = Path.Combine(dir, "user_patterns.json");
        }

        LoadHistory();
    }

    public IReadOnlyList<ObservedSessionPattern> History => _history;

    public void RecordSession(string triggerApp, IEnumerable<string> runningBackgroundApps)
    {
        var pattern = new ObservedSessionPattern
        {
            TriggerApp = triggerApp,
            Timestamp = DateTime.UtcNow,
            CoRunningBackgroundApps = runningBackgroundApps.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        };

        _history.Add(pattern);
        SaveHistory();
    }

    public List<LocalLearningReport> GenerateIntelligentSuggestions()
    {
        var suggestions = new List<LocalLearningReport>();

        if (_history.Count == 0)
        {
            suggestions.Add(new LocalLearningReport
            {
                SuggestionTitle = "Observación Pasiva Inicial",
                Explanation = "El motor está aprendiendo tus hábitos locales de uso. A medida que ejecutes juegos o herramientas de diseño, generará sugerencias personalizadas de afinidad y exclusión.",
                RecommendedAction = "Continúa usando el sistema con normalidad. Toda la información permanece 100% privada en tu disco local.",
                ConfidencePercentage = 100.0
            });
            return suggestions;
        }

        // Analyze frequency of Discord with gaming sessions
        var gameSessions = _history.Where(h => h.TriggerApp.Equals("cs2", StringComparison.OrdinalIgnoreCase)).ToList();
        if (gameSessions.Count >= 2)
        {
            int discordCount = gameSessions.Count(s => s.CoRunningBackgroundApps.Contains("Discord", StringComparer.OrdinalIgnoreCase));
            double ratio = (double)discordCount / gameSessions.Count;

            if (ratio >= 0.70)
            {
                suggestions.Add(new LocalLearningReport
                {
                    SuggestionTitle = "Sugerencia para Discord en CS2",
                    Explanation = $"Discord se encuentra activo en el {ratio * 100:F0}% de tus sesiones de CS2.",
                    RecommendedAction = "Mantener Discord en la Whitelist con 'Modo Background' activo pero sin suspenderlo, para asegurar audio de voz ininterrumpido.",
                    ConfidencePercentage = ratio * 100.0
                });
            }
        }

        return suggestions;
    }

    private void LoadHistory()
    {
        _history.Clear();
        if (File.Exists(_storagePath))
        {
            try
            {
                string json = File.ReadAllText(_storagePath);
                var items = JsonSerializer.Deserialize<List<ObservedSessionPattern>>(json);
                if (items != null) _history.AddRange(items);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load pattern history: {ex.Message}");
            }
        }
    }

    private void SaveHistory()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_history, options);
            File.WriteAllText(_storagePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save pattern history: {ex.Message}");
        }
    }
}
