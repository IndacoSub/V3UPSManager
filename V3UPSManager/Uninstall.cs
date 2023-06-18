namespace V3UPSManager;

public partial class MainWindow : Form
{
	private void Uninstall()
	{
		// Check if the installation folder is valid or not
		if (string.IsNullOrWhiteSpace(verified_installation_folder) || verified_installation_folder.Length <= 0 ||
			!Directory.Exists(verified_installation_folder))
		{
			DisplayInfo.Print(info[25]);
			return;
		}

		if (IsUnity)
		{
			DisplayInfo.Print(info[41]);
			return;
		}

		List<string> filelist = new List<string>();
		// Count and get all .spc backup files (".spc_bak") in the patch folder
		// TODO: Why does this only work with the non-Unity versions?
		// Probably because it's *much* easier to uninstall mods on Switch/Emulators
		string[] files1 = Directory.GetFiles(verified_installation_folder, "*.spc_bak", SearchOption.AllDirectories);
		string[] files2 = Directory.GetFiles(verified_installation_folder, "*.SPC_bak", SearchOption.AllDirectories);
		if (files1 == null || files2 == null || files1.Length == 0 || files2.Length == 0)
		{
			DisplayInfo.Print(info[32]);
			return;
		}

		// Add lowercase files to the list
		foreach (string file in files1)
		{
			filelist.Add(file);
		}

		// Add uppercase files to the list
		foreach (string file in files2)
		{
			filelist.Add(file);
		}

		int count_uninstalled = 0;
		string bak_str = "_bak";
		foreach (string file in filelist)
		{
			// The file we're currently handing is a ".spc_bak" file
			string actual_file = file;
			// To get the ".spc" file, we just need to remove the bak_str ("_bak") part
			string normal_file = actual_file.Substring(0, actual_file.Length - bak_str.Length); // "_bak"'s length is 4

			// If the .spc file doesn't exist
			if (!File.Exists(normal_file) || string.IsNullOrWhiteSpace(normal_file))
			{
				continue;
			}

			// If the spc_bak file doesn't exist
			if (!File.Exists(actual_file) || !File.Exists(file))
			{
				continue;
			}

			// Delete the "normal" spc file
			File.Delete(normal_file);

			// Copy the spc_bak file to the "normal" file (a.k.a. restore backup)
			// spc_bak -> spc
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
			// *Insert "we've achieved something" meme*
			DisplayInfo.Print(info[33]);
		}
		else
		{
			DisplayInfo.Print(info[32]);
		}
	}
}