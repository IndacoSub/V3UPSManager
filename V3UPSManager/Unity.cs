using System.Security.Cryptography;

namespace V3UPSManager;

public partial class MainWindow : Form
{
	// Allow any folder name(?)
    const bool dev_mode = false;

	private bool CheckUnityConfiguration()
	{
		switch (CurrentGame.GameID)
		{
			case Game.DanganronpaV3:
				return CheckDRV3UnityConfiguration();
			case Game.AITheSomniumFiles:
				return CheckAITSFUnityConfiguration();
			default:
				return false;
		}
	}

	private bool CheckAITSFUnityConfiguration()
	{
		// AI only has a Unity version

		// Check if Dangan3Win.exe exists
		string exe = Path.Combine(installation_folder, CurrentGame.UNITY_EXE_NAME);
		if (!File.Exists(exe))
		{
			// Maybe the user deleted it?
			Log(info[48], new Dictionary<string, string>(){
				{ "VAR_UNITY_EXE_NAME", CurrentGame.UNITY_EXE_NAME },
			});
			return false;
		}

		string unity_root = "AI_TheSomniumFiles_Data";

		string unity_find = Path.Combine(installation_folder, unity_root);

		if (!Directory.Exists(unity_find))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		// NOTE: Only for the Unity version
		// Check if the "StreamingAssets" folder exists
		unity_find = Path.Combine(unity_find, "StreamingAssets");
		if (!Directory.Exists(unity_find))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		string movie = Path.Combine(unity_find, "Movie");
		bool is_xbox = Directory.Exists(movie);

		unity_find = Path.Combine(unity_find, "AssetBundles");
		if (!Directory.Exists(unity_find))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		unity_find = Path.Combine(unity_find, "StandaloneWindows64");
		if (!Directory.Exists(unity_find))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		string luabytecode = Path.Combine(unity_find, "luabytecode");
		if(!File.Exists(luabytecode))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		string etc = Path.Combine(unity_find, "etc");
		if (!File.Exists(etc))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		string fonts = Path.Combine(unity_find, "fonts");
		if (!File.Exists(fonts))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		Log("XboxPath: " + movie, null, Verbosity.Debug, LogType.ConsoleOnly);
		Log("Xbox: " + is_xbox, null, Verbosity.Debug, LogType.ConsoleOnly);

		if (!is_xbox)
		{
			// Calculate MD5 hash of the executable
			string exe_md5 = "";
			using (var md5 = MD5.Create())
			{
				using (var stream = File.OpenRead(exe))
				{
					var hash = md5.ComputeHash(stream);
					if (hash != null)
					{
						exe_md5 = ToHex(hash, false);
					}
				}
			}

			// Check if the MD5 hash is valid
			if (exe_md5.Length == 0 || string.IsNullOrWhiteSpace(exe_md5))
			{
				Log(info[7], new Dictionary<string, string>(){
				{ "VAR_LEGACY_EXE_NAME", CurrentGame.LEGACY_EXE_NAME },
			});
				return false;
			}

			Log("EXE MD5: " + exe_md5.ToLower(), null, Verbosity.Debug, LogType.ConsoleOnly);

			// Check if the calculated hash corresponds to the expected hash
			// (as of June 2023, the latest game version is 1.01)
			const string expected_hash = "fb4c224a824f9cc3665c9d1098612815";
			if (exe_md5.ToLower() != expected_hash.ToLower()) // Added tolower to the hash just in case
			{
				var proceed = Log(info[8], null, Verbosity.Info, LogType.Ask);
				if (proceed == DialogResult.No || proceed == DialogResult.Cancel)
				{
					return false;
				}
			}
		} else
		{
			CanAccessExe = false;
		}

		// Check if ReShade ( https://reshade.me/ ) is present
		string reshade_ini = Path.Combine(installation_folder, "ReShade.ini");
		if (File.Exists(reshade_ini))
		{
			var proceed = Log(info[9], null, Verbosity.Info, LogType.Ask);
			if (proceed == DialogResult.No || proceed == DialogResult.Cancel)
			{
				return false;
			}
		}

		// Check if AISomniumFilesFix ( https://github.com/Lyall/AISomniumFilesFix ) is present
		string aifix = Path.Combine(installation_folder, "BepInEx");
		if (Directory.Exists(aifix))
		{
			var proceed = Log(info[10], new Dictionary<string, string>() {
				{ "VAR_GAME_FIX", "AISomniumFilesFix" },
			},
			Verbosity.Info, LogType.Ask);
			if (proceed == DialogResult.No || proceed == DialogResult.Cancel)
			{
				return false;
			}
		}

		// Check if SomniumDebugEnabler ( https://github.com/slavanomics/SomniumDebugEnabler ) is present
		string sde = Path.Combine(installation_folder, "Mods", "SomniumDebugEnabler.dll");
		if (File.Exists(sde))
		{
			var proceed = Log(info[10], new Dictionary<string, string>() {
				{ "VAR_GAME_FIX", "SomniumDebugEnabler" },
			},
			Verbosity.Info, LogType.Ask);
			if (proceed == DialogResult.No || proceed == DialogResult.Cancel)
			{
				return false;
			}
		}

		// Check if Special-K ( https://github.com/SpecialKO/SpecialK ) or Reloaded-II (https://github.com/Sewer56/CriFs.V2.Hook.ReloadedII) are present
		string dinput_ini = Path.Combine(installation_folder, "dinput8.ini");
		if (File.Exists(dinput_ini))
		{
			var proceed = Log(info[49], null, Verbosity.Info, LogType.Ask);
			if (proceed == DialogResult.No || proceed == DialogResult.Cancel)
			{
				return false;
			}
		}

		return true;
	}

