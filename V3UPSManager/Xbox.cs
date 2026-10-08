namespace V3UPSManager;

public partial class MainWindow : Form
{
	private bool CheckXboxConfiguration()
	{
		if (!CurrentGame.SupportsXboxEdition)
		{
			return false;
		}

		// Only Danganronpa V3 currently has an Xbox/Anniversary validator.
		return CurrentGame.GameID == Game.DanganronpaV3 &&
			CheckDRV3XboxConfiguration();
	}

	// Xbox = Microsoft Store "Anniversary Edition" version
	private bool CheckDRV3XboxConfiguration()
	{
		string exe = Path.Combine(
			_installationFolder,
			CurrentGame.ANNIVERSARY_EXE_NAME);

		if (!File.Exists(exe))
		{
			Log(info[37], new Dictionary<string, string>()
			{
				{ "VAR_ANNIVERSARY_EXE_NAME", CurrentGame.ANNIVERSARY_EXE_NAME },
			});

			return false;
		}

		string windata = CurrentGame.GetXboxDataFolder(_installationFolder);

		if (!Directory.Exists(windata))
		{
			Log(info[4], null, Verbosity.Error);
			return false;
		}

		foreach (string archiveFile in CurrentGame.LegacyArchiveFiles)
		{
			if (File.Exists(Path.Combine(windata, archiveFile)))
			{
				Log(info[39], null, Verbosity.Error);
				return false;
			}
		}

		foreach (string archiveFile in CurrentGame.AnniversaryArchiveFiles)
		{
			if (File.Exists(Path.Combine(windata, archiveFile)))
			{
				Log(info[40], null, Verbosity.Error);
				return false;
			}
		}

		string reshadeIni = Path.Combine(
			_installationFolder,
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
		_canAccessExe = false;

		return true;
	}
}
