using System.Diagnostics;
using DesktopPerformance.Models;

namespace DesktopPerformance.Services;

public class DiagnosticFinding
{
    public string Category { get; set; } = string.Empty; // "GPU", "Disk I/O", "CPU Wakeups", "Multi-Monitor"
    public string ProcessOrComponent { get; set; } = string.Empty;
    public string HumanExplanation { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
    public bool IsSevere { get; set; }
}

public class DesktopBusyAnalyzer
{
    private readonly HardwareInspector _hardwareInspector;

    public DesktopBusyAnalyzer(HardwareInspector hardwareInspector)
    {
        _hardwareInspector = hardwareInspector;
    }

    public List<DiagnosticFinding> AnalyzeCurrentInterference()
    {
        var findings = new List<DiagnosticFinding>();

        // 1. Check Multi-Monitor Mixed Refresh Rates
        var hw = _hardwareInspector.CaptureCurrentHardwareStatus();
        if (hw.Displays.Count > 1)
        {
            var refreshRates = hw.Displays.Select(d => d.RefreshRateHz).Distinct().ToList();
            if (refreshRates.Count > 1)
            {
                int maxHz = refreshRates.Max();
                int minHz = refreshRates.Min();
                findings.Add(new DiagnosticFinding
                {
                    Category = "Multi-Monitor",
                    ProcessOrComponent = "DWM Compositor Clocks",
                    HumanExplanation = $"Tienes monitores con frecuencias de refresco distintas ({minHz} Hz y {maxHz} Hz). Si ejecutas video o ventanas aceleradas por GPU en la pantalla secundaria, DWM y el planificador gráfico pueden inducir microstuttering en tu pantalla de {maxHz} Hz.",
                    RecommendedAction = "Minimiza o pausa ventanas animadas en el monitor secundario durante sesiones críticas.",
                    IsSevere = true
                });
            }
        }

        // 2. Check GPU Hardware-Accelerated Apps
        var activeProcesses = Process.GetProcesses();
        var procNames = new HashSet<string>(activeProcesses.Select(p => p.ProcessName), StringComparer.OrdinalIgnoreCase);

        var electronApps = new List<string>();
        if (procNames.Contains("Discord")) electronApps.Add("Discord");
        if (procNames.Contains("Spotify")) electronApps.Add("Spotify");
        if (procNames.Contains("steamwebhelper")) electronApps.Add("Steam Web Helper");

        if (electronApps.Count > 0)
        {
            findings.Add(new DiagnosticFinding
            {
                Category = "GPU Workload",
                ProcessOrComponent = string.Join(", ", electronApps),
                HumanExplanation = $"Las aplicaciones ({string.Join(", ", electronApps)}) utilizan aceleración por hardware (Chromium/Electron). Cada ventana abierta dibuja a 60 fps en la GPU aun sin foco, compitiendo con el renderloop de la aplicación principal.",
                RecommendedAction = "Aplica el perfil 'Low Interference' o 'Max Response' para reducir su prioridad y desviar sus hilos a núcleos secundarios.",
                IsSevere = false
            });
        }

        // 3. Check Windows Search Indexer
        if (procNames.Contains("SearchIndexer"))
        {
            findings.Add(new DiagnosticFinding
            {
                Category = "Disk I/O",
                ProcessOrComponent = "SearchIndexer (WSearch)",
                HumanExplanation = "El indexador de Windows Search está activo en segundo plano monitoreando cambios en el diario NTFS de tus unidades de disco.",
                RecommendedAction = "Pausar temporalmente el servicio WSearch en sesiones de alto rendimiento.",
                IsSevere = false
            });
        }

        // 4. Check Cloud Sync
        var syncEngines = new List<string>();
        if (procNames.Contains("OneDrive")) syncEngines.Add("OneDrive");
        if (procNames.Contains("Dropbox")) syncEngines.Add("Dropbox");
        if (procNames.Contains("GoogleDriveFS")) syncEngines.Add("Google Drive");

        if (syncEngines.Count > 0)
        {
            findings.Add(new DiagnosticFinding
            {
                Category = "Disk & Network I/O",
                ProcessOrComponent = string.Join(", ", syncEngines),
                HumanExplanation = $"Los sincronizadores ({string.Join(", ", syncEngines)}) mantienen sockets TCP abiertos y escanean hashes de archivos en disco periódicamente.",
                RecommendedAction = "Pausar sincronización mientras juegas o trabajas con tareas de baja latencia.",
                IsSevere = false
            });
        }

        // 5. Check RGB / Peripheral Software
        var rgbApps = new List<string>();
        if (procNames.Contains("iCUE") || procNames.Contains("Corsair.Service")) rgbApps.Add("Corsair iCUE");
        if (procNames.Contains("Razer Synapse Service") || procNames.Contains("RazerCentralService")) rgbApps.Add("Razer Synapse");
        if (procNames.Contains("ArmouryCrate.Service") || procNames.Contains("LightingService")) rgbApps.Add("ASUS Armoury Crate / Aura");

        if (rgbApps.Count > 0)
        {
            findings.Add(new DiagnosticFinding
            {
                Category = "CPU Wakeups & Jitter",
                ProcessOrComponent = string.Join(", ", rgbApps),
                HumanExplanation = $"Las suites de periféricos ({string.Join(", ", rgbApps)}) realizan sondeo USB constante a 1000 Hz y bloquean la resolución del temporizador a 1.0 ms o 0.5 ms, despertando núcleos de CPU de estados de reposo C-States.",
                RecommendedAction = "Permitir que el mouse y teclado operen con sus perfiles de memoria onboard en sesiones críticas.",
                IsSevere = true
            });
        }

        // Cleanup process handles
        foreach (var p in activeProcesses) p.Dispose();

        // 6. If no notable findings
        if (findings.Count == 0)
        {
            findings.Add(new DiagnosticFinding
            {
                Category = "Optimal State",
                ProcessOrComponent = "System Idle",
                HumanExplanation = "Tu escritorio se encuentra en reposo óptimo con mínima interferencia de fondo detectada.",
                RecommendedAction = "No se requieren acciones adicionales.",
                IsSevere = false
            });
        }

        return findings;
    }
}
