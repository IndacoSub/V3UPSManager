namespace V3UPSManager;

public partial class MainWindow : Form
{
    private bool CheckUnityConfiguration()
    {
        List<string> supported_titleids = new List<string>();
        supported_titleids.Add("010063F014176000"); // Standalone V3 from Nintendo eShop (Europe)

        if (!supported_titleids.Any(installation_folder.Contains))
        {
            // Xbox version?
            if (!installation_folder.Contains("V3- Killing Harmony"))
            {
                // Unknown version
                DisplayInfo.Print(info[31]);
                return false;
            }
            else
            {
                // Mobile?
            }
        }
        else
        {
            // Assume titleid (for now)
            TitleID = "010063F014176000";
        }

        string unity_root = "Data";

        string unity_find = Path.Combine(installation_folder, unity_root);

        // NOTE: Only for the Unity version
        // Check if the "Data" folder exists

        if (!Directory.Exists(unity_find))
        {
            DisplayInfo.Print(info[31]);
            return false;
        }

        if (!DirExistsMatchCase(unity_find))
        {
            DisplayInfo.Print(info[31]);
            return false;
        }

        // NOTE: Only for the Unity version
        // Check if the "StreamingAssets" folder exists
        unity_find = Path.Combine(unity_find, "StreamingAssets");
        if (!Directory.Exists(unity_find))
        {
            DisplayInfo.Print(info[31]);
            return false;
        }

        string platform = GetUnityPlatformByExclusion(unity_find);
        unity_find = Path.Combine(unity_find, platform);
        if (!Directory.Exists(unity_find))
        {
            DisplayInfo.Print(info[31]);
            return false;
        }

        // NOTE: Only for the Unity version
        // Check if the "all" folder exists
        unity_find = Path.Combine(unity_find, "all");
        if (!Directory.Exists(unity_find))
        {
            DisplayInfo.Print(info[31]);
            return false;
        }

        // Count .pb (???) files and .ab/.assets (Unity) files in the installation folder
        string[] files = Directory.GetFiles(installation_folder, "*.ab", SearchOption.AllDirectories);
        string[] files2 = Directory.GetFiles(installation_folder, "*.assets", SearchOption.AllDirectories);
        string[] files3 = Directory.GetFiles(installation_folder, "*.pb", SearchOption.AllDirectories);
        if (files == null || files2 == null || files3 == null)
        {
            DisplayInfo.Print(info[31]);
            return false;
        }

        if (files.Length <= 0 && files2.Length <= 0 && files3.Length <= 0)
        {
            DisplayInfo.Print(info[31]);
            return false;
        }

        return true;
    }

    private string GetUnityPlatformByExclusion(string path)
    {
        string ret = "";
        List<string> excluded = new List<string>
        {
            "map",
            "master",
            "static"
        };
        string[] dirs = Directory.GetDirectories(path);
        foreach (string dir in dirs)
        {
            if (!excluded.Any(dir.Contains))
            {
                ret = dir;
                break;
            }
        }

        return ret;
    }
}