namespace V3UPSManager;

public partial class MainWindow : Form
{
	private GameBase CurrentGame { get; set; } = new DRV3();

	private string _installationFolder = "";
	private string _patchFolder = "";
	private string _verifiedInstallationFolder = "";

	private bool _manuallySelectedGame;
	private bool _manuallySelectedInstall;
	private bool _manuallySelectedPatch;

	private int _fileOperationRunning;

	public MainWindow(string[] args)
	{
		InitializeComponent();

		PrepareLanguageSelector();
		LoadLanguages();
		InitLanguages();
		ProcessCommandLineArguments(args);

		if (info_it.Count != info_en.Count ||
			ui_messages_it.Count != ui_messages_en.Count)
		{
			Log(ui_messages[7], null, Verbosity.Error);
		}
	}

	/// <summary>
	/// Prevents the language selector from firing while its initial contents
	/// are being populated.
	/// </summary>
	private void PrepareLanguageSelector()
	{
		if (LanguageComboBox == null)
		{
			return;
		}

		LanguageComboBox.SelectedIndexChanged -= LanguageComboBox_SelectedIndexChanged;
		LanguageComboBox.Enabled = false;
		LanguageComboBox.SelectedIndex = -1;
	}

	private void ProcessCommandLineArguments(string[] args)
	{
		if (args == null || args.Length == 0)
		{
			return;
		}

		string? requestedInstallFolder = null;
		string? requestedPatchFolder = null;
		Game? requestedGame = null;

		for (int i = 0; i < args.Length; i++)
		{
			switch (args[i])
			{
				case "--game":
					if (!TryReadArgument(args, ref i, info[45], out string? gameValue))
					{
						continue;
					}

					if (Enum.TryParse(gameValue, true, out Game parsedGame) &&
						parsedGame != Game.None &&
						Enum.IsDefined(typeof(Game), parsedGame))
					{
						requestedGame = parsedGame;
					}
					else
					{
						Log(info[45], null, Verbosity.Error);
					}
					break;

				case "--install-dir":
					TryReadArgument(
						args,
						ref i,
						info[51],
						out requestedInstallFolder);
					break;

				case "--patch-folder":
					TryReadArgument(
						args,
						ref i,
						info[52],
						out requestedPatchFolder);
					break;
			}
		}

		ApplyCommandLineGame(requestedGame);
		ApplyCommandLineInstallationFolder(requestedInstallFolder);
		ApplyCommandLinePatchFolder(requestedPatchFolder);
	}

	private bool TryReadArgument(
		string[] args,
		ref int index,
		string errorMessage,
		out string? value)
	{
		if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
		{
			Log(errorMessage, null, Verbosity.Error);
			value = null;
			return false;
		}

		value = args[++index];
		return true;
	}

	private void ApplyCommandLineGame(Game? requestedGame)
	{
		if (!requestedGame.HasValue)
		{
			return;
		}

		CurrentGame = GameCatalog.Create(requestedGame.Value);
		_manuallySelectedGame = CurrentGame.GameID != Game.None;

		if (_manuallySelectedGame)
		{
			Text = $"V3 UPS Manager - {CurrentGame.GameID}";
		}
	}

	private void ApplyCommandLineInstallationFolder(string? folder)
	{
		if (string.IsNullOrWhiteSpace(folder))
		{
			return;
		}

		_installationFolder = folder;
		_manuallySelectedInstall = true;

		if (!LoadInstallationFolder())
		{
			_installationFolder = "";
			_verifiedInstallationFolder = "";
			_manuallySelectedInstall = false;
			return;
		}

		InstallationPathPreviewTextbox.Text = _installationFolder;
		SelectInstallationFolderButton.Enabled = false;
	}

	private void ApplyCommandLinePatchFolder(string? folder)
	{
		if (string.IsNullOrWhiteSpace(folder) ||
			string.IsNullOrWhiteSpace(_verifiedInstallationFolder))
		{
			return;
		}

		_patchFolder = folder;
		_manuallySelectedPatch = true;

		if (!LoadPatchFolder())
		{
			_patchFolder = "";
			_manuallySelectedPatch = false;
			return;
		}

		PatchPathPreviewTextbox.Text = _patchFolder;
		SelectPatchFolderButton.Enabled = false;
	}

	private void SelectInstallationFolderButton_Click(object sender, EventArgs e)
		=> LoadInstallationFolder();

	private void SelectPatchFolderButton_Click(object sender, EventArgs e)
		=> LoadPatchFolder();

