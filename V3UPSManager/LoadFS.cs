namespace V3UPSManager;

public partial class MainWindow : Form
{
	private readonly HashSet<string> already_checked =
		new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> couldnt_be_found =
		new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private bool IsLegacy;
	private bool IsUnity;
	private string TitleID = "";
	public bool CanAccessExe = true;

	private bool LoadInstallationFolder()
	{
		string previousInstallationFolder = installation_folder;
		string previousVerifiedFolder = verified_installation_folder;
		bool previousManuallySelectedInstall = manually_selected_install;
		bool previousSelectButtonEnabled = SelectInstallationFolderButton.Enabled;
		bool previousUninstallButtonEnabled = UninstallButton.Enabled;
		bool previousCanAccessExe = CanAccessExe;
		bool previousIsLegacy = IsLegacy;
		bool previousIsUnity = IsUnity;
		string previousTitleID = TitleID;
		GameBase previousGame = CurrentGame;
		string previousWindowTitle = Text;

		verified_installation_folder = "";
		CanAccessExe = true;
		TitleID = "";
		IsLegacy = false;
		IsUnity = false;

		SelectInstallationFolderButton.Enabled = false;
		UninstallButton.Enabled = false;

		try
		{
			if (!manually_selected_install)
			{
				using FolderBrowserDialog dialog = new FolderBrowserDialog();

				DialogResult result = ShowFolderBrowserDialogWithHiddenFiles(dialog);

				if (result != DialogResult.OK ||
					string.IsNullOrWhiteSpace(dialog.SelectedPath))
				{
					// Cancellation is not an error. Restore the complete previous state,
					// including the detected game/platform information.
					installation_folder = previousInstallationFolder;
					verified_installation_folder = previousVerifiedFolder;
					manually_selected_install = previousManuallySelectedInstall;
					CanAccessExe = previousCanAccessExe;
					IsLegacy = previousIsLegacy;
					IsUnity = previousIsUnity;
					TitleID = previousTitleID;
					CurrentGame = previousGame;
					Text = previousWindowTitle;
					SelectInstallationFolderButton.Enabled = previousSelectButtonEnabled;
					UninstallButton.Enabled = previousUninstallButtonEnabled;
					return false;
				}

				installation_folder = dialog.SelectedPath;
			}

			if (string.IsNullOrWhiteSpace(installation_folder) ||
				!Directory.Exists(installation_folder))
			{
				Log(info[0], null, Verbosity.Error);
				return false;
			}

			installation_folder = Path.GetFullPath(installation_folder);

			string dataFolder = installation_folder;

			if (!manually_selected_game)
			{
				Game currentGameID = GetGameByFolder(installation_folder);

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

			// Detect the game edition/platform.
			if (CheckLegacyConfiguration())
			{
				switch (CurrentGame.GameID)
				{
					case Game.DanganronpaV3:
						dataFolder = Path.Combine(
							installation_folder,
							"data",
							"win");
						break;
				}

				IsLegacy = true;
				IsUnity = false;
			}
			else if (CheckUnityConfiguration())
			{
				switch (CurrentGame.GameID)
				{
					case Game.DanganronpaV3:
					case Game.AITheSomniumFiles:
						string unityDataFolder = Path.Combine(
							installation_folder,
							CurrentGame.UnityDataFolder);

						string streamingAssetsFolder = Path.Combine(
							unityDataFolder,
							"StreamingAssets");

						// The platform directory is selected by exclusion inside
						// StreamingAssets. The helper returns only the directory name.
						string platform = GetUnityPlatformByExclusion(streamingAssetsFolder);

						dataFolder = streamingAssetsFolder;

						if (!string.IsNullOrWhiteSpace(platform))
						{
							dataFolder = Path.Combine(
								dataFolder,
								platform);
						}

						break;

					case Game.TokyoPsychodemic:
						dataFolder = Path.Combine(
							installation_folder,
							CurrentGame.UnityDataFolder,
							"StreamingAssets",
							"AssetBundles");
						break;
				}

				IsLegacy = false;
				IsUnity = true;
			}
			else if (CheckXboxConfiguration())
			{
				switch (CurrentGame.GameID)
				{
					case Game.DanganronpaV3:
						dataFolder = Path.Combine(
							installation_folder,
							"data",
							"WIN");
						break;
				}

				IsLegacy = false;
				IsUnity = false;
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

			verified_installation_folder = installation_folder;
			InstallationPathPreviewTextbox.Text =
				verified_installation_folder;

			SelectInstallationFolderButton.Enabled =
				previousSelectButtonEnabled;

			UninstallButton.Enabled = true;

			return true;
		}
		catch
		{
			// Configuration checks and filesystem enumeration can fail for
			// inaccessible, corrupt or disappearing files. Do not crash the GUI.
			installation_folder = "";
			verified_installation_folder = "";
			manually_selected_install = false;
			CanAccessExe = true;
			IsLegacy = false;
			IsUnity = false;
			TitleID = "";
			CurrentGame = previousGame;
			Text = previousWindowTitle;

			Log(info[25], null, Verbosity.Error);
			return false;
		}
		finally
		{
			if (string.IsNullOrWhiteSpace(verified_installation_folder))
			{
				// Any failed validation must not leave partially detected game/platform
				// state behind. Preserve only the UI state belonging to a previous
				// verified installation.
				CurrentGame = previousGame;
				Text = previousWindowTitle;
				CanAccessExe = previousCanAccessExe;
				IsLegacy = previousIsLegacy;
				IsUnity = previousIsUnity;
				TitleID = previousTitleID;

				SelectInstallationFolderButton.Enabled = previousSelectButtonEnabled;
				UninstallButton.Enabled = false;
			}
		}
	}

	private bool LoadPatchFolder()
	{
		string previousPatchFolder = ups_folder;
		bool previousManuallySelectedPatch = manually_selected_patch;
		bool previousSelectButtonEnabled = SelectPatchFolderButton.Enabled;
		bool previousInstallButtonEnabled = InstallButton.Enabled;
		List<string> previousToApply = new List<string>(to_apply);
		List<string> previousToBeApplied = new List<string>(to_be_applied);
		HashSet<string> previousMissingFiles = new HashSet<string>(
			couldnt_be_found,
			StringComparer.OrdinalIgnoreCase);
		bool operationCancelled = false;

		SelectPatchFolderButton.Enabled = false;
		InstallButton.Enabled = false;

		try
		{
			if (string.IsNullOrWhiteSpace(verified_installation_folder) ||
				!Directory.Exists(verified_installation_folder))
			{
				Log(info[19], null, Verbosity.Error);
				return false;
			}

			if (!manually_selected_patch)
			{
				using FolderBrowserDialog dialog = new FolderBrowserDialog();

				DialogResult result = ShowFolderBrowserDialogWithHiddenFiles(dialog);

				if (result != DialogResult.OK ||
					string.IsNullOrWhiteSpace(dialog.SelectedPath))
				{
					// Cancellation is not an error. Restore the previous patch selection
					// and leave the current install button state intact.
					ups_folder = previousPatchFolder;
					manually_selected_patch = previousManuallySelectedPatch;

					to_apply = previousToApply;
					to_be_applied = previousToBeApplied;

					already_checked.Clear();
					couldnt_be_found.Clear();

					foreach (string file in previousMissingFiles)
					{
						couldnt_be_found.Add(file);
					}

					SelectPatchFolderButton.Enabled = previousSelectButtonEnabled;
					InstallButton.Enabled = previousInstallButtonEnabled;
					operationCancelled = true;
					return false;
				}

				ups_folder = dialog.SelectedPath;
			}

			if (string.IsNullOrWhiteSpace(ups_folder) ||
				!Directory.Exists(ups_folder))
			{
				Log(info[20], null, Verbosity.Error);
				return false;
			}

			// The previous state has already been preserved at this point, so a
			// real new scan can safely discard its old pair/missing-file data.
			already_checked.Clear();
			couldnt_be_found.Clear();
			to_apply = new List<string>();
			to_be_applied = new List<string>();

			ups_folder = Path.GetFullPath(ups_folder);

			string patchRoot = GetPatchRoot(ups_folder);

			if (string.IsNullOrWhiteSpace(patchRoot) ||
				!Directory.Exists(patchRoot))
			{
				Log(info[21], null, Verbosity.Error);
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

				return false;
			}

			// GetToApply produces two aligned lists: each entry at index N in
			// to_apply belongs to the patch at index N in to_be_applied.
			GetToApply(patchCandidates.ToArray());

			Log(
				info[74],
				new Dictionary<string, string>()
				{
					{ "VAR_PATCH_COUNT", patchCandidates.Count.ToString() },
					{ "VAR_MATCHED_COUNT", to_apply.Count.ToString() },
				},
				Verbosity.Info,
				LogType.ConsoleOnly);
			RefreshLogConsole();

			if (to_be_applied.Count != patchCandidates.Count)
			{
				WriteDiagnosticFiles();
				Log(info[53], null, Verbosity.Error);
				return false;
			}

			if (to_apply.Count == 0)
			{
				Log(info[27], new Dictionary<string, string>()
				{
					{ "VAR_PATCH_FORMAT", CurrentGame.PatchFormat.ToString() },
				}, Verbosity.Error);

				return false;
			}

			if (couldnt_be_found.Count > 0)
			{
				foreach (string file in couldnt_be_found)
				{
					Log(info[47], new Dictionary<string, string>()
					{
						{ "VAR_SOME_FILE", file },
					}, Verbosity.Info, LogType.ConsoleOnly);
				}

				WriteDiagnosticFiles();
				Log(info[53], null, Verbosity.Error);
				return false;
			}

			Log(info[23]);

			PatchPathPreviewTextbox.Text = ups_folder;
			SelectInstallationFolderButton.Enabled = true;
			SelectPatchFolderButton.Enabled = previousSelectButtonEnabled;

			if (!string.IsNullOrWhiteSpace(verified_installation_folder))
			{
				InstallButton.Enabled = true;
			}

			return true;
		}
		catch
		{
			ups_folder = "";
			manually_selected_patch = false;

			Log(info[20], null, Verbosity.Error);
			return false;
		}
		finally
		{
			if (operationCancelled)
			{
				InstallButton.Enabled = previousInstallButtonEnabled;
			}
			else if (to_apply.Count == 0)
			{
				InstallButton.Enabled = false;
			}
		}
	}

	private string GetPatchRoot(string patchFolder)
	{
		if (CurrentGame == null)
		{
			return "";
		}

		if (!IsUnity)
		{
			switch (CurrentGame.GameID)
			{
				case Game.DanganronpaV3:
					string dataFolder = Path.Combine(patchFolder, "data");
					string winFolder = Path.Combine(
						dataFolder,
						IsLegacy ? "win" : "WIN");

					if (!Directory.Exists(winFolder))
					{
						return "";
					}

					return winFolder;

				default:
					return patchFolder;
			}
		}

		// Unity patch packages keep their files below the game's Unity
		// Data/StreamingAssets tree.
		string unityStreamingAssets = Path.Combine(
			patchFolder,
			CurrentGame.UnityDataFolder,
			"StreamingAssets");

		if (!Directory.Exists(unityStreamingAssets))
		{
			return "";
		}

		return patchFolder;
	}

	private void RemoveFromMissingList(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return;
		}

		couldnt_be_found.Remove(path);
	}

	private void GetToApply(string[] upsFiles)
	{
		to_apply = new List<string>();
		to_be_applied = new List<string>();
		already_checked.Clear();
		couldnt_be_found.Clear();

		if (upsFiles == null || upsFiles.Length == 0)
		{
			return;
		}

		IEnumerable<string> extensions;

		switch (CurrentGame.GameID)
		{
			case Game.DanganronpaV3:
				extensions = IsUnity
					? new[] { ".spc", ".awb", ".pb", ".ab", ".assets" }
					: new[] { ".spc", ".awb" };
				break;

			case Game.AITheSomniumFiles:
				extensions = CanAccessExe
					? new[] { "", ".assets", ".exe" }
					: new[] { "", ".assets" };
				break;

			case Game.TokyoPsychodemic:
				extensions = new[] { "", ".assets", ".dll" };
				break;

			default:
				extensions = Array.Empty<string>();
				break;
		}

		foreach (string extension in extensions)
		{
			TryToApplyFiles(upsFiles, extension);
		}
	}

	private void TryToApplyFiles(string[] upsFiles, string extension)
	{
		foreach (string file in upsFiles)
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
					ups_folder,
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

			if (already_checked.Contains(normalizedRelativePath))
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
					verified_installation_folder,
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
						couldnt_be_found.Add(approximateFile);
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
					couldnt_be_found.Add(
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
					couldnt_be_found.Add(approximateFile);
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
			already_checked.Contains(relativePatchPath))
		{
			return;
		}

		if (to_apply.Contains(targetFile, StringComparer.OrdinalIgnoreCase))
		{
			return;
		}

		to_apply.Add(targetFile);
		to_be_applied.Add(patchFile);
		already_checked.Add(relativePatchPath);

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
