using V3UPSManager;
using V3UPSManager.Core;
using V3UPSManager.Core.Wizard;

namespace V3UPSWizard;

/// <summary>A guided setup front end over the shared validation and transactional installer.</summary>
public partial class Form1 : Form
{
    private enum Step { Agreement, Package, Readme, Installation, Working, Ready, Complete }
    private readonly Panel content = new() { Dock = DockStyle.Fill, AutoScroll = true };
    private readonly Label heading = new() { Dock = DockStyle.Fill, AutoEllipsis = true };
    private readonly Button back = new() { Text = "< Back", Width = 90 };
    private readonly Button next = new() { Text = "Next >", Width = 118 };
    private readonly Button cancel = new() { Text = "Cancel setup", Width = 118 };
    private readonly CheckBox agree = new() { Text = "I have read and accept these terms" };
    private readonly TextBox packagePath = new();
    private readonly TextBox installPath = new();
    private readonly ComboBox game = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox readme = MultilineText();
    private readonly TextBox log = MultilineText();
    private readonly ProgressBar progress = new() { Dock = DockStyle.Fill, Style = ProgressBarStyle.Marquee };
    private PackageSession? session;
    private PreparedInstallation? prepared;
    private CancellationTokenSource? operation;
    private CancellationTokenSource? searchOperation;
    private Step step;
    private Step retryStep;
    private bool busy;
    private bool searching;
    private bool installRunning;
    private bool closeRequested;

    public Form1()
    {
        InitializeComponent();
        BuildShell();
        BuildPages();
        game.Items.AddRange(GameCatalog.All.Select(value => new GameChoice(value.GameID, GameName(value.GameID))).Cast<object>().ToArray());
        game.SelectedIndex = 0;
        agree.CheckedChanged += (_, _) => UpdateNavigation();
        back.Click += (_, _) => GoBack();
        next.Click += async (_, _) => await AdvanceAsync();
        cancel.Click += (_, _) =>
        {
            if (installRunning) return;
            if (searching) searchOperation?.Cancel();
            else if (busy) operation?.Cancel();
            else Close();
        };
        AcceptButton = next;
        CancelButton = cancel;
        ShowStep(Step.Agreement);
    }

    private static TextBox MultilineText() => new()
    {
        Dock = DockStyle.Fill, Multiline = true, ReadOnly = true,
        ScrollBars = ScrollBars.Vertical, BackColor = SystemColors.Window,
        BorderStyle = BorderStyle.FixedSingle
    };

    private void ShowStep(Step value)
    {
        step = value;
        foreach ((Step key, Panel page) in pages) page.Visible = key == value;
        pages[value].BringToFront();
        content.AutoScrollPosition = Point.Empty;
        (heading.Text, subtitle.Text) = value switch
        {
            Step.Agreement => ("Welcome to V3UPS Setup", "Prepare your patch package, choose the game, and review everything before installing."),
            Step.Package => ("Choose a patch package", "Use the files supplied by the mod author. Your original package will remain unchanged."),
            Step.Readme => ("Read the mod instructions", "Check the required game version and any preparation steps before continuing."),
            Step.Installation => ("Choose your game location", "Select the game's installation root. A manual folder selection is always available."),
            Step.Working => ("Preparing your installation", "You can review the operation details below."),
            Step.Ready => ("Review and install", "Check the game and folder below. No game files have been changed yet."),
            _ => ("Installation complete", "All patches in the prepared installation plan were applied successfully.")
        };
        if (value == Step.Readme) readme.Text = session?.Readme;
        if (value == Step.Working) workingStatus.Text = "Preparing the package and checking the selected game...";
        if (value == Step.Ready)
        {
            reviewSummary.Text = $"GAME\r\n{game.SelectedItem}\r\n\r\nEDITION\r\n{prepared?.Validation.Edition}\r\n\r\n" +
                $"INSTALLATION DIRECTORY\r\n{installPath.Text}\r\n\r\nFILES TO PATCH\r\n{prepared?.Discovery.Targets.Count}\r\n\r\n" +
                "Close the game before installing. Original files are preserved in _bak backups. " +
                "Once installation starts, let it finish or roll back before closing this window.";
        }
        if (value == Step.Complete)
        {
            completionSummary.Text = $"{game.SelectedItem}\r\nFiles patched: {prepared?.Discovery.Targets.Count}\r\n\r\n" +
                "The original files remain in _bak backups beside the game files. " +
                "You can review the setup details below, or select Finish to close.";
        }
        UpdateNavigation();
    }

