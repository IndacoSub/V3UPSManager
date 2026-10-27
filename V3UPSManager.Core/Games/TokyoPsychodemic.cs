// Tokyo Psychodemic

namespace V3UPSManager;

public sealed class TokyoPsychodemic : GameBase
{
	public override Game GameID => Game.TokyoPsychodemic;

	public override string UnityDataFolder => "TOKYO_PSYCHODEMIC_Data";
	public override string UNITY_EXE_NAME => "TOKYO_PSYCHODEMIC.exe";

	public override bool UsesUnityPlatformFolder => false;

	public override string UnityExecutableHash =>
		"5786d34a424f5f731dd3751b7bd480f0";

	public override IReadOnlyList<string> UnityPatchExtensions { get; } =
		new[] { "", ".assets", ".dll" };

	public override IReadOnlyList<string> UninstallExtensions { get; } =
		new[] { ".assets", "*" };

	public override string GetUnityDataFolderPath(
		string installationFolder,
		string platformFolder)
	{
		return Path.Combine(
			installationFolder,
			UnityDataFolder,
			"StreamingAssets",
			"AssetBundles");
	}

	public override IReadOnlyList<string> FolderIdentifiers { get; } =
		new[]
		{
			"東京サイコデミック",
		};
}
