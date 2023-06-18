namespace V3UPSManager;

public partial class MainWindow : Form
{
    private void Uninstall()
    {
        // Check if the installation folder is not valid
        if (string.IsNullOrWhiteSpace(verified_installation_folder) || verified_installation_folder.Length <= 0 ||
            !Directory.Exists(verified_installation_folder))
        {
            DisplayInfo.Print(info[25]);
            return;
        }

        List<string> filelist = new List<string>();
        // Count and get all .spc backup files in the patch folder
        string[] files1 = Directory.GetFiles(verified_installation_folder, "*.spc_bak", SearchOption.AllDirectories);
        string[] files2 = Directory.GetFiles(verified_installation_folder, "*.SPC_bak", SearchOption.AllDirectories);
        if (files1 == null || files2 == null || files1.Length == 0 || files2.Length == 0)
        {
            DisplayInfo.Print(info[32]);
            return;
        }

        foreach (string file in files1)
        {
            filelist.Add(file);
        }

        foreach (string file in files2)
        {
            filelist.Add(file);
        }

        int count_uninstalled = 0;
        foreach (string file in filelist)
        {
            // .spc_bak
            string actual_file = file;
            // .spc
            string normal_file = actual_file.Substring(0, actual_file.Length - 4); // 4 is "_bak"'s length
            if (!File.Exists(normal_file) || string.IsNullOrWhiteSpace(normal_file))
            {
                continue;
            }

            if (!File.Exists(actual_file) || !File.Exists(file))
            {
                continue;
            }

            // Delete the "normal" file
            File.Delete(normal_file);

            // Copy the bak file to the "normal" file (a.k.a. restore backup)
            File.Move(actual_file, normal_file, true);

            // If for some reason it still exists even after being moved
            if (File.Exists(actual_file))
            {
                // Delete the .bak file
                File.Delete(actual_file);
            }

            //DisplayInfo.Print("Normal: " + normal_file + ", actual: " + actual_file + "...");
            count_uninstalled++;
        }

        if (count_uninstalled > 0)
        {
            DisplayInfo.Print(info[33]);
        }
        else
        {
            DisplayInfo.Print(info[32]);
        }
    }
}