    private void BrowseFolder(TextBox target)
    {
        using var picker = new FolderBrowserDialog { Description = "Select a folder", UseDescriptionForTitle = true };
        if (Directory.Exists(target.Text)) picker.SelectedPath = target.Text;
        if (picker.ShowDialog(this) == DialogResult.OK) target.Text = picker.SelectedPath;
    }

    private void SelectGame(Game id)
    {
        for (int index = 0; index < game.Items.Count; index++)
            if (game.Items[index] is GameChoice choice && choice.Id == id)
            {
                game.SelectedIndex = index;
                return;
            }
    }

    private void UpdateNavigation()
    {
        back.Enabled = !busy && !searching && step is Step.Package or Step.Readme or Step.Installation or Step.Ready;
        next.Enabled = !busy && !searching && (step != Step.Agreement || agree.Checked);
        next.Text = step switch { Step.Ready => "Install", Step.Complete => "Finish", _ => "Next >" };
        cancel.Enabled = !installRunning;
        cancel.Text = searching ? "Stop search" : busy ? "Cancel" : "Cancel setup";
        cancel.Visible = step != Step.Complete;
        int active = step switch
        {
            Step.Agreement => 0, Step.Package => 1, Step.Readme => 2, Step.Installation => 3,
            Step.Ready => 4, Step.Complete => 5,
            _ => installRunning ? 5 : retryStep switch { Step.Package => 1, Step.Installation => 3, Step.Ready => 5, _ => 0 }
        };
        footerStatus.Text = installRunning ? "Installing — please wait" : searching ? "Searching the selected drive" : $"Step {active + 1} of 6";
        for (int index = 0; index < stepLabels.Count; index++)
        {
            stepLabels[index].BackColor = index == active ? SystemColors.Highlight : SystemColors.Control;
            stepLabels[index].ForeColor = index == active ? SystemColors.HighlightText : SystemColors.ControlText;
        }
        viewLog.Visible = log.TextLength > 0;
        UpdateDiscoveryControls();
    }

    private void UpdateDiscoveryControls()
    {
        bool allowed = allowDiscovery.Checked && !busy;
        allowDiscovery.Enabled = !busy;
        searchButton.Enabled = allowed;
        searchButton.Text = searching ? "Stop search" : "Search";
        searchDrive.Enabled = allowed && !searching;
        searchResults.Enabled = allowed && !searching;
        useResult.Enabled = allowed && !searching && searchResults.SelectedRows.Count == 1;
    }

