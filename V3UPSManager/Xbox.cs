namespace V3UPSManager;

public partial class MainWindow : Form
{
	private bool CheckXboxConfiguration()
	{
		switch (CurrentGame.GameID)
		{
			case Game.DanganronpaV3:
				return CheckDRV3XboxConfiguration();

			case Game.AITheSomniumFiles:
			// The Steam and Xbox versions of AI are very similar (but not the same)
			// The main differences are the presence of the Movie folder inside StreamingAssets and PathID changes
			// But they are "similar enough" that we can get away with only checking once
			//[[fallthrough]]
			case Game.TokyoPsychodemic:
			default:
				return false;
		}
	}

	// Xbox = Microsoft Store "Anniversary Edition" version
	private bool CheckDRV3XboxConfiguration()
	{
		string exe = Path.Combine(
			installation_folder,
			CurrentGame.ANNIVERSARY_EXE_NAME);

		if (!File.Exists(exe))
		{
			Log(info[37], new Dictionary<string, string>()
			{
				{ "VAR_ANNIVERSARY_EXE_NAME", CurrentGame.ANNIVERSARY_EXE_NAME },
			});

			return false;
		}

		string windata = Path.Combine(
			installation_folder,
			"data",
			"WIN");

		if (!Directory.Exists(windata))
		{
			Log(info[4], null, Verbosity.Error);
			return false;
		}

		string[] legacyCpkFiles =
		{
			"partition_data_win.cpk",
			"partition_data_win_us.cpk",
			"partition_resident_win.cpk",
		};

		foreach (string cpkFile in legacyCpkFiles)
		{
			if (File.Exists(Path.Combine(windata, cpkFile)))
			{
				Log(info[39], null, Verbosity.Error);
				return false;
			}
		}

		string[] anniversaryArcFiles =
		{
			"partition_data_win_us.arc",
			"partition_data_win.arc",
			"partition_resident_win.arc",
			"partition_data_win_jp.arc",
			"partition_data_win_zh.arc",
		};

		foreach (string arcFile in anniversaryArcFiles)
		{
			if (File.Exists(Path.Combine(windata, arcFile)))
			{
				Log(info[40], null, Verbosity.Error);
				return false;
			}
		}

		string reshadeIni = Path.Combine(
			installation_folder,
			"ReShade.ini");

		if (File.Exists(reshadeIni))
		{
			DialogResult proceed = Log(
				info[9],
				null,
				Verbosity.Info,
				LogType.Ask);

			if (proceed == DialogResult.No ||
				proceed == DialogResult.Cancel)
			{
				return false;
			}
		}

		// Microsoft Store/Xbox installations do not expose the executable in
		// the same way as the Legacy edition, so executable patching is disabled.
		CanAccessExe = false;

		return true;
	}
}
