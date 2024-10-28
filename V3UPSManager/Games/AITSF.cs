// AI: The Somnium Files

namespace V3UPSManager
{
	public class AITSF : GameBase
	{
		public override Game GameID { get; } = Game.AITheSomniumFiles;

		public override FormatType PatchFormat { get; } = FormatType.XDelta;

		public override string PatchFormatExtension { get; } = ".xdelta";

		// XDelta(3) is licensed under GPLv2 or later GPL versions
		// https://github.com/jmacd/xdelta-gpl/blob/release3_1/
		// https://github.com/jmacd/xdelta-gpl/blob/release3_1/xdelta3/xdelta3.h

		public override string PatchFormatInstaller { get; } = "xdelta3-3.1.0-x86_64";

		public override string PatchSpecificString { get; } = "_patch.xdelta";

		public override string UnityDataFolder { get; } = "AI_TheSomniumFiles_Data";

		public override string UNITY_EXE_NAME { get; } = "AI_TheSomniumFiles.exe";

		public override List<string> FolderIdentifiers { get; } = new List<string>()
		{
			"AI The Somnium Files",
			"AI- THE SOMNIUM FILES/Content"
		};
	}
}