using System.Security.Cryptography;

namespace V3UPSManager;

public partial class Form1 : Form
{
    private bool CheckLegacyConfiguration()
    {
        // Check if Dangan3Win.exe exists
        string exe = Path.Combine(installation_folder, "Dangan3Win.exe");
        if (!File.Exists(exe))
        {
            // Maybe they deleted it?
            DisplayInfo.Print(info[1]);
            return false;
        }

        // Check if language.txt exists
        string config_language = Path.Combine(installation_folder, "language.txt");
        if (!File.Exists(config_language))
        {
            DisplayInfo.Print(info[2]);
            return false;
        }

        // Read language.txt
        string language = File.ReadAllText(config_language);
        if (string.IsNullOrWhiteSpace(language) || language != "US")
        {
            DisplayInfo.Print(info[3]);
            return false;
        }

        // Check if /data/win/ exists
        string windata = Path.Combine(installation_folder, "data");
        windata = Path.Combine(windata, "win");
        if (!Directory.Exists(windata))
        {
            DisplayInfo.Print(info[4]);
            return false;
        }

        // NOTE: Only for the Legacy PC version (Steam)
        // Check if the CPKs are in /data/win/
        string cpk_01_en = Path.Combine(windata, "partition_data_win.cpk");
        string cpk_02_en = Path.Combine(windata, "partition_data_win_us.cpk");
        string cpk_03_en = Path.Combine(windata, "partition_resident_win.cpk");
        bool cpk_01_en_exists = File.Exists(cpk_01_en);
        bool cpk_02_en_exists = File.Exists(cpk_02_en);
        bool cpk_03_en_exists = File.Exists(cpk_03_en);
        if (cpk_01_en_exists || cpk_02_en_exists || cpk_03_en_exists)
        {
            DisplayInfo.Print(info[5]);
            return false;
        }

        // Check if there are any .arc files from the Anniversary Edition
        string arc_01 = Path.Combine(windata, "partition_data_win_us.arc");
        string arc_02 = Path.Combine(windata, "partition_data_win.arc");
        string arc_03 = Path.Combine(windata, "partition_resident_win.arc");
        string arc_04 = Path.Combine(windata, "partition_data_win_jp.arc");
        string arc_05 = Path.Combine(windata, "partition_data_win_zh.arc");
        bool arc_01_exists = File.Exists(arc_01);
        bool arc_02_exists = File.Exists(arc_02);
        bool arc_03_exists = File.Exists(arc_03);
        bool arc_04_exists = File.Exists(arc_04);
        bool arc_05_exists = File.Exists(arc_05);
        if(arc_01_exists || arc_02_exists || arc_03_exists)
        {
            DisplayInfo.Print(info[38]);
            return false;
        }

        // NOTE: Only for the Legacy (Steam) PC version
        // Count the number of CPKs in /data/win/
        var count_lowercase = Directory.GetFiles(windata, "*.cpk", SearchOption.TopDirectoryOnly).Length;
        var count_uppercase = Directory.GetFiles(windata, "*.CPK", SearchOption.TopDirectoryOnly).Length;
        if (count_lowercase + count_uppercase > 0)
        {
            DisplayInfo.Print(info[6]);
            return false;
        }

        // Calculate MD5 hash of the executable
        string exe_md5 = "";
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(exe))
            {
                var hash = md5.ComputeHash(stream);
                if (hash != null)
                {
                    exe_md5 = ToHex(hash, false);
                }
            }
        }

        // Check if the MD5 hash is valid
        if (exe_md5.Length == 0 || string.IsNullOrWhiteSpace(exe_md5))
        {
            DisplayInfo.Print(info[7]);
            return false;
        }

        // Check if the calculated hash corresponds to the expected hash
        const string expected_hash = "ff2dd8163a9b2f5b018339fbee69f5ea";
        if (exe_md5.ToLower() != expected_hash)
        {
            var proceed = DisplayInfo.Ask(info[8]);
            if (proceed == DialogResult.No || proceed == DialogResult.Cancel)
            {
                return false;
            }
        }

        // Check if ReShade is present
        string reshade_ini = Path.Combine(installation_folder, "ReShade.ini");
        if (File.Exists(reshade_ini))
        {
            var proceed = DisplayInfo.Ask(info[9]);
            if (proceed == DialogResult.No || proceed == DialogResult.Cancel)
            {
                return false;
            }
        }

        // Check if DR3Fix is present
        string dr3fix_cfg = Path.Combine(installation_folder, "dr3fix.cfg");
        if (File.Exists(dr3fix_cfg))
        {
            var proceed = DisplayInfo.Ask(info[10]);
            if (proceed == DialogResult.No || proceed == DialogResult.Cancel)
            {
                return false;
            }
        }

        return true;
    }
}