// Danganronpa V3

namespace V3UPSManager
{
	public class DRV3 : GameBase
	{
		public override Game GameID { get; } = Game.DanganronpaV3;

		// The .exe name in the "Legacy" (Steam) edition
		public override string LEGACY_EXE_NAME { get; } = "Dangan3Win.exe";

		// The .exe name in the "Xbox" (Microsoft Store) Anniversary Edition
		public override string ANNIVERSARY_EXE_NAME { get; } = "Dangan3Desktop.exe";

		public override List<string> FolderIdentifiers { get; } = new List<string>()
		{
			"Danganronpa V3 Killing Harmony",
			"Danganronpa V3- Killing Harmony Anniversary Edition/Content",
			"010063F014176000",
		};
	}
}