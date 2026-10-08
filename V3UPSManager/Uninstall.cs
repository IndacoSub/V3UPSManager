namespace V3UPSManager;

public partial class MainWindow : Form
{
	private void Uninstall()
	{
		if (string.IsNullOrWhiteSpace(verified_installation_folder) ||
			!Directory.Exists(verified_installation_folder))
		{
			Log(info[25], null, Verbosity.Error);
			return;
		}

		switch (CurrentGame.GameID)
		{
			case Game.DanganronpaV3:
				// Unity/Switch installations do not use the backup-based
				// uninstaller implemented here.
				if (IsUnity)
				{
					Log(info[41]);
					return;
				}
				break;
		}

		List<string> extensionsToUninstall = CurrentGame.GameID switch
		{
			Game.DanganronpaV3 => new List<string>
			{
				".spc",
				".awb",
			},

			Game.AITheSomniumFiles => new List<string>
			{
				".assets",
				".exe",
				"*",
			},

			Game.TokyoPsychodemic => new List<string>
			{
				".assets",
				"*",
			},

			_ => new List<string>(),
		};

		if (extensionsToUninstall.Count == 0)
		{
			return;
		}

		int countUninstalled = 0;

		foreach (string extension in extensionsToUninstall)
		{
			string searchPattern =
				extension == "*"
					? "*_bak"
					: "*" + extension + "_bak";

			List<string> backupFiles;

			try
			{
				backupFiles = Directory.EnumerateFiles(
					verified_installation_folder,
					searchPattern,
					SearchOption.AllDirectories)
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.ToList();
			}
			catch
			{
				Log(info[25], null, Verbosity.Error);
				return;
			}

			foreach (string backupFile in backupFiles)
			{
				if (!backupFile.EndsWith(
					"_bak",
					StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				string normalFile = backupFile.Substring(
					0,
					backupFile.Length - "_bak".Length);

				if (string.IsNullOrWhiteSpace(normalFile) ||
					!File.Exists(backupFile))
				{
					continue;
				}

				try
				{
					// Copying the backup over the live file is safer than deleting
					// the live file first. If the copy fails, the existing file is
					// left untouched and can still be repaired manually.
					File.Copy(
						backupFile,
						normalFile,
						true);

					// Only remove the backup after the restored file is confirmed
					// to exist. This prevents losing the recovery copy on failure.
					if (!File.Exists(normalFile))
					{
						continue;
					}

					File.Delete(backupFile);

					countUninstalled++;
				}
				catch
				{
					// Continue restoring other files. A single locked file should
					// not prevent all remaining backups from being restored.
				}
			}
		}

		if (countUninstalled > 0)
		{
			Log(info[33]);
		}
		else
		{
			Log(info[32], null, Verbosity.Error);
		}
	}
}
