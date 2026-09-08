namespace V3UPSManager;

public partial class MainWindow : Form
{

	public GameBase CurrentGame = new DRV3();

	private string installation_folder = "";
	private string ups_folder = "";
	private string verified_installation_folder = "";

	private bool manually_selected_game = false;
	private bool manually_selected_install = false;
	private bool manually_selected_patch = false;

	public MainWindow(string[] args)
	{
		InitializeComponent();
		if (LanguageComboBox != null)
		{
			LanguageComboBox.SelectedIndexChanged -= LanguageComboBox_SelectedIndexChanged;
			LanguageComboBox.Enabled = false;
			LanguageComboBox.SelectedIndex = -1; // Default to Italian
		}
		LoadLanguages();
		InitLanguages();
		CheckIndexChange();

		if (info_it.Count != info_en.Count)
		{
			Log(ui_messages[7]);
		}

		if(args.Length > 0)
		{
			// Simple manual parsing
			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					case "--game":
						if (i + 1 < args.Length)
						{
							if (Enum.TryParse<Game>(args[++i], true, out var parsedGame))
							{
								var gameInstance = SpawnGameByID(parsedGame);
								CurrentGame = gameInstance;
								manually_selected_game = true;
								this.Text = "V3 UPS Manager" + " - " + CurrentGame.GameID.ToString();
							}
							else
							{
								//MessageBox.Show($"Unknown game: {args[i]}");
								manually_selected_game = false;
							}
						}
						break;
					case "--install-dir":
						installation_folder = args[++i];
						manually_selected_install = true;
						if(LoadInstallationFolder())
						{
							InstallationPathPreviewTextbox.Text = installation_folder;
							SelectInstallationFolderButton.Enabled = false;
						} else
						{
							verified_installation_folder = "";
							installation_folder = "";
							manually_selected_install = false;
						}
						break;
					case "--patch-folder":
						if(verified_installation_folder.Length > 0)
						{
							ups_folder = args[++i];
							manually_selected_patch = true;
							if (LoadPatchFolder())
							{
								PatchPathPreviewTextbox.Text = ups_folder;
								SelectPatchFolderButton.Enabled = false;
							} else
							{
								manually_selected_patch = false;
								ups_folder = "";
							}
						}
						break;
				}
			}
		}
	}

	private void InitLanguages()
	{
		var cur = Directory.GetCurrentDirectory();
		var locfolder = Path.Combine(cur, "Localization");

		var itafolder = Path.Combine(locfolder, "Italiano");
		var engfolder = Path.Combine(locfolder, "English");

		if (Directory.Exists(itafolder))
		{
			Directory.Delete(itafolder, true);
		}

		if (Directory.Exists(engfolder))
		{
			Directory.Delete(engfolder, true);
		}

		Directory.CreateDirectory(itafolder);
		Directory.CreateDirectory(engfolder);

		using (FileStream fs = new FileStream(Path.Combine(itafolder, "info.txt"), FileMode.OpenOrCreate, FileAccess.ReadWrite))
		{
			using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default))
			{
				foreach (string line in info_it)
				{
					sw.WriteLine(line);
				}
			}
		}

		using (FileStream fs = new FileStream(Path.Combine(itafolder, "ui.txt"), FileMode.OpenOrCreate, FileAccess.ReadWrite))
		{
			using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default))
			{
				foreach (string line in ui_messages_it)
				{
					sw.WriteLine(line);
				}
			}
		}

		using (FileStream fs = new FileStream(Path.Combine(engfolder, "info.txt"), FileMode.OpenOrCreate, FileAccess.ReadWrite))
		{
			using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default))
			{
				foreach (string line in info_en)
				{
					sw.WriteLine(line);
				}
			}
		}

		using (FileStream fs = new FileStream(Path.Combine(engfolder, "ui.txt"), FileMode.OpenOrCreate, FileAccess.ReadWrite))
		{
			using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default))
			{
				foreach (string line in ui_messages_en)
				{
					sw.WriteLine(line);
				}
			}
		}

		if (LanguageComboBox != null && LanguageComboBox.Items != null)
		{
			LanguageComboBox.Items.Add("Italiano");
			LanguageComboBox.Items.Add("English");
		}

		var dirs = Directory.GetDirectories(locfolder).ToList();
		foreach(var dir in dirs)
		{
			string fn = Path.GetFileName(dir);
			if (LanguageComboBox != null && LanguageComboBox.Items != null && !LanguageComboBox.Items.Contains(fn))
			{
				LanguageComboBox.Items.Add(fn);
			}
		}

		if (LanguageComboBox != null)
		{
			LanguageComboBox.Enabled = true;

			LanguageComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;

			LanguageComboBox.SelectedIndex = 1;
		}
	}

	private void SelectInstallationFolderButton_Click(object sender, EventArgs e)
	{
		LoadInstallationFolder();
	}

	private void SelectPatchFolderButton_Click(object sender, EventArgs e)
	{
		LoadPatchFolder();
	}

	private void InstallButton_Click(object sender, EventArgs e)
	{
		/* 
		 Wait... this makes backups AFTER installing?
		Oh my god, it actually does!
		Let's change that!
		(August 2023)
		*/

		BackupChanges(false);
		Log("", null, Verbosity.Debug, LogType.ConsoleOnly);
		Install();
		Log("", null, Verbosity.Debug, LogType.ConsoleOnly);
		BackupChanges(true);
		DisplayStatus();
	}

	private void UninstallButton_Click(object sender, EventArgs e)
	{
		Uninstall();
	}
}