	private void InstallButton_Click(object sender, EventArgs e)
	{
		RunFileOperation(
			operation: () =>
			{
				Log(info[95], null, Verbosity.Info, LogType.ConsoleOnly);
				RefreshLogConsole();

				// Take the clean snapshot before touching the installation.
				if (!BackupChanges(false))
				{
					Log(info[96], null, Verbosity.Error, LogType.ConsoleOnly);
					RefreshLogConsole();
					return false;
				}

				// Install() owns the patching/rollback transaction.
				if (!Install())
				{
					Log(info[97], null, Verbosity.Error, LogType.ConsoleOnly);
					RefreshLogConsole();
					return false;
				}

				// Keep a separate snapshot of the successfully patched files.
				if (!BackupChanges(true))
				{
					Log(info[98], null, Verbosity.Error, LogType.ConsoleOnly);
					RefreshLogConsole();
					return false;
				}

				DisplayStatus();
				return true;
			},
			keepInstallButtonDisabledOnSuccess: true);
	}

	private void UninstallButton_Click(object sender, EventArgs e)
		=> RunFileOperation(
			operation: () =>
			{
				return Uninstall();
			},
			keepInstallButtonDisabledOnSuccess: false);

	/// <summary>
	/// Common synchronization/UI wrapper for all operations that mutate files.
	/// Keeping this in one place prevents install and uninstall from drifting
	/// apart when controls or locking behaviour change.
	/// </summary>
	private void RunFileOperation(
		Func<bool> operation,
		bool keepInstallButtonDisabledOnSuccess)
	{
		if (Interlocked.Exchange(ref _fileOperationRunning, 1) != 0)
		{
			Log(info[99], null, Verbosity.Error, LogType.ConsoleOnly);
			RefreshLogConsole();
			return;
		}

		Mutex? operationMutex = null;
		bool mutexAcquired = false;
		bool operationSucceeded = false;
		ControlState previousState = CaptureControlState();

		try
		{
			operationMutex = new Mutex(false, FileOperationLock.Name);
			mutexAcquired = FileOperationLock.TryAcquire(operationMutex);
			if (!mutexAcquired)
			{
				Log(info[99], null, Verbosity.Error, LogType.ConsoleOnly);
				RefreshLogConsole();
				return;
			}

			SetOperationControlsEnabled(false);
			operationSucceeded = operation();
		}
		catch (Exception exception)
		{
			Log(info[70], new()
			{
				["VAR_OUTPUT_FILE"] = _verifiedInstallationFolder,
				["VAR_EXCEPTION"] = exception.Message
			}, Verbosity.Error);
		}
		finally
		{
			if (mutexAcquired)
			{
				operationMutex!.ReleaseMutex();
			}

			operationMutex?.Dispose();
			Interlocked.Exchange(ref _fileOperationRunning, 0);
			RestoreControlState(previousState, operationSucceeded, keepInstallButtonDisabledOnSuccess);
		}
	}

	private ControlState CaptureControlState()
	{
		return new ControlState(
			InstallButton.Enabled,
			SelectInstallationFolderButton.Enabled,
			SelectPatchFolderButton.Enabled,
			UninstallButton.Enabled,
			ShouldMakeBackupsCheckbox.Enabled);
	}

	private void SetOperationControlsEnabled(bool enabled)
	{
		InstallButton.Enabled = enabled;
		SelectInstallationFolderButton.Enabled = enabled;
		SelectPatchFolderButton.Enabled = enabled;
		UninstallButton.Enabled = enabled;
		ShouldMakeBackupsCheckbox.Enabled = enabled;
	}

	private void RestoreControlState(
		ControlState state,
		bool operationSucceeded,
		bool keepInstallButtonDisabledOnSuccess)
	{
		InstallButton.Enabled = operationSucceeded
			? !keepInstallButtonDisabledOnSuccess && _filesToPatch.Count > 0
			: state.InstallButtonEnabled;

		SelectInstallationFolderButton.Enabled = state.InstallationButtonEnabled;
		SelectPatchFolderButton.Enabled = state.PatchButtonEnabled;
		UninstallButton.Enabled = state.UninstallButtonEnabled;
		ShouldMakeBackupsCheckbox.Enabled = state.BackupCheckboxEnabled;
	}

	private readonly record struct ControlState(
		bool InstallButtonEnabled,
		bool InstallationButtonEnabled,
		bool PatchButtonEnabled,
		bool UninstallButtonEnabled,
		bool BackupCheckboxEnabled);
}
