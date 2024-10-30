namespace V3UPSManager;

public partial class MainWindow : Form
{
	private List<string> info = new List<string>();
    private List<string> ui_messages = new List<string>();

	//
	// Actual localization strings/data moved to Localization.cs
	//

	private void LanguageComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		CheckIndexChange();
	}
}