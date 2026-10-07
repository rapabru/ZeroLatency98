using System.Diagnostics;
using DesktopPerformance.Models;
using DesktopPerformance.Services;

namespace DesktopPerformance.UI;

public class MainForm : Form
{
    private readonly HardwareInspector _hardwareInspector;
    private readonly VisualEffectsManager _visualEffects;
    private readonly ServiceSupervisor _services;
    private readonly ProcessSupervisor _processes;
    private readonly SnapshotManager _snapshotManager;
    private readonly ProfileEngine _profileEngine;
    private readonly BenchmarkEngine _benchmarkEngine;
    private readonly SmartProfileManager _smartProfileManager;
    private readonly ProcessWatcher _processWatcher;
    private readonly DesktopBusyAnalyzer _diagnosticAnalyzer;
    private readonly BackgroundDatabase _backgroundDatabase;
    private readonly ExportManager _exportManager;
    private readonly MultiMonitorOptimizer _multiMonitorOptimizer;
    private readonly AdvancedBenchmarkEngine _advancedBenchmarkEngine;
    private readonly RetroShellEngine _retroShellEngine;
    private readonly ClassicThemeManager _classicThemeManager;
    private readonly ProfileLearner _profileLearner;

    private BenchmarkComparison? _lastBenchmarkComparison;

    // UI Controls
    private MenuStrip _menuStrip = null!;
    private RetroPanel _summaryPanel = null!;
    private Label _lblSummary = null!;
    private TabControl _tabControl = null!;

    // Menu controls for localization
    private ToolStripMenuItem _menuFile = null!;
    private ToolStripMenuItem _menuRefreshAll = null!;
    private ToolStripMenuItem _menuExportBenchmark = null!;
    private ToolStripMenuItem _menuExportProfiles = null!;
    private ToolStripMenuItem _menuExit = null!;

    private ToolStripMenuItem _menuProfiles = null!;
    private ToolStripMenuItem _menuProfileNormal = null!;
    private ToolStripMenuItem _menuProfileLow = null!;
    private ToolStripMenuItem _menuProfileMax = null!;
    private ToolStripMenuItem _menuRestoreNormal = null!;

    private ToolStripMenuItem _menuDiagnostics = null!;
    private ToolStripMenuItem _menuWhyBusy = null!;
    private ToolStripMenuItem _menuProcessDb = null!;

    private ToolStripMenuItem _menuLanguage = null!;
    private ToolStripMenuItem _menuLangEnglish = null!;
    private ToolStripMenuItem _menuLangSpanish = null!;

    private ToolStripMenuItem _menuHelp = null!;
    private ToolStripMenuItem _menuAbout = null!;

    // Tab 1: Topology
    private TabPage _tabTopology = null!;
    private GroupBox _grpTopologyDisplays = null!;
    private GroupBox _grpTopologyHardware = null!;
    private GroupBox _grpTopologyVisual = null!;
    private ListView _lvDisplays = null!;
    private Label _lblHardwareDetails = null!;
    private Label _lblVisualDetails = null!;
    private RetroButton _btnRefreshTopology = null!;

    // Tab 2: Profiles
    private TabPage _tabProfiles = null!;
    private GroupBox _grpProfilesSelector = null!;
    private GroupBox _grpProfilesLog = null!;
    private RadioButton _rbNormal = null!;
    private RadioButton _rbLowInterference = null!;
    private RadioButton _rbMaxResponse = null!;
    private RetroButton _btnApplyProfile = null!;
    private RetroButton _btnRestoreNormal = null!;
    private TextBox _txtLog = null!;

    // Tab 3: Background Manager
    private TabPage _tabBackground = null!;
    private Label _lblBackgroundInfo = null!;
    private ListView _lvApps = null!;
    private RetroButton _btnRefreshApps = null!;
    private RetroButton _btnPauseApp = null!;
    private RetroButton _btnResumeApp = null!;
    private RetroButton _btnBackgroundPriority = null!;

    // Tab 4: Benchmark
    private TabPage _tabBenchmark = null!;
    private GroupBox _grpBenchmarkHeader = null!;
    private Label _lblBenchmarkNotice = null!;
    private RetroButton _btnRunBenchmark = null!;
    private RetroButton _btnRunAdvancedBenchmark = null!;
    private Label _lblBenchmarkStatus = null!;
    private ListView _lvBenchmarkResults = null!;
    private GroupBox _grpBenchmarkVerdict = null!;
    private TextBox _txtBenchmarkVerdict = null!;
    private RetroButton _btnExportBenchmark = null!;

    // Tab 5: Phase 3 Diagnostics & Smart Profiles
    private TabPage _tabDiagnostics = null!;
    private GroupBox _grpDiagnosticWhy = null!;
    private ListView _lvDiagnosticFindings = null!;
    private RetroButton _btnRunDiagnostic = null!;
    private GroupBox _grpSmartProfiles = null!;
    private Label _lblGameMode = null!;
    private RadioButton _rbAutoGame = null!;
    private RadioButton _rbManualGame = null!;
    private RadioButton _rbDisabledGame = null!;
    private Label _lblWatcherStatus = null!;
    private ListView _lvSmartProfiles = null!;
    private RetroButton _btnApplySmartProfile = null!;
    private TextBox _txtLearningSuggestions = null!;

    // Tab 6: Multi-Monitor & VRR Studio
    private TabPage _tabMultiMonitor = null!;
    private Label _lblMultiMonitorSummary = null!;
    private GroupBox _grpMultiMonitorOffenders = null!;
    private ListView _lvSecondaryOffenders = null!;
    private RetroButton _btnAnalyzeMonitors = null!;
    private RetroButton _btnBlankSecondary = null!;

    // Tab 7: Retro Desktop Shell & Themes (Phase 5)
    private TabPage _tabRetroShell = null!;
    private GroupBox _grpThemeStatus = null!;
    private Label _lblThemeStatus = null!;
    private Label _lblThemeNotice = null!;
    private GroupBox _grpRetroThemes = null!;
    private ListView _lvThemes = null!;
    private RetroButton _btnApplyTheme = null!;
    private RetroButton _btnRestoreTheme = null!;
    private GroupBox _grpCompanion = null!;
    private ListView _lvCompanionTools = null!;
    private RetroButton _btnLaunchCompanion = null!;
    private RetroButton _btnGetCompanion = null!;
    private GroupBox _grpRetroGuide = null!;
    private TextBox _txtShellGuide = null!;

    // Status bar
    private StatusStrip _statusStrip = null!;
    private ToolStripStatusLabel _statusLabel = null!;
    private ToolStripStatusLabel _statusProfile = null!;
    private ToolStripStatusLabel _statusSnapshot = null!;

    public MainForm()
    {
        _hardwareInspector = new HardwareInspector();
        _visualEffects = new VisualEffectsManager();
        _services = new ServiceSupervisor();
        _processes = new ProcessSupervisor();
        _snapshotManager = new SnapshotManager();
        _profileEngine = new ProfileEngine(_visualEffects, _services, _processes, _snapshotManager);
        _benchmarkEngine = new BenchmarkEngine();
        _smartProfileManager = new SmartProfileManager();
        _processWatcher = new ProcessWatcher(_smartProfileManager, _profileEngine);
        _diagnosticAnalyzer = new DesktopBusyAnalyzer(_hardwareInspector);
        _backgroundDatabase = new BackgroundDatabase();
        _exportManager = new ExportManager();
        _multiMonitorOptimizer = new MultiMonitorOptimizer(_hardwareInspector);
        _advancedBenchmarkEngine = new AdvancedBenchmarkEngine(_benchmarkEngine);
        _retroShellEngine = new RetroShellEngine();
        _classicThemeManager = new ClassicThemeManager(_visualEffects);
        _profileLearner = new ProfileLearner();

        InitializeComponent();
        LocalizationManager.Instance.OnLanguageChanged += ApplyLocalization;
        ApplyLocalization();
        _profileEngine.Initialize();
        SetupProcessWatcher();
        RefreshAllData();
    }

    private void SetupProcessWatcher()
    {
        _processWatcher.OnGameStarted += (proc, profile) =>
        {
            if (InvokeRequired)
            {
                Invoke(() => HandleGameStarted(proc, profile));
                return;
            }
            HandleGameStarted(proc, profile);
        };

        _processWatcher.OnGameExited += (proc) =>
        {
            if (InvokeRequired)
            {
                Invoke(() => HandleGameExited(proc));
                return;
            }
            HandleGameExited(proc);
        };

        _processWatcher.OnGameDetectedManual += (proc, profile) =>
        {
            if (InvokeRequired)
            {
                Invoke(() => AppendLog($"[AUTO-DETECTOR] Registered game detected: {proc}. Mode is MANUAL (no action forced)."));
                return;
            }
            AppendLog($"[AUTO-DETECTOR] Registered game detected: {proc}. Mode is MANUAL (no action forced).");
        };

        _processWatcher.Start(2000);
    }

    private void HandleGameStarted(string proc, SmartProfile profile)
    {
        AppendLog($"[AUTO-DETECTOR] Process '{proc}' started! Automatically applied profile: '{profile.Name}'.");
        _lblWatcherStatus.Text = $"Active Game Hook: {proc} (Profile: {profile.Name})";

        // Log session into local pattern learner
        var activeApps = _processes.ScanManagedApps().Where(a => a.CurrentState != ProcessExecutionState.NotRunning).Select(a => a.ProcessName);
        _profileLearner.RecordSession(proc, activeApps);

        RefreshAllData();
    }

    private void HandleGameExited(string proc)
    {
        AppendLog($"[AUTO-DETECTOR] Process '{proc}' exited. Automatically restored NORMAL baseline.");
        _lblWatcherStatus.Text = "Active Game Hook: Idle (Watching for registered games...)";
        RefreshAllData();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _processWatcher.Dispose();
        base.OnFormClosing(e);
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        Text = "ZeroLatency98 - Windows Latency & Classic Theme Suite";
        Width = 880;
        Height = 680;
        MinimumSize = new Size(820, 620);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = RetroTheme.BackgroundColor;
        Font = RetroTheme.DefaultFont;
        ForeColor = RetroTheme.Black;

        // 1. MenuStrip
        _menuStrip = new MenuStrip
        {
            BackColor = RetroTheme.BackgroundColor,
            Font = RetroTheme.DefaultFont,
            RenderMode = ToolStripRenderMode.System
        };

        _menuFile = new ToolStripMenuItem("&File");
        _menuRefreshAll = new ToolStripMenuItem("&Refresh All Data", null, (s, e) => RefreshAllData());
        _menuExportBenchmark = new ToolStripMenuItem("&Export Benchmark JSON...", null, (s, e) => ExportBenchmark());
        _menuExportProfiles = new ToolStripMenuItem("&Export Profiles JSON...", null, (s, e) => ExportProfiles());
        _menuExit = new ToolStripMenuItem("E&xit", null, (s, e) => Close());
        _menuFile.DropDownItems.AddRange(new ToolStripItem[] { _menuRefreshAll, new ToolStripSeparator(), _menuExportBenchmark, _menuExportProfiles, new ToolStripSeparator(), _menuExit });

        _menuProfiles = new ToolStripMenuItem("&Profiles");
        _menuProfileNormal = new ToolStripMenuItem("&Normal (Baseline)", null, async (s, e) => await ApplyProfile(ProfileType.Normal));
        _menuProfileLow = new ToolStripMenuItem("&Low Interference", null, async (s, e) => await ApplyProfile(ProfileType.LowInterference));
        _menuProfileMax = new ToolStripMenuItem("&Max Response", null, async (s, e) => await ApplyProfile(ProfileType.MaxResponse));
        _menuRestoreNormal = new ToolStripMenuItem("&Restore to Normal (Undo All)", null, async (s, e) => await RestoreToNormal());
        _menuProfiles.DropDownItems.AddRange(new ToolStripItem[] { _menuProfileNormal, _menuProfileLow, _menuProfileMax, new ToolStripSeparator(), _menuRestoreNormal });

        _menuDiagnostics = new ToolStripMenuItem("&Diagnostics");
        _menuWhyBusy = new ToolStripMenuItem("&Why Is My Desktop Busy?", null, (s, e) => RunDesktopDiagnostic());
        _menuProcessDb = new ToolStripMenuItem("&Process Safety Database...", null, (s, e) => ShowProcessDatabaseDialog());
        _menuDiagnostics.DropDownItems.AddRange(new ToolStripItem[] { _menuWhyBusy, _menuProcessDb });

        _menuLanguage = new ToolStripMenuItem("&Language");
        _menuLangEnglish = new ToolStripMenuItem("&English (US)", null, (s, e) => LocalizationManager.Instance.CurrentLanguage = AppLanguage.English);
        _menuLangSpanish = new ToolStripMenuItem("&Español", null, (s, e) => LocalizationManager.Instance.CurrentLanguage = AppLanguage.Spanish);
        _menuLanguage.DropDownItems.AddRange(new ToolStripItem[] { _menuLangEnglish, _menuLangSpanish });

        _menuHelp = new ToolStripMenuItem("&Help");
        _menuAbout = new ToolStripMenuItem("&About ZeroLatency98", null, (s, e) =>
        {
            MessageBox.Show(
                "ZeroLatency98 - Windows Latency & Classic Theme Suite\n\n" +
                "Authentic Windows 98/2000 Retro Aesthetics for Windows 10/11.\n" +
                "Zero bloat, zero placebo, atomic snapshots, VRR protection, and System-Wide Classic Theme Mode.\n\n" +
                "Version 1.5.0",
                "About ZeroLatency98", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
        _menuHelp.DropDownItems.Add(_menuAbout);

        _menuStrip.Items.AddRange(new ToolStripItem[] { _menuFile, _menuProfiles, _menuDiagnostics, _menuLanguage, _menuHelp });

        // 2. Summary Panel (Sunken)
        _summaryPanel = new RetroPanel
        {
            Dock = DockStyle.Top,
            Height = 44,
            Padding = new Padding(8),
            IsSunken = true
        };
        _lblSummary = new Label
        {
            Dock = DockStyle.Fill,
            Font = RetroTheme.DefaultFont,
            ForeColor = RetroTheme.Black,
            TextAlign = ContentAlignment.MiddleLeft,
            Text = "Loading hardware topology..."
        };
        _summaryPanel.Controls.Add(_lblSummary);

        // 3. TabControl
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = RetroTheme.DefaultFont,
            Padding = new Point(10, 4)
        };

        BuildTopologyTab();
        BuildProfilesTab();
        BuildBackgroundManagerTab();
        BuildBenchmarkTab();
        BuildDiagnosticsAndSmartProfilesTab();
        BuildMultiMonitorTab();
        BuildRetroShellTab();

        // 4. StatusStrip
        _statusStrip = new StatusStrip
        {
            BackColor = RetroTheme.BackgroundColor,
            Font = RetroTheme.DefaultFont,
            SizingGrip = true
        };

        _statusLabel = new ToolStripStatusLabel("Ready") { Spring = true, TextAlign = ContentAlignment.MiddleLeft, BorderSides = ToolStripStatusLabelBorderSides.All, BorderStyle = Border3DStyle.Sunken };
        _statusProfile = new ToolStripStatusLabel("Profile: NORMAL") { Width = 160, BorderSides = ToolStripStatusLabelBorderSides.All, BorderStyle = Border3DStyle.Sunken };
        _statusSnapshot = new ToolStripStatusLabel("Snapshot: None") { Width = 180, BorderSides = ToolStripStatusLabelBorderSides.All, BorderStyle = Border3DStyle.Sunken };

        _statusStrip.Items.AddRange(new ToolStripItem[] { _statusLabel, _statusProfile, _statusSnapshot });

        // Add controls in order
        Controls.Add(_tabControl);
        Controls.Add(_summaryPanel);
        Controls.Add(_menuStrip);
        Controls.Add(_statusStrip);

        MainMenuStrip = _menuStrip;
        ResumeLayout(false);
        PerformLayout();
    }

    #region Tab 1: System Topology
    private void BuildTopologyTab()
    {
        _tabTopology = new TabPage("System Topology & Monitors")
        {
            BackColor = RetroTheme.BackgroundColor,
            Padding = new Padding(8)
        };

        _grpTopologyDisplays = new GroupBox
        {
            Text = "Active Displays and Monitors",
            Dock = DockStyle.Top,
            Height = 180,
            Font = RetroTheme.BoldFont
        };

        _lvDisplays = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = RetroTheme.DefaultFont,
            BorderStyle = BorderStyle.Fixed3D
        };
        _lvDisplays.Columns.Add("Monitor / Device", 190);
        _lvDisplays.Columns.Add("Resolution", 110);
        _lvDisplays.Columns.Add("Refresh Rate", 90);
        _lvDisplays.Columns.Add("Depth", 70);
        _lvDisplays.Columns.Add("Primary", 80);
        ListViewColumnSorter.Attach(_lvDisplays);
        _grpTopologyDisplays.Controls.Add(_lvDisplays);

        _grpTopologyHardware = new GroupBox
        {
            Text = "Hardware and Memory State",
            Dock = DockStyle.Top,
            Height = 110,
            Font = RetroTheme.BoldFont
        };
        _lblHardwareDetails = new Label
        {
            Dock = DockStyle.Fill,
            Font = RetroTheme.DefaultFont,
            Padding = new Padding(8),
            Text = "CPU: ...\nRAM: ...\nGPU: ..."
        };
        _grpTopologyHardware.Controls.Add(_lblHardwareDetails);

        _grpTopologyVisual = new GroupBox
        {
            Text = "Windows 11 Visual Subsystem State",
            Dock = DockStyle.Fill,
            Font = RetroTheme.BoldFont
        };
        _lblVisualDetails = new Label
        {
            Dock = DockStyle.Top,
            Height = 90,
            Font = RetroTheme.DefaultFont,
            Padding = new Padding(8),
            Text = "Transparency: ...\nAnimations: ...\nWidgets: ..."
        };

        _btnRefreshTopology = new RetroButton
        {
            Text = "&Refresh Topology",
            Dock = DockStyle.Bottom,
            Height = 30
        };
        _btnRefreshTopology.Click += (s, e) => RefreshAllData();

        _grpTopologyVisual.Controls.Add(_lblVisualDetails);
        _grpTopologyVisual.Controls.Add(_btnRefreshTopology);

        _tabTopology.Controls.Add(_grpTopologyVisual);
        _tabTopology.Controls.Add(_grpTopologyHardware);
        _tabTopology.Controls.Add(_grpTopologyDisplays);

        _tabControl.TabPages.Add(_tabTopology);
    }
    #endregion

    #region Tab 2: Profiles & Optimization
    private void BuildProfilesTab()
    {
        _tabProfiles = new TabPage("Profiles & Optimization")
        {
            BackColor = RetroTheme.BackgroundColor,
            Padding = new Padding(8)
        };

        _grpProfilesSelector = new GroupBox
        {
            Text = "Performance Profile Selection",
            Dock = DockStyle.Top,
            Height = 150,
            Font = RetroTheme.BoldFont
        };

        _rbNormal = new RadioButton
        {
            Text = "NORMAL - Baseline (Windows 11 default, zero modifications)",
            Location = new Point(16, 26),
            Size = new Size(600, 24),
            Font = RetroTheme.DefaultFont,
            Checked = true
        };

        _rbLowInterference = new RadioButton
        {
            Text = "LOW INTERFERENCE - Safe Visual & I/O Reduction (Transparency OFF, Anim OFF, Search Pause, OneDrive Pause)",
            Location = new Point(16, 56),
            Size = new Size(720, 24),
            Font = RetroTheme.DefaultFont
        };

        _rbMaxResponse = new RadioButton
        {
            Text = "MAX RESPONSE - Low Interference + Background CPU Isolation & Priority Demotion",
            Location = new Point(16, 86),
            Size = new Size(720, 24),
            Font = RetroTheme.DefaultFont
        };

        _grpProfilesSelector.Controls.AddRange(new Control[] { _rbNormal, _rbLowInterference, _rbMaxResponse });

        var pnlButtons = new Panel
        {
            Dock = DockStyle.Top,
            Height = 44,
            Padding = new Padding(0, 6, 0, 6)
        };

        _btnApplyProfile = new RetroButton
        {
            Text = "[ APPLY SELECTED PROFILE ]",
            Location = new Point(4, 6),
            Size = new Size(220, 32),
            Font = RetroTheme.BoldFont
        };
        _btnApplyProfile.Click += async (s, e) =>
        {
            var target = _rbLowInterference.Checked ? ProfileType.LowInterference :
                         _rbMaxResponse.Checked ? ProfileType.MaxResponse : ProfileType.Normal;
            await ApplyProfile(target);
        };

        _btnRestoreNormal = new RetroButton
        {
            Text = "[ RESTORE TO NORMAL (UNDO ALL) ]",
            Location = new Point(236, 6),
            Size = new Size(270, 32),
            Font = RetroTheme.BoldFont,
            ForeColor = Color.DarkRed
        };
        _btnRestoreNormal.Click += async (s, e) => await RestoreToNormal();

        pnlButtons.Controls.AddRange(new Control[] { _btnApplyProfile, _btnRestoreNormal });

        _grpProfilesLog = new GroupBox
        {
            Text = "Execution & Snapshot Journal",
            Dock = DockStyle.Fill,
            Font = RetroTheme.BoldFont
        };

        _txtLog = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = RetroTheme.MonospaceFont,
            BackColor = Color.White,
            ForeColor = Color.Black,
            BorderStyle = BorderStyle.Fixed3D
        };
        _grpProfilesLog.Controls.Add(_txtLog);

        _tabProfiles.Controls.Add(_grpProfilesLog);
        _tabProfiles.Controls.Add(pnlButtons);
        _tabProfiles.Controls.Add(_grpProfilesSelector);

        _tabControl.TabPages.Add(_tabProfiles);
    }
    #endregion

    #region Tab 3: Background Manager
    private void BuildBackgroundManagerTab()
    {
        _tabBackground = new TabPage("Background Manager")
        {
            BackColor = RetroTheme.BackgroundColor,
            Padding = new Padding(8)
        };

        _lblBackgroundInfo = new Label
        {
            Text = "Supervised background applications. Whitelisted components (Steam, OBS, audio drivers) are permanently protected.",
            Dock = DockStyle.Top,
            Height = 24,
            Font = RetroTheme.DefaultFont
        };

        _lvApps = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = RetroTheme.DefaultFont,
            BorderStyle = BorderStyle.Fixed3D
        };
        _lvApps.Columns.Add("Application", 160);
        _lvApps.Columns.Add("Process", 110);
        _lvApps.Columns.Add("State", 120);
        _lvApps.Columns.Add("Protection / Whitelist", 140);
        _lvApps.Columns.Add("Details", 220);
        ListViewColumnSorter.Attach(_lvApps);

        var pnlActions = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            Padding = new Padding(0, 6, 0, 0)
        };

        _btnPauseApp = new RetroButton
        {
            Text = "Pause App",
            Location = new Point(0, 6),
            Size = new Size(110, 28)
        };
        _btnPauseApp.Click += (s, e) =>
        {
            if (_lvApps.SelectedItems.Count > 0)
            {
                var proc = _lvApps.SelectedItems[0].SubItems[1].Text;
                if (_processes.SuspendProcess(proc))
                {
                    AppendLog($"[BACKGROUND] Suspended process: {proc}");
                    RefreshBackgroundApps();
                }
            }
        };

        _btnBackgroundPriority = new RetroButton
        {
            Text = "Lower Priority & Affinity",
            Location = new Point(116, 6),
            Size = new Size(170, 28)
        };
        _btnBackgroundPriority.Click += (s, e) =>
        {
            if (_lvApps.SelectedItems.Count > 0)
            {
                var proc = _lvApps.SelectedItems[0].SubItems[1].Text;
                if (_processes.SetBackgroundModeAndAffinity(proc, true, out _, out _))
                {
                    AppendLog($"[BACKGROUND] Set background priority and isolated CPU cores for: {proc}");
                    RefreshBackgroundApps();
                }
            }
        };

        _btnResumeApp = new RetroButton
        {
            Text = "Resume / Normal",
            Location = new Point(292, 6),
            Size = new Size(130, 28)
        };
        _btnResumeApp.Click += (s, e) =>
        {
            if (_lvApps.SelectedItems.Count > 0)
            {
                var proc = _lvApps.SelectedItems[0].SubItems[1].Text;
                _processes.ResumeProcess(proc);
                _processes.RestoreNormalModeAndAffinity(proc, (int)ProcessPriorityClass.Normal, -1);
                AppendLog($"[BACKGROUND] Resumed normal execution for: {proc}");
                RefreshBackgroundApps();
            }
        };

        _btnRefreshApps = new RetroButton
        {
            Text = "Refresh List",
            Location = new Point(428, 6),
            Size = new Size(110, 28)
        };
        _btnRefreshApps.Click += (s, e) => RefreshBackgroundApps();

        pnlActions.Controls.AddRange(new Control[] { _btnPauseApp, _btnBackgroundPriority, _btnResumeApp, _btnRefreshApps });

        _tabBackground.Controls.Add(_lvApps);
        _tabBackground.Controls.Add(pnlActions);
        _tabBackground.Controls.Add(_lblBackgroundInfo);

        _tabControl.TabPages.Add(_tabBackground);
    }
    #endregion

    #region Tab 4: Benchmark
    private void BuildBenchmarkTab()
    {
        _tabBenchmark = new TabPage("Verification & Benchmark")
        {
            BackColor = RetroTheme.BackgroundColor,
            Padding = new Padding(8)
        };

        _grpBenchmarkHeader = new GroupBox
        {
            Text = "Performance Data Helper (PDH) Measurement Engine",
            Dock = DockStyle.Top,
            Height = 85,
            Font = RetroTheme.BoldFont
        };

        _lblBenchmarkNotice = new Label
        {
            Text = "Principio: 'Medición antes que afirmaciones. No inventar mejoras.'\n" +
                   "Este módulo toma muestras antes y después de aplicar el perfil. Si la diferencia es menor al 2.0%,\n" +
                   "se reportará honestamente: 'No measurable improvement detected'.",
            Location = new Point(12, 22),
            Size = new Size(420, 52),
            Font = RetroTheme.DefaultFont
        };

        _btnRunBenchmark = new RetroButton
        {
            Text = "[ RUN BENCHMARK ]",
            Location = new Point(440, 26),
            Size = new Size(140, 44),
            Font = RetroTheme.BoldFont
        };
        _btnRunBenchmark.Click += async (s, e) => await RunBenchmarkAsync();

        _btnRunAdvancedBenchmark = new RetroButton
        {
            Text = "[ TRILATERAL TEST ]",
            Location = new Point(586, 26),
            Size = new Size(140, 44),
            Font = RetroTheme.BoldFont
        };
        _btnRunAdvancedBenchmark.Click += async (s, e) => await RunTrilateralBenchmarkAsync();

        _btnExportBenchmark = new RetroButton
        {
            Text = "Export JSON",
            Location = new Point(732, 26),
            Size = new Size(100, 44)
        };
        _btnExportBenchmark.Click += (s, e) => ExportBenchmark();

        _grpBenchmarkHeader.Controls.AddRange(new Control[] { _lblBenchmarkNotice, _btnRunBenchmark, _btnRunAdvancedBenchmark, _btnExportBenchmark });

        _lblBenchmarkStatus = new Label
        {
            Text = "Status: Benchmark idle.",
            Dock = DockStyle.Top,
            Height = 22,
            Font = RetroTheme.BoldFont,
            ForeColor = Color.Navy
        };

        _lvBenchmarkResults = new ListView
        {
            Dock = DockStyle.Top,
            Height = 160,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = RetroTheme.DefaultFont,
            BorderStyle = BorderStyle.Fixed3D
        };
        _lvBenchmarkResults.Columns.Add("Metric Indicator", 220);
        _lvBenchmarkResults.Columns.Add("BEFORE (Baseline)", 140);
        _lvBenchmarkResults.Columns.Add("AFTER (Profile)", 140);
        _lvBenchmarkResults.Columns.Add("Delta Variation", 120);
        _lvBenchmarkResults.Columns.Add("Evaluation", 140);
        ListViewColumnSorter.Attach(_lvBenchmarkResults);

        _grpBenchmarkVerdict = new GroupBox
        {
            Text = "Telemetry Verdict",
            Dock = DockStyle.Fill,
            Font = RetroTheme.BoldFont
        };

        _txtBenchmarkVerdict = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            Font = RetroTheme.DefaultFont,
            BackColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D,
            Text = "Run the benchmark to analyze real-world system noise and interference delta."
        };
        _grpBenchmarkVerdict.Controls.Add(_txtBenchmarkVerdict);

        _tabBenchmark.Controls.Add(_grpBenchmarkVerdict);
        _tabBenchmark.Controls.Add(_lvBenchmarkResults);
        _tabBenchmark.Controls.Add(_lblBenchmarkStatus);
        _tabBenchmark.Controls.Add(_grpBenchmarkHeader);

        _tabControl.TabPages.Add(_tabBenchmark);
    }
    #endregion

    #region Tab 5: Phase 3 Diagnostics & Smart Profiles
    private void BuildDiagnosticsAndSmartProfilesTab()
    {
        _tabDiagnostics = new TabPage("Diagnostics & Smart Profiles")
        {
            BackColor = RetroTheme.BackgroundColor,
            Padding = new Padding(8)
        };

        // Section A: Why is my desktop busy?
        _grpDiagnosticWhy = new GroupBox
        {
            Text = "Diagnostic: Why Is My Desktop Busy?",
            Dock = DockStyle.Top,
            Height = 180,
            Font = RetroTheme.BoldFont
        };

        var pnlDiagTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 36
        };

        _btnRunDiagnostic = new RetroButton
        {
            Text = "[ Run Interference Diagnostic ]",
            Location = new Point(4, 4),
            Size = new Size(220, 28),
            Font = RetroTheme.BoldFont
        };
        _btnRunDiagnostic.Click += (s, e) => RunDesktopDiagnostic();
        pnlDiagTop.Controls.Add(_btnRunDiagnostic);

        _lvDiagnosticFindings = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = RetroTheme.DefaultFont,
            BorderStyle = BorderStyle.Fixed3D
        };
        _lvDiagnosticFindings.Columns.Add("Category", 130);
        _lvDiagnosticFindings.Columns.Add("Component / App", 150);
        _lvDiagnosticFindings.Columns.Add("Human Explanation", 360);
        _lvDiagnosticFindings.Columns.Add("Recommended Action", 170);
        ListViewColumnSorter.Attach(_lvDiagnosticFindings);

        _grpDiagnosticWhy.Controls.Add(_lvDiagnosticFindings);
        _grpDiagnosticWhy.Controls.Add(pnlDiagTop);

        // Section B: Smart Profiles & Auto Game Detection
        _grpSmartProfiles = new GroupBox
        {
            Text = "Smart Profiles & Auto Game Detection",
            Dock = DockStyle.Fill,
            Font = RetroTheme.BoldFont
        };

        var pnlSmartTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60
        };

        _lblGameMode = new Label
        {
            Text = "Game Detection Mode:",
            Location = new Point(4, 6),
            Size = new Size(140, 20),
            Font = RetroTheme.BoldFont
        };

        _rbAutoGame = new RadioButton
        {
            Text = "AUTO (Auto-apply on launch, auto-restore on exit)",
            Location = new Point(150, 4),
            Size = new Size(330, 20),
            Checked = true,
            Font = RetroTheme.DefaultFont
        };
        _rbAutoGame.CheckedChanged += (s, e) =>
        {
            if (_rbAutoGame.Checked) _processWatcher.Mode = AutoGameDetectionMode.Auto;
        };

        _rbManualGame = new RadioButton
        {
            Text = "MANUAL (Prompt only)",
            Location = new Point(490, 4),
            Size = new Size(160, 20),
            Font = RetroTheme.DefaultFont
        };
        _rbManualGame.CheckedChanged += (s, e) =>
        {
            if (_rbManualGame.Checked) _processWatcher.Mode = AutoGameDetectionMode.Manual;
        };

        _rbDisabledGame = new RadioButton
        {
            Text = "DISABLED",
            Location = new Point(660, 4),
            Size = new Size(90, 20),
            Font = RetroTheme.DefaultFont
        };
        _rbDisabledGame.CheckedChanged += (s, e) =>
        {
            if (_rbDisabledGame.Checked) _processWatcher.Mode = AutoGameDetectionMode.Disabled;
        };

        _lblWatcherStatus = new Label
        {
            Text = "Active Game Hook: Idle (Watching for registered games like CS2, OBS, Premiere...)",
            Location = new Point(4, 30),
            Size = new Size(600, 22),
            Font = RetroTheme.DefaultFont,
            ForeColor = Color.DarkBlue
        };

        pnlSmartTop.Controls.AddRange(new Control[] { _lblGameMode, _rbAutoGame, _rbManualGame, _rbDisabledGame, _lblWatcherStatus });

        _lvSmartProfiles = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = RetroTheme.DefaultFont,
            BorderStyle = BorderStyle.Fixed3D
        };
        _lvSmartProfiles.Columns.Add("Profile Name", 190);
        _lvSmartProfiles.Columns.Add("Trigger Process", 130);
        _lvSmartProfiles.Columns.Add("Description", 350);
        _lvSmartProfiles.Columns.Add("CPU Isolation", 100);
        ListViewColumnSorter.Attach(_lvSmartProfiles);

        var pnlSmartBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 80
        };

        _btnApplySmartProfile = new RetroButton
        {
            Text = "Apply Selected Smart Profile",
            Location = new Point(4, 4),
            Size = new Size(200, 28)
        };
        _btnApplySmartProfile.Click += async (s, e) =>
        {
            if (_lvSmartProfiles.SelectedItems.Count > 0)
            {
                var profileName = _lvSmartProfiles.SelectedItems[0].Text;
                var prof = _smartProfileManager.Profiles.FirstOrDefault(p => p.Name == profileName);
                if (prof != null)
                {
                    await _profileEngine.ApplyProfileAsync(ProfileType.MaxResponse);
                    AppendLog($"[SMART-PROFILE] Manually applied profile: {prof.Name}");
                    RefreshAllData();
                }
            }
        };

        _txtLearningSuggestions = new TextBox
        {
            Location = new Point(210, 4),
            Size = new Size(620, 72),
            Multiline = true,
            ReadOnly = true,
            Font = RetroTheme.DefaultFont,
            BackColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D
        };
        UpdateLearningSuggestions();

        pnlSmartBottom.Controls.AddRange(new Control[] { _btnApplySmartProfile, _txtLearningSuggestions });

        _grpSmartProfiles.Controls.Add(_lvSmartProfiles);
        _grpSmartProfiles.Controls.Add(pnlSmartBottom);
        _grpSmartProfiles.Controls.Add(pnlSmartTop);

        _tabDiagnostics.Controls.Add(_grpSmartProfiles);
        _tabDiagnostics.Controls.Add(_grpDiagnosticWhy);

        _tabControl.TabPages.Add(_tabDiagnostics);

        LoadSmartProfilesList();
    }

    private void UpdateLearningSuggestions()
    {
        var suggestions = _profileLearner.GenerateIntelligentSuggestions();
        if (suggestions.Count > 0)
        {
            var s = suggestions[0];
            _txtLearningSuggestions.Text = $"[Smart Pattern Suggestion: {s.SuggestionTitle}]\n{s.Explanation}\n-> {s.RecommendedAction}";
        }
    }
    #endregion

    #region Tab 6: Multi-Monitor & VRR Studio
    private void BuildMultiMonitorTab()
    {
        _tabMultiMonitor = new TabPage("Multi-Monitor Studio & VRR")
        {
            BackColor = RetroTheme.BackgroundColor,
            Padding = new Padding(8)
        };

        var grpVRR = new GroupBox
        {
            Text = "VRR (G-Sync/FreeSync) & Compositor Desync Analysis",
            Dock = DockStyle.Top,
            Height = 110,
            Font = RetroTheme.BoldFont
        };

        _lblMultiMonitorSummary = new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            Font = RetroTheme.DefaultFont,
            Text = "Analyzing multi-monitor configuration..."
        };
        grpVRR.Controls.Add(_lblMultiMonitorSummary);

        _grpMultiMonitorOffenders = new GroupBox
        {
            Text = "Secondary Monitor Hardware-Accelerated Windows",
            Dock = DockStyle.Fill,
            Font = RetroTheme.BoldFont
        };

        _lvSecondaryOffenders = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = RetroTheme.DefaultFont,
            BorderStyle = BorderStyle.Fixed3D
        };
        _lvSecondaryOffenders.Columns.Add("Process & Window Title", 450);
        _lvSecondaryOffenders.Columns.Add("Impact on Primary Monitor", 300);
        ListViewColumnSorter.Attach(_lvSecondaryOffenders);

        var pnlActions = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 40
        };

        _btnAnalyzeMonitors = new RetroButton
        {
            Text = "Analyze Multi-Display Desync",
            Location = new Point(4, 6),
            Size = new Size(200, 28)
        };
        _btnAnalyzeMonitors.Click += (s, e) => RefreshMultiMonitorTab();

        _btnBlankSecondary = new RetroButton
        {
            Text = "Apply Static Background to Secondary Monitors",
            Location = new Point(210, 6),
            Size = new Size(280, 28)
        };
        _btnBlankSecondary.Click += (s, e) =>
        {
            _multiMonitorOptimizer.ApplySecondaryDisplayStaticBackground(true);
            AppendLog("[MULTI-MONITOR] Forced static desktop pattern to eliminate DirectComposition secondary repaints.");
        };

        pnlActions.Controls.AddRange(new Control[] { _btnAnalyzeMonitors, _btnBlankSecondary });

        _grpMultiMonitorOffenders.Controls.Add(_lvSecondaryOffenders);
        _grpMultiMonitorOffenders.Controls.Add(pnlActions);

        _tabMultiMonitor.Controls.Add(_grpMultiMonitorOffenders);
        _tabMultiMonitor.Controls.Add(grpVRR);

        _tabControl.TabPages.Add(_tabMultiMonitor);
    }

    private void RefreshMultiMonitorTab()
    {
        var analysis = _multiMonitorOptimizer.AnalyzeDisplays();
        string riskColor = analysis.IsVrrAtRisk ? "WARNING: VRR / DWM Desync Risk Detected" : "STATUS: Safe / Optimal";
        _lblMultiMonitorSummary.Text = $"Monitors: {analysis.TotalMonitors} | Range: {analysis.MinRefreshRateHz} Hz - {analysis.MaxRefreshRateHz} Hz\n" +
                                       $"[{riskColor}]\n{analysis.VrrRiskReason}";

        _lvSecondaryOffenders.BeginUpdate();
        _lvSecondaryOffenders.Items.Clear();
        foreach (var app in analysis.SecondaryMonitorRunningApps)
        {
            var item = new ListViewItem(app);
            item.SubItems.Add("Draws 60 fps to GPU; risk of v-blank stalls on primary display");
            item.ForeColor = Color.DarkRed;
            _lvSecondaryOffenders.Items.Add(item);
        }

        if (analysis.SecondaryMonitorRunningApps.Count == 0)
        {
            var item = new ListViewItem("No problematic hardware-accelerated windows found on secondary displays");
            item.SubItems.Add("Low interference confirmed");
            item.ForeColor = Color.DarkGreen;
            _lvSecondaryOffenders.Items.Add(item);
        }

        if (_lvSecondaryOffenders.ListViewItemSorter is ListViewColumnSorter secSorter && secSorter.Order != SortOrder.None)
        {
            _lvSecondaryOffenders.Sort();
        }
        _lvSecondaryOffenders.EndUpdate();
    }
    #endregion

    #region Tab 7: Retro Shell & Classic Theme (Phase 5)
    private void BuildRetroShellTab()
    {
        _tabRetroShell = new TabPage("Classic Theme & Retro Shell (Phase 5)")
        {
            BackColor = RetroTheme.BackgroundColor,
            Padding = new Padding(8)
        };

        // Section 1: System Theme Status & Notice
        _grpThemeStatus = new GroupBox
        {
            Text = "System Theme Status (Phase 5)",
            Dock = DockStyle.Top,
            Height = 68,
            Font = RetroTheme.BoldFont
        };

        _lblThemeStatus = new Label
        {
            Text = "System Theme: ACTIVE Windows 11 Modern (DWM Shaders On)",
            Location = new Point(8, 20),
            Size = new Size(800, 18),
            Font = RetroTheme.BoldFont,
            ForeColor = Color.Navy
        };

        _lblThemeNotice = new Label
        {
            Text = "Applies native .theme file, SetSysColors palette, solid teal desktop, and shuts down DWM blur shaders for maximum system responsiveness.",
            Location = new Point(8, 40),
            Size = new Size(800, 20),
            Font = RetroTheme.DefaultFont,
            ForeColor = Color.FromArgb(64, 64, 64)
        };

        _grpThemeStatus.Controls.Add(_lblThemeStatus);
        _grpThemeStatus.Controls.Add(_lblThemeNotice);

        // Section 2: Classic Themes Presets
        _grpRetroThemes = new GroupBox
        {
            Text = "System-Wide Classic Windows 95/98 Themes (Low Load)",
            Dock = DockStyle.Top,
            Height = 210,
            Font = RetroTheme.BoldFont
        };

        _lvThemes = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = RetroTheme.DefaultFont,
            BorderStyle = BorderStyle.Fixed3D
        };
        _lvThemes.Columns.Add("Theme Name", 220);
        _lvThemes.Columns.Add("Description", 450);
        _lvThemes.Columns.Add("Variant", 130);
        ListViewColumnSorter.Attach(_lvThemes);

        foreach (var kvp in ClassicThemeManager.Definitions)
        {
            var def = kvp.Value;
            var item = new ListViewItem(def.DisplayName);
            item.SubItems.Add(def.Description);
            item.SubItems.Add(def.Variant.ToString());
            _lvThemes.Items.Add(item);
        }

        var pnlThemeActions = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 36
        };

        _btnApplyTheme = new RetroButton
        {
            Text = "[ Apply Classic Theme to System ]",
            Location = new Point(4, 4),
            Size = new Size(250, 28),
            Font = RetroTheme.BoldFont
        };
        _btnApplyTheme.Click += (s, e) =>
        {
            if (_lvThemes.SelectedItems.Count > 0)
            {
                var variantStr = _lvThemes.SelectedItems[0].SubItems[2].Text;
                if (Enum.TryParse<ClassicThemeVariant>(variantStr, out var variant))
                {
                    bool ok = _classicThemeManager.ApplyClassicTheme(variant, launchThemeFile: true);
                    if (ok)
                    {
                        AppendLog($"[CLASSIC-THEME] Applied system-wide classic theme: {variant}. DWM transparency and animations disabled.");
                        UpdateThemeStatusLabel();
                    }
                }
            }
        };

        _btnRestoreTheme = new RetroButton
        {
            Text = "[ Restore Windows 11 Default Theme ]",
            Location = new Point(260, 4),
            Size = new Size(260, 28),
            Font = RetroTheme.BoldFont
        };
        _btnRestoreTheme.Click += (s, e) =>
        {
            bool ok = _classicThemeManager.RestoreModernTheme();
            if (ok)
            {
                AppendLog("[CLASSIC-THEME] Restored default modern Windows 11 theme and visual effects.");
                UpdateThemeStatusLabel();
            }
        };

        pnlThemeActions.Controls.Add(_btnApplyTheme);
        pnlThemeActions.Controls.Add(_btnRestoreTheme);

        _grpRetroThemes.Controls.Add(_lvThemes);
        _grpRetroThemes.Controls.Add(pnlThemeActions);

        // Section 3: Companion Tools
        _grpCompanion = new GroupBox
        {
            Text = "Retro Shell Companion Ecosystem (Optional Taskbar & Start Menu)",
            Dock = DockStyle.Top,
            Height = 125,
            Font = RetroTheme.BoldFont
        };

        _lvCompanionTools = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = RetroTheme.DefaultFont,
            BorderStyle = BorderStyle.Fixed3D
        };
        _lvCompanionTools.Columns.Add("Application", 130);
        _lvCompanionTools.Columns.Add("Status", 130);
        _lvCompanionTools.Columns.Add("Description", 520);
        ListViewColumnSorter.Attach(_lvCompanionTools);

        var pnlCompanionActions = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 36
        };

        _btnLaunchCompanion = new RetroButton
        {
            Text = "Launch Selected Tool",
            Location = new Point(4, 4),
            Size = new Size(180, 28)
        };
        _btnLaunchCompanion.Click += (s, e) =>
        {
            if (_lvCompanionTools.SelectedItems.Count > 0 && _lvCompanionTools.SelectedItems[0].Tag is CompanionToolInfo tool)
            {
                if (tool.IsInstalled)
                {
                    _classicThemeManager.LaunchCompanionTool(tool.ExecutablePath);
                    AppendLog($"[COMPANION] Launched {tool.Name}");
                    RefreshCompanionTools();
                }
                else
                {
                    MessageBox.Show($"{tool.Name} is not detected on this system. Click 'Get Tool (GitHub)' to download it.", tool.Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        };

        _btnGetCompanion = new RetroButton
        {
            Text = "Get Tool (GitHub)",
            Location = new Point(190, 4),
            Size = new Size(160, 28)
        };
        _btnGetCompanion.Click += (s, e) =>
        {
            if (_lvCompanionTools.SelectedItems.Count > 0 && _lvCompanionTools.SelectedItems[0].Tag is CompanionToolInfo tool)
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = tool.DownloadUrl, UseShellExecute = true });
                }
                catch { }
            }
        };

        pnlCompanionActions.Controls.Add(_btnLaunchCompanion);
        pnlCompanionActions.Controls.Add(_btnGetCompanion);

        _grpCompanion.Controls.Add(_lvCompanionTools);
        _grpCompanion.Controls.Add(pnlCompanionActions);

        // Section 4: Retro Guide
        _grpRetroGuide = new GroupBox
        {
            Text = "Safe Retro Shell Ecosystem Guide",
            Dock = DockStyle.Fill,
            Font = RetroTheme.BoldFont
        };

        _txtShellGuide = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            Font = RetroTheme.DefaultFont,
            BackColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D
        };
        _grpRetroGuide.Controls.Add(_txtShellGuide);

        // Add to Tab in docking order
        _tabRetroShell.Controls.Add(_grpRetroGuide);
        _tabRetroShell.Controls.Add(_grpCompanion);
        _tabRetroShell.Controls.Add(_grpRetroThemes);
        _tabRetroShell.Controls.Add(_grpThemeStatus);

        _tabControl.TabPages.Add(_tabRetroShell);

        RefreshCompanionTools();
        UpdateThemeStatusLabel();
    }

    private void RefreshCompanionTools()
    {
        _lvCompanionTools.BeginUpdate();
        _lvCompanionTools.Items.Clear();

        var rb = _classicThemeManager.GetRetroBarInfo();
        var itemRb = new ListViewItem(rb.Name);
        itemRb.SubItems.Add(rb.IsRunning ? "RUNNING" : (rb.IsInstalled ? "INSTALLED" : "NOT DETECTED"));
        itemRb.SubItems.Add(rb.Description);
        itemRb.Tag = rb;
        itemRb.ForeColor = rb.IsRunning ? Color.DarkGreen : (rb.IsInstalled ? Color.DarkBlue : Color.Gray);
        _lvCompanionTools.Items.Add(itemRb);

        var os = _classicThemeManager.GetOpenShellInfo();
        var itemOs = new ListViewItem(os.Name);
        itemOs.SubItems.Add(os.IsRunning ? "RUNNING" : (os.IsInstalled ? "INSTALLED" : "NOT DETECTED"));
        itemOs.SubItems.Add(os.Description);
        itemOs.Tag = os;
        itemOs.ForeColor = os.IsRunning ? Color.DarkGreen : (os.IsInstalled ? Color.DarkBlue : Color.Gray);
        _lvCompanionTools.Items.Add(itemOs);

        _lvCompanionTools.EndUpdate();
    }

    private void UpdateThemeStatusLabel()
    {
        var loc = LocalizationManager.Instance;
        if (_classicThemeManager.IsClassicThemeActive)
        {
            _lblThemeStatus.Text = $"{loc.T("RetroShell_StatusTitle")} {loc.T("RetroShell_StatusClassic")} ({_classicThemeManager.ActiveVariant})";
            _lblThemeStatus.ForeColor = Color.DarkGreen;
        }
        else
        {
            _lblThemeStatus.Text = $"{loc.T("RetroShell_StatusTitle")} {loc.T("RetroShell_StatusModern")}";
            _lblThemeStatus.ForeColor = Color.Navy;
        }
    }
    #endregion

    #region Business Logic & Orchestration
    private void LoadSmartProfilesList()
    {
        _lvSmartProfiles.BeginUpdate();
        _lvSmartProfiles.Items.Clear();
        foreach (var p in _smartProfileManager.Profiles)
        {
            var item = new ListViewItem(p.Name);
            item.SubItems.Add(string.IsNullOrWhiteSpace(p.TriggerProcessName) ? "None" : p.TriggerProcessName);
            item.SubItems.Add(p.Description);
            item.SubItems.Add(p.IsolateCpuCores ? "YES" : "No");
            _lvSmartProfiles.Items.Add(item);
        }

        if (_lvSmartProfiles.ListViewItemSorter is ListViewColumnSorter smartSorter && smartSorter.Order != SortOrder.None)
        {
            _lvSmartProfiles.Sort();
        }
        _lvSmartProfiles.EndUpdate();
    }

    private void RunDesktopDiagnostic()
    {
        _lvDiagnosticFindings.BeginUpdate();
        _lvDiagnosticFindings.Items.Clear();
        var findings = _diagnosticAnalyzer.AnalyzeCurrentInterference();

        foreach (var f in findings)
        {
            var item = new ListViewItem(f.Category);
            item.SubItems.Add(f.ProcessOrComponent);
            item.SubItems.Add(f.HumanExplanation);
            item.SubItems.Add(f.RecommendedAction);

            if (f.IsSevere)
            {
                item.ForeColor = Color.DarkRed;
            }
            else if (f.Category == "Optimal State")
            {
                item.ForeColor = Color.DarkGreen;
            }

            _lvDiagnosticFindings.Items.Add(item);
        }

        if (_lvDiagnosticFindings.ListViewItemSorter is ListViewColumnSorter diagSorter && diagSorter.Order != SortOrder.None)
        {
            _lvDiagnosticFindings.Sort();
        }
        _lvDiagnosticFindings.EndUpdate();

        AppendLog($"[DIAGNOSTIC] Analyzed desktop interference: {findings.Count} findings.");
    }

    private void ShowProcessDatabaseDialog()
    {
        var loc = LocalizationManager.Instance;
        using var dlg = new Form
        {
            Text = loc.CurrentLanguage == AppLanguage.Spanish ? "Base de Datos de Seguridad de Procesos" : "Process & Service Safety Database",
            Width = 750,
            Height = 480,
            StartPosition = FormStartPosition.CenterParent,
            BackColor = RetroTheme.BackgroundColor,
            Font = RetroTheme.DefaultFont
        };

        var lv = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Font = RetroTheme.DefaultFont,
            BorderStyle = BorderStyle.Fixed3D
        };
        lv.Columns.Add(loc.T("Col_Process"), 110);
        lv.Columns.Add(loc.T("Col_Classification"), 110);
        lv.Columns.Add(loc.T("Col_Category"), 110);
        lv.Columns.Add(loc.T("Col_Vendor"), 90);
        lv.Columns.Add(loc.T("Col_ImpactSafety"), 280);
        ListViewColumnSorter.Attach(lv);

        foreach (var entry in _backgroundDatabase.GetAllEntries())
        {
            var item = new ListViewItem(entry.ProcessName);
            item.SubItems.Add(entry.Classification.ToString());
            item.SubItems.Add(entry.Category);
            item.SubItems.Add(entry.Vendor);
            item.SubItems.Add($"{entry.ImpactDescription} [If Paused: {entry.WhatHappensIfPaused}]");

            switch (entry.Classification)
            {
                case SafetyClassification.DoNotTouch:
                    item.ForeColor = Color.DarkRed;
                    break;
                case SafetyClassification.Safe:
                    item.ForeColor = Color.DarkGreen;
                    break;
                case SafetyClassification.LowRisk:
                    item.ForeColor = Color.Navy;
                    break;
            }

            lv.Items.Add(item);
        }

        dlg.Controls.Add(lv);
        dlg.ShowDialog(this);
    }

    private void ExportBenchmark()
    {
        if (_lastBenchmarkComparison == null)
        {
            MessageBox.Show("No benchmark session has been run yet. Run a benchmark first.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "JSON Files (*.json)|*.json",
            FileName = $"benchmark_report_{DateTime.Now:yyyyMMdd_HHmmss}.json"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            if (_exportManager.ExportBenchmarkSession(_lastBenchmarkComparison, sfd.FileName))
            {
                MessageBox.Show($"Benchmark exported successfully to:\n{sfd.FileName}", "Exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private void ExportProfiles()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "JSON Files (*.json)|*.json",
            FileName = "smart_profiles.json"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            if (_exportManager.ExportProfiles(_smartProfileManager.Profiles, sfd.FileName))
            {
                MessageBox.Show($"Profiles exported successfully to:\n{sfd.FileName}", "Exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private void RefreshAllData()
    {
        try
        {
            // Hardware
            var hw = _hardwareInspector.CaptureCurrentHardwareStatus();
            _lblSummary.Text = $"CPU: {hw.CpuName} ({hw.LogicalCores} Threads) | " +
                               $"RAM: {hw.TotalPhysicalMemoryMB} MB (Free: {hw.AvailablePhysicalMemoryMB} MB / {hw.MemoryUsagePercentage}% Used) | " +
                               $"Displays: {hw.Displays.Count} active";

            _lblHardwareDetails.Text = $"Processor: {hw.CpuName} [{hw.LogicalCores} Logical Threads]\n" +
                                       $"Physical RAM: {hw.TotalPhysicalMemoryMB} MB Total, {hw.AvailablePhysicalMemoryMB} MB Free\n" +
                                       $"GPU Adapter: {hw.GpuAdapterName}\n" +
                                       $"Active System Processes: {hw.TotalProcessesCount} | Threads: {hw.TotalThreadsCount}";

            // Displays
            _lvDisplays.BeginUpdate();
            _lvDisplays.Items.Clear();
            foreach (var d in hw.Displays)
            {
                var item = new ListViewItem(d.MonitorName);
                item.SubItems.Add($"{d.Width} x {d.Height}");
                item.SubItems.Add($"{d.RefreshRateHz} Hz");
                item.SubItems.Add($"{d.BitsPerPixel}-bit");
                item.SubItems.Add(d.IsPrimary ? "YES" : "No");
                _lvDisplays.Items.Add(item);
            }
            if (_lvDisplays.ListViewItemSorter is ListViewColumnSorter dispSorter && dispSorter.Order != SortOrder.None)
            {
                _lvDisplays.Sort();
            }
            _lvDisplays.EndUpdate();

            // Visuals
            var vis = _visualEffects.CaptureCurrentVisualState();
            _lblVisualDetails.Text = $"Desktop Transparency (Mica/Acrylic): {(vis.TransparencyEnabled ? "ENABLED" : "Disabled (Low Interference)")}\n" +
                                     $"Shell Window Animations: {(vis.WindowAnimationsEnabled ? "ENABLED" : "Disabled (Instant UI)")}\n" +
                                     $"Taskbar Widgets Panel: {(vis.TaskbarWidgetsEnabled ? "ENABLED" : "Disabled / Hidden")}\n" +
                                     $"Wallpaper: {(string.IsNullOrEmpty(vis.WallpaperPath) ? "Solid Color / Default" : Path.GetFileName(vis.WallpaperPath))}";

            // Background apps
            RefreshBackgroundApps();

            // Multi-monitor tab
            RefreshMultiMonitorTab();

            // Status bar
            UpdateStatusLabels();
        }
        catch (Exception ex)
        {
            AppendLog($"[ERROR] Data refresh error: {ex.Message}");
        }
    }

    private void RefreshBackgroundApps()
    {
        _lvApps.BeginUpdate();
        _lvApps.Items.Clear();
        var apps = _processes.ScanManagedApps();

        foreach (var a in apps)
        {
            var item = new ListViewItem(a.DisplayName);
            item.SubItems.Add(a.ProcessName);
            item.SubItems.Add(a.CurrentState.ToString());
            item.SubItems.Add(a.IsWhitelisted ? "NEVER TOUCH (WHITELIST)" : "Configurable");
            item.SubItems.Add(a.Details);

            if (a.IsWhitelisted)
            {
                item.ForeColor = Color.DarkGreen;
            }
            else if (a.CurrentState == ProcessExecutionState.Suspended)
            {
                item.ForeColor = Color.Gray;
            }

            _lvApps.Items.Add(item);
        }

        if (_lvApps.ListViewItemSorter is ListViewColumnSorter sorter && sorter.Order != SortOrder.None)
        {
            _lvApps.Sort();
        }
        _lvApps.EndUpdate();
    }

    private void UpdateStatusLabels()
    {
        var loc = LocalizationManager.Instance;
        _statusProfile.Text = $"{loc.T("Status_ProfilePrefix")}{_profileEngine.CurrentProfile.ToString().ToUpper()}";
        bool hasSnap = _snapshotManager.HasActiveSnapshot();
        _statusSnapshot.Text = hasSnap ? loc.T("Status_SnapshotActive") : loc.T("Status_SnapshotNone");

        if (_profileEngine.CurrentProfile == ProfileType.LowInterference)
            _rbLowInterference.Checked = true;
        else if (_profileEngine.CurrentProfile == ProfileType.MaxResponse)
            _rbMaxResponse.Checked = true;
        else
            _rbNormal.Checked = true;
    }

    private void ApplyLocalization()
    {
        var loc = LocalizationManager.Instance;
        Text = loc.T("App_Title");

        // Menus
        _menuFile.Text = loc.T("Menu_File");
        _menuRefreshAll.Text = loc.T("Menu_RefreshAll");
        _menuExportBenchmark.Text = loc.T("Menu_ExportBenchmark");
        _menuExportProfiles.Text = loc.T("Menu_ExportProfiles");
        _menuExit.Text = loc.T("Menu_Exit");

        _menuProfiles.Text = loc.T("Menu_Profiles");
        _menuProfileNormal.Text = loc.T("Menu_Normal");
        _menuProfileLow.Text = loc.T("Menu_LowInterference");
        _menuProfileMax.Text = loc.T("Menu_MaxResponse");
        _menuRestoreNormal.Text = loc.T("Menu_RestoreNormal");

        _menuDiagnostics.Text = loc.T("Menu_Diagnostics");
        _menuWhyBusy.Text = loc.T("Menu_WhyBusy");
        _menuProcessDb.Text = loc.T("Menu_ProcessDb");

        _menuLanguage.Text = loc.T("Menu_Language");
        _menuLangEnglish.Text = loc.T("Menu_LangEnglish");
        _menuLangSpanish.Text = loc.T("Menu_LangSpanish");
        _menuLangEnglish.Checked = loc.CurrentLanguage == AppLanguage.English;
        _menuLangSpanish.Checked = loc.CurrentLanguage == AppLanguage.Spanish;

        _menuHelp.Text = loc.T("Menu_Help");
        _menuAbout.Text = loc.T("Menu_About");

        // Tabs
        _tabTopology.Text = loc.T("Tab_Topology");
        _tabProfiles.Text = loc.T("Tab_Profiles");
        _tabBackground.Text = loc.T("Tab_Background");
        _tabBenchmark.Text = loc.T("Tab_Benchmark");
        _tabDiagnostics.Text = loc.T("Tab_Diagnostics");
        _tabMultiMonitor.Text = loc.T("Tab_MultiMonitor");
        _tabRetroShell.Text = loc.T("Tab_RetroShell");

        // GroupBoxes & labels
        _grpTopologyDisplays.Text = loc.T("Topology_DisplaysGrp");
        _grpTopologyHardware.Text = loc.T("Topology_HardwareGrp");
        _grpTopologyVisual.Text = loc.T("Topology_VisualGrp");
        _btnRefreshTopology.Text = loc.T("Topology_RefreshBtn");

        _grpProfilesSelector.Text = loc.T("Profiles_SelectorGrp");
        _rbNormal.Text = loc.T("Profiles_Normal_Desc");
        _rbLowInterference.Text = loc.T("Profiles_Low_Desc");
        _rbMaxResponse.Text = loc.T("Profiles_Max_Desc");
        _btnApplyProfile.Text = loc.T("Profiles_ApplyBtn");
        _btnRestoreNormal.Text = loc.T("Profiles_RestoreBtn");
        _grpProfilesLog.Text = loc.T("Profiles_LogGrp");

        _lblBackgroundInfo.Text = loc.T("Background_Info");
        _btnPauseApp.Text = loc.T("Background_PauseBtn");
        _btnBackgroundPriority.Text = loc.T("Background_PriorityBtn");
        _btnResumeApp.Text = loc.T("Background_ResumeBtn");
        _btnRefreshApps.Text = loc.T("Background_RefreshBtn");

        _grpBenchmarkHeader.Text = loc.T("Benchmark_HeaderGrp");
        _lblBenchmarkNotice.Text = loc.T("Benchmark_Notice");
        _btnRunBenchmark.Text = loc.T("Benchmark_RunBtn");
        _btnRunAdvancedBenchmark.Text = loc.T("Benchmark_TrilateralBtn");
        _btnExportBenchmark.Text = loc.T("Benchmark_ExportBtn");
        _grpBenchmarkVerdict.Text = loc.T("Benchmark_VerdictGrp");

        _grpDiagnosticWhy.Text = loc.T("Diag_WhyGrp");
        _btnRunDiagnostic.Text = loc.T("Diag_RunBtn");
        _grpSmartProfiles.Text = loc.T("Diag_SmartGrp");
        _lblGameMode.Text = loc.T("Diag_ModeLbl");
        _rbAutoGame.Text = loc.T("Diag_ModeAuto");
        _rbManualGame.Text = loc.T("Diag_ModeManual");
        _rbDisabledGame.Text = loc.T("Diag_ModeDisabled");
        _btnApplySmartProfile.Text = loc.T("Diag_ApplySmartBtn");

        _grpMultiMonitorOffenders.Text = loc.T("MultiMonitor_OffendersGrp");
        _btnAnalyzeMonitors.Text = loc.T("MultiMonitor_AnalyzeBtn");
        _btnBlankSecondary.Text = loc.T("MultiMonitor_BlankBtn");

        _grpThemeStatus.Text = loc.T("RetroShell_StatusTitle");
        UpdateThemeStatusLabel();
        _lblThemeNotice.Text = loc.T("RetroShell_Notice");
        _grpRetroThemes.Text = loc.T("RetroShell_ThemesGrp");
        _btnApplyTheme.Text = loc.T("RetroShell_ApplyBtn");
        _btnRestoreTheme.Text = loc.T("RetroShell_RestoreBtn");
        _grpCompanion.Text = loc.T("RetroShell_CompanionGrp");
        _btnLaunchCompanion.Text = loc.T("RetroShell_LaunchRetroBar");
        _btnGetCompanion.Text = loc.T("RetroShell_GetRetroBar");
        _grpRetroGuide.Text = loc.T("RetroShell_GuideGrp");
        _txtShellGuide.Text = loc.T("RetroShell_GuideText");

        // Columns
        SetColumnTitle(_lvDisplays, 0, "Col_Monitor");
        SetColumnTitle(_lvDisplays, 1, "Col_Resolution");
        SetColumnTitle(_lvDisplays, 2, "Col_RefreshRate");
        SetColumnTitle(_lvDisplays, 3, "Col_Depth");
        SetColumnTitle(_lvDisplays, 4, "Col_Primary");

        SetColumnTitle(_lvApps, 0, "Col_Application");
        SetColumnTitle(_lvApps, 1, "Col_Process");
        SetColumnTitle(_lvApps, 2, "Col_State");
        SetColumnTitle(_lvApps, 3, "Col_Protection");
        SetColumnTitle(_lvApps, 4, "Col_Details");

        SetColumnTitle(_lvBenchmarkResults, 0, "Col_Metric");
        SetColumnTitle(_lvBenchmarkResults, 1, "Col_Before");
        SetColumnTitle(_lvBenchmarkResults, 2, "Col_After");
        SetColumnTitle(_lvBenchmarkResults, 3, "Col_Delta");
        SetColumnTitle(_lvBenchmarkResults, 4, "Col_Evaluation");

        SetColumnTitle(_lvDiagnosticFindings, 0, "Col_Category");
        SetColumnTitle(_lvDiagnosticFindings, 1, "Col_Component");
        SetColumnTitle(_lvDiagnosticFindings, 2, "Col_Explanation");
        SetColumnTitle(_lvDiagnosticFindings, 3, "Col_Action");

        SetColumnTitle(_lvSmartProfiles, 0, "Col_ProfileName");
        SetColumnTitle(_lvSmartProfiles, 1, "Col_TriggerProcess");
        SetColumnTitle(_lvSmartProfiles, 2, "Col_Description");
        SetColumnTitle(_lvSmartProfiles, 3, "Col_CpuIsolation");

        SetColumnTitle(_lvSecondaryOffenders, 0, "Col_WindowTitle");
        SetColumnTitle(_lvSecondaryOffenders, 1, "Col_Impact");

        SetColumnTitle(_lvThemes, 0, "Col_ThemeName");
        SetColumnTitle(_lvThemes, 1, "Col_Description");
        SetColumnTitle(_lvThemes, 2, "Col_Preset");

        SetColumnTitle(_lvCompanionTools, 0, "Col_Application");
        SetColumnTitle(_lvCompanionTools, 1, "Col_State");
        SetColumnTitle(_lvCompanionTools, 2, "Col_Description");

        UpdateStatusLabels();
    }

    private void SetColumnTitle(ListView lv, int colIndex, string key)
    {
        if (lv != null && lv.Columns.Count > colIndex)
        {
            string localized = LocalizationManager.Instance.T(key);
            var col = lv.Columns[colIndex];
            col.Tag = localized;
            if (lv.ListViewItemSorter is ListViewColumnSorter sorter && sorter.SortColumn == colIndex && sorter.Order != SortOrder.None)
            {
                col.Text = sorter.Order == SortOrder.Ascending ? $"{localized} ▲" : $"{localized} ▼";
            }
            else
            {
                col.Text = localized;
            }
        }
    }

    private async Task ApplyProfile(ProfileType profile)
    {
        _btnApplyProfile.Enabled = false;
        _statusLabel.Text = $"Applying profile {profile}...";

        try
        {
            var (success, log) = await _profileEngine.ApplyProfileAsync(profile);
            foreach (var line in log)
            {
                AppendLog(line);
            }
            RefreshAllData();
            _statusLabel.Text = $"Profile {profile} active.";
        }
        finally
        {
            _btnApplyProfile.Enabled = true;
        }
    }

    private async Task RestoreToNormal()
    {
        _btnRestoreNormal.Enabled = false;
        _statusLabel.Text = "Restoring normal baseline...";

        try
        {
            var (success, log) = await _profileEngine.RestoreToNormalAsync();
            foreach (var line in log)
            {
                AppendLog(line);
            }
            RefreshAllData();
            _statusLabel.Text = "System baseline restored.";
        }
        finally
        {
            _btnRestoreNormal.Enabled = true;
        }
    }

    private async Task RunBenchmarkAsync()
    {
        _btnRunBenchmark.Enabled = false;
        _lvBenchmarkResults.Items.Clear();
        _txtBenchmarkVerdict.Text = "Running verification benchmark...";

        try
        {
            // 1. BEFORE Metric
            _lblBenchmarkStatus.Text = "Phase 1/3: Capturing BEFORE state (3 seconds baseline)...";
            var progress = new Progress<string>(msg => _lblBenchmarkStatus.Text = msg);
            var before = await _benchmarkEngine.SampleAsync(3000, progress);

            // 2. APPLY PROFILE if not applied
            _lblBenchmarkStatus.Text = "Phase 2/3: Applying optimization profile...";
            var targetProfile = _rbMaxResponse.Checked ? ProfileType.MaxResponse : ProfileType.LowInterference;
            await _profileEngine.ApplyProfileAsync(targetProfile);
            await Task.Delay(1000); // Allow system to stabilize

            // 3. AFTER Metric
            _lblBenchmarkStatus.Text = "Phase 3/3: Capturing AFTER state (3 seconds optimized)...";
            var after = await _benchmarkEngine.SampleAsync(3000, progress);

            // 4. Comparison
            var comp = _benchmarkEngine.Compare(before, after);
            _lastBenchmarkComparison = comp;

            // Populate ListView
            AddMetricRow("Context Switches / sec", $"{before.ContextSwitchesPerSecond:N0}", $"{after.ContextSwitchesPerSecond:N0}", $"{comp.DeltaContextSwitchesPercent:+0.0;-0.0}%", comp.DeltaContextSwitchesPercent <= -2.0 ? "IMPROVED" : "NOISE");
            AddMetricRow("CPU Background Load %", $"{before.CpuLoadPercentage:F1}%", $"{after.CpuLoadPercentage:F1}%", $"{comp.DeltaCpuPercent:+0.0;-0.0}%", comp.DeltaCpuPercent <= -2.0 ? "IMPROVED" : "NOISE");
            AddMetricRow("Disk I/O Bytes / sec", $"{before.DiskBytesPerSecond:N0}", $"{after.DiskBytesPerSecond:N0}", $"{comp.DeltaDiskPercent:+0.0;-0.0}%", comp.DeltaDiskPercent <= -2.0 ? "IMPROVED" : "NOISE");
            AddMetricRow("DWM Memory Working Set", $"{before.DwmWorkingSetMB:F1} MB", $"{after.DwmWorkingSetMB:F1} MB", $"{comp.DeltaDwmMemoryPercent:+0.0;-0.0}%", comp.DeltaDwmMemoryPercent <= -2.0 ? "IMPROVED" : "NOISE");
            AddMetricRow("Active Processes", $"{before.ActiveProcessCount}", $"{after.ActiveProcessCount}", $"{after.ActiveProcessCount - before.ActiveProcessCount}", "INFO");

            // Verdict
            _txtBenchmarkVerdict.Text = comp.SummaryText;
            _lblBenchmarkStatus.Text = "Benchmark complete.";
            AppendLog($"[BENCHMARK] Completed: {comp.SummaryText}");
            RefreshAllData();
        }
        catch (Exception ex)
        {
            _txtBenchmarkVerdict.Text = $"Benchmark error: {ex.Message}";
            _lblBenchmarkStatus.Text = "Benchmark failed.";
        }
        finally
        {
            _btnRunBenchmark.Enabled = true;
        }
    }

    private async Task RunTrilateralBenchmarkAsync()
    {
        _btnRunAdvancedBenchmark.Enabled = false;
        _lvBenchmarkResults.Items.Clear();
        _txtBenchmarkVerdict.Text = "Running Advanced Trilateral Benchmark (Normal vs Low vs Max)...";

        try
        {
            var progress = new Progress<string>(msg => _lblBenchmarkStatus.Text = msg);

            // 1. Normal
            _lblBenchmarkStatus.Text = "Trilateral 1/3: Measuring NORMAL Baseline...";
            await _profileEngine.RestoreToNormalAsync();
            await Task.Delay(1000);
            var mNormal = await _advancedBenchmarkEngine.SampleAdvancedAsync(2500, progress);

            // 2. Low Interference
            _lblBenchmarkStatus.Text = "Trilateral 2/3: Measuring LOW INTERFERENCE...";
            await _profileEngine.ApplyProfileAsync(ProfileType.LowInterference);
            await Task.Delay(1000);
            var mLow = await _advancedBenchmarkEngine.SampleAdvancedAsync(2500, progress);

            // 3. Max Response
            _lblBenchmarkStatus.Text = "Trilateral 3/3: Measuring MAX RESPONSE...";
            await _profileEngine.ApplyProfileAsync(ProfileType.MaxResponse);
            await Task.Delay(1000);
            var mMax = await _advancedBenchmarkEngine.SampleAdvancedAsync(2500, progress);

            var report = new TrilateralBenchmarkReport
            {
                NormalBaseline = mNormal,
                LowInterference = mLow,
                MaxResponse = mMax
            };

            AddMetricRow("Context Switches/s", $"{mNormal.ContextSwitchesPerSecond:N0}", $"{mMax.ContextSwitchesPerSecond:N0}", $"{report.ContextSwitchesReductionPercent:+0.0;-0.0}%", report.ContextSwitchesReductionPercent <= -2.0 ? "IMPROVED" : "NOISE");
            AddMetricRow("NT Timer Resolution", $"{mNormal.CurrentTimerResolutionMs:F3} ms", $"{mMax.CurrentTimerResolutionMs:F3} ms", $"{mMax.CurrentTimerResolutionMs - mNormal.CurrentTimerResolutionMs:+0.000;-0.000} ms", "INFO");
            AddMetricRow("P1 Jitter (Queue wait)", $"{mNormal.EstimatedP1JitterMs:F2} ms", $"{mMax.EstimatedP1JitterMs:F2} ms", $"{((mMax.EstimatedP1JitterMs - mNormal.EstimatedP1JitterMs)/mNormal.EstimatedP1JitterMs)*100:+0.0;-0.0}%", "ESTIMATE");
            AddMetricRow("DWM Memory (MB)", $"{mNormal.DwmWorkingSetMB:F1} MB", $"{mMax.DwmWorkingSetMB:F1} MB", $"{report.DwmMemoryReductionPercent:+0.0;-0.0}%", report.DwmMemoryReductionPercent <= -2.0 ? "IMPROVED" : "NOISE");

            _txtBenchmarkVerdict.Text = $"[TRILATERAL VERDICT]\n{report.SummaryText}\n" +
                                        $"Timer resolution is running at {mMax.CurrentTimerResolutionMs:F3} ms (Native precision range: {mMax.MinTimerResolutionMs:F3} - {mMax.MaxTimerResolutionMs:F3} ms).";

            _lblBenchmarkStatus.Text = "Trilateral benchmark complete.";
            AppendLog($"[TRILATERAL] Complete: {report.SummaryText}");
            RefreshAllData();
        }
        catch (Exception ex)
        {
            _txtBenchmarkVerdict.Text = $"Trilateral error: {ex.Message}";
        }
        finally
        {
            _btnRunAdvancedBenchmark.Enabled = true;
        }
    }

    private void AddMetricRow(string name, string before, string after, string delta, string eval)
    {
        var item = new ListViewItem(name);
        item.SubItems.Add(before);
        item.SubItems.Add(after);
        item.SubItems.Add(delta);
        item.SubItems.Add(eval);

        if (eval == "IMPROVED")
            item.ForeColor = Color.DarkGreen;
        else if (eval == "NOISE")
            item.ForeColor = Color.Gray;

        _lvBenchmarkResults.Items.Add(item);
    }

    private void AppendLog(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        _txtLog.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
    }
    #endregion
}
