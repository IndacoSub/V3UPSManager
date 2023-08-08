namespace V3UPSManager;

public partial class MainWindow : Form
{
	private string installation_folder = "";
	private string ups_folder = "";
	private string verified_installation_folder = "";

	public MainWindow()
	{
		InitializeComponent();
		LoadLanguages();
		info = info_it;
		LanguageComboBox.SelectedIndex = 1; // Default to Italian
		CheckIndexChange();
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