namespace V3UPSManager;

public partial class MainWindow : Form
{

	public GameBase CurrentGame = new DRV3();

	private string installation_folder = "";
	private string ups_folder = "";
	private string verified_installation_folder = "";

	public MainWindow()
	{
		InitializeComponent();
		LanguageComboBox.SelectedIndexChanged -= LanguageComboBox_SelectedIndexChanged;
		LanguageComboBox.Enabled = false;
		LanguageComboBox.SelectedIndex = -1; // Default to Italian
		LoadLanguages();
		InitLanguages();
		CheckIndexChange();
	}

	private void InitLanguages()
	{
		var cur = Directory.GetCurrentDirectory();
		var locfolder = Path.Combine(cur, "Localization");

		if (!Directory.Exists("Localization"))
		{
			var itafolder = Path.Combine(locfolder, "Italiano");
			var engfolder = Path.Combine(locfolder, "English");

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

		}

		LanguageComboBox.Items.Add("Italiano");
		LanguageComboBox.Items.Add("English");

		var dirs = Directory.GetDirectories(locfolder).ToList();
		foreach(var dir in dirs)
		{
			string fn = Path.GetFileName(dir);
			if (!LanguageComboBox.Items.Contains(fn))
			{
				LanguageComboBox.Items.Add(fn);
			}
		}

		LanguageComboBox.Enabled = true;

		LanguageComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;

		LanguageComboBox.SelectedIndex = 0;
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
		Install();
		BackupChanges(true);
		DisplayStatus();
	}

	private void UninstallButton_Click(object sender, EventArgs e)
	{
		Uninstall();
	}
}