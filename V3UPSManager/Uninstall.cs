namespace V3UPSManager;

public partial class MainWindow : Form
{
	private void Uninstall()
	{
		// Check if the installation folder is valid or not
		if (string.IsNullOrWhiteSpace(verified_installation_folder) || verified_installation_folder.Length <= 0 ||
			!Directory.Exists(verified_installation_folder))
		{
			Log(info[25], null, Verbosity.Error);
			return;
		}

        switch(CurrentGame.GameID) {
            case Game.DanganronpaV3:
                // Unity is only in the Switch version
                if (IsUnity)
		        {
			        Log(info[41]);
			        return;
		        }
                break;
            default:
                break;
        }

        // This is a nightmare to maintain
		List<string> extensions_to_uninstall = new List<string>();
        switch(CurrentGame.GameID)
        {
            case Game.DanganronpaV3:
                extensions_to_uninstall.Add(".spc");
                extensions_to_uninstall.Add(".awb");
                break;
            case Game.AITheSomniumFiles:
                extensions_to_uninstall.Add("*");
                break;
            default:
                break;
        }

        int count_uninstalled = 0;

        foreach (string extension in extensions_to_uninstall)
		{

            List<string> filelist = new List<string>();
            // Count and get all (extension, ex. spc) backup files (ex. ".spc_bak") in the patch folder
            // TODO: Why does this only work with the non-Unity versions?
            // Probably because it's *much* easier to uninstall mods on Switch/Emulators (just delete the mod folder)
            string[] lowercase_files = Directory.GetFiles(verified_installation_folder, "*" + (extension == "*" ? "" : extension.ToLowerInvariant()) + "_bak", SearchOption.AllDirectories);
            string[] uppercase_files = Directory.GetFiles(verified_installation_folder, "*" + (extension == "*" ? "" : extension.ToUpperInvariant()) + "_bak", SearchOption.AllDirectories);
            if (lowercase_files == null || uppercase_files == null ||lowercase_files.Length == 0 || uppercase_files.Length == 0)
            {
                Log(info[32]);
                return;
            }

            // Add lowercase files to the list
            foreach (string file in lowercase_files)
            {
                filelist.Add(file);
            }

            // Add uppercase files to the list
            foreach (string file in uppercase_files)
            {
                filelist.Add(file);
            }

            string bak_str = "_bak";
            foreach (string file in filelist)
            {
                // The file we're currently handing is a (backup extension, ex. ".spc_bak") file
                string actual_file = file;
                // To get the (extension, ex. ".spc") file, we just need to remove the bak_str ("_bak") part
                string normal_file = actual_file.Substring(0, actual_file.Length - bak_str.Length);

                // If the (extension, ex. .spc) file doesn't exist
                if (!File.Exists(normal_file) || string.IsNullOrWhiteSpace(normal_file))
                {
                    continue;
                }

                // If the backup file (ex. spc_bak) doesn't exist
                if (!File.Exists(actual_file) || !File.Exists(file))
                {
                    continue;
                }

                // Delete the "normal" extension (ex. spc) file
                File.Delete(normal_file);

                // Copy the backup file (ex. spc_bak) file to the "normal" file (a.k.a. restore backup)
                // (ex. spc_bak -> spc)
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
        }

		if (count_uninstalled > 0)
		{
			// *Insert "we've achieved something" meme*
			Log(info[33]);
		}
		else
		{
			Log(info[32], null, Verbosity.Error);
		}
	}
}