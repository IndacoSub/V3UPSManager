namespace V3UPSManager;

public partial class MainWindow : Form
{
	private readonly HashSet<string> _alreadyChecked =
		new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _missingPatchFiles =
		new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private bool _isLegacy;
	private bool _isUnity;
	private string _titleId = "";
	private bool _canAccessExe = true;

	private bool LoadInstallationFolder()
	{
		string previousInstallationFolder = _installationFolder;
		string previousVerifiedFolder = _verifiedInstallationFolder;
		bool previousManuallySelectedInstall = _manuallySelectedInstall;
		bool previousSelectButtonEnabled = SelectInstallationFolderButton.Enabled;
		bool previousUninstallButtonEnabled = UninstallButton.Enabled;
		bool previousCanAccessExe = _canAccessExe;
		bool previousIsLegacy = _isLegacy;
		bool previousIsUnity = _isUnity;
		string previousTitleID = _titleId;
		GameBase previousGame = CurrentGame;
		string previousWindowTitle = Text;

		_verifiedInstallationFolder = "";
		_canAccessExe = true;
		_titleId = "";
		_isLegacy = false;
		_isUnity = false;

		SelectInstallationFolderButton.Enabled = false;
		UninstallButton.Enabled = false;

		try
		{
			if (!_manuallySelectedInstall)
			{
				using FolderBrowserDialog dialog = new FolderBrowserDialog();

				DialogResult result = ShowFolderBrowserDialogWithHiddenFiles(dialog);

				if (result != DialogResult.OK ||
					string.IsNullOrWhiteSpace(dialog.SelectedPath))
				{
					// Cancellation is not an error. Restore the complete previous state,
					// including the detected game/platform information.
					_installationFolder = previousInstallationFolder;
					_verifiedInstallationFolder = previousVerifiedFolder;
					_manuallySelectedInstall = previousManuallySelectedInstall;
					_canAccessExe = previousCanAccessExe;
					_isLegacy = previousIsLegacy;
					_isUnity = previousIsUnity;
					_titleId = previousTitleID;
					CurrentGame = previousGame;
					Text = previousWindowTitle;
					SelectInstallationFolderButton.Enabled = previousSelectButtonEnabled;
					UninstallButton.Enabled = previousUninstallButtonEnabled;
					return false;
				}

				_installationFolder = dialog.SelectedPath;
			}

			if (string.IsNullOrWhiteSpace(_installationFolder) ||
				!Directory.Exists(_installationFolder))
			{
				Log(info[0], null, Verbosity.Error);
				return false;
			}

			_installationFolder = Path.GetFullPath(_installationFolder);

			string dataFolder = _installationFolder;

			if (!_manuallySelectedGame)
			{
				Game currentGameID = GetGameByFolder(_installationFolder);

				if (currentGameID == Game.None)
				{
					Log(info[45], null, Verbosity.Error);
					return false;
				}

				CurrentGame = SpawnGameByID(currentGameID);

				if (CurrentGame == null || CurrentGame.GameID == Game.None)
				{
					Log(info[46], null, Verbosity.Error);
					return false;
				}

				Text = "V3 UPS Manager" + " - " + CurrentGame.GameID.ToString();
			}
			else if (CurrentGame == null || CurrentGame.GameID == Game.None)
			{
				Log(info[46], null, Verbosity.Error);
				return false;
			}

			// Detect the game edition/platform. The path rules themselves belong to
			// the selected game so this workflow does not need another Game switch.
			if (CheckLegacyConfiguration())
			{
				dataFolder = CurrentGame.GetLegacyDataFolder(_installationFolder);
				_isLegacy = true;
				_isUnity = false;
			}
			else if (CheckUnityConfiguration())
			{
				string streamingAssetsFolder = Path.Combine(
					_installationFolder,
					CurrentGame.UnityDataFolder,
					"StreamingAssets");

				string platform = CurrentGame.UsesUnityPlatformFolder
					? GetUnityPlatformByExclusion(streamingAssetsFolder)
					: string.Empty;

				dataFolder = CurrentGame.GetUnityDataFolderPath(
					_installationFolder,
					platform);

				_isLegacy = false;
				_isUnity = true;
			}
			else if (CheckXboxConfiguration())
			{
				dataFolder = CurrentGame.GetXboxDataFolder(_installationFolder);
				_isLegacy = false;
				_isUnity = false;
			}
			else
			{
				return false;
			}

			if (!CheckInstall(dataFolder))
			{
				return false;
			}

			Log(info[18], null, Verbosity.Info, LogType.Print);

			_verifiedInstallationFolder = _installationFolder;
			InstallationPathPreviewTextbox.Text =
				_verifiedInstallationFolder;

			SelectInstallationFolderButton.Enabled =
				previousSelectButtonEnabled;

			UninstallButton.Enabled = true;

			return true;
		}
		catch
		{
			// Configuration checks and filesystem enumeration can fail for
			// inaccessible, corrupt or disappearing files. Do not crash the GUI.
			_installationFolder = "";
			_verifiedInstallationFolder = "";
			_manuallySelectedInstall = false;
			_canAccessExe = true;
			_isLegacy = false;
			_isUnity = false;
			_titleId = "";
			CurrentGame = previousGame;
			Text = previousWindowTitle;

			Log(info[25], null, Verbosity.Error);
			return false;
		}
		finally
		{
			if (string.IsNullOrWhiteSpace(_verifiedInstallationFolder))
			{
				// Any failed validation must not leave partially detected game/platform
				// state behind. Preserve only the UI state belonging to a previous
				// verified installation.
				CurrentGame = previousGame;
				Text = previousWindowTitle;
				_canAccessExe = previousCanAccessExe;
				_isLegacy = previousIsLegacy;
				_isUnity = previousIsUnity;
				_titleId = previousTitleID;

				SelectInstallationFolderButton.Enabled = previousSelectButtonEnabled;
				UninstallButton.Enabled = false;
			}
		}
	}

