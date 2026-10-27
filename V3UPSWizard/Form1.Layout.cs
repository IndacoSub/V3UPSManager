using V3UPSManager;
using V3UPSManager.Core.Wizard;

namespace V3UPSWizard;

public partial class Form1
{
    private readonly Dictionary<Step, Panel> pages = new();
    private readonly List<Label> stepLabels = [];
    private TableLayoutPanel? sidebarPanel;
    private readonly Label subtitle = new() { Dock = DockStyle.Fill, AutoSize = false };
    private readonly Label footerStatus = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
    private readonly LinkLabel viewLog = new() { Text = "View details", AutoSize = true, Visible = false };
    private readonly Label workingStatus = new() { Dock = DockStyle.Top, Height = 42 };
    private readonly Label reviewSummary = new() { Dock = DockStyle.Fill, Padding = new Padding(0, 12, 0, 0) };
    private readonly Label completionSummary = new() { Dock = DockStyle.Top, Height = 140 };
    private readonly CheckBox allowDiscovery = new()
    {
        Text = "Allow local game search on the drive I choose (optional)", AutoSize = true,
        Checked = false, AccessibleDescription = "Optional. Enables the Search button; it does not start a search."
    };
    private readonly ComboBox searchDrive = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 105 };
    private readonly Button searchButton = new() { Text = "Search", AutoSize = true, Enabled = false };
    private readonly Button useResult = new() { Text = "Use selected folder", AutoSize = true, Enabled = false };
    private readonly Label searchStatus = new() { Dock = DockStyle.Fill, AutoSize = true, Text = "Search is off. You can choose a folder manually above." };
    private readonly DataGridView searchResults = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false, RowHeadersVisible = false, MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor = SystemColors.Window, BorderStyle = BorderStyle.FixedSingle, Enabled = false,
        AccessibleName = "Supported game search results", Height = 150
    };

    private sealed record GameChoice(Game Id, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }

    private static string GameName(Game id) => id switch
    {
        Game.DanganronpaV3 => "Danganronpa V3: Killing Harmony",
        Game.AITheSomniumFiles => "AI: The Somnium Files",
        Game.TokyoPsychodemic => "Tokyo Psychodemic",
        _ => id.ToString()
    };

    private void BuildShell()
    {
        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var sidebar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 1, RowCount = 9,
            BackColor = SystemColors.Control, Padding = new Padding(20, 28, 16, 20), Margin = Padding.Empty
        };
        sidebarPanel = sidebar;
        var brand = new Label { Text = "V3UPS Setup", Dock = DockStyle.Fill, Font = new Font(Font.FontFamily, 18, FontStyle.Bold) };
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        sidebar.Controls.Add(brand, 0, 0);
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        sidebar.Controls.Add(new Label { Text = "Install a game translation\nor another supported mod.", Dock = DockStyle.Fill }, 0, 1);
        string[] names = ["1  Welcome", "2  Patch package", "3  Instructions", "4  Game location", "5  Review", "6  Install"];
        for (int index = 0; index < names.Length; index++)
        {
            var label = new Label { Text = names[index], Dock = DockStyle.Fill, Padding = new Padding(10, 10, 4, 0), Margin = new Padding(0, 4, 0, 4) };
            sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            sidebar.Controls.Add(label, 0, index + 2);
            stepLabels.Add(label);
        }
        sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        sidebar.Controls.Add(new Label { Text = "Your game files stay on this PC.", Dock = DockStyle.Bottom, AutoSize = true }, 0, 8);
        shell.Controls.Add(sidebar, 0, 0);

        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = Padding.Empty, BackColor = SystemColors.Window };
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(28, 24, 28, 0), Margin = Padding.Empty };
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        heading.Font = new Font(Font.FontFamily, 18, FontStyle.Bold);
        heading.Dock = DockStyle.Fill;
        header.Controls.Add(heading, 0, 0);
        header.Controls.Add(subtitle, 0, 1);
        body.Controls.Add(header, 0, 0);
        content.Padding = new Padding(28, 8, 28, 20);
        body.Controls.Add(content, 0, 1);
        var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(28, 12, 24, 12), BackColor = SystemColors.Control, Margin = Padding.Empty };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var status = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Margin = Padding.Empty };
        footerStatus.AutoSize = true;
        footerStatus.Margin = Padding.Empty;
        viewLog.Margin = Padding.Empty;
        status.Controls.AddRange([footerStatus, viewLog]);
        footer.Controls.Add(status, 0, 0);
        var actions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, WrapContents = false, Margin = Padding.Empty };
        foreach (Button button in new[] { back, next, cancel }) { button.Height = 32; button.Margin = new Padding(5, 0, 0, 0); }
        actions.Controls.AddRange([back, next, cancel]);
        footer.Controls.Add(actions, 1, 0);
        body.Controls.Add(footer, 0, 2);
        shell.Controls.Add(body, 1, 0);
        Controls.Add(shell);
        viewLog.LinkClicked += (_, _) => ShowDetails();
    }

    private void BuildPages()
    {
        foreach (Step value in Enum.GetValues<Step>())
        {
            var page = new Panel { Dock = DockStyle.Fill, Visible = false };
            pages[value] = page;
            content.Controls.Add(page);
        }
        var terms = MultilineText();
        terms.Text = "V3UPSWizard — End User Agreement\r\n\r\n" +
            "This wizard prepares the patch package you select and installs it into the game directory you choose. " +
            "No data is uploaded. Drives are not searched automatically.\r\n\r\n" +
            "You may optionally enable local game search later. That separate checkbox allows a search of known Steam and Xbox game-library locations " +
            "on the drive you select. Search begins only when you click Search; you can leave it off and choose a folder manually.\r\n\r\n" +
            "Read the mod author's instructions and keep a backup of your game files. Installing a mod changes files in the selected game directory. " +
            "The program applies UPS or XDelta patches and does not resolve conflicts between mods.\r\n\r\n" +
            "This software is provided 'as is', without warranties of any kind. The authors and distributors assume no responsibility for damages or data loss " +
            "arising from its use. If you do not agree, exit the wizard.\r\n\r\n" +
            "Source and licenses: https://github.com/IndacoSub/V3UPSManager\r\n\r\n" +
            "The project is not affiliated with the game publishers, GitHub or Microsoft.";
        var agreement = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        agreement.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        agreement.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        agree.AutoSize = true;
        agree.Margin = new Padding(0, 16, 0, 0);
        agreement.Controls.Add(terms, 0, 0);
        agreement.Controls.Add(agree, 0, 1);
        pages[Step.Agreement].Controls.Add(agreement);

        var package = PageStack();
        AddStackRow(package, Description("Choose an extracted patch folder or a .DAIX.7z package supplied by the mod author."));
        AddStackRow(package, FieldLabel("Patch package"));
        packagePath.Dock = DockStyle.Top;
        packagePath.AccessibleName = "Patch package path";
        AddStackRow(package, packagePath);
        var choices = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 10, 0, 14) };
        var folder = new Button { Text = "Choose folder...", AutoSize = true };
        var archive = new Button { Text = "Choose archive...", AutoSize = true };
        folder.Click += (_, _) => BrowseFolder(packagePath);
        archive.Click += (_, _) =>
        {
            using var picker = new OpenFileDialog { Filter = "DAIX 7-Zip packages|*.DAIX.7z", CheckFileExists = true };
            if (picker.ShowDialog(this) == DialogResult.OK) packagePath.Text = picker.FileName;
        };
        choices.Controls.AddRange([folder, archive]);
        AddStackRow(package, choices);
        AddStackRow(package, Description("Compressed packages need the bundled archive tool. You can also select a package that has already been extracted."));
        pages[Step.Package].Controls.Add(package);
        pages[Step.Readme].Controls.Add(readme);
        BuildInstallationPage();

        var working = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        working.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        working.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        working.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        working.Controls.Add(workingStatus, 0, 0);
        working.Controls.Add(progress, 0, 1);
        log.Font = new Font(FontFamily.GenericMonospace, 9);
        log.AccessibleName = "Installation details";
        working.Controls.Add(log, 0, 2);
        pages[Step.Working].Controls.Add(working);
        pages[Step.Ready].Controls.Add(reviewSummary);
        pages[Step.Complete].Controls.Add(completionSummary);
    }

    private void BuildInstallationPage()
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = Padding.Empty };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var manual = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Margin = Padding.Empty };
        manual.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        manual.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        manual.Controls.Add(FieldLabel("Game"), 0, 0);
        game.Dock = DockStyle.Fill;
        game.AccessibleName = "Game to install the mod for";
        manual.Controls.Add(game, 0, 1);
        manual.SetColumnSpan(game, 2);
        manual.Controls.Add(FieldLabel("Game installation directory"), 0, 2);
        installPath.Dock = DockStyle.Fill;
        installPath.AccessibleName = "Game installation directory";
        manual.Controls.Add(installPath, 0, 3);
        var browse = new Button { Text = "Browse...", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
        browse.Click += (_, _) => BrowseFolder(installPath);
        manual.Controls.Add(browse, 1, 3);
        layout.Controls.Add(manual, 0, 0);

        var search = new GroupBox { Text = "Optional game search", Dock = DockStyle.Fill, Padding = new Padding(14, 20, 14, 12) };
        var searchLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Margin = Padding.Empty };
        searchLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        searchLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        searchLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        searchLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        searchLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        searchLayout.Controls.Add(allowDiscovery, 0, 0);
        searchLayout.Controls.Add(Description("Reads Steam library metadata and checks known Steam/Xbox game folders on one selected drive. Results stay on this PC."), 0, 1);
        var tools = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false, Margin = new Padding(0, 4, 0, 8) };
        tools.Controls.Add(new Label { Text = "Drive", AutoSize = true, Margin = new Padding(0, 7, 8, 0) });
        tools.Controls.Add(searchDrive);
        tools.Controls.Add(searchButton);
        tools.Controls.Add(useResult);
        searchLayout.Controls.Add(tools, 0, 2);
        searchResults.Columns.Add(new DataGridViewTextBoxColumn { Name = "Game", HeaderText = "Game", FillWeight = 35 });
        searchResults.Columns.Add(new DataGridViewTextBoxColumn { Name = "Source", HeaderText = "Library", FillWeight = 15 });
        searchResults.Columns.Add(new DataGridViewTextBoxColumn { Name = "Path", HeaderText = "Installation directory", FillWeight = 50 });
        searchLayout.Controls.Add(searchResults, 0, 3);
        searchStatus.Margin = new Padding(0, 8, 0, 0);
        searchLayout.Controls.Add(searchStatus, 0, 4);
        search.Controls.Add(searchLayout);
        layout.Controls.Add(search, 0, 1);
        pages[Step.Installation].Controls.Add(layout);
        allowDiscovery.CheckedChanged += (_, _) =>
        {
            if (!allowDiscovery.Checked)
            {
                searchOperation?.Cancel();
                searchResults.Rows.Clear();
                searchStatus.Text = "Search is off. You can choose a folder manually above.";
            }
            else
            {
                PopulateSearchDrives();
                searchStatus.Text = "Choose a drive and click Search. No search has started yet.";
            }
            UpdateNavigation();
        };
        searchDrive.SelectedIndexChanged += (_, _) =>
        {
            // Changing the search scope invalidates suggestions. It never starts I/O.
            searchOperation?.Cancel();
            searchResults.Rows.Clear();
            searchStatus.Text = allowDiscovery.Checked
                ? "Drive selected. Click Search to check known game-library locations on this drive."
                : "Search is off. You can choose a folder manually above.";
            UpdateDiscoveryControls();
        };
        searchButton.Click += async (_, _) => await SearchGamesAsync();
        searchResults.SelectionChanged += (_, _) => UpdateDiscoveryControls();
        useResult.Click += (_, _) => UseSelectedResult();
        searchResults.CellDoubleClick += (_, args) => { if (args.RowIndex >= 0) UseSelectedResult(); };
    }

    private void PopulateSearchDrives()
    {
        if (searchDrive.Items.Count > 0) return;
        // GetLogicalDrives reads the logical-drive list without probing volume readiness.
        // Do not use DriveInfo.IsReady or enumerate the contents of every drive.
        try { searchDrive.Items.AddRange(Environment.GetLogicalDrives().Cast<object>().ToArray()); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        string preferred = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
        if (searchDrive.Items.Count == 0) searchDrive.Items.Add(preferred);
        searchDrive.SelectedItem = preferred;
        if (searchDrive.SelectedIndex < 0) searchDrive.SelectedIndex = 0;
    }

    private static TableLayoutPanel PageStack() => new() { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, Margin = Padding.Empty };
    private static Label FieldLabel(string text) => new() { Text = text, AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 6) };
    private static Label Description(string text) => new() { Text = text, AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 12), MaximumSize = new Size(660, 0) };
    private static void AddStackRow(TableLayoutPanel layout, Control control)
    {
        int row = layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(control, 0, row);
    }

    private void ShowDetails()
    {
        using var details = new Form { Text = "Setup details", StartPosition = FormStartPosition.CenterParent, Size = new Size(820, 520), MinimizeBox = false, ShowInTaskbar = false };
        var text = MultilineText();
        text.Text = log.Text;
        text.Font = new Font(FontFamily.GenericMonospace, 9);
        details.Controls.Add(text);
        details.ShowDialog(this);
    }
}
