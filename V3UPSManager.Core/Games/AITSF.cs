// AI: The Somnium Files

namespace V3UPSManager;

public sealed class AITSF : GameBase
{
	public override Game GameID => Game.AITheSomniumFiles;

	public override FormatType PatchFormat => FormatType.XDelta;
	public override string PatchFormatExtension => ".xdelta";

	// XDelta(3) is licensed under GPLv2 or later GPL versions.
	// https://github.com/jmacd/xdelta-gpl/blob/release3_1/
	public override string PatchFormatInstaller => "xdelta3-3.1.0-x86_64";
	public override string PatchSpecificString => "_patch.xdelta";

	public override string UnityDataFolder => "AI_TheSomniumFiles_Data";
	public override string UNITY_EXE_NAME => "AI_TheSomniumFiles.exe";

	public override string UnityExecutableHash =>
		"fb4c224a824f9cc3665c9d1098612815";

	public override IReadOnlyList<string> UnityPatchExtensions { get; } =
		new[] { "", ".assets", ".exe" };

	public override IReadOnlyList<string> UninstallExtensions { get; } =
		new[] { ".assets", ".exe", "*" };

	public override IReadOnlyList<string> FolderIdentifiers { get; } =
		new[]
		{
			"AI The Somnium Files",
			"AI- THE SOMNIUM FILES/Content",
		};
}
