// Danganronpa V3

namespace V3UPSManager;

public sealed class DRV3 : GameBase
{
	public override Game GameID => Game.DanganronpaV3;

	public override string LEGACY_EXE_NAME => "Dangan3Win.exe";
	public override string ANNIVERSARY_EXE_NAME => "Dangan3Desktop.exe";

	public override bool SupportsLegacyEdition => true;
	public override bool SupportsXboxEdition => true;

	public override string LegacyExecutableHash =>
		"ff2dd8163a9b2f5b018339fbee69f5ea";

	public override string LegacyLanguageFile => "language.txt";
	public override string LegacyLanguageValue => "US";

	public override IReadOnlyList<string> SupportedUnityTitleIds { get; } =
		new[]
		{
			"010063F014176000", // Standalone V3 from Nintendo eShop (Europe)
		};

	public override IReadOnlyList<string> RequiredDataDirectories { get; } =
		new[]
		{
			"boot",
			"flash",
			"game_resident",
			"minigame",
			"trial_font",
			"wrd_data",
			"wrd_script",
		};

	public override IReadOnlyList<string> LegacyPatchExtensions { get; } =
		new[] { ".spc", ".awb" };

	public override IReadOnlyList<string> UnityPatchExtensions { get; } =
		new[] { ".spc", ".awb", ".pb", ".ab", ".assets" };

	public override IReadOnlyList<string> UnityValidationExtensions { get; } =
		new[] { ".ab", ".assets", ".pb" };

	public override IReadOnlyList<string> UninstallExtensions { get; } =
		new[] { ".spc", ".awb" };

	public override IReadOnlyList<string> LegacyArchiveFiles { get; } =
		new[]
		{
			"partition_data_win.cpk",
			"partition_data_win_us.cpk",
			"partition_resident_win.cpk",
		};

	public override IReadOnlyList<string> AnniversaryArchiveFiles { get; } =
		new[]
		{
			"partition_data_win_us.arc",
			"partition_data_win.arc",
			"partition_resident_win.arc",
			"partition_data_win_jp.arc",
			"partition_data_win_zh.arc",
		};

	public override string GetLegacyDataFolder(string installationFolder)
		=> Path.Combine(installationFolder, "data", "win");

	public override string GetXboxDataFolder(string installationFolder)
		=> Path.Combine(installationFolder, "data", "WIN");

	public override string GetPatchRoot(
		string patchFolder,
		bool isUnity,
		bool isLegacy)
	{
		if (isUnity)
		{
			return base.GetPatchRoot(patchFolder, isUnity, isLegacy);
		}

		string winFolder = Path.Combine(
			patchFolder,
			"data",
			isLegacy ? "win" : "WIN");

		return Directory.Exists(winFolder) ? winFolder : string.Empty;
	}

	public override IReadOnlyList<string> FolderIdentifiers { get; } =
		new[]
		{
			"Danganronpa V3 Killing Harmony",
			"Danganronpa V3- Killing Harmony Anniversary Edition/Content",
			"010063F014176000",
		};

	public override bool SupportsBackupUninstall(bool isUnity) => !isUnity;
}
