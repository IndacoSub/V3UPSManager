namespace V3UPSManager;

public partial class MainWindow : Form
{
	private bool CheckLegacyConfiguration()
	{
		if (!CurrentGame.SupportsLegacyEdition)
		{
			return false;
		}

		// Only Danganronpa V3 currently implements a Legacy-specific validator.
		return CurrentGame.GameID == Game.DanganronpaV3 &&
			CheckDRV3LegacyConfiguration();
	}

	// Legacy = Steam version
	private bool CheckDRV3LegacyConfiguration()
	{
		string exe = Path.Combine(
			_installationFolder,
			CurrentGame.LEGACY_EXE_NAME);

		if (!File.Exists(exe))
		{
			Log(info[1], new Dictionary<string, string>()
			{
				{ "VAR_LEGACY_EXE_NAME", CurrentGame.LEGACY_EXE_NAME },
			});

			return false;
		}

		string configLanguage = Path.Combine(
			_installationFolder,
			CurrentGame.LegacyLanguageFile);

		if (!File.Exists(configLanguage))
		{
			Log(info[2]);
			return false;
		}

		string language = File.ReadAllText(configLanguage);

		// language.txt may contain a trailing newline or whitespace depending
		// on how it was created. Trim it before validating the language code.
		if (!string.Equals(
			language.Trim(),
			CurrentGame.LegacyLanguageValue,
			StringComparison.OrdinalIgnoreCase))
		{
			Log(info[3]);
			return false;
		}

		string windata = CurrentGame.GetLegacyDataFolder(_installationFolder);

		if (!Directory.Exists(windata))
		{
			Log(info[4], null, Verbosity.Error);
			return false;
		}

		foreach (string archiveFile in CurrentGame.LegacyArchiveFiles)
		{
			if (File.Exists(Path.Combine(windata, archiveFile)))
			{
				Log(info[5], null, Verbosity.Error);
				return false;
			}
		}

		// Any Anniversary ARC file means this is not a clean Legacy installation.
		foreach (string archiveFile in CurrentGame.AnniversaryArchiveFiles)
		{
			if (File.Exists(Path.Combine(windata, archiveFile)))
			{
				Log(info[38]);
				return false;
			}
		}

		try
		{
			int cpkCount = Directory.EnumerateFiles(
				windata,
				"*.cpk",
				SearchOption.TopDirectoryOnly)
				.Count();

			if (cpkCount > 0)
			{
				Log(info[6], null, Verbosity.Error);
				return false;
			}
		}
		catch
		{
			Log(info[4], null, Verbosity.Error);
			return false;
		}

		string exeMd5 = ComputeMd5(exe);

		if (string.IsNullOrWhiteSpace(exeMd5))
		{
			Log(info[7], new Dictionary<string, string>()
			{
				{ "VAR_LEGACY_EXE_NAME", CurrentGame.LEGACY_EXE_NAME },
			});

			return false;
		}

		// This hash is tied to the supported game build. Do not treat a hash
		// mismatch as a hard failure because a legitimate newer build may exist.
		if (!string.IsNullOrWhiteSpace(CurrentGame.LegacyExecutableHash) &&
			!string.Equals(
				exeMd5,
				CurrentGame.LegacyExecutableHash,
			StringComparison.OrdinalIgnoreCase))
		{
			DialogResult proceed = Log(
				info[8],
				null,
				Verbosity.Info,
				LogType.Ask);

			if (proceed == DialogResult.No ||
				proceed == DialogResult.Cancel)
			{
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

		string dr3FixConfig = Path.Combine(
			_installationFolder,
			"dr3fix.cfg");

		if (File.Exists(dr3FixConfig))
		{
			DialogResult proceed = Log(
				info[10],
				new Dictionary<string, string>()
				{
					{ "VAR_GAME_FIX", "DR3Fix" },
				},
				Verbosity.Info,
				LogType.Ask);

			if (proceed == DialogResult.No ||
				proceed == DialogResult.Cancel)
			{
				return false;
			}
		}

		string dinputIni = Path.Combine(
			_installationFolder,
			"dinput8.ini");

		if (File.Exists(dinputIni))
		{
			DialogResult proceed = Log(
				info[49],
				null,
				Verbosity.Info,
				LogType.Ask);

			if (proceed == DialogResult.No ||
				proceed == DialogResult.Cancel)
			{
				return false;
			}
		}

		return true;
	}
}
