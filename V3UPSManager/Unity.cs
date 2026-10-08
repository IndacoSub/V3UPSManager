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
			_installationFolder,
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
			_installationFolder,
			CurrentGame.UnityDataFolder);

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
			string exeMd5 = ComputeMd5(exe);

			if (string.IsNullOrWhiteSpace(exeMd5))
			{
				Log(info[7], new Dictionary<string, string>()
				{
					{ "VAR_LEGACY_EXE_NAME", CurrentGame.LEGACY_EXE_NAME },
				});

				return false;
			}

			if (!string.IsNullOrWhiteSpace(CurrentGame.UnityExecutableHash) &&
				!string.Equals(
					exeMd5,
					CurrentGame.UnityExecutableHash,
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
			_canAccessExe = false;
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

		string aiFix = Path.Combine(
			_installationFolder,
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
			_installationFolder,
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

	private bool CheckTokyoPsychodemicUnityConfiguration()
	{
		string exe = Path.Combine(
			_installationFolder,
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
			_installationFolder,
			CurrentGame.UnityDataFolder,
			"StreamingAssets",
			"AssetBundles");

		if (!Directory.Exists(unityFind))
		{
			Log(info[31], null, Verbosity.Error);
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

		if (!string.IsNullOrWhiteSpace(CurrentGame.UnityExecutableHash) &&
			!string.Equals(
				exeMd5,
				CurrentGame.UnityExecutableHash,
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

	private bool CheckDRV3UnityConfiguration()
	{
		IReadOnlyList<string> supportedTitleIds = CurrentGame.SupportedUnityTitleIds;

		string normalizedInstallationFolder;

		try
		{
			normalizedInstallationFolder = Path.GetFullPath(
			_installationFolder)
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
			_titleId = supportedTitleIds.FirstOrDefault() ?? string.Empty;
		}

		Log(info[42]);

		string unityFind = Path.Combine(
			_installationFolder,
			CurrentGame.UnityDataFolder);

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
			bool hasPatchableFiles = CurrentGame.UnityValidationExtensions.Any(
				extension => Directory.EnumerateFiles(
					_installationFolder,
					"*" + extension,
					SearchOption.AllDirectories).Any());

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
