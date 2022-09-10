namespace V3UPSManager;

public partial class Form1 : Form
{
    private string installation_folder;
    private string ups_folder;
    private string verified_installation_folder;

    public Form1()
    {
        InitializeComponent();
        LoadLanguages();
        info = new string[info_it.Length];
        comboBox1.SelectedIndex = 1; // Default to Italian
        CheckIndexChange();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        LoadInstallationFolder();
    }

    private void button2_Click(object sender, EventArgs e)
    {
        LoadPatchFolder();
    }

    private void button3_Click(object sender, EventArgs e)
    {
        Install();
        BackupChanges();
        DisplayStatus();
    }

    private void button4_Click(object sender, EventArgs e)
    {
        Uninstall();
    }
}