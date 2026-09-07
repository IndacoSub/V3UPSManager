// AI: The Somnium Files

namespace V3UPSManager
{
	public class TokyoPsychodemic : GameBase
	{
		public override Game GameID { get; } = Game.TokyoPsychodemic;

		public override string UnityDataFolder { get; } = "TOKYO_PSYCHODEMIC_Data";

		public override string UNITY_EXE_NAME { get; } = "TOKYO_PSYCHODEMIC.exe";

		public override List<string> FolderIdentifiers { get; } = new List<string>()
		{
			"東京サイコデミック",
		};
	}
}