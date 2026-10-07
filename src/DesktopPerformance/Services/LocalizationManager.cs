using System.Globalization;

namespace DesktopPerformance.Services;

public enum AppLanguage
{
    English,
    Spanish
}

public class LocalizationManager
{
    private static readonly Lazy<LocalizationManager> _instance = new(() => new LocalizationManager());
    public static LocalizationManager Instance => _instance.Value;

    private readonly string _settingsFilePath;
    private AppLanguage _currentLanguage;

    public event Action? OnLanguageChanged;

    public AppLanguage CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value)
            {
                _currentLanguage = value;
                SaveLanguagePreference(value);
                OnLanguageChanged?.Invoke();
            }
        }
    }

    public LocalizationManager()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string folder = Path.Combine(appData, "DesktopPerformance98");
        _settingsFilePath = Path.Combine(folder, "language.txt");

        _currentLanguage = LoadLanguagePreference();
    }

    private AppLanguage LoadLanguagePreference()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                string text = File.ReadAllText(_settingsFilePath).Trim();
                if (text.Equals("es", StringComparison.OrdinalIgnoreCase)) return AppLanguage.Spanish;
                if (text.Equals("en", StringComparison.OrdinalIgnoreCase)) return AppLanguage.English;
            }
        }
        catch
        {
            // Fallback to culture detection
        }

        // Automatic system culture detection
        string twoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return twoLetter.Equals("es", StringComparison.OrdinalIgnoreCase) ? AppLanguage.Spanish : AppLanguage.English;
    }

    private void SaveLanguagePreference(AppLanguage language)
    {
        try
        {
            string? dir = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(_settingsFilePath, language == AppLanguage.Spanish ? "es" : "en");
        }
        catch
        {
            // Non-critical persistence failure
        }
    }

    public string T(string key)
    {
        if (Translations.TryGetValue(_currentLanguage, out var dict) && dict.TryGetValue(key, out var val))
        {
            return val;
        }

        // Fallback to English
        if (Translations[AppLanguage.English].TryGetValue(key, out var fallback))
        {
            return fallback;
        }

        return key;
    }

    private static readonly Dictionary<AppLanguage, Dictionary<string, string>> Translations = new()
    {
        [AppLanguage.English] = new Dictionary<string, string>
        {
            // App Title
            ["App_Title"] = "ZeroLatency98 - Windows Latency & Classic Theme Suite",

            // Menus
            ["Menu_File"] = "&File",
            ["Menu_RefreshAll"] = "&Refresh All Data",
            ["Menu_ExportBenchmark"] = "&Export Benchmark JSON...",
            ["Menu_ExportProfiles"] = "&Export Profiles JSON...",
            ["Menu_Exit"] = "E&xit",

            ["Menu_Profiles"] = "&Profiles",
            ["Menu_Normal"] = "&Normal (Baseline)",
            ["Menu_LowInterference"] = "&Low Interference",
            ["Menu_MaxResponse"] = "&Max Response",
            ["Menu_RestoreNormal"] = "&Restore to Normal (Undo All)",

            ["Menu_Diagnostics"] = "&Diagnostics",
            ["Menu_WhyBusy"] = "&Why Is My Desktop Busy?",
            ["Menu_ProcessDb"] = "&Process Safety Database...",

            ["Menu_Language"] = "&Language",
            ["Menu_LangEnglish"] = "&English (US)",
            ["Menu_LangSpanish"] = "&Español",

            ["Menu_Help"] = "&Help",
            ["Menu_About"] = "&About ZeroLatency98",

            // Tabs
            ["Tab_Topology"] = "System Topology & Monitors",
            ["Tab_Profiles"] = "Profiles & Optimization",
            ["Tab_Background"] = "Background Manager",
            ["Tab_Benchmark"] = "Verification & Benchmark",
            ["Tab_Diagnostics"] = "Diagnostics & Smart Profiles",
            ["Tab_MultiMonitor"] = "Multi-Monitor Studio & VRR",
            ["Tab_RetroShell"] = "Retro Shell & Themes",

            // Tab 1: Topology
            ["Topology_DisplaysGrp"] = "Active Displays and Monitors",
            ["Topology_HardwareGrp"] = "Hardware and Memory State",
            ["Topology_VisualGrp"] = "Windows 11 Visual Subsystem State",
            ["Topology_RefreshBtn"] = "&Refresh Topology",

            // Tab 2: Profiles
            ["Profiles_SelectorGrp"] = "Performance Profile Selection",
            ["Profiles_Normal_Desc"] = "NORMAL - Baseline (Windows 11 default, zero modifications)",
            ["Profiles_Low_Desc"] = "LOW INTERFERENCE - Safe Visual & I/O Reduction (Transparency OFF, Anim OFF, Search Pause, OneDrive Pause)",
            ["Profiles_Max_Desc"] = "MAX RESPONSE - Low Interference + Background CPU Isolation & Priority Demotion",
            ["Profiles_ApplyBtn"] = "Apply Selected Profile",
            ["Profiles_RestoreBtn"] = "Restore to Normal (Undo All)",
            ["Profiles_LogGrp"] = "Execution & Snapshot Journal",

            // Tab 3: Background Manager
            ["Background_Info"] = "Supervised background applications. Whitelisted components (Steam, OBS, audio drivers) are permanently protected.",
            ["Background_PauseBtn"] = "Pause App",
            ["Background_PriorityBtn"] = "Lower Priority & Affinity",
            ["Background_ResumeBtn"] = "Resume / Normal",
            ["Background_RefreshBtn"] = "Refresh List",

            // Tab 4: Benchmark
            ["Benchmark_HeaderGrp"] = "Performance Data Helper (PDH) Measurement Engine",
            ["Benchmark_Notice"] = "Principle: 'Measurement before claims. Never fabricate improvements.'\nThis module samples system noise before and after profile activation. If difference is below 2.0%,\nit honestly reports: 'No measurable improvement detected'.",
            ["Benchmark_RunBtn"] = "Run Benchmark",
            ["Benchmark_TrilateralBtn"] = "Trilateral Test",
            ["Benchmark_ExportBtn"] = "Export JSON",
            ["Benchmark_StatusIdle"] = "Status: Benchmark idle.",
            ["Benchmark_VerdictGrp"] = "Telemetry Verdict",
            ["Benchmark_DefaultVerdict"] = "Run the benchmark to analyze real-world system noise and interference delta.",

            // Tab 5: Diagnostics
            ["Diag_WhyGrp"] = "Diagnostic: Why Is My Desktop Busy?",
            ["Diag_RunBtn"] = "Run Interference Diagnostic",
            ["Diag_SmartGrp"] = "Smart Profiles & Auto Game Detection",
            ["Diag_ModeLbl"] = "Game Detection Mode:",
            ["Diag_ModeAuto"] = "AUTO (Auto-apply on launch, auto-restore on exit)",
            ["Diag_ModeManual"] = "MANUAL (Prompt only)",
            ["Diag_ModeDisabled"] = "DISABLED",
            ["Diag_ApplySmartBtn"] = "Apply Selected Smart Profile",

            // Tab 6: Multi-Monitor
            ["MultiMonitor_OffendersGrp"] = "Secondary Display Offender Applications",
            ["MultiMonitor_AnalyzeBtn"] = "Analyze Multi-Display Desync",
            ["MultiMonitor_BlankBtn"] = "Apply Static Background to Secondary Monitors",

            // Tab 7: Retro Shell & Classic Theme (Phase 5)
            ["RetroShell_ThemesGrp"] = "System-Wide Classic Windows 95/98 Themes (Low Load)",
            ["RetroShell_ApplyBtn"] = "Apply Classic Theme to System",
            ["RetroShell_RestoreBtn"] = "Restore Windows 11 Default Theme",
            ["RetroShell_Notice"] = "Applies native .theme file, SetSysColors palette, solid teal desktop, and shuts down DWM blur shaders for maximum system responsiveness.",
            ["RetroShell_StatusTitle"] = "System Theme Status:",
            ["RetroShell_StatusClassic"] = "ACTIVE: Classic Theme (Zero-Overhead)",
            ["RetroShell_StatusModern"] = "ACTIVE: Windows 11 Modern (DWM Shaders On)",
            ["RetroShell_CompanionGrp"] = "Retro Shell Companion Ecosystem (Optional Taskbar & Start Menu)",
            ["RetroShell_LaunchRetroBar"] = "Launch RetroBar",
            ["RetroShell_GetRetroBar"] = "Get RetroBar (GitHub)",
            ["RetroShell_LaunchOpenShell"] = "Launch Open-Shell",
            ["RetroShell_GetOpenShell"] = "Get Open-Shell (GitHub)",
            ["RetroShell_GuideGrp"] = "Safe Retro Shell Ecosystem Guide",
            ["RetroShell_GuideText"] = "RECOMMENDATIONS FOR FULL RETRO SHELL (WINDOWS 11):\n\n" +
                                       "1. Open-Shell (Recommended - SAFE):\n" +
                                       "   Replaces the modern Start menu with a native Win32 classic menu. Zero GPU composition, no destructive file injection.\n\n" +
                                       "2. Windhawk (Recommended for Window Borders - LOW RISK):\n" +
                                       "   Applies classic 98/2000 square border modifications entirely in RAM without modifying system binaries on disk.\n\n" +
                                       "3. ExplorerPatcher (WARNING - HIGH RISK):\n" +
                                       "   Not recommended. Injects dxgi.dll directly into explorer.exe, frequently causing shell crashes during monthly Windows 11 cumulative updates.",

            // Status Bar
            ["Status_Ready"] = "Ready",
            ["Status_ProfilePrefix"] = "Profile: ",
            ["Status_SnapshotActive"] = "Snapshot: ACTIVE",
            ["Status_SnapshotNone"] = "Snapshot: None",

            // Column Headers
            ["Col_Application"] = "Application",
            ["Col_Process"] = "Process",
            ["Col_State"] = "State",
            ["Col_Protection"] = "Protection / Whitelist",
            ["Col_Details"] = "Details",
            ["Col_Monitor"] = "Monitor / Device",
            ["Col_Resolution"] = "Resolution",
            ["Col_RefreshRate"] = "Refresh Rate",
            ["Col_Depth"] = "Depth",
            ["Col_Primary"] = "Primary",
            ["Col_Metric"] = "Metric Indicator",
            ["Col_Before"] = "BEFORE (Baseline)",
            ["Col_After"] = "AFTER (Profile)",
            ["Col_Delta"] = "Delta Variation",
            ["Col_Evaluation"] = "Evaluation",
            ["Col_Category"] = "Category",
            ["Col_Component"] = "Component / App",
            ["Col_Explanation"] = "Human Explanation",
            ["Col_Action"] = "Recommended Action",
            ["Col_ProfileName"] = "Profile Name",
            ["Col_TriggerProcess"] = "Trigger Process",
            ["Col_Description"] = "Description",
            ["Col_CpuIsolation"] = "CPU Isolation",
            ["Col_WindowTitle"] = "Process & Window Title",
            ["Col_Impact"] = "Impact on Primary Monitor",
            ["Col_ThemeName"] = "Theme Name",
            ["Col_Preset"] = "Preset",
            ["Col_Classification"] = "Classification",
            ["Col_Vendor"] = "Vendor",
            ["Col_ImpactSafety"] = "Technical Impact & Safety Details"
        },

        [AppLanguage.Spanish] = new Dictionary<string, string>
        {
            // App Title
            ["App_Title"] = "ZeroLatency98 - Modo Baja Latencia y Tema Clásico",

            // Menus
            ["Menu_File"] = "&Archivo",
            ["Menu_RefreshAll"] = "&Actualizar Todos los Datos",
            ["Menu_ExportBenchmark"] = "&Exportar Benchmark JSON...",
            ["Menu_ExportProfiles"] = "&Exportar Perfiles JSON...",
            ["Menu_Exit"] = "&Salir",

            ["Menu_Profiles"] = "&Perfiles",
            ["Menu_Normal"] = "&Normal (Línea Base)",
            ["Menu_LowInterference"] = "&Baja Interferencia",
            ["Menu_MaxResponse"] = "&Máxima Respuesta",
            ["Menu_RestoreNormal"] = "&Restaurar a Normal (Deshacer Todo)",

            ["Menu_Diagnostics"] = "&Diagnósticos",
            ["Menu_WhyBusy"] = "&¿Por Qué Mi Desktop Está Ocupado?",
            ["Menu_ProcessDb"] = "&Base de Datos de Seguridad de Procesos...",

            ["Menu_Language"] = "&Idioma",
            ["Menu_LangEnglish"] = "&English (US)",
            ["Menu_LangSpanish"] = "&Español",

            ["Menu_Help"] = "A&yuda",
            ["Menu_About"] = "&Acerca de ZeroLatency98",

            // Tabs
            ["Tab_Topology"] = "Topología de Sistema y Monitores",
            ["Tab_Profiles"] = "Perfiles y Optimización",
            ["Tab_Background"] = "Administrador de Fondo",
            ["Tab_Benchmark"] = "Verificación y Benchmark",
            ["Tab_Diagnostics"] = "Diagnósticos y Perfiles Inteligentes",
            ["Tab_MultiMonitor"] = "Estudio Multi-Monitor y VRR",
            ["Tab_RetroShell"] = "Shell Retro y Temas",

            // Tab 1: Topology
            ["Topology_DisplaysGrp"] = "Pantallas y Monitores Activos",
            ["Topology_HardwareGrp"] = "Estado de Hardware y Memoria",
            ["Topology_VisualGrp"] = "Estado del Subsistema Visual de Windows 11",
            ["Topology_RefreshBtn"] = "&Actualizar Topología",

            // Tab 2: Profiles
            ["Profiles_SelectorGrp"] = "Selección de Perfil de Rendimiento",
            ["Profiles_Normal_Desc"] = "NORMAL - Línea Base (predeterminado de Windows 11, cero modificaciones)",
            ["Profiles_Low_Desc"] = "BAJA INTERFERENCIA - Reducción Segura Visual y E/S (Transparencia OFF, Anim OFF, Pausa Search y OneDrive)",
            ["Profiles_Max_Desc"] = "MÁXIMA RESPUESTA - Baja Interferencia + Aislamiento CPU y Prioridad de Fondo",
            ["Profiles_ApplyBtn"] = "Aplicar Perfil Seleccionado",
            ["Profiles_RestoreBtn"] = "Restaurar a Normal (Deshacer Todo)",
            ["Profiles_LogGrp"] = "Registro de Ejecución y Snapshots",

            // Tab 3: Background Manager
            ["Background_Info"] = "Aplicaciones de fondo supervisadas. Componentes en lista blanca (Steam, OBS, drivers de audio) están protegidos permanentemente.",
            ["Background_PauseBtn"] = "Pausar App",
            ["Background_PriorityBtn"] = "Bajar Prioridad y Afinidad",
            ["Background_ResumeBtn"] = "Reanudar / Normal",
            ["Background_RefreshBtn"] = "Actualizar Lista",

            // Tab 4: Benchmark
            ["Benchmark_HeaderGrp"] = "Motor de Medición Performance Data Helper (PDH)",
            ["Benchmark_Notice"] = "Principio: 'Medición antes que afirmaciones. No inventar mejoras.'\nEste módulo toma muestras antes y después de aplicar el perfil. Si la diferencia es menor al 2.0%,\nse reportará honestamente: 'No measurable improvement detected'.",
            ["Benchmark_RunBtn"] = "Ejecutar Benchmark",
            ["Benchmark_TrilateralBtn"] = "Prueba Trilateral",
            ["Benchmark_ExportBtn"] = "Exportar JSON",
            ["Benchmark_StatusIdle"] = "Estado: Benchmark inactivo.",
            ["Benchmark_VerdictGrp"] = "Veredicto de Telemetría",
            ["Benchmark_DefaultVerdict"] = "Ejecute el benchmark para analizar el ruido y delta real de interferencia del sistema.",

            // Tab 5: Diagnostics
            ["Diag_WhyGrp"] = "Diagnóstico: ¿Por Qué Mi Desktop Está Ocupado?",
            ["Diag_RunBtn"] = "Ejecutar Diagnóstico de Interferencia",
            ["Diag_SmartGrp"] = "Perfiles Inteligentes y Detección Automática de Juegos",
            ["Diag_ModeLbl"] = "Modo de Detección de Juegos:",
            ["Diag_ModeAuto"] = "AUTO (Auto-aplicar al iniciar, auto-restaurar al salir)",
            ["Diag_ModeManual"] = "MANUAL (Sólo avisar)",
            ["Diag_ModeDisabled"] = "DESACTIVADO",
            ["Diag_ApplySmartBtn"] = "Aplicar Perfil Inteligente",

            // Tab 6: Multi-Monitor
            ["MultiMonitor_OffendersGrp"] = "Aplicaciones Problemáticas en Pantallas Secundarias",
            ["MultiMonitor_AnalyzeBtn"] = "Analizar Desincronización",
            ["MultiMonitor_BlankBtn"] = "Fondo Estático en Pantallas Secundarias",

            // Tab 7: Retro Shell & Classic Theme (Fase 5)
            ["RetroShell_ThemesGrp"] = "Temas Clásicos de Windows 95/98 para el Sistema (Baja Carga)",
            ["RetroShell_ApplyBtn"] = "Aplicar Tema Clásico al Sistema",
            ["RetroShell_RestoreBtn"] = "Restaurar Tema Original de Windows 11",
            ["RetroShell_Notice"] = "Aplica archivo .theme nativo, paleta SetSysColors, fondo verde azulado sólido y apaga shaders de desenfoque de DWM para máxima respuesta del sistema.",
            ["RetroShell_StatusTitle"] = "Estado del Tema del Sistema:",
            ["RetroShell_StatusClassic"] = "ACTIVO: Tema Clásico (Cero Sobrecarga)",
            ["RetroShell_StatusModern"] = "ACTIVO: Windows 11 Moderno (Shaders DWM Activos)",
            ["RetroShell_CompanionGrp"] = "Ecosistema de Shell Retro Complementario (Barra de Tareas y Menú Inicio)",
            ["RetroShell_LaunchRetroBar"] = "Ejecutar RetroBar",
            ["RetroShell_GetRetroBar"] = "Obtener RetroBar (GitHub)",
            ["RetroShell_LaunchOpenShell"] = "Ejecutar Open-Shell",
            ["RetroShell_GetOpenShell"] = "Obtener Open-Shell (GitHub)",
            ["RetroShell_GuideGrp"] = "Guía de Ecosistema Shell Retro Seguro",
            ["RetroShell_GuideText"] = "RECOMENDACIONES PARA EL SHELL RETRO COMPLETO (WINDOWS 11):\n\n" +
                                       "1. Open-Shell (Recomendado - SEGURO):\n" +
                                       "   Reemplaza el menú inicio por un menú Win32 clásico nativo. Cero uso de GPU, sin inyecciones destructivas.\n\n" +
                                       "2. Windhawk (Recomendado para Bordes Clásicos - RIESGO BAJO):\n" +
                                       "   Aplica mods de bordes cuadrados clásicos en RAM sin modificar archivos binarios de Windows en disco.\n\n" +
                                       "3. ExplorerPatcher (ADVERTENCIA - RIESGO ALTO):\n" +
                                       "   No recomendado. Inyecta dxgi.dll en explorer.exe, provocando bloqueos frecuentes en actualizaciones acumulativas mensuales de Windows 11.",

            // Status Bar
            ["Status_Ready"] = "Listo",
            ["Status_ProfilePrefix"] = "Perfil: ",
            ["Status_SnapshotActive"] = "Instantánea: ACTIVA",
            ["Status_SnapshotNone"] = "Instantánea: Ninguna",

            // Column Headers
            ["Col_Application"] = "Aplicación",
            ["Col_Process"] = "Proceso",
            ["Col_State"] = "Estado",
            ["Col_Protection"] = "Protección / Lista Blanca",
            ["Col_Details"] = "Detalles",
            ["Col_Monitor"] = "Monitor / Dispositivo",
            ["Col_Resolution"] = "Resolución",
            ["Col_RefreshRate"] = "Tasa de Refresco",
            ["Col_Depth"] = "Profundidad",
            ["Col_Primary"] = "Principal",
            ["Col_Metric"] = "Indicador Métrico",
            ["Col_Before"] = "ANTES (Línea Base)",
            ["Col_After"] = "DESPUÉS (Perfil)",
            ["Col_Delta"] = "Variación Delta",
            ["Col_Evaluation"] = "Evaluación",
            ["Col_Category"] = "Categoría",
            ["Col_Component"] = "Componente / App",
            ["Col_Explanation"] = "Explicación Técnica",
            ["Col_Action"] = "Acción Recomendada",
            ["Col_ProfileName"] = "Nombre de Perfil",
            ["Col_TriggerProcess"] = "Proceso Disparador",
            ["Col_Description"] = "Descripción",
            ["Col_CpuIsolation"] = "Aislamiento CPU",
            ["Col_WindowTitle"] = "Proceso y Título de Ventana",
            ["Col_Impact"] = "Impacto en Monitor Principal",
            ["Col_ThemeName"] = "Nombre del Tema",
            ["Col_Preset"] = "Preset",
            ["Col_Classification"] = "Clasificación",
            ["Col_Vendor"] = "Proveedor",
            ["Col_ImpactSafety"] = "Impacto Técnico y Detalles de Seguridad"
        }
    };
}