    private bool CheckDRV3UnityConfiguration()
	{
		// We already established that it's not the Legacy (Steam) PC version, at this point

		List<string> supported_titleids = new List<string>();

		// For now, we only support the standalone version of V3, not the one in Decadence
		supported_titleids.Add("010063F014176000"); // Standalone V3 from Nintendo eShop (Europe)

		bool folder_contains_valid_titleid = supported_titleids.Any(installation_folder.Contains);

        if (!folder_contains_valid_titleid && !dev_mode)
		{
            // Xbox version?
            // The Xbox App automatically creates a folder called
			// "Danganronpa V3- Killing Harmony Anniversary Edition"
			// and cannot be renamed AFAIK
            if (!installation_folder.Contains("V3- Killing Harmony"))
			{
				// Unknown version: not PC, Xbox or Switch, so... mobile? PS4/Vita?
				Log(info[31], null, Verbosity.Error);
				return false;
			}
			else
			{
				// Xbox version... now what?
			}
		}
		else
		{
			// Switch (Unity) version
			// Assume TitleID (for now? until we support the Decadence version)
			TitleID = "010063F014176000";
		}

		Log(info[42]);

		string unity_root = "Data";

		string unity_find = Path.Combine(installation_folder, unity_root);

		// NOTE: Only for the Unity version
		// Check if the "Data" folder exists

		if (!Directory.Exists(unity_find))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		if (!DirExistsMatchCase(unity_find))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		// NOTE: Only for the Unity version
		// Check if the "StreamingAssets" folder exists
		unity_find = Path.Combine(unity_find, "StreamingAssets");
		if (!Directory.Exists(unity_find))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		// Ex. "Switch"
		string platform = GetUnityPlatformByExclusion(unity_find);
		unity_find = Path.Combine(unity_find, platform);
		if (!Directory.Exists(unity_find))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		// NOTE: Only for the Unity version
		// Check if the "all" folder exists
		unity_find = Path.Combine(unity_find, "all");
		if (!Directory.Exists(unity_find))
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		// Count .pb (???) files and .ab/.assets (Unity) files in the installation folder
		string[] files = Directory.GetFiles(installation_folder, "*.ab", SearchOption.AllDirectories);
		string[] files2 = Directory.GetFiles(installation_folder, "*.assets", SearchOption.AllDirectories);
		string[] files3 = Directory.GetFiles(installation_folder, "*.pb", SearchOption.AllDirectories);
		if (files == null || files2 == null || files3 == null)
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		if (files.Length <= 0 && files2.Length <= 0 && files3.Length <= 0)
		{
			Log(info[31], null, Verbosity.Error);
			return false;
		}

		return true;
	}

	private string GetUnityPlatformByExclusion(string path)
	{
		// The "StreamingAssets" folder contains four subfolders
		// The fourth subfolder is the one we're looking for
		// And is the name of the platform the game was compiled for

		string ret = "";
		// These are the three subfolders we don't care about
		List<string> excluded = new List<string>
		{
			"map",
			"master",
			"static"
		};
		// Look for literally any subfolder in StreamingAssets that's not excluded (^)
		// and return it (the first one found)
		string[] dirs = Directory.GetDirectories(path);
		foreach (string dir in dirs)
		{
			if (!excluded.Any(dir.Contains))
			{
				ret = dir;
				break;
			}
		}

		return ret;
	}
}