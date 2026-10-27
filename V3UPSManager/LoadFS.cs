namespace V3UPSManager;

public partial class MainWindow : Form
{
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

			if (!_manuallySelectedGame)
			{
				Game currentGameID = GameCatalog.Detect(_installationFolder);

				if (currentGameID == Game.None)
				{
					Log(info[45], null, Verbosity.Error);
					return false;
				}

				CurrentGame = GameCatalog.Create(currentGameID);

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

			InstallationValidationResult? validation = ValidateInstallation();
			if (validation is null) return false;
			_isLegacy = validation.IsLegacy;
			_isUnity = validation.IsUnity;
			_canAccessExe = validation.CanAccessExecutable;
			_titleId = validation.TitleId;

			Log(info[18], null, Verbosity.Info, LogType.Print);

			_verifiedInstallationFolder = _installationFolder;
			InstallationPathPreviewTextbox.Text =
				_verifiedInstallationFolder;

			SelectInstallationFolderButton.Enabled =
				previousSelectButtonEnabled;

			if (!string.Equals(previousVerifiedFolder, _verifiedInstallationFolder, StringComparison.OrdinalIgnoreCase))
			{
				_filesToPatch.Clear();
				_patchesToApply.Clear();
				_missingPatchFiles.Clear();
				_patchFolder = "";
				_manuallySelectedPatch = false;
				PatchPathPreviewTextbox.Clear();
				InstallButton.Enabled = false;
				SelectPatchFolderButton.Enabled = true;
			}

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
				InstallButton.Enabled = false;
				_filesToPatch.Clear();
				_patchesToApply.Clear();
				_missingPatchFiles.Clear();
				InstallationPathPreviewTextbox.Clear();
				PatchPathPreviewTextbox.Clear();
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
		bool planAccepted = false;

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
			_missingPatchFiles.Clear();
			_filesToPatch = new List<string>();
			_patchesToApply = new List<string>();

			_patchFolder = Path.GetFullPath(_patchFolder);

			PatchDiscoveryResult plan = PatchDiscovery.Discover(CurrentGame,
				_verifiedInstallationFolder, _patchFolder, _isUnity, _isLegacy, _canAccessExe);
			_filesToPatch = plan.Targets.Select(target => target.FilePath).ToList();
			_patchesToApply = plan.Targets.Select(target => target.PatchPath).ToList();
			foreach (string missing in plan.MissingFiles)
			{
				_missingPatchFiles.Add(missing);
				Log(info[47], new() { ["VAR_SOME_FILE"] = missing }, Verbosity.Error, LogType.ConsoleOnly);
			}
			Log(info[74], new()
			{
				["VAR_PATCH_COUNT"] = plan.CandidateCount.ToString(),
				["VAR_MATCHED_COUNT"] = plan.Targets.Count.ToString()
			}, Verbosity.Info, LogType.ConsoleOnly);
			if (!plan.IsComplete)
			{
				WriteDiagnosticFiles();
				Log(info[53], null, Verbosity.Error);
				SelectPatchFolderButton.Enabled = true;
				return false;
			}

			planAccepted = true;
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
			else if (!planAccepted)
			{
				_filesToPatch.Clear();
				_patchesToApply.Clear();
				PatchPathPreviewTextbox.Clear();
				InstallButton.Enabled = false;
			}
		}
	}

}