    private async Task SearchGamesAsync()
    {
        if (searching) { searchOperation?.Cancel(); return; }
        if (!allowDiscovery.Checked || busy || step != Step.Installation) return;
        string selectedDrive = (string?)searchDrive.SelectedItem ?? "";
        if (selectedDrive.Length == 0) return;
        searching = true;
        searchOperation = new CancellationTokenSource();
        searchResults.Rows.Clear();
        searchStatus.Text = $"Checking known game-library locations on {selectedDrive}...";
        UpdateNavigation();
        try
        {
            IReadOnlyList<DiscoveredGame> found = await Task.Run(() =>
                GameInstallationDiscovery.Discover(selectedDrive, cancellationToken: searchOperation.Token), searchOperation.Token);
            if (!allowDiscovery.Checked || closeRequested || searchOperation.IsCancellationRequested) return;
            foreach (DiscoveredGame suggestion in found)
            {
                int row = searchResults.Rows.Add(GameName(suggestion.Game), suggestion.Source, suggestion.InstallationFolder);
                searchResults.Rows[row].Tag = suggestion;
            }
            searchResults.ClearSelection();
            searchStatus.Text = found.Count == 0
                ? "No supported games were found in those locations. Choose the game folder manually above."
                : $"Found {found.Count} supported installation(s). Select a result and click Use selected folder.";
        }
        catch (OperationCanceledException)
        {
            searchStatus.Text = allowDiscovery.Checked ? "Search stopped. You can retry or choose a folder manually." : "Search is off. You can choose a folder manually above.";
        }
        catch (Exception exception)
        {
            searchStatus.Text = "The selected drive could not be searched. Choose a folder manually or try another drive.";
            MessageBox.Show(this, exception.Message, "Game search", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        finally
        {
            searching = false;
            searchOperation.Dispose();
            searchOperation = null;
            UpdateNavigation();
            if (closeRequested) Close();
        }
    }

    private void UseSelectedResult()
    {
        if (!allowDiscovery.Checked || searching || busy || searchResults.SelectedRows.Count != 1 ||
            searchResults.SelectedRows[0].Tag is not DiscoveredGame suggestion) return;
        SelectGame(suggestion.Game);
        installPath.Text = suggestion.InstallationFolder;
        prepared = null;
        searchStatus.Text = "Folder selected. Click Next to validate this installation before continuing.";
    }

    private void GoBack()
    {
        if (busy || searching) return;
        ShowStep(step switch
        {
            Step.Package => Step.Agreement,
            Step.Readme => Step.Package,
            Step.Installation => Step.Readme,
            Step.Ready => Step.Installation,
            _ => step
        });
    }

    private async Task AdvanceAsync()
    {
        if (busy || searching) return;
        switch (step)
        {
            case Step.Agreement:
                if (agree.Checked) ShowStep(Step.Package);
                break;
            case Step.Package:
                await ExecuteAsync(Step.Package, async token =>
                {
                    prepared = null;
                    session?.Dispose();
                    session = new PackageSession(Path.Combine(AppContext.BaseDirectory, "7za.exe"));
                    string source = packagePath.Text.Trim();
                    IProgress<string> report = LogProgress();
                    await Task.Run(() => session.OpenAsync(source, report, token), token);
                    game.SelectedIndex = 0;
                    if (Enum.TryParse(session.GameName, out Game namedGame)) SelectGame(namedGame);
                    ShowStep(Step.Readme);
                });
                break;
            case Step.Readme:
                ShowStep(Step.Installation);
                break;
            case Step.Installation:
                if (!Directory.Exists(installPath.Text.Trim()))
                {
                    MessageBox.Show(this, "Select an existing game installation directory.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                installPath.Text = Path.GetFullPath(installPath.Text.Trim());
                await ExecuteAsync(Step.Installation, async token =>
                {
                    IProgress<string> report = LogProgress();
                    prepared = null;
                    await Task.Run(() => session!.PreparePatchesAsync(report, token), token);
                    string installation = installPath.Text;
                    string patches = session!.PatchFolder;
                    Game selectedGame = ((GameChoice)game.SelectedItem!).Id;
                    prepared = await Task.Run(() => WizardInstallation.Prepare(selectedGame,
                        installation, patches, AppContext.BaseDirectory, ReportValidation), token);
                    token.ThrowIfCancellationRequested();
                    ShowStep(Step.Ready);
                });
                break;
            case Step.Ready:
                await ExecuteAsync(Step.Ready, async _ =>
                {
                    PreparedInstallation plan = prepared ?? throw new InvalidOperationException("Prepare the installation first.");
                    installRunning = true;
                    heading.Text = "Installing your mod";
                    subtitle.Text = "Keep this window open and leave the game closed until installation finishes.";
                    workingStatus.Text = "Applying patches and preserving the original game files...";
                    UpdateNavigation();
                    IProgress<string> report = LogProgress();
                    bool succeeded = await Task.Run(() => WizardInstallation.Install(plan,
                        report: message => report.Report(WizardMessages.Format(message))));
                    installRunning = false;
                    if (!succeeded)
                        throw new IOException("Installation failed. The installer attempted to restore the previous game files. " +
                            "Review the output for restoration errors before retrying.");
                    ShowStep(Step.Complete);
                });
                break;
            case Step.Complete:
                Close();
                break;
        }
    }

    private bool ReportValidation(ValidationMessage message)
    {
        if (InvokeRequired) return (bool)Invoke(() => ReportValidation(message));
        string text = WizardMessages.Format(message);
        log.AppendText(text + Environment.NewLine);
        if (operation?.IsCancellationRequested == true) return false;
        if (!message.RequiresConfirmation) return !message.IsError;
        return MessageBox.Show(this, text, "Confirm game configuration", MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
    }

    private IProgress<string> LogProgress() => new Progress<string>(line =>
    {
        if (IsDisposed || Disposing) return;
        if (log.TextLength > 100_000) log.Clear();
        log.AppendText(line + Environment.NewLine);
        viewLog.Visible = true;
    });

    private async Task ExecuteAsync(Step failureStep, Func<CancellationToken, Task> action)
    {
        retryStep = failureStep;
        busy = true;
        operation = new CancellationTokenSource();
        log.Clear();
        ShowStep(Step.Working);
        try { await action(operation.Token); }
        catch (OperationCanceledException) { if (!closeRequested) ShowStep(retryStep); }
        catch (Exception exception)
        {
            if (!closeRequested)
            {
                string details = log.TextLength == 0 ? "" : "\r\n\r\nDetails:\r\n" + log.Text[^Math.Min(log.TextLength, 2000)..];
                MessageBox.Show(this, exception.Message + details, "Unable to continue", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShowStep(retryStep);
            }
        }
        finally
        {
            installRunning = false;
            busy = false;
            operation.Dispose();
            operation = null;
            UpdateNavigation();
            if (closeRequested) Close();
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        FitToWorkingArea(center: true);
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);
        FitToWorkingArea(center: false);
    }

    private void FitToWorkingArea(bool center)
    {
        // Bounds and WorkingArea both include the window frame/taskbar exclusion.
        // Clamp after WinForms DPI scaling, rather than assuming a logical screen size.
        Rectangle area = Screen.FromControl(this).WorkingArea;
        float scale = DeviceDpi / 96f;
        MinimumSize = new Size(Math.Min((int)(920 * scale), area.Width),
            Math.Min((int)(640 * scale), area.Height));
        content.AutoScrollMinSize = new Size((int)(620 * scale), (int)(420 * scale));
        if (sidebarPanel is not null) sidebarPanel.AutoScrollMinSize = new Size(0, (int)(430 * scale));
        int width = Math.Min(Width, area.Width);
        int height = Math.Min(Height, area.Height);
        int left = center ? area.Left + (area.Width - width) / 2 : Math.Clamp(Left, area.Left, area.Right - width);
        int top = center ? area.Top + (area.Height - height) / 2 : Math.Clamp(Top, area.Top, area.Bottom - height);
        if (center) StartPosition = FormStartPosition.Manual;
        Bounds = new Rectangle(left, top, width, height);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (installRunning)
        {
            e.Cancel = true;
            MessageBox.Show(this, "Installation is changing game files and cannot be interrupted. Wait until it finishes before closing setup.", Text);
        }
        else if (busy || searching)
        {
            e.Cancel = true;
            closeRequested = true;
            operation?.Cancel();
            searchOperation?.Cancel();
        }
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        session?.Dispose();
        base.OnFormClosed(e);
    }
}
