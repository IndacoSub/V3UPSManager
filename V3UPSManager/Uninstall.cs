namespace V3UPSManager;

public partial class MainWindow : Form
{
	private void Uninstall()
	{
		if (string.IsNullOrWhiteSpace(_verifiedInstallationFolder) ||
			!Directory.Exists(_verifiedInstallationFolder))
		{
			Log(info[25], null, Verbosity.Error);
			return;
		}

		if (!CurrentGame.SupportsBackupUninstall(_isUnity))
		{
			// Some game/edition combinations cannot be restored through the
			// backup-based uninstaller. The rule belongs to the game definition.
			Log(info[41]);
			return;
		}

		IReadOnlyList<string> extensions = CurrentGame.UninstallExtensions;
		if (extensions.Count == 0)
		{
			return;
		}

		int restoredCount = 0;

		foreach (string extension in extensions)
		{
			string searchPattern = extension == "*"
				? "*_bak"
				: "*" + extension + "_bak";

			IEnumerable<string> backupFiles;
			try
			{
				backupFiles = Directory.EnumerateFiles(
						_verifiedInstallationFolder,
						searchPattern,
						SearchOption.AllDirectories)
					.Distinct(StringComparer.OrdinalIgnoreCase);
			}
			catch
			{
				Log(info[25], null, Verbosity.Error);
				return;
			}

			foreach (string backupFile in backupFiles)
			{
				if (TryRestoreBackupFile(backupFile))
				{
					restoredCount++;
				}
			}
		}

		Log(
			restoredCount > 0 ? info[33] : info[32],
			null,
			restoredCount > 0 ? Verbosity.Info : Verbosity.Error);
	}

	private static bool TryRestoreBackupFile(string backupFile)
	{
		const string BackupSuffix = "_bak";

		if (!backupFile.EndsWith(
			BackupSuffix,
			StringComparison.OrdinalIgnoreCase) ||
			!File.Exists(backupFile))
		{
			return false;
		}

		string restoredFile = backupFile[..^BackupSuffix.Length];
		if (string.IsNullOrWhiteSpace(restoredFile))
		{
			return false;
		}

		try
		{
			// Restore first, then delete the recovery copy. If the copy fails,
			// the original backup remains available for manual recovery.
			File.Copy(backupFile, restoredFile, true);

			if (!File.Exists(restoredFile))
			{
				return false;
			}

			File.Delete(backupFile);
			return true;
		}
		catch
		{
			// A locked/corrupt file must not prevent other backups from restoring.
			return false;
		}
	}
}
