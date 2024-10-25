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
				return true;
			default:
				return false;
		}
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
				DisplayInfo.Print(info[31]);
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

		DisplayInfo.Print(info[42]);

		string unity_root = "Data";

		string unity_find = Path.Combine(installation_folder, unity_root);

		// NOTE: Only for the Unity version
		// Check if the "Data" folder exists

		if (!Directory.Exists(unity_find))
		{
			DisplayInfo.Print(info[31]);
			return false;
		}

		if (!DirExistsMatchCase(unity_find))
		{
			DisplayInfo.Print(info[31]);
			return false;
		}

		// NOTE: Only for the Unity version
		// Check if the "StreamingAssets" folder exists
		unity_find = Path.Combine(unity_find, "StreamingAssets");
		if (!Directory.Exists(unity_find))
		{
			DisplayInfo.Print(info[31]);
			return false;
		}

		// Ex. "Switch"
		string platform = GetUnityPlatformByExclusion(unity_find);
		unity_find = Path.Combine(unity_find, platform);
		if (!Directory.Exists(unity_find))
		{
			DisplayInfo.Print(info[31]);
			return false;
		}

		// NOTE: Only for the Unity version
		// Check if the "all" folder exists
		unity_find = Path.Combine(unity_find, "all");
		if (!Directory.Exists(unity_find))
		{
			DisplayInfo.Print(info[31]);
			return false;
		}

		// Count .pb (???) files and .ab/.assets (Unity) files in the installation folder
		string[] files = Directory.GetFiles(installation_folder, "*.ab", SearchOption.AllDirectories);
		string[] files2 = Directory.GetFiles(installation_folder, "*.assets", SearchOption.AllDirectories);
		string[] files3 = Directory.GetFiles(installation_folder, "*.pb", SearchOption.AllDirectories);
		if (files == null || files2 == null || files3 == null)
		{
			DisplayInfo.Print(info[31]);
			return false;
		}

		if (files.Length <= 0 && files2.Length <= 0 && files3.Length <= 0)
		{
			DisplayInfo.Print(info[31]);
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