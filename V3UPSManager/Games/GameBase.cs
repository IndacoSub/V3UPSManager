namespace V3UPSManager;

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

/// <summary>
/// Describes everything that is intrinsic to a supported game.
///
/// The UI and the installer contain the workflow; the concrete game classes
/// own the values and filesystem rules that differ from one title to another.
/// This keeps additions to the game list local to Games/*.cs instead of
/// scattering another switch/dictionary through the application.
/// </summary>
public abstract class GameBase
{
	public virtual Game GameID => Game.None;

	// Patch format -----------------------------------------------------------
	public virtual FormatType PatchFormat => FormatType.UPS;
	public virtual string PatchFormatExtension => ".ups";
	public virtual string PatchFormatInstaller => "ups";
	public virtual string PatchSpecificString => "_patch.ups";

	// Unity ------------------------------------------------------------------
	public virtual string UnityDataFolder => "Data";
	public virtual bool UsesUnityPlatformFolder => true;


	// Executables ------------------------------------------------------------
	public virtual string LEGACY_EXE_NAME => "DefaultLegacyExe.exe";
	public virtual string ANNIVERSARY_EXE_NAME => "DefaultAnniversaryExe.exe";
	public virtual string UNITY_EXE_NAME => "DefaultUnityExe.exe";

	// Installation detection ------------------------------------------------
	public virtual IReadOnlyList<string> FolderIdentifiers => Array.Empty<string>();
	public virtual IReadOnlyList<string> RequiredDataDirectories => Array.Empty<string>();
	public virtual bool SupportsLegacyEdition => false;
	public virtual bool SupportsXboxEdition => false;

	// Patch / uninstall targets ---------------------------------------------
	/// <summary>
	/// File extensions searched for normal (non-Unity) installations.
	/// An empty string means files without an extension.
	/// </summary>
	public virtual IReadOnlyList<string> LegacyPatchExtensions => Array.Empty<string>();

	/// <summary>
	/// File extensions searched for Unity installations.
	/// An empty string means files without an extension.
	/// </summary>
	public virtual IReadOnlyList<string> UnityPatchExtensions => Array.Empty<string>();
	public virtual IReadOnlyList<string> UnityValidationExtensions => UnityPatchExtensions;

	/// <summary>
	/// Backup suffix targets restored by Uninstall(). The wildcard entry "*"
	/// means every *_bak file, matching the historical uninstaller behaviour.
	/// </summary>
	public virtual IReadOnlyList<string> UninstallExtensions => Array.Empty<string>();

	/// <summary>
	/// Returns the patch target extensions for the currently detected edition.
	/// Executables are filtered here because executable access is a runtime
	/// capability of the selected installation, not part of the GUI workflow.
	/// </summary>
	public virtual IReadOnlyList<string> GetPatchTargetExtensions(
		bool isUnity,
		bool canAccessExe)
	{
		IReadOnlyList<string> extensions = isUnity
			? UnityPatchExtensions
			: LegacyPatchExtensions;

		if (canAccessExe)
		{
			return extensions;
		}

		return extensions
			.Where(extension => !string.Equals(
				extension,
				".exe",
				StringComparison.OrdinalIgnoreCase))
			.ToArray();
	}

	// Edition-specific filesystem layout -----------------------------------
	public virtual string GetLegacyDataFolder(string installationFolder)
		=> installationFolder;

	public virtual string GetXboxDataFolder(string installationFolder)
		=> installationFolder;

	/// <summary>
	/// Resolves the directory that contains patchable files for a Unity install.
	/// Most games use Data/StreamingAssets/[platform], while titles with a
	/// different layout can override this in their own Games/*.cs file.
	/// </summary>
	public virtual string GetUnityDataFolderPath(
		string installationFolder,
		string platformFolder)
	{
		string streamingAssets = Path.Combine(
			installationFolder,
			UnityDataFolder,
			"StreamingAssets");

		return string.IsNullOrWhiteSpace(platformFolder)
			? streamingAssets
			: Path.Combine(streamingAssets, platformFolder);
	}

	/// <summary>
	/// Resolves the root of a patch package. By default Unity packages use the
	/// game's Data/StreamingAssets tree, while non-Unity packages live at root.
	/// Games with a different non-Unity package layout override this method.
	/// </summary>
	public virtual string GetPatchRoot(
		string patchFolder,
		bool isUnity,
		bool isLegacy)
	{
		if (!isUnity)
		{
			return patchFolder;
		}

		string unityStreamingAssets = Path.Combine(
			patchFolder,
			UnityDataFolder,
			"StreamingAssets");

		return Directory.Exists(unityStreamingAssets)
			? patchFolder
			: string.Empty;
	}

	// Optional validation metadata -----------------------------------------
	public virtual string? LegacyExecutableHash => null;
	public virtual string LegacyLanguageFile => string.Empty;
	public virtual string LegacyLanguageValue => string.Empty;
	public virtual string? UnityExecutableHash => null;
	public virtual IReadOnlyList<string> SupportedUnityTitleIds => Array.Empty<string>();

	// Danganronpa-style archive checks are optional and therefore empty by
	// default. They remain game-owned data rather than global dictionaries.
	public virtual IReadOnlyList<string> LegacyArchiveFiles => Array.Empty<string>();
	public virtual IReadOnlyList<string> AnniversaryArchiveFiles => Array.Empty<string>();

	/// <summary>
	/// Some games cannot use the backup-based uninstaller for Unity editions.
	/// Other games retain the historical backup restore behaviour.
	/// </summary>
	public virtual bool SupportsBackupUninstall(bool isUnity) => true;
}

public sealed class NoneGame : GameBase
{
}

public partial class MainWindow : Form
{
	private static readonly IReadOnlyDictionary<Game, Func<GameBase>> GameFactories =
		new Dictionary<Game, Func<GameBase>>
		{
			[Game.DanganronpaV3] = static () => new DRV3(),
			[Game.AITheSomniumFiles] = static () => new AITSF(),
			[Game.TokyoPsychodemic] = static () => new TokyoPsychodemic(),
		};

	private static readonly GameBase[] SupportedGames =
		GameFactories.Values.Select(factory => factory()).ToArray();

	public static Game GetGameByFolder(string folder)
	{
		if (string.IsNullOrWhiteSpace(folder))
		{
			return Game.None;
		}

		string normalizedFolder;
		try
		{
			normalizedFolder = NormalizePath(folder);
		}
		catch
		{
			return Game.None;
		}

		if (normalizedFolder.Length == 0)
		{
			return Game.None;
		}

		foreach (GameBase game in SupportedGames)
		{
			foreach (string identifier in game.FolderIdentifiers)
			{
				if (string.IsNullOrWhiteSpace(identifier))
				{
					continue;
				}

				string normalizedIdentifier = NormalizePathSegment(identifier);

				// A segment boundary is required so a folder such as
				// "My Danganronpa V3" cannot accidentally match a title.
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
		return GameFactories.TryGetValue(
			currentGameID,
			out Func<GameBase>? factory)
			? factory()
			: new NoneGame();
	}

	private static string NormalizePath(string path)
	{
		return Path.GetFullPath(path)
			.TrimEnd(
				Path.DirectorySeparatorChar,
				Path.AltDirectorySeparatorChar)
			.Replace('\\', '/');
	}

	private static string NormalizePathSegment(string path)
	{
		return path.Trim().Trim('/').Replace('\\', '/');
	}
}
