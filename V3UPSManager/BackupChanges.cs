﻿namespace V3UPSManager;

public partial class MainWindow : Form
{

	private string GetBackupFolder()
	{

        // Get the current directory
        // ex. C:\V3UPSManager\
        string cur = Directory.GetCurrentDirectory();

        // Get the backup directory
        // ex. C:\V3UPSManager\Backup
        string backupdir = Path.Combine(cur, "Backup");

		return backupdir;
    }

	private string GetModFolder()
	{
		// See GetBackupFolder for comments

		string cur = Directory.GetCurrentDirectory();
		string moddir = Path.Combine(cur, "ModdedFiles");

		return moddir;
	}

	private void BackupChanges(bool use_mod_folder)
	{

		if(!ShouldMakeBackupsCheckbox.Checked)
		{
			return;
		}

		string backupdir = use_mod_folder ? GetModFolder() : GetBackupFolder();

		// Delete the current backups, recursively (TODO: why?)
		// Answer (maybe): because then you may include files that might not be needed anymore in a later patch?
		if (Directory.Exists(backupdir))
		{
			Directory.Delete(backupdir, true);
		}
		else
		{
			// TODO: Why is this in an "else"?
			Directory.CreateDirectory(backupdir);
		}

		// If the platform is detected as Switch (or emulator on PC?):
		if (TitleID.Length > 0)
		{
			// Note: consoles often use a unique "ProductID" or a "TitleID" to differentiate between games, apps etc.
			// since using their names -- or internal names -- might be "unreliable" and slower
			// And that TitleID/ProductID is obviously different between games/apps
			// A Nintendo Switch TitleID is composed of sixteen (16) alphanumerical characters
			// often starting with "0100"
			// A possible mod installation folder for the Switch version is
			// ex. E:\Atmosphere\contents\TITLEID\romfs\ where "romfs" indicates that the mod
			// is accessing the assets' section of the game, not the code itself
			// Another possible mod installation folder for the Switch version, ONLY when using an emulator, is
			// ex. C:\MySwitchEmulator\SomeFolders\TITLEID\MODNAME\romfs\
			// So, ONLY when using emulators, you should also include the "mod name" as well
			// TODO: Why is this not implemented?

			backupdir = Path.Combine(backupdir, TitleID);
			backupdir = Path.Combine(backupdir, "romfs");
		}

		foreach (string file in to_apply)
		{
			// Get the relative path of the file compared to the installation folder
			// ex. C:\Games\DanganronpaV3\data\win\game_resident\game_resident_US.spc
			// would return \data\win\game_resident\game_resident_US.spc

			string after_installdir = file.Substring(verified_installation_folder.Length + 1);

			// Combine the backup path and the relative path
			// ex. backup: C:\V3UPSManager\Backup
			// ex. relative path: \data\win\game_resident\game_resident_US.spc
			// would return
			// ex. C:\V3UPSManager\Backup\data\win\game_resident\game_resident_US.spc
			string newpath = Path.Combine(backupdir, after_installdir);
			if (newpath == null || newpath.Length <= 0)
			{
				continue;
			}

			// Get the parent directory
			// ex. \data\win\game_resident\game_resident_US.spc
			// would return \data\win\game_resident\
			string directory = Path.GetDirectoryName(newpath);

			// Create the directory if it doesn't already exist
			// TODO: Check if it already exists?
			Directory.CreateDirectory(directory);

			// Back up the file by copying it to the desired backup folder
			if (!File.Exists(newpath))
			{
				Log("📋 VAR_BEFORE ⮞ VAR_AFTER", new Dictionary<string, string>()
				{
					{ "VAR_BEFORE", file },
					{ "VAR_AFTER", newpath },
				}, Verbosity.Info, LogType.ConsoleOnly);

				if(!file.EndsWith(".exe") || CanAccessExe)
				{
					File.Copy(file, newpath);
				}
			}
		}
	}

	private void DisplayStatus()
	{
		// Count backup (containing "_bak") files in the installation folder
		string[] files = Directory.GetFiles(installation_folder, "*_bak", SearchOption.AllDirectories);
		if (files != null && files.Length > 0 && files.Length == to_apply.Count)
		{
			Log(info[30]);
		}
		else
		{
			Log(info[34], null, Verbosity.Error);
			string apply_file = "to_apply.txt";
			string bak_files = "bak_files.txt";
			if (File.Exists(bak_files))
			{
				File.Delete(bak_files);
			}
			File.WriteAllText(bak_files, string.Join('\n', files));
			if (!File.Exists(apply_file))
			{
				File.WriteAllText(apply_file, string.Join('\n', to_apply));
			}
		}
	}
}