// Game base

namespace V3UPSManager
{
	public enum Game
	{
		None,
		DanganronpaV3,
		AITheSomniumFiles,
		TokyoPsychodemic,
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
		public virtual string PatchSpecificString { get; } = "_patch.ups";

		// UNITY-SPECIFIC
		public virtual string UnityDataFolder { get; } = "Data";

		// EXES
		public virtual string LEGACY_EXE_NAME { get; } = "DefaultLegacyExe.exe";
		public virtual string ANNIVERSARY_EXE_NAME { get; } = "DefaultAnniversaryExe.exe";
		public virtual string UNITY_EXE_NAME { get; } = "DefaultUnityExe.exe";

		public virtual List<string> FolderIdentifiers { get; } = new List<string>();

		public virtual string GetUnityDataFolder()
		{
			return UnityDataFolder;
		}
	}

	public class NoneGame : GameBase
	{
	};

	public partial class MainWindow : Form
	{
		public static Game GetGameByFolder(string folder)
		{
			if (string.IsNullOrWhiteSpace(folder))
			{
				return Game.None;
			}

			GameBase[] games =
			{
				new DRV3(),
				new AITSF(),
				new TokyoPsychodemic(),
			};

			string normalizedFolder;

			try
			{
				normalizedFolder = Path.GetFullPath(folder)
					.TrimEnd(
						Path.DirectorySeparatorChar,
						Path.AltDirectorySeparatorChar)
					.Replace('\\', '/');
			}
			catch
			{
				return Game.None;
			}

			if (normalizedFolder.Length == 0)
			{
				return Game.None;
			}

			foreach (GameBase game in games)
			{
				foreach (string identifier in game.FolderIdentifiers)
				{
					if (string.IsNullOrWhiteSpace(identifier))
					{
						continue;
					}

					string normalizedIdentifier = identifier
						.Trim()
						.Trim('/')
						.Replace('\\', '/');

					// Require either an exact path match or a path-segment
					// boundary before the identifier. This avoids false positives
					// from unrelated folders whose names merely end the same way.
					if (string.Equals(
						normalizedFolder,
						normalizedIdentifier,
						StringComparison.OrdinalIgnoreCase) ||
						normalizedFolder.EndsWith(
							"/" + normalizedIdentifier,
							StringComparison.OrdinalIgnoreCase))
					{
						return game.GameID;
					}
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
				case Game.TokyoPsychodemic:
					return new TokyoPsychodemic();
				default:
					return new NoneGame();
			}
		}
	}
}
