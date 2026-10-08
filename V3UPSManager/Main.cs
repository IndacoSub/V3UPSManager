namespace V3UPSManager;

public partial class MainWindow : Form
{
	public GameBase CurrentGame = new DRV3();

	private string installation_folder = "";
	private string ups_folder = "";
	private string verified_installation_folder = "";

	private bool manually_selected_game = false;
	private bool manually_selected_install = false;
	private bool manually_selected_patch = false;

	// Interlocked gate: prevents re-entry in this process even if another event
	// path invokes the click handler while an installation is still active.
	private int file_operation_running = 0;

	// Named mutex: prevents a second V3UPSManager process from modifying the same
	// game/backup state concurrently. The mutex is machine-local and only covers
	// the file-mutating operation itself.
	private const string file_operation_mutex_name =
		"Local\\V3UPSManager.FileOperation";

	public MainWindow(string[] args)
	{
		InitializeComponent();

		if (LanguageComboBox != null)
		{
			// The designer wires the event during InitializeComponent().
			// Disable it while the localization list is being built so that
			// temporary SelectedIndex changes cannot trigger a partial load.
			LanguageComboBox.SelectedIndexChanged -= LanguageComboBox_SelectedIndexChanged;
			LanguageComboBox.Enabled = false;
			LanguageComboBox.SelectedIndex = -1;
		}

		LoadLanguages();
		InitLanguages();
		ProcessCommandLineArguments(args);

		if (info_it.Count != info_en.Count ||
			ui_messages_it.Count != ui_messages_en.Count)
		{
			// Keep this warning localized through the normal translation table.
			Log(ui_messages[7], null, Verbosity.Error);
		}
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
			string argument = args[i];

			switch (argument)
			{
				case "--game":
					if (i + 1 >= args.Length)
					{
						Log(info[45], null, Verbosity.Error);
						continue;
					}

					string gameValue = args[++i];

					if (Enum.TryParse<Game>(gameValue, true, out Game parsedGame) &&
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
					if (i + 1 >= args.Length)
					{
						Log(info[51], null, Verbosity.Error);
						continue;
					}

					requestedInstallFolder = args[++i];
					break;

				case "--patch-folder":
					if (i + 1 >= args.Length)
					{
						Log(info[52], null, Verbosity.Error);
						continue;
					}

					requestedPatchFolder = args[++i];
					break;
			}
		}

		// Apply the requested game first. This also makes --install-dir and
		// --patch-folder independent from their order on the command line.
		if (requestedGame.HasValue)
		{
			CurrentGame = SpawnGameByID(requestedGame.Value);
			manually_selected_game = CurrentGame.GameID != Game.None;

			if (manually_selected_game)
			{
				Text = "V3 UPS Manager" + " - " + CurrentGame.GameID;
			}
		}

		if (!string.IsNullOrWhiteSpace(requestedInstallFolder))
		{
			installation_folder = requestedInstallFolder;
			manually_selected_install = true;

			if (LoadInstallationFolder())
			{
				InstallationPathPreviewTextbox.Text = installation_folder;
				SelectInstallationFolderButton.Enabled = false;
			}
			else
			{
				installation_folder = "";
				verified_installation_folder = "";
				manually_selected_install = false;
			}
		}

		if (!string.IsNullOrWhiteSpace(requestedPatchFolder) &&
			!string.IsNullOrWhiteSpace(verified_installation_folder))
		{
			ups_folder = requestedPatchFolder;
			manually_selected_patch = true;

			if (LoadPatchFolder())
			{
				PatchPathPreviewTextbox.Text = ups_folder;
				SelectPatchFolderButton.Enabled = false;
			}
			else
			{
				ups_folder = "";
				manually_selected_patch = false;
			}
		}
	}

	private void SelectInstallationFolderButton_Click(object sender, EventArgs e)
	{
		LoadInstallationFolder();
	}

	private void SelectPatchFolderButton_Click(object sender, EventArgs e)
	{
		LoadPatchFolder();
	}

