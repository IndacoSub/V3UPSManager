using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3UPSManager
{
    public partial class Form1 : Form
    {

        bool CheckUnityConfiguration()
        {

            List<string> supported_titleids = new List<string>();
            supported_titleids.Add("010063F014176000"); // Standalone V3 from Nintendo eShop (Europe)

            if (!supported_titleids.Any(installation_folder.Contains))
            {
                // UWP version?
                if(!installation_folder.Contains("SpikeChunsoftCo.Ltd"))
                {
                    // Unknown version
                    DisplayInfo.Print(info[31]);
                    return false;
                }
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

            // Count .ab/.assets (Unity) files in the installation folder
            string[] files = Directory.GetFiles(installation_folder, "*.ab", SearchOption.AllDirectories);
            string[] files2 = Directory.GetFiles(installation_folder, "*.assets", SearchOption.AllDirectories);
            if (files == null || files2 == null)
            {
                DisplayInfo.Print(info[31]);
                return false;
            }

            if (files.Length <= 0 && files2.Length <= 0)
            {
                DisplayInfo.Print(info[31]);
                return false;
            }

            return true;
        }

        string GetUnityPlatformByExclusion(string path)
        {
            string ret = "";
            List<string> excluded = new List<string> {
                "map",
                "master",
                "static"
            };
            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                if(!excluded.Any(dir.Contains))
                {
                    ret = dir;
                    break;
                }
            }
            return ret;
        }
    }
}
