using System.Security.Cryptography;

namespace V3UPSManager;

public partial class MainWindow : Form
{
	// Reserved for development builds which intentionally allow unknown folder
	// layouts. Release builds keep this disabled.
	const bool dev_mode = false;

	private bool CheckUnityConfiguration()
	{
		switch (CurrentGame.GameID)
		{
			case Game.DanganronpaV3:
				return CheckDRV3UnityConfiguration();

			case Game.AITheSomniumFiles:
				return CheckAITSFUnityConfiguration();

			case Game.TokyoPsychodemic:
				return CheckTokyoPsychodemicUnityConfiguration();

			default:
				return false;
		}
	}

	private bool CheckAITSFUnityConfiguration()
	{
		string exe = Path.Combine(
			installation_folder,
			CurrentGame.UNITY_EXE_NAME);

		if (!File.Exists(exe))
		{
			Log(info[48], new Dictionary<string, string>()
			{
				{ "VAR_UNITY_EXE_NAME", CurrentGame.UNITY_EXE_NAME },
			});

			return false;
		}

		string unityFind = Path.Combine(
			installation_folder,
			"AI_TheSomniumFiles_Data");

		if (!Directory.Exists(unityFind))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		unityFind = Path.Combine(
			unityFind,
			"StreamingAssets");

		if (!Directory.Exists(unityFind))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		string movie = Path.Combine(
			unityFind,
			"Movie");

		bool isXbox = Directory.Exists(movie);

		unityFind = Path.Combine(
			unityFind,
			"AssetBundles");

		if (!Directory.Exists(unityFind))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		unityFind = Path.Combine(
			unityFind,
			"StandaloneWindows64");

		if (!Directory.Exists(unityFind))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		string luaBytecode = Path.Combine(
			unityFind,
			"luabytecode");

		if (!File.Exists(luaBytecode))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		string etc = Path.Combine(
			unityFind,
			"etc");

		if (!File.Exists(etc))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		string fonts = Path.Combine(
			unityFind,
			"fonts");

		if (!File.Exists(fonts))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		if (!isXbox)
		{
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

			const string expectedHash =
				"fb4c224a824f9cc3665c9d1098612815";

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
		}
		else
		{
			CanAccessExe = false;
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

		string aiFix = Path.Combine(
			installation_folder,
			"BepInEx");

		if (Directory.Exists(aiFix))
		{
			DialogResult proceed = Log(
				info[10],
				new Dictionary<string, string>()
				{
					{ "VAR_GAME_FIX", "AISomniumFilesFix" },
				},
				Verbosity.Info,
				LogType.Ask);

			if (proceed == DialogResult.No ||
				proceed == DialogResult.Cancel)
			{
				return false;
			}
		}

		string somniumDebugEnabler = Path.Combine(
			installation_folder,
			"Mods",
			"SomniumDebugEnabler.dll");

		if (File.Exists(somniumDebugEnabler))
		{
			DialogResult proceed = Log(
				info[10],
				new Dictionary<string, string>()
				{
					{ "VAR_GAME_FIX", "SomniumDebugEnabler" },
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

	private bool CheckTokyoPsychodemicUnityConfiguration()
	{
		string exe = Path.Combine(
			installation_folder,
			CurrentGame.UNITY_EXE_NAME);

		if (!File.Exists(exe))
		{
			Log(info[48], new Dictionary<string, string>()
			{
				{ "VAR_UNITY_EXE_NAME", CurrentGame.UNITY_EXE_NAME },
			});

			return false;
		}

		string unityFind = Path.Combine(
			installation_folder,
			CurrentGame.UnityDataFolder,
			"StreamingAssets",
			"AssetBundles");

		if (!Directory.Exists(unityFind))
		{
			Log(info[31], null, Verbosity.Error);
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

		const string expectedHash =
			"5786d34a424f5f731dd3751b7bd480f0";

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

	private bool CheckDRV3UnityConfiguration()
	{
		List<string> supportedTitleIds = new List<string>
		{
			"010063F014176000", // Standalone V3 from Nintendo eShop (Europe)
		};

		string normalizedInstallationFolder;

		try
		{
			normalizedInstallationFolder = Path.GetFullPath(
			installation_folder)
			.TrimEnd(
				Path.DirectorySeparatorChar,
				Path.AltDirectorySeparatorChar)
			.Replace('\\', '/');
		}
		catch
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		bool folderContainsValidTitleId = supportedTitleIds.Any(
			titleId =>
				normalizedInstallationFolder.Equals(
					titleId,
					StringComparison.OrdinalIgnoreCase) ||
				normalizedInstallationFolder.EndsWith(
					"/" + titleId,
					StringComparison.OrdinalIgnoreCase));

		if (!folderContainsValidTitleId && !dev_mode)
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}
		else if (folderContainsValidTitleId)
		{
			TitleID = "010063F014176000"; // Assume standalone V3 from Nintendo eShop (Europe)
		}

		Log(info[42]);

		string unityFind = Path.Combine(
			installation_folder,
			"Data");

		if (!Directory.Exists(unityFind))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		if (!DirExistsMatchCase(unityFind))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		unityFind = Path.Combine(
			unityFind,
			"StreamingAssets");

		if (!Directory.Exists(unityFind))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		string platform = GetUnityPlatformByExclusion(unityFind);

		if (string.IsNullOrWhiteSpace(platform))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		unityFind = Path.Combine(
			unityFind,
			platform);

		if (!Directory.Exists(unityFind))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		unityFind = Path.Combine(
			unityFind,
			"all");

		if (!Directory.Exists(unityFind))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		try
		{
			bool hasPatchableFiles =
				Directory.EnumerateFiles(
					installation_folder,
					"*.ab",
					SearchOption.AllDirectories).Any() ||
				Directory.EnumerateFiles(
					installation_folder,
					"*.assets",
					SearchOption.AllDirectories).Any() ||
				Directory.EnumerateFiles(
					installation_folder,
					"*.pb",
					SearchOption.AllDirectories).Any();

			if (!hasPatchableFiles)
			{
				Log(info[31], null, Verbosity.Error);
				return false;
			}
		}
		catch
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		return true;
	}

	private string GetUnityPlatformByExclusion(string path)
	{
		if (string.IsNullOrWhiteSpace(path) ||
			!Directory.Exists(path))
		{
			return string.Empty;
		}

		// These folders are known non-platform folders. Compare the immediate
		// directory name rather than searching the whole path for a substring.
		HashSet<string> excluded = new HashSet<string>(
			StringComparer.OrdinalIgnoreCase)
		{
			"map",
			"master",
			"static",
		};

		try
		{
			foreach (string directory in Directory.EnumerateDirectories(
				path,
				"*",
				SearchOption.TopDirectoryOnly))
			{
				string name = Path.GetFileName(
					directory.TrimEnd(
						Path.DirectorySeparatorChar,
						Path.AltDirectorySeparatorChar));

				if (!excluded.Contains(name))
				{
					return name;
				}
			}
		}
		catch
		{
			return string.Empty;
		}

		return string.Empty;
	}
}
