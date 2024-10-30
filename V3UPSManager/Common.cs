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
			Log(info[25], null, Verbosity.Error);
			return false;
		}

		string check_boot = Path.Combine(data_folder, "boot");
		if (!Directory.Exists(check_boot))
		{
			Log(info[11], null, Verbosity.Error);
			return false;
		}

		string check_flash = Path.Combine(data_folder, "flash");
		if (!Directory.Exists(check_flash))
		{
			Log(info[12], null, Verbosity.Error);
			return false;
		}

		string check_game_resident = Path.Combine(data_folder, "game_resident");
		if (!Directory.Exists(check_game_resident))
		{
			Log(info[13], null, Verbosity.Error);
			return false;
		}

		string check_minigame = Path.Combine(data_folder, "minigame");
		if (!Directory.Exists(check_minigame))
		{
			Log(info[14], null, Verbosity.Error);
			return false;
		}

		string check_trial_font = Path.Combine(data_folder, "trial_font");
		if (!Directory.Exists(check_trial_font))
		{
			Log(info[15], null, Verbosity.Error);
			return false;
		}

		string check_wrd_data = Path.Combine(data_folder, "wrd_data");
		if (!Directory.Exists(check_wrd_data))
		{
			Log(info[16], null, Verbosity.Error);
			return false;
		}

		string check_wrd_script = Path.Combine(data_folder, "wrd_script");
		if (!Directory.Exists(check_wrd_script))
		{
			Log(info[17], null, Verbosity.Error);
			return false;
		}

		return true;
	}
}