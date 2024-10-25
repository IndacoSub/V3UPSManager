// AI: The Somnium Files

namespace V3UPSManager
{
	public class AITSF : GameBase
	{
		public override Game GameID { get; } = Game.AITheSomniumFiles;

		public override FormatType PatchFormat { get; } = FormatType.XDelta;

		public override string PatchFormatExtension { get; } = ".xdelta";

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