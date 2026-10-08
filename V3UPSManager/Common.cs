namespace V3UPSManager;

public partial class MainWindow : Form
{
	private bool CheckInstall(string dataFolder)
	{
		// Preserve the old default branch: an unrecognised game is never
		// considered a valid installation merely because the folder exists.
		if (CurrentGame.GameID == Game.None)
		{
			return false;
		}

		if (!DirectoryExistsOrLogError(dataFolder))
		{
			return false;
		}

		// Required directories are part of the game definition. This deliberately
		// lives on GameBase instead of in a Game -> string[] lookup here.
		foreach (string requiredDirectory in CurrentGame.RequiredDataDirectories)
		{
			string path = Path.Combine(dataFolder, requiredDirectory);

			if (Directory.Exists(path))
			{
				continue;
			}

			Log(GetMissingDirectoryMessage(requiredDirectory), null, Verbosity.Error);
			return false;
		}

		return true;
	}

	private bool DirectoryExistsOrLogError(string path)
	{
		if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
		{
			return true;
		}

		Log(info[25], null, Verbosity.Error);
		return false;
	}

	private string GetMissingDirectoryMessage(string directoryName)
	{
		// The directory names are game-owned data; the numeric message IDs remain
		// here because localization is a UI concern, not a game-definition concern.
		return directoryName switch
		{
			"boot" => info[11],
			"flash" => info[12],
			"game_resident" => info[13],
			"minigame" => info[14],
			"trial_font" => info[15],
			"wrd_data" => info[16],
			"wrd_script" => info[17],
			_ => info[25],
		};
	}
}
