namespace V3UPSManager;

public partial class MainWindow : Form
{
	private List<string> info = new List<string>();

    // The .exe name in the "Legacy" (Steam) edition
    private const string LEGACY_EXE_NAME = "Dangan3Win.exe";

	// The .exe name in the "Xbox" (Microsoft Store) Anniversary Edition
	private const string ANNIVERSARY_EXE_NAME = "Dangan3Desktop.exe";

    // Patch-file-specific string
    // AKA: All .ups patch files have "_patch" in common, or any custom string really (as long as they actually contain it in their name)
    private const string PatchSpecificString = "_patch.ups";

    //
    // Actual localization strings/data moved to Localization.cs
    //

    private void LanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		CheckIndexChange();
	}
}