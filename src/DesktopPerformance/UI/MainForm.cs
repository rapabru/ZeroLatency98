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

    // UI Controls
    private MenuStrip _menuStrip = null!;
    private Panel _bannerPanel = null!;
    private RetroPanel _summaryPanel = null!;
    private Label _lblSummary = null!;
    private TabControl _tabControl = null!;

    // Tab 1: Topology
    private ListView _lvDisplays = null!;
    private Label _lblHardwareDetails = null!;
    private Label _lblVisualDetails = null!;
    private RetroButton _btnRefreshTopology = null!;

    // Tab 2: Profiles
    private RadioButton _rbNormal = null!;
    private RadioButton _rbLowInterference = null!;
    private RadioButton _rbMaxResponse = null!;
    private RetroButton _btnApplyProfile = null!;
    private RetroButton _btnRestoreNormal = null!;
    private TextBox _txtLog = null!;

    // Tab 3: Background Manager
    private ListView _lvApps = null!;
    private RetroButton _btnRefreshApps = null!;
    private RetroButton _btnPauseApp = null!;
    private RetroButton _btnResumeApp = null!;
    private RetroButton _btnBackgroundPriority = null!;

    // Tab 4: Benchmark
    private RetroButton _btnRunBenchmark = null!;
    private Label _lblBenchmarkStatus = null!;
    private ListView _lvBenchmarkResults = null!;
    private TextBox _txtBenchmarkVerdict = null!;

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

        InitializeComponent();
        _profileEngine.Initialize();
        RefreshAllData();
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        Text = "Desktop Performance - Low-Interference Mode [Win98]";
        Width = 840;
        Height = 650;
        MinimumSize = new Size(780, 580);
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

        var fileMenu = new ToolStripMenuItem("&File");
        fileMenu.DropDownItems.Add("&Refresh All", null, (s, e) => RefreshAllData());
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("E&xit", null, (s, e) => Close());

        var profilesMenu = new ToolStripMenuItem("&Profiles");
        profilesMenu.DropDownItems.Add("&Normal (Baseline)", null, async (s, e) => await ApplyProfile(ProfileType.Normal));
        profilesMenu.DropDownItems.Add("&Low Interference", null, async (s, e) => await ApplyProfile(ProfileType.LowInterference));
        profilesMenu.DropDownItems.Add("&Max Response", null, async (s, e) => await ApplyProfile(ProfileType.MaxResponse));
        profilesMenu.DropDownItems.Add(new ToolStripSeparator());
        profilesMenu.DropDownItems.Add("&Restore to Normal (Undo All)", null, async (s, e) => await RestoreToNormal());

        var helpMenu = new ToolStripMenuItem("&Help");
        helpMenu.DropDownItems.Add("&About Win98 Desktop Performance Mode", null, (s, e) =>
        {
            MessageBox.Show(
                "Desktop Performance & Low-Interference Mode\n\n" +
                "Authentic Windows 98/2000 Retro Aesthetics for Windows 11.\n" +
                "Zero bloat, zero placebo, transactional snapshots with 1-click restore.\n\n" +
                "Version 1.0 (Phase 2 MVP)",
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });

        _menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, profilesMenu, helpMenu });

        // 2. Banner Panel
        _bannerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 28
        };
        _bannerPanel.Paint += (s, e) =>
        {
            RetroTheme.DrawTitleBar(e.Graphics, _bannerPanel.ClientRectangle, "  Desktop Performance / Low-Interference System Controller");
        };

        // 3. Summary Panel (Sunken)
        _summaryPanel = new RetroPanel
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(6),
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

        // 4. TabControl
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

        // 5. StatusStrip
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
        Controls.Add(_bannerPanel);
        Controls.Add(_menuStrip);
        Controls.Add(_statusStrip);

        MainMenuStrip = _menuStrip;
        ResumeLayout(false);
        PerformLayout();
    }

    #region Tab 1: System Topology
    private void BuildTopologyTab()
    {
        var tab = new TabPage("System Topology & Monitors")
        {
            BackColor = RetroTheme.BackgroundColor,
            Padding = new Padding(8)
        };

        var pnlTop = new GroupBox
        {
            Text = "Active Displays & Monitors",
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
        pnlTop.Controls.Add(_lvDisplays);

        var pnlMid = new GroupBox
        {
            Text = "Hardware & Memory State",
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
        pnlMid.Controls.Add(_lblHardwareDetails);

        var pnlBottom = new GroupBox
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

        pnlBottom.Controls.Add(_lblVisualDetails);
        pnlBottom.Controls.Add(_btnRefreshTopology);

        tab.Controls.Add(pnlBottom);
        tab.Controls.Add(pnlMid);
        tab.Controls.Add(pnlTop);

        _tabControl.TabPages.Add(tab);
    }
    #endregion

    #region Tab 2: Profiles & Optimization
    private void BuildProfilesTab()
    {
        var tab = new TabPage("Profiles & Optimization")
        {
            BackColor = RetroTheme.BackgroundColor,
            Padding = new Padding(8)
        };

        var grpSelector = new GroupBox
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

        grpSelector.Controls.AddRange(new Control[] { _rbNormal, _rbLowInterference, _rbMaxResponse });

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

        var grpLog = new GroupBox
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
        grpLog.Controls.Add(_txtLog);

        tab.Controls.Add(grpLog);
        tab.Controls.Add(pnlButtons);
        tab.Controls.Add(grpSelector);

        _tabControl.TabPages.Add(tab);
    }
    #endregion

    #region Tab 3: Background Manager
    private void BuildBackgroundManagerTab()
    {
        var tab = new TabPage("Background Manager")
        {
            BackColor = RetroTheme.BackgroundColor,
            Padding = new Padding(8)
        };

        var lblInfo = new Label
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

        tab.Controls.Add(_lvApps);
        tab.Controls.Add(pnlActions);
        tab.Controls.Add(lblInfo);

        _tabControl.TabPages.Add(tab);
    }
    #endregion

    #region Tab 4: Benchmark
    private void BuildBenchmarkTab()
    {
        var tab = new TabPage("Verification & Benchmark")
        {
            BackColor = RetroTheme.BackgroundColor,
            Padding = new Padding(8)
        };

        var pnlHeader = new GroupBox
        {
            Text = "Performance Data Helper (PDH) Measurement Engine",
            Dock = DockStyle.Top,
            Height = 85,
            Font = RetroTheme.BoldFont
        };

        var lblNotice = new Label
        {
            Text = "Principio: 'Medición antes que afirmaciones. No inventar mejoras.'\n" +
                   "Este módulo toma muestras antes y después de aplicar el perfil. Si la diferencia es menor al 2.0%,\n" +
                   "se reportará honestamente: 'No measurable improvement detected'.",
            Location = new Point(12, 22),
            Size = new Size(620, 52),
            Font = RetroTheme.DefaultFont
        };

        _btnRunBenchmark = new RetroButton
        {
            Text = "[ RUN BEFORE / AFTER BENCHMARK ]",
            Location = new Point(640, 26),
            Size = new Size(150, 44),
            Font = RetroTheme.BoldFont
        };
        _btnRunBenchmark.Click += async (s, e) => await RunBenchmarkAsync();

        pnlHeader.Controls.AddRange(new Control[] { lblNotice, _btnRunBenchmark });

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

        var grpVerdict = new GroupBox
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
        grpVerdict.Controls.Add(_txtBenchmarkVerdict);

        tab.Controls.Add(grpVerdict);
        tab.Controls.Add(_lvBenchmarkResults);
        tab.Controls.Add(_lblBenchmarkStatus);
        tab.Controls.Add(pnlHeader);

        _tabControl.TabPages.Add(tab);
    }
    #endregion

    #region Business Logic & Orchestration
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

            // Visuals
            var vis = _visualEffects.CaptureCurrentVisualState();
            _lblVisualDetails.Text = $"Desktop Transparency (Mica/Acrylic): {(vis.TransparencyEnabled ? "ENABLED" : "Disabled (Low Interference)")}\n" +
                                     $"Shell Window Animations: {(vis.WindowAnimationsEnabled ? "ENABLED" : "Disabled (Instant UI)")}\n" +
                                     $"Taskbar Widgets Panel: {(vis.TaskbarWidgetsEnabled ? "ENABLED" : "Disabled / Hidden")}\n" +
                                     $"Wallpaper: {(string.IsNullOrEmpty(vis.WallpaperPath) ? "Solid Color / Default" : Path.GetFileName(vis.WallpaperPath))}";

            // Background apps
            RefreshBackgroundApps();

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
    }

    private void UpdateStatusLabels()
    {
        _statusProfile.Text = $"Profile: {_profileEngine.CurrentProfile.ToString().ToUpper()}";
        bool hasSnap = _snapshotManager.HasActiveSnapshot();
        _statusSnapshot.Text = hasSnap ? "Snapshot: ACTIVE" : "Snapshot: None";

        if (_profileEngine.CurrentProfile == ProfileType.LowInterference)
            _rbLowInterference.Checked = true;
        else if (_profileEngine.CurrentProfile == ProfileType.MaxResponse)
            _rbMaxResponse.Checked = true;
        else
            _rbNormal.Checked = true;
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
