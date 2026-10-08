namespace V3UPSManager;

public partial class MainWindow : Form
{
	private string GetBackupFolder()
	{
		// Always keep application-owned backups next to the executable.
		// The current working directory can be changed by launchers or other
		// processes and is therefore not a reliable application root.
		return Path.Combine(AppContext.BaseDirectory, "Backup");
	}

	private string GetModFolder()
	{
		return Path.Combine(AppContext.BaseDirectory, "ModdedFiles");
	}

	private bool BackupChanges(bool use_mod_folder)
	{
		if (!ShouldMakeBackupsCheckbox.Checked)
		{
			return true;
		}

		if (string.IsNullOrWhiteSpace(_verifiedInstallationFolder) ||
			!Directory.Exists(_verifiedInstallationFolder))
		{
			Log(info[82], null, Verbosity.Error, LogType.ConsoleOnly);
			RefreshLogConsole();
			Log(info[25], null, Verbosity.Error);
			return false;
		}

		if (_filesToPatch == null || _filesToPatch.Count == 0)
		{
			Log(info[84], null, Verbosity.Error, LogType.ConsoleOnly);
			RefreshLogConsole();
			Log(info[34], null, Verbosity.Error);
			return false;
		}

		// Snapshot the operation inputs so later UI/state changes cannot mutate
		// the collection while the backup transaction is iterating it.
		List<string> filesToBackup = new List<string>(_filesToPatch);
		string installationRoot = Path.GetFullPath(_verifiedInstallationFolder);
		string backupDirectory = use_mod_folder
			? GetModFolder()
			: GetBackupFolder();

		// Unique staging names prevent stale files from a crashed run from ever
		// colliding with the current transaction. The install mutex prevents two
		// V3UPSManager processes from publishing the same final directory at once.
		string operationId = Guid.NewGuid().ToString("N");
		string temporaryDirectory = backupDirectory + ".tmp." + operationId;
		string oldDirectory = backupDirectory + ".old." + operationId;
		string currentSourceFile = string.Empty;

		try
		{
			Log(
				info[71],
				new Dictionary<string, string>()
				{
					{ "VAR_BACKUP_DIRECTORY", backupDirectory },
				},
				Verbosity.Info,
				LogType.ConsoleOnly);
			RefreshLogConsole();

			// Build a complete snapshot in a temporary directory first. If any
			// copy fails, the previous valid snapshot remains untouched.
			if (Directory.Exists(temporaryDirectory))
			{
				Directory.Delete(temporaryDirectory, true);
			}

			Directory.CreateDirectory(temporaryDirectory);

			string actualBackupRoot = temporaryDirectory;

			// Switch/emulator installations keep their content below _titleId/romfs.
			if (!string.IsNullOrEmpty(_titleId))
			{
				actualBackupRoot = Path.Combine(
					actualBackupRoot,
					_titleId,
					"romfs");

				Directory.CreateDirectory(actualBackupRoot);
			}

			foreach (string file in filesToBackup)
			{
				currentSourceFile = file ?? string.Empty;

				if (string.IsNullOrWhiteSpace(file))
				{
					Log(info[76], null, Verbosity.Error, LogType.ConsoleOnly);
					RefreshLogConsole();
					return false;
				}

				bool isExe = string.Equals(
					Path.GetExtension(file),
					".exe",
					StringComparison.OrdinalIgnoreCase);

				if (isExe && !_canAccessExe)
				{
					continue;
				}

				string sourceFile = file;
				string backupFile = file + "_bak";

				// A previous installation may have removed the live file while
				// retaining its backup. In that case the backup is still the
				// correct clean source for the snapshot.
				if (!File.Exists(sourceFile) && File.Exists(backupFile))
				{
					sourceFile = backupFile;

					Log(
						info[59],
						new Dictionary<string, string>()
						{
							{ "VAR_BACKUP_FILE", backupFile },
						},
						Verbosity.Info,
						LogType.ConsoleOnly);
					RefreshLogConsole();
				}

				if (!File.Exists(sourceFile))
				{
					Log(
						info[77],
						new Dictionary<string, string>()
						{
							{ "VAR_SOURCE_FILE", sourceFile },
						},
						Verbosity.Error,
						LogType.ConsoleOnly);
					RefreshLogConsole();
					return false;
				}

				string fullSource = Path.GetFullPath(sourceFile);
				string relativePath = Path.GetRelativePath(
					installationRoot,
					fullSource);

				// Refuse to copy anything outside the verified installation
				// folder, even if a malformed path somehow enters _filesToPatch.
				if (relativePath == ".." ||
					relativePath.StartsWith(
						".." + Path.DirectorySeparatorChar,
						StringComparison.Ordinal) ||
					Path.IsPathRooted(relativePath))
				{
					Log(
						info[78],
						new Dictionary<string, string>()
						{
							{ "VAR_SOURCE_FILE", sourceFile },
						},
						Verbosity.Error,
						LogType.ConsoleOnly);
					RefreshLogConsole();
					return false;
				}

				string destinationFile = Path.Combine(
					actualBackupRoot,
					relativePath);

				string? destinationDirectory =
					Path.GetDirectoryName(destinationFile);

				if (string.IsNullOrWhiteSpace(destinationDirectory))
				{
					Log(
						info[79],
						new Dictionary<string, string>()
						{
							{ "VAR_SOURCE_FILE", sourceFile },
						},
						Verbosity.Error,
						LogType.ConsoleOnly);
					RefreshLogConsole();
					return false;
				}

				Directory.CreateDirectory(destinationDirectory);

				// Do not overwrite inside the temporary snapshot: each target
				// is expected to be unique and duplicates indicate a bug upstream.
				if (File.Exists(destinationFile))
				{
					Log(
						info[80],
						new Dictionary<string, string>()
						{
							{ "VAR_BACKUP_FILE", destinationFile },
						},
						Verbosity.Error,
						LogType.ConsoleOnly);
					RefreshLogConsole();
					return false;
				}

				File.Copy(sourceFile, destinationFile, false);

				// Report every successfully created backup immediately. The displayed path
				// is the final Backup/ModdedFiles path, not the temporary staging directory.
				// This keeps the console useful while the backup operation is still running.
				string finalBackupFile = Path.Combine(
					backupDirectory,
					relativePath);

				Log(
					info[55],
					new Dictionary<string, string>()
					{
						{ "VAR_BACKUP_FILE", finalBackupFile },
					},
					Verbosity.Info,
					LogType.ConsoleOnly);

				// BackupChanges is synchronous, so give WinForms an opportunity to
				// repaint the console before copying the next file.
				RefreshLogConsole();
			}

			// Replace the previous snapshot only after the temporary snapshot is
			// complete. The .old directory is kept until the new one is in place.
			if (Directory.Exists(oldDirectory))
			{
				Directory.Delete(oldDirectory, true);
			}

			bool hadPrevious = Directory.Exists(backupDirectory);

			if (hadPrevious)
			{
				Directory.Move(backupDirectory, oldDirectory);
			}

			try
			{
				Directory.Move(temporaryDirectory, backupDirectory);
			}
			catch
			{
				if (hadPrevious &&
					!Directory.Exists(backupDirectory) &&
					Directory.Exists(oldDirectory))
				{
					Directory.Move(oldDirectory, backupDirectory);
				}

				throw;
			}

			try
			{
				if (Directory.Exists(oldDirectory))
				{
					Directory.Delete(oldDirectory, true);
				}
			}
			catch
			{
				// The new snapshot is already in place. Failure to remove the stale
				// temporary previous snapshot must not invalidate a successful backup.
			}

			Log(
				info[72],
				new Dictionary<string, string>()
				{
					{ "VAR_BACKUP_DIRECTORY", backupDirectory },
				},
				Verbosity.Info,
				LogType.ConsoleOnly);
			RefreshLogConsole();

			return true;
		}
		catch (Exception ex)
		{
			try
			{
				if (Directory.Exists(temporaryDirectory))
				{
					Directory.Delete(temporaryDirectory, true);
				}
			}
			catch
			{
				// A failed cleanup must not hide the original backup failure.
			}

			Log(
				info[70],
				new Dictionary<string, string>()
				{
					{ "VAR_OUTPUT_FILE", currentSourceFile },
					{ "VAR_EXCEPTION", ex.Message },
				},
				Verbosity.Error,
				LogType.ConsoleOnly);
			RefreshLogConsole();

			// Keep a phase-specific failure message as well as the exception so the
			// console makes it obvious that this happened while publishing the snapshot.
			Log(
				info[81],
				new Dictionary<string, string>()
				{
					{ "VAR_BACKUP_DIRECTORY", backupDirectory },
				},
				Verbosity.Error,
				LogType.ConsoleOnly);
			RefreshLogConsole();

			Log(info[34], null, Verbosity.Error);
			return false;
		}
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