	private bool LoadPatchFolder()
	{
		string previousPatchFolder = _patchFolder;
		bool previousManuallySelectedPatch = _manuallySelectedPatch;
		bool previousSelectButtonEnabled = SelectPatchFolderButton.Enabled;
		bool previousInstallButtonEnabled = InstallButton.Enabled;
		List<string> previousToApply = new List<string>(_filesToPatch);
		List<string> previousToBeApplied = new List<string>(_patchesToApply);
		HashSet<string> previousMissingFiles = new HashSet<string>(
			_missingPatchFiles,
			StringComparer.OrdinalIgnoreCase);
		bool operationCancelled = false;

		SelectPatchFolderButton.Enabled = false;
		InstallButton.Enabled = false;

		try
		{
			if (string.IsNullOrWhiteSpace(_verifiedInstallationFolder) ||
				!Directory.Exists(_verifiedInstallationFolder))
			{
				Log(info[19], null, Verbosity.Error);
				SelectPatchFolderButton.Enabled = true;
				return false;
			}

			if (!_manuallySelectedPatch)
			{
				using FolderBrowserDialog dialog = new FolderBrowserDialog();

				DialogResult result = ShowFolderBrowserDialogWithHiddenFiles(dialog);

				if (result != DialogResult.OK ||
					string.IsNullOrWhiteSpace(dialog.SelectedPath))
				{
					// Cancellation is not an error. Restore the previous patch selection
					// and leave the current install button state intact.
					_patchFolder = previousPatchFolder;
					_manuallySelectedPatch = previousManuallySelectedPatch;

					_filesToPatch = previousToApply;
					_patchesToApply = previousToBeApplied;

					_alreadyChecked.Clear();
					_missingPatchFiles.Clear();

					foreach (string file in previousMissingFiles)
					{
						_missingPatchFiles.Add(file);
					}

					SelectPatchFolderButton.Enabled = previousSelectButtonEnabled;
					InstallButton.Enabled = previousInstallButtonEnabled;
					operationCancelled = true;
					return false;
				}

				_patchFolder = dialog.SelectedPath;
			}

			if (string.IsNullOrWhiteSpace(_patchFolder) ||
				!Directory.Exists(_patchFolder))
			{
				Log(info[20], null, Verbosity.Error);
				SelectPatchFolderButton.Enabled = true;
				return false;
			}

			// The previous state has already been preserved at this point, so a
			// real new scan can safely discard its old pair/missing-file data.
			_alreadyChecked.Clear();
			_missingPatchFiles.Clear();
			_filesToPatch = new List<string>();
			_patchesToApply = new List<string>();

			_patchFolder = Path.GetFullPath(_patchFolder);

			string patchRoot = GetPatchRoot(_patchFolder);

			if (string.IsNullOrWhiteSpace(patchRoot) ||
				!Directory.Exists(patchRoot))
			{
				Log(info[21], null, Verbosity.Error);
				SelectPatchFolderButton.Enabled = true;
				return false;
			}

			string patchFormat = CurrentGame.PatchFormatExtension;

			string[] files = Directory.EnumerateFiles(
				patchRoot,
				"*",
				SearchOption.AllDirectories)
			.Where(file =>
				file.EndsWith(
					patchFormat,
					StringComparison.OrdinalIgnoreCase))
			.ToArray();

			if (files.Length == 0)
			{
				Log(info[22], new Dictionary<string, string>()
				{
					{ "VAR_PATCH_FORMAT_EXTENSION", patchFormat },
				}, Verbosity.Error);
				SelectPatchFolderButton.Enabled = true;
				return false;
			}

			List<string> actualPatchFiles = new List<string>();

			foreach (string file in files)
			{
				if (IsDirectory(file))
				{
					continue;
				}

				actualPatchFiles.Add(file);

				// PatchSpecificString identifies files which belong to this
				// patch package. Use a case-insensitive comparison so the same
				// package behaves consistently across filesystems.
				if (!file.EndsWith(
					CurrentGame.PatchSpecificString,
					StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
			}

			if (actualPatchFiles.Count == 0)
			{
				Log(info[22], new Dictionary<string, string>()
				{
					{ "VAR_PATCH_FORMAT_EXTENSION", patchFormat },
				}, Verbosity.Error);
				SelectPatchFolderButton.Enabled = true;
				return false;
			}

			List<string> patchCandidates = actualPatchFiles
				.Where(file =>
					file.EndsWith(
						CurrentGame.PatchSpecificString,
						StringComparison.OrdinalIgnoreCase))
				.ToList();

			if (patchCandidates.Count == 0)
			{
				Log(info[44], new Dictionary<string, string>()
				{
					{ "VAR_PATCH_FORMAT", CurrentGame.PatchFormat.ToString() },
					{ "VAR_PATCH_SPECIFIC_STRING", CurrentGame.PatchSpecificString },
				}, Verbosity.Error);
				SelectPatchFolderButton.Enabled = true;
				return false;
			}

			// GetToApply produces two aligned lists: each entry at index N in
			// _filesToPatch belongs to the patch at index N in _patchesToApply.
			GetToApply(patchCandidates.ToArray());

			Log(
				info[74],
				new Dictionary<string, string>()
				{
					{ "VAR_PATCH_COUNT", patchCandidates.Count.ToString() },
					{ "VAR_MATCHED_COUNT", _filesToPatch.Count.ToString() },
				},
				Verbosity.Info,
				LogType.ConsoleOnly);
			RefreshLogConsole();

			if (_patchesToApply.Count != patchCandidates.Count)
			{
				WriteDiagnosticFiles();
				Log(info[53], null, Verbosity.Error);
				SelectPatchFolderButton.Enabled = true;
				return false;
			}

			if (_filesToPatch.Count == 0)
			{
				Log(info[27], new Dictionary<string, string>()
				{
					{ "VAR_PATCH_FORMAT", CurrentGame.PatchFormat.ToString() },
				}, Verbosity.Error);
				SelectPatchFolderButton.Enabled = true;
				return false;
			}

			if (_missingPatchFiles.Count > 0)
			{
				foreach (string file in _missingPatchFiles)
				{
					Log(info[47], new Dictionary<string, string>()
					{
						{ "VAR_SOME_FILE", file },
					}, Verbosity.Info, LogType.ConsoleOnly);
				}

				WriteDiagnosticFiles();
				Log(info[53], null, Verbosity.Error);
				SelectPatchFolderButton.Enabled = true;
				return false;
			}

			Log(info[23]);

			PatchPathPreviewTextbox.Text = _patchFolder;
			SelectInstallationFolderButton.Enabled = true;
			SelectPatchFolderButton.Enabled = previousSelectButtonEnabled;

			if (!string.IsNullOrWhiteSpace(_verifiedInstallationFolder))
			{
				InstallButton.Enabled = true;
			}

			return true;
		}
		catch
		{
			_patchFolder = "";
			_manuallySelectedPatch = false;

			Log(info[20], null, Verbosity.Error);
			SelectPatchFolderButton.Enabled = true;
			return false;
		}
		finally
		{
			if (operationCancelled)
			{
				InstallButton.Enabled = previousInstallButtonEnabled;
			}
			else if (_filesToPatch.Count == 0)
			{
				InstallButton.Enabled = false;
			}
		}
	}

	private string GetPatchRoot(string patchFolder)
	{
		if (CurrentGame == null)
		{
			return string.Empty;
		}

		return CurrentGame.GetPatchRoot(
			patchFolder,
			_isUnity,
			_isLegacy);
	}

	private void RemoveFromMissingList(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return;
		}

		_missingPatchFiles.Remove(path);
	}

	private void GetToApply(string[] patchFiles)
	{
		_filesToPatch = new List<string>();
		_patchesToApply = new List<string>();
		_alreadyChecked.Clear();
		_missingPatchFiles.Clear();

		if (patchFiles == null || patchFiles.Length == 0)
		{
			return;
		}

		IReadOnlyList<string> extensions = CurrentGame.GetPatchTargetExtensions(
			_isUnity,
			_canAccessExe);

		foreach (string extension in extensions)
		{
			TryToApplyFiles(patchFiles, extension);
		}
	}

	private void TryToApplyFiles(string[] patchFiles, string extension)
	{
		foreach (string file in patchFiles)
		{
			if (string.IsNullOrWhiteSpace(file) ||
				!file.EndsWith(
					CurrentGame.PatchSpecificString,
					StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			string secondHalf;

			try
			{
				secondHalf = Path.GetRelativePath(
					_patchFolder,
					file);
			}
			catch
			{
				continue;
			}

			if (string.IsNullOrWhiteSpace(secondHalf) ||
				secondHalf == "." ||
				secondHalf.StartsWith(
					"..",
					StringComparison.Ordinal))
			{
				continue;
			}

			string normalizedRelativePath =
				secondHalf.Replace(
					Path.AltDirectorySeparatorChar,
					Path.DirectorySeparatorChar);

			if (_alreadyChecked.Contains(normalizedRelativePath))
			{
				continue;
			}

			string approximateFile;

			try
			{
				string fileWithoutPatchSuffix = normalizedRelativePath
					.Substring(
						0,
						normalizedRelativePath.Length -
						CurrentGame.PatchSpecificString.Length);

				if (string.IsNullOrWhiteSpace(fileWithoutPatchSuffix))
				{
					continue;
				}

				approximateFile = Path.Combine(
					_verifiedInstallationFolder,
					fileWithoutPatchSuffix);
			}
			catch
			{
				continue;
			}

			if (IsDirectory(approximateFile))
			{
				DialogResult result = Log(
					ui_messages[1],
					null,
					Verbosity.Info,
					LogType.Ask);

				if (result != DialogResult.Yes)
				{
					continue;
				}
			}

			if (extension.Length > 0)
			{
				string existingExtension = Path.GetExtension(approximateFile);

				if (string.Equals(
					existingExtension,
					extension,
					StringComparison.OrdinalIgnoreCase))
				{
					// If the patch name already contains the target extension,
					// accept either the live file or an existing backup. Install()
					// can reconstruct a missing live file from that backup.
					if (!FileExistsCaseSensitive(approximateFile) &&
						!File.Exists(approximateFile) &&
						!File.Exists(approximateFile + "_bak"))
					{
						_missingPatchFiles.Add(approximateFile);
						continue;
					}

					AddPatchPair(
						approximateFile,
						file,
						normalizedRelativePath);

					continue;
				}

				string lowercaseCandidate =
					approximateFile + extension.ToLowerInvariant();

				string uppercaseCandidate =
					approximateFile + extension.ToUpperInvariant();

				string? targetFile = null;

				if (FileExistsCaseSensitive(lowercaseCandidate))
				{
					targetFile = lowercaseCandidate;
				}
				else if (FileExistsCaseSensitive(uppercaseCandidate))
				{
					targetFile = uppercaseCandidate;
				}
				else if (File.Exists(lowercaseCandidate))
				{
					// On a case-insensitive filesystem the exact-case helper can
					// legitimately return false even though the target exists.
					// Prefer the lowercase candidate because it matches the
					// normal naming convention used by these game files.
					targetFile = lowercaseCandidate;
				}
				else if (File.Exists(uppercaseCandidate))
				{
					targetFile = uppercaseCandidate;
				}
				else if (File.Exists(lowercaseCandidate + "_bak"))
				{
					targetFile = lowercaseCandidate;
				}
				else if (File.Exists(uppercaseCandidate + "_bak"))
				{
					targetFile = uppercaseCandidate;
				}

				if (targetFile == null)
				{
					_missingPatchFiles.Add(
						lowercaseCandidate);

					continue;
				}

				AddPatchPair(
					targetFile,
					file,
					normalizedRelativePath);
			}
			else
			{
				// Files without an extension are valid for some Unity titles.
				// Do not modify the installation during discovery: Install()
				// will restore an existing _bak only when it is actually needed.
				if (!File.Exists(approximateFile) &&
					!File.Exists(approximateFile + "_bak"))
				{
					_missingPatchFiles.Add(approximateFile);
					continue;
				}

				AddPatchPair(
					approximateFile,
					file,
					normalizedRelativePath);
			}
		}
	}

	private void AddPatchPair(
		string targetFile,
		string patchFile,
		string relativePatchPath)
	{
		if (string.IsNullOrWhiteSpace(targetFile) ||
			string.IsNullOrWhiteSpace(patchFile) ||
			_alreadyChecked.Contains(relativePatchPath))
		{
			return;
		}

		if (_filesToPatch.Contains(targetFile, StringComparer.OrdinalIgnoreCase))
		{
			return;
		}

		_filesToPatch.Add(targetFile);
		_patchesToApply.Add(patchFile);
		_alreadyChecked.Add(relativePatchPath);

		// Record the exact patch-to-target association during discovery. This
		// makes it possible to see what the installer intends to modify before
		// the external patch tool is launched.
		Log(
			info[73],
			new Dictionary<string, string>()
			{
				{ "VAR_PATCH_FILE", patchFile },
				{ "VAR_OUTPUT_FILE", targetFile },
			},
			Verbosity.Info,
			LogType.ConsoleOnly);
		RefreshLogConsole();

		RemoveFromMissingList(targetFile);
		RemoveFromMissingList(targetFile.ToLowerInvariant());
	}
}