	private void InstallButton_Click(object sender, EventArgs e)
	{
		// The installer must never be re-entrant. This is the primary protection
		// against two install flows touching the same files at the same time.
		if (Interlocked.Exchange(ref file_operation_running, 1) != 0)
		{
			Log(info[99], null, Verbosity.Error, LogType.ConsoleOnly);
			RefreshLogConsole();
			return;
		}

		using Mutex operationMutex =
			new Mutex(false, file_operation_mutex_name);

		bool mutexAcquired = false;
		bool installationSucceeded = false;
		bool previousInstallButtonEnabled = InstallButton.Enabled;
		bool previousInstallFolderButtonEnabled = SelectInstallationFolderButton.Enabled;
		bool previousPatchFolderButtonEnabled = SelectPatchFolderButton.Enabled;
		bool previousUninstallButtonEnabled = UninstallButton.Enabled;
		bool previousBackupCheckboxEnabled = ShouldMakeBackupsCheckbox.Enabled;

		try
		{
			try
			{
				// Zero-timeout acquisition is intentional: if another V3UPSManager
				// process is installing, fail immediately instead of ever touching
				// the shared Backup/ModdedFiles directories concurrently.
				mutexAcquired = operationMutex.WaitOne(0);
			}
			catch (AbandonedMutexException)
			{
				// The previous process died while holding the mutex. Windows has
				// transferred ownership to us; continuing is safe because no live
				// process still owns the operation lock.
				mutexAcquired = true;
			}

			if (!mutexAcquired)
			{
				Log(info[99], null, Verbosity.Error, LogType.ConsoleOnly);
				RefreshLogConsole();
				return;
			}

			// Disable every control which could change the installer state while
			// the transaction is running. The backup checkbox is included because
			// BackupChanges() is intentionally based on its value.
			InstallButton.Enabled = false;
			SelectInstallationFolderButton.Enabled = false;
			SelectPatchFolderButton.Enabled = false;
			UninstallButton.Enabled = false;
			ShouldMakeBackupsCheckbox.Enabled = false;

			// Mark the exact moment the user starts the operation.
			Log(info[95], null, Verbosity.Info, LogType.ConsoleOnly);
			RefreshLogConsole();

			// First create the immutable pre-install snapshot. If this fails,
			// do not touch the game files.
			if (!BackupChanges(false))
			{
				Log(info[96], null, Verbosity.Error, LogType.ConsoleOnly);
				RefreshLogConsole();
				return;
			}

			// Install() performs the patching transactionally. A failed patch
			// is rolled back and does not produce the post-install snapshot.
			if (!Install())
			{
				Log(info[97], null, Verbosity.Error, LogType.ConsoleOnly);
				RefreshLogConsole();
				return;
			}

			// Store the successfully patched files separately.
			if (!BackupChanges(true))
			{
				Log(info[98], null, Verbosity.Error, LogType.ConsoleOnly);
				RefreshLogConsole();
				return;
			}

			DisplayStatus();
			installationSucceeded = true;
		}
		finally
		{
			if (mutexAcquired)
			{
				operationMutex.ReleaseMutex();
			}

			Interlocked.Exchange(ref file_operation_running, 0);

			// Restore only controls which are allowed to become usable after the
			// transaction. A successful installation keeps Install disabled until
			// the normal status flow decides otherwise; failure restores the exact
			// pre-operation state.
			InstallButton.Enabled = installationSucceeded
				? false
				: previousInstallButtonEnabled;
			SelectInstallationFolderButton.Enabled = previousInstallFolderButtonEnabled;
			SelectPatchFolderButton.Enabled = previousPatchFolderButtonEnabled;
			UninstallButton.Enabled = previousUninstallButtonEnabled;
			ShouldMakeBackupsCheckbox.Enabled = previousBackupCheckboxEnabled;
		}
	}

	private void UninstallButton_Click(object sender, EventArgs e)
	{
		// Use the same process-wide and cross-process lock as installation so an
		// uninstall can never modify game files while another manager instance is
		// installing/updating them.
		if (Interlocked.Exchange(ref file_operation_running, 1) != 0)
		{
			Log(info[99], null, Verbosity.Error, LogType.ConsoleOnly);
			RefreshLogConsole();
			return;
		}

		using Mutex operationMutex =
			new Mutex(false, file_operation_mutex_name);

		bool mutexAcquired = false;
		bool previousInstallButtonEnabled = InstallButton.Enabled;
		bool previousInstallFolderButtonEnabled = SelectInstallationFolderButton.Enabled;
		bool previousPatchFolderButtonEnabled = SelectPatchFolderButton.Enabled;
		bool previousUninstallButtonEnabled = UninstallButton.Enabled;
		bool previousBackupCheckboxEnabled = ShouldMakeBackupsCheckbox.Enabled;

		try
		{
			try
			{
				mutexAcquired = operationMutex.WaitOne(0);
			}
			catch (AbandonedMutexException)
			{
				// The previous process died while holding the mutex; ownership was
				// transferred to this process, so the operation lock is ours now.
				mutexAcquired = true;
			}

			if (!mutexAcquired)
			{
				Log(info[99], null, Verbosity.Error, LogType.ConsoleOnly);
				RefreshLogConsole();
				return;
			}

			InstallButton.Enabled = false;
			SelectInstallationFolderButton.Enabled = false;
			SelectPatchFolderButton.Enabled = false;
			UninstallButton.Enabled = false;
			ShouldMakeBackupsCheckbox.Enabled = false;

			Uninstall();
		}
		finally
		{
			if (mutexAcquired)
			{
				operationMutex.ReleaseMutex();
			}

			Interlocked.Exchange(ref file_operation_running, 0);

			InstallButton.Enabled = previousInstallButtonEnabled;
			SelectInstallationFolderButton.Enabled = previousInstallFolderButtonEnabled;
			SelectPatchFolderButton.Enabled = previousPatchFolderButtonEnabled;
			UninstallButton.Enabled = previousUninstallButtonEnabled;
			ShouldMakeBackupsCheckbox.Enabled = previousBackupCheckboxEnabled;
		}
	}
}
