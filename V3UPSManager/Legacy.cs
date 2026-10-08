using System.Security.Cryptography;

namespace V3UPSManager;

public partial class MainWindow : Form
{
	private bool CheckLegacyConfiguration()
	{
		switch (CurrentGame.GameID)
		{
			case Game.DanganronpaV3:
				return CheckDRV3LegacyConfiguration();

			case Game.AITheSomniumFiles:
			case Game.TokyoPsychodemic:
			default:
				// These titles currently have no Legacy configuration.
				return false;
		}
	}

	// Legacy = Steam version
	private bool CheckDRV3LegacyConfiguration()
	{
		string exe = Path.Combine(
			installation_folder,
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
			installation_folder,
			"language.txt");

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
			"US",
			StringComparison.OrdinalIgnoreCase))
		{
			Log(info[3]);
			return false;
		}

		string windata = Path.Combine(
			installation_folder,
			"data",
			"win");

		if (!Directory.Exists(windata))
		{
			Log(info[4], null, Verbosity.Error);
			return false;
		}

		string cpk1 = Path.Combine(
			windata,
			"partition_data_win.cpk");

		string cpk2 = Path.Combine(
			windata,
			"partition_data_win_us.cpk");

		string cpk3 = Path.Combine(
			windata,
			"partition_resident_win.cpk");

		if (File.Exists(cpk1) ||
			File.Exists(cpk2) ||
			File.Exists(cpk3))
		{
			Log(info[5], null, Verbosity.Error);
			return false;
		}

		// Any of the Anniversary ARC files means this is not a clean Legacy
		// installation. Check all known language variants, not only the first
		// three names.
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

		string exeMd5;

		try
		{
			using MD5 md5 = MD5.Create();
			using FileStream stream = File.OpenRead(exe);

			exeMd5 = ToHex(
				md5.ComputeHash(stream),
				false);
		}
		catch
		{
			Log(info[7], new Dictionary<string, string>()
			{
				{ "VAR_LEGACY_EXE_NAME", CurrentGame.LEGACY_EXE_NAME },
			});

			return false;
		}

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
		const string expectedHash = "ff2dd8163a9b2f5b018339fbee69f5ea";

		if (!string.Equals(
			exeMd5,
			expectedHash,
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

		string dr3FixConfig = Path.Combine(
			installation_folder,
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
			installation_folder,
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
