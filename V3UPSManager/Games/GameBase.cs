// Game base

namespace V3UPSManager
{
	public enum Game
	{
		None,
		DanganronpaV3,
		AITheSomniumFiles,
	}

	public enum FormatType
	{
		UPS,
		XDelta,
	}

	public abstract class GameBase
	{
		public virtual Game GameID { get; } = Game.None;

		// PATCH
		public virtual FormatType PatchFormat { get; } = FormatType.UPS;
		public virtual string PatchFormatExtension { get; } = ".ups";
		public virtual string PatchFormatInstaller { get; } = "ups";

		// UPS Tools is licensed under the MIT license
		// https://github.com/rameshvarun/ups
		// https://github.com/rameshvarun/ups/LICENSE

		// Patch-file-specific string
		// AKA: All .ups patch files have "_patch" in common, or any custom string really (as long as they actually contain it in their name)
		public virtual string PatchSpecificString { get; } = "_patch.ups";

		// UNITY-SPECIFIC
		public virtual string UnityDataFolder { get; } = "Data";

		// EXES
		public virtual string LEGACY_EXE_NAME { get; } = "DefaultLegacyExe.exe";
		public virtual string ANNIVERSARY_EXE_NAME { get; } = "DefaultAnniversaryExe.exe";
		public virtual string UNITY_EXE_NAME { get; } = "DefaultUnityExe.exe";

		public virtual List<string> FolderIdentifiers { get; } = new List<string>();
	}

	public class NoneGame : GameBase
	{

	};

	public partial class MainWindow : Form
	{
		public static Game GetGameByFolder(string folder)
		{
			GameBase DRV3 = new DRV3();
			GameBase AITSF = new AITSF();

			// List of all games (outside of None)
			List<GameBase> games = new List<GameBase>()
			{
				DRV3,
				AITSF,
			};

			foreach (GameBase game in games)
			{
				List<string> folders = game.FolderIdentifiers;
				if(folders.Any(x => folder.Replace("\\", "/").EndsWith(x.Replace("\\", "/"))))
				{
					return game.GameID;
				}
			}

			return Game.None;
		}

		public GameBase SpawnGameByID(Game currentGameID)
		{
			switch (currentGameID)
			{
				case Game.DanganronpaV3:
					return new DRV3();
				case Game.AITheSomniumFiles:
					return new AITSF();
				default:
					break;
			}
			return new NoneGame();
		}
	}
}