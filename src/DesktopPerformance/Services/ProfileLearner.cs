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

        // Group history by TriggerApp dynamically (e.g. cs2, valorant, obs64, premiere, etc.)
        var appGroups = _history
            .GroupBy(h => h.TriggerApp, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() >= 2)
            .ToList();

        if (appGroups.Count == 0)
        {
            suggestions.Add(new LocalLearningReport
            {
                SuggestionTitle = "Recolección de Patrones en Curso",
                Explanation = $"Se han registrado {_history.Count} sesión(es) en total. Se requieren al menos 2 ejecuciones de una misma aplicación para inferir patrones de afinidad confiables.",
                RecommendedAction = "Al abrir tu juego o software habitual nuevamente, se generarán sugerencias de optimización contextuales.",
                ConfidencePercentage = Math.Min(95.0, _history.Count * 30.0)
            });
            return suggestions;
        }

        foreach (var group in appGroups)
        {
            string triggerApp = group.Key;
            int totalSessions = group.Count();

            // Count occurrences of each background app
            var bgAppCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var session in group)
            {
                foreach (var bgApp in session.CoRunningBackgroundApps)
                {
                    bgAppCounts[bgApp] = bgAppCounts.GetValueOrDefault(bgApp, 0) + 1;
                }
            }

            foreach (var (bgApp, count) in bgAppCounts)
            {
                double ratio = (double)count / totalSessions;
                if (ratio >= 0.60) // Present in 60%+ of sessions
                {
                    double confidence = Math.Round(ratio * 100.0, 0);

                    // Categorize suggestion based on app nature
                    if (bgApp.Equals("Discord", StringComparison.OrdinalIgnoreCase) ||
                        bgApp.Equals("Slack", StringComparison.OrdinalIgnoreCase) ||
                        bgApp.Equals("Teams", StringComparison.OrdinalIgnoreCase))
                    {
                        suggestions.Add(new LocalLearningReport
                        {
                            SuggestionTitle = $"Protección de Voz ({bgApp} con {triggerApp})",
                            Explanation = $"{bgApp} está activo en el {confidence}% de tus sesiones de {triggerApp}.",
                            RecommendedAction = $"Mantener {bgApp} en la Whitelist con 'Modo Background' pero sin suspender, asegurando llamadas de voz fluidas sin jitter.",
                            ConfidencePercentage = confidence
                        });
                    }
                    else if (bgApp.Equals("Spotify", StringComparison.OrdinalIgnoreCase) ||
                             bgApp.Equals("Tidal", StringComparison.OrdinalIgnoreCase))
                    {
                        suggestions.Add(new LocalLearningReport
                        {
                            SuggestionTitle = $"Streaming de Música ({bgApp} con {triggerApp})",
                            Explanation = $"{bgApp} acompaña el {confidence}% de tus sesiones de {triggerApp}.",
                            RecommendedAction = $"Asignar a {bgApp} afinidad en núcleos de CPU secundarios para que la decodificación de audio no compita con el renderizado de {triggerApp}.",
                            ConfidencePercentage = confidence
                        });
                    }
                    else if (bgApp.Equals("OneDrive", StringComparison.OrdinalIgnoreCase) ||
                             bgApp.Equals("Dropbox", StringComparison.OrdinalIgnoreCase) ||
                             bgApp.Equals("GoogleDriveFS", StringComparison.OrdinalIgnoreCase))
                    {
                        suggestions.Add(new LocalLearningReport
                        {
                            SuggestionTitle = $"Pausar Sincronización en la Nube ({bgApp} con {triggerApp})",
                            Explanation = $"{bgApp} compite por ancho de banda y acceso a disco durante el {confidence}% de las sesiones de {triggerApp}.",
                            RecommendedAction = $"Suspender {bgApp} automáticamente durante sesiones de {triggerApp} para evitar caídas súbitas de FPS por lectura/escritura en disco.",
                            ConfidencePercentage = confidence
                        });
                    }
                    else if (bgApp.Equals("chrome", StringComparison.OrdinalIgnoreCase) ||
                             bgApp.Equals("msedge", StringComparison.OrdinalIgnoreCase) ||
                             bgApp.Equals("firefox", StringComparison.OrdinalIgnoreCase))
                    {
                        suggestions.Add(new LocalLearningReport
                        {
                            SuggestionTitle = $"Aislamiento de Navegador ({bgApp} con {triggerApp})",
                            Explanation = $"Navegador {bgApp} detectado en segundo plano en el {confidence}% de sesiones de {triggerApp}.",
                            RecommendedAction = $"Aplicar Eco Priority a {bgApp} para impedir que pestañas secundarias con Javascript roben ciclos de CPU o GPU a {triggerApp}.",
                            ConfidencePercentage = confidence
                        });
                    }
                    else
                    {
                        suggestions.Add(new LocalLearningReport
                        {
                            SuggestionTitle = $"Optimización de Afinidad ({bgApp} con {triggerApp})",
                            Explanation = $"{bgApp} se ejecuta simultáneamente en el {confidence}% de sesiones de {triggerApp}.",
                            RecommendedAction = $"Aislar {bgApp} a núcleos secundarios durante el uso de {triggerApp}.",
                            ConfidencePercentage = confidence
                        });
                    }
                }
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
