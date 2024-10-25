namespace V3UPSManager;

public partial class MainWindow : Form
{
	private bool CheckInstall(string data_folder)
	{
		switch (CurrentGame.GameID)
		{
			case Game.DanganronpaV3:
				return CheckInstallV3(data_folder);
			case Game.AITheSomniumFiles:
				// TODO: Implement
				return true;
			default:
				return false;
		}
	}

	private bool CheckInstallV3(string data_folder)
	{
		// Dome some checks about the folder where the .ups files will be installed

		if (data_folder == null || string.IsNullOrEmpty(data_folder) || !Directory.Exists(data_folder))
		{
			DisplayInfo.Print(info[25]);
			return false;
		}

		string check_boot = Path.Combine(data_folder, "boot");
		if (!Directory.Exists(check_boot))
		{
			DisplayInfo.Print(info[11]);
			return false;
		}

		string check_flash = Path.Combine(data_folder, "flash");
		if (!Directory.Exists(check_flash))
		{
			DisplayInfo.Print(info[12]);
			return false;
		}

		string check_game_resident = Path.Combine(data_folder, "game_resident");
		if (!Directory.Exists(check_game_resident))
		{
			DisplayInfo.Print(info[13]);
			return false;
		}

		string check_minigame = Path.Combine(data_folder, "minigame");
		if (!Directory.Exists(check_minigame))
		{
			DisplayInfo.Print(info[14]);
			return false;
		}

		string check_trial_font = Path.Combine(data_folder, "trial_font");
		if (!Directory.Exists(check_trial_font))
		{
			DisplayInfo.Print(info[15]);
			return false;
		}

		string check_wrd_data = Path.Combine(data_folder, "wrd_data");
		if (!Directory.Exists(check_wrd_data))
		{
			DisplayInfo.Print(info[16]);
			return false;
		}

		string check_wrd_script = Path.Combine(data_folder, "wrd_script");
		if (!Directory.Exists(check_wrd_script))
		{
			DisplayInfo.Print(info[17]);
			return false;
		}

		return true;
	}
}