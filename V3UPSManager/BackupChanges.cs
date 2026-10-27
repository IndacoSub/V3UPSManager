using V3UPSManager.Core;

namespace V3UPSManager;

public partial class MainWindow : Form
{
    private bool BackupChanges(bool use_mod_folder)
    {
        if (!ShouldMakeBackupsCheckbox.Checked) return true;
        var request = new SnapshotRequest(
            _verifiedInstallationFolder,
            Path.Combine(AppContext.BaseDirectory, use_mod_folder ? "ModdedFiles" : "Backup"),
            _filesToPatch.ToArray(),
            UseCleanBackups: !use_mod_folder,
            CanAccessExecutable: _canAccessExe,
            TitleId: _titleId);
        bool succeeded = new SnapshotService().Create(request, ReportCoreOperation);
        if (!succeeded) Log(info[34], null, Verbosity.Error);
        return succeeded;
    }

	private void DisplayStatus()
	{
		if (_filesToPatch == null || _filesToPatch.Count == 0)
		{
			Log(info[34], null, Verbosity.Error);
			return;
		}

		int expectedFiles = 0;
		int successfulFiles = 0;
		List<string> missingBackups = new List<string>();

		foreach (string file in _filesToPatch)
		{
			if (string.Equals(
				Path.GetExtension(file),
				".exe",
				StringComparison.OrdinalIgnoreCase) &&
				!_canAccessExe)
			{
				// Executable patching is intentionally disabled for editions where the
				// executable cannot be safely accessed. It must not count as a failed
				// backup in the final verification.
				continue;
			}

			expectedFiles++;

			if (File.Exists(file) && File.Exists(file + "_bak"))
			{
				successfulFiles++;
			}
			else
			{
				missingBackups.Add(file);
			}
		}

		if (expectedFiles > 0 && successfulFiles == expectedFiles)
		{
			Log(info[30]);
			InstallButton.Enabled = false;
			return;
		}

		Log(info[34], null, Verbosity.Error);

		// Keep useful diagnostic lists next to the executable instead of in an
		// arbitrary current working directory.
		try
		{
			string applyFile = Path.Combine(
				AppContext.BaseDirectory,
				"_filesToPatch.txt");

			string backupFiles = Path.Combine(
				AppContext.BaseDirectory,
				"bak_files.txt");

			File.WriteAllLines(
				applyFile,
				_filesToPatch,
				System.Text.Encoding.UTF8);

			File.WriteAllLines(
				backupFiles,
				missingBackups,
				System.Text.Encoding.UTF8);
		}
		catch
		{
			// Diagnostics are optional and must never turn a completed operation
			// into a crash.
		}
	}
}
