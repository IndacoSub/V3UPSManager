using System.Text;

namespace V3UPSManager;

public partial class MainWindow : Form
{
	private List<string> info_en = new List<string>();
	private List<string> info_it = new List<string>();

	private List<string> ui_messages_en = new List<string>();
	private List<string> ui_messages_it = new List<string>();

	private void LoadLanguages()
	{
		/* Italian */

		info_it = new List<string>
		{
			"La cartella di installazione non esiste!", // 0
			"L'eseguibile (\"VAR_LEGACY_EXE_NAME\") non è stato trovato (normale per installazioni versione Unity o Xbox (Anniversary))!", // 1
			"Il file \"language.txt\" non è stato trovato!\\nPer favore avvia il gioco almeno una volta\\n(e controlla che funzioni).", // 2
			"Il gioco non è in inglese.\\nPer favore disinstalla e installa nuovamente il gioco\\ncon la lingua inglese.", // 3
			"La/e cartella/e \"data\" e \"win\" (o \"WIN\") non sono state trovate!", // 4
			"Sono stati trovati dei file CPK in \"data\\win\"!\\nAvresti dovuto estrarli e spostarli altrove.", // 5
			"Sono stati trovati dei file CPK non-inglesi in \"data\\win\"!\\nPer favore disinstalla e installa nuovamente il gioco\\ncon la lingua inglese.", // 6
			"Non è stato possibile calcolare l'hash MD5 per \"VAR_LEGACY_EXE_NAME\"! (È uscito un nuovo aggiornamento?)", // 7
			"Non hai l'ultima versione del gioco,\\nhai selezionato la versione demo, oppure\\nstai usando una copia pirata del gioco.\\nNota: noi non supportiamo la pirateria e non\\nriceverai supporto/aiuti per l'installazione, nel caso.\\nIl gioco potrebbe non funzionare a dovere usando una versione vecchia del gioco.\\nProcedere comunque?", // 8
			"È stata trovata un'installazione di ReShade.\\nIl gioco potrebbe non funzionare a dovere con ReShade attivo.\\nProcedere comunque?", // 9
			"È stato trovato VAR_GAME_FIX.\\nIl gioco potrebbe non funzionare a dovere con VAR_GAME_FIX attivo.\\nProcedere comunque?", // 10
			"La cartella \"data\\win\\boot\" non è stata trovata!", // 11
			"La cartella \"data\\win\\flash\" non è stata trovata!", // 12
			"La cartella \"data\\win\\game_resident\" non è stata trovata!", // 13
			"La cartella \"data\\win\\minigame\" non è stata trovata!", // 14
			"La cartella \"data\\win\\trial_font\" non è stata trovata!", // 15
			"La cartella \"data\\win\\wrd_data\" non è stata trovata!", // 16
			"La cartella \"data\\win\\wrd_script\" non è stata trovata!", // 17
			"Non è stato trovato alcun problema!\\nEvviva!", // 18
			"La cartella di installazione non è valida!", // 19
			"La cartella contenente le patch non è valida!", // 20
			"La/e cartella/e \"data\" e/o \"data\\win\" non sono state trovate!", // 21
			"Non è stato trovato alcun file VAR_PATCH_FORMAT_EXTENSION!", // 22
			"Informazioni sui file caricate! Ci siamo!", // 23
			"\"VAR_PATCH_FORMAT_INSTALLER_EXE\" non trovato!\\nLeggi le istruzioni nel file README!", // 24
			"Impossibile caricare la cartella di installazione o la cartella delle patch!", // 25
			"Non sono state trovate informazioni sui file SPC!", // 26
			"Non sono state trovate informazioni sui file VAR_PATCH_FORMAT!", // 27
			"Qualcosa non quadra coi file SPC!", // 28
			"Qualcosa non quadra coi file VAR_PATCH_FORMAT!", // 29
			"Fatto! Buon divertimento!", // 30
			"Versione del gioco sconosciuta/non supportata.", // 31
			"Non è stato trovato alcun file da disinstallare.", // 32
			"Disinstallazione completata!", // 33
			"Non è stato possibile installare alcuni file...", // 34
			"La/e cartella/e \"Data\" e/o \"Data\\StreamingAssets\" non sono state trovate!", // 35
			"Non sono state trovate informazioni sui file SPC/AB/Assets!", // 36
			"Non è stato trovato l'eseguibile (\"VAR_ANNIVERSARY_EXE_NAME\") (normale per installazioni versione Switch o \"Legacy\" (Steam))!", // 37
			"La cartella del gioco (versione \"Legacy\" (Steam)) contiene dei file ARC provenienti dalla versione Xbox (Anniversary).", // 38
			"La cartella del gioco (\"Anniversary Edition\") contiene dei file provenienti dalla versione \"Legacy\" (Steam).", // 39
			"Sono stati trovati dei file ARC in \"data\\WIN\"!\\nAvresti dovuto estrarli e spostarli altrove.\\n(Al momento non siamo in grado di estrarli, sfortunatamente...)", // 40
			"Il pulsante \"Disinstalla\" funziona solamente con le versioni Legacy (Steam) e Xbox!\\nÈ molto facile disinstallare una mod per Switch/Emulatori, basta eliminare la cartella della mod!", // 41
			"Versione Switch (Anniversary Edition) riconosciuta!", // 42
			"Non sono stati trovati file a cui applicare la patch nella cartella di installazione?\\nL'installazione non può procedere, assicurati che sia la cartella giusta (es. non \"win\" o \"data\" nel caso di Danganronpa V3)", // 43
			"Sono stati trovati dei file VAR_PATCH_FORMAT, ma non contengono \"VAR_PATCH_SPECIFIC_STRING\" come invece dovrebbero (rinominali manualmente?)\\nInoltre, non devono contenere due estensioni (ad esempio \"myfile.SPCVAR_PATCH_SPECIFIC_STRING\" non va bene), ma \"VAR_PATCH_SPECIFIC_STRING\" (esempio corretto: \"myfileVAR_PATCH_SPECIFIC_STRING\")", // 44
			"Non è stato possibile identificare il gioco (segnala l'errore)", // 45
			"Non è stato possibile creare un'istanza per il gioco (segnala l'errore)", // 46
			"Non è stato possibile trovare VAR_SOME_FILE", // 47
			"Non è stato trovato l'eseguibile (\"VAR_UNITY_EXE_NAME\") (normale per installazioni versioni non-Unity)!", // 48
			"Sono stati trovati SpecialK o Reloaded-II.\\nIl gioco potrebbe non funzionare a dovere.\\nProcedere comunque?", // 49
			"Il comando dell'installazione è vuoto (segnala l'errore)", // 50
			"Manca il valore per --install-dir.", // 51
			"Manca il valore per --patch-folder.", // 52
			"Non è stato possibile associare tutti i file della patch ai file dell'installazione.", // 53
			"Un file non è stato installato correttamente. L'installazione è stata annullata e i file modificati sono stati ripristinati.", // 54
			"Backup creato: VAR_BACKUP_FILE", // 55
			"Installazione avviata. File da elaborare: VAR_FILE_COUNT.", // 56
			"Elaborazione file VAR_FILE_INDEX/VAR_FILE_COUNT: VAR_OUTPUT_FILE", // 57
			"Eseguibile saltato: VAR_OUTPUT_FILE (accesso agli EXE disabilitato).", // 58
			"Backup originale già presente: VAR_BACKUP_FILE", // 59
			"Backup originale creato: VAR_BACKUP_FILE", // 60
			"Applicazione patch: VAR_PATCH_FILE -> VAR_OUTPUT_FILE", // 61
			"Comando patch: VAR_COMMAND", // 62
			"Tool patch terminato con codice VAR_EXIT_CODE per VAR_OUTPUT_FILE.", // 63
			"Output del tool patch: VAR_TOOL_OUTPUT", // 64
			"Errore/output stderr del tool patch: VAR_TOOL_ERROR", // 65
			"File patchato correttamente: VAR_OUTPUT_FILE", // 66
			"Verifica file VAR_FILE_INDEX/VAR_FILE_COUNT: VAR_OUTPUT_FILE", // 67
			"Installazione completata correttamente. File modificati: VAR_MODIFIED_COUNT/VAR_FILE_COUNT.", // 68
			"Ripristino file: VAR_OUTPUT_FILE", // 69
			"Operazione fallita su VAR_OUTPUT_FILE: VAR_EXCEPTION", // 70
			"Creazione snapshot backup: VAR_BACKUP_DIRECTORY", // 71
			"Snapshot backup completato: VAR_BACKUP_DIRECTORY", // 72
			"Associazione patch: VAR_PATCH_FILE -> VAR_OUTPUT_FILE", // 73
			"Trovati VAR_PATCH_COUNT file patch; associati VAR_MATCHED_COUNT file all'installazione.", // 74
			"Riepilogo patch pronto. File da installare: VAR_FILE_COUNT.", // 75
			"Backup fallito: è stato trovato un percorso file vuoto nell'elenco di installazione.", // 76
			"Backup fallito: file sorgente non trovato: VAR_SOURCE_FILE", // 77
			"Backup fallito: è stato richiesto un file esterno all'installazione verificata: VAR_SOURCE_FILE", // 78
			"Backup fallito: impossibile determinare il percorso di destinazione per VAR_SOURCE_FILE", // 79
			"Backup fallito: destinazione backup duplicata: VAR_BACKUP_FILE", // 80
			"Snapshot backup non pubblicabile: VAR_BACKUP_DIRECTORY", // 81
			"Installazione impossibile da avviare: la cartella di installazione verificata non è valida.", // 82
			"Installazione impossibile da avviare: la cartella delle patch non è valida.", // 83
			"Installazione impossibile da avviare: nessun file è pronto per essere patchato.", // 84
			"Installazione impossibile da avviare: numero di patch/file non corrispondente. Patch: VAR_PATCH_COUNT; file: VAR_FILE_COUNT.", // 85
			"Installazione impossibile da avviare: patch tool non trovato: VAR_PATCH_TOOL", // 86
			"Installazione annullata: è stato trovato un percorso file di destinazione vuoto.", // 87
			"Installazione annullata: file di destinazione non trovato: VAR_OUTPUT_FILE", // 88
			"Installazione annullata: file patch non trovato: VAR_PATCH_FILE", // 89
			"Installazione annullata: il patch tool ha segnalato successo ma non ha prodotto il file di output: VAR_OUTPUT_FILE", // 90
			"Installazione annullata: impossibile avviare il patch tool.", // 91
			"Installazione annullata: formato patch non supportato.", // 92
			"Verifica installazione fallita: manca il file di destinazione o il backup originale: VAR_OUTPUT_FILE", // 93
			"Preparazione dei backup originali prima dell'avvio del patch tool.", // 94
			"Richiesta installazione/aggiornamento.", // 95
			"Backup pre-installazione fallito. Il patch tool non è stato avviato.", // 96
			"Fase di installazione fallita. Lo snapshot post-installazione non è stato creato.", // 97
			"Snapshot post-installazione fallito. I file patchati del gioco rimangono installati.", // 98
			"Un'operazione di installazione/aggiornamento/disinstallazione è già in corso. Attendi che termini prima di avviarne un'altra.", // 99
		};

		ui_messages_it = new List<string>
		{
			"ATTENZIONE", // 0
			"Sembra esserci una cartella con lo stesso nome del file. Procedere con l'installazione?", // 1
			"Seleziona cartella di installazione", // 2
			"Seleziona cartella patch", // 3
			"Installa / Aggiorna", // 4
			"Disinstalla", // 5
			"POST-INSTALL - File inesistente: VAR_OUTPUT_FILE, lo creo manualmente dal backup... (segnala l'errore)", // 6
			"Qualcuno non ha aggiornato le traduzioni IT o EN 👀 (segnala l'errore)", // 7
		};

		/* English */

		info_en = new List<string>
		{
			"The installation folder doesn't exist!", // 0
			"The executable (\"VAR_LEGACY_EXE_NAME\") couldn't be found (normal for Unity-version or Xbox (Anniversary) installations)!", // 1
			"The file \"language.txt\" couldn't be found!\\nPlease boot the game at least once\\n(and make sure it's working).", // 2
			"Your game language is not set to English.\\nPlease uninstall and reinstall the game\\nwith the English language instead.", // 3
			"The \"data\" and/or \"win\" (or \"WIN\") folders couldn't be found!", // 4
			"CPK file(s) found in \"data\\win\"!\\nYou were supposed to extract them\\nand move them elsewhere.", // 5
			"Non-English CPK file(s) found in \"data\\win\"!\\nPlease uninstall and reinstall the game\\nwith the English language instead.", // 6
			"Couldn't compute MD5 hash for \"VAR_LEGACY_EXE_NAME\"! (Did a new update come out?)", // 7
			"You aren't using the latest version of the game,\\nmaybe you chose the Demo version folder,\\nor you might be using a pirated copy.\\nPlease note that we don't support piracy and you won't\\nreceive support for this installation, if so.\\nThings might not work as expected while using an older version of the game.\\nProceed anyway?", // 8
			"A ReShade installation was found.\\nThings might not work as expected while using ReShade.\\nProceed anyway?", // 9
			"VAR_GAME_FIX was found.\\nThings might not work as expected while using VAR_GAME_FIX.\\nProceed anyway?", // 10
			"The \"data\\win\\boot\" folder couldn't be found!", // 11
			"The \"data\\win\\flash\" folder couldn't be found!", // 12
			"The \"data\\win\\game_resident\" folder couldn't be found!", // 13
			"The \"data\\win\\minigame\" folder couldn't be found!", // 14
			"The \"data\\win\\trial_font\" folder couldn't be found!", // 15
			"The \"data\\win\\wrd_data\" folder couldn't be found!", // 16
			"The \"data\\win\\wrd_script\" folder couldn't be found!", // 17
			"No problems found with your installation!\\nHooray!", // 18
			"The installation folder is not valid!", // 19
			"The patch folder doesn't exist!", // 20
			"The \"data\" and/or \"data\\win\" folders couldn't be found!", // 21
			"Couldn't find any VAR_PATCH_FORMAT_EXTENSION file(s)!", // 22
			"File information loaded! Ready to install / update!", // 23
			"\"VAR_PATCH_FORMAT_INSTALLER_EXE\" not found!\\nRead the instructions in the README file!", // 24
			"The installation folder and/or the patch folder couldn't be loaded!", // 25
			"No SPC file info found!", // 26
			"No VAR_PATCH_FORMAT file info found!", // 27
			"Something is wrong with the SPC files!", // 28
			"Something is wrong with the VAR_PATCH_FORMAT files!", // 29
			"Done! Have fun!", // 30
			"Unknown/Unsupported game version.", // 31
			"No file to be uninstalled was found.", // 32
			"Done uninstalling!", // 33
			"Some files couldn't be installed...", // 34
			"The \"Data\" and/or \"Data\\StreamingAssets\" folders couldn't be found!", // 35
			"No SPC/AB/Assets file info found!", // 36
			"The executable (\"VAR_ANNIVERSARY_EXE_NAME\") couldn't be found (normal for Switch and \"Legacy\" (Steam) installations)!", // 37
			"The game's folder (\"Legacy\" (Steam) version) contains files from the Anniversary Edition.", // 38
			"The game's folder (\"Anniversary Edition\") contains files from the \"Legacy\" (Steam) version.", // 39
			"ARC file(s) found in \"data\\win\"!\\nYou were supposed to extract them\\nand move them elsewhere.\\n(At this time we can't extract them, unfortunately...)", // 40
			"The \"Uninstall\" button only works with the Legacy (Steam) and Xbox versions!\\nIt's much easier to uninstall mods on Switch/Emulators, you just need to delete the mod folder(s)!", // 41
			"Switch version (Anniversary Edition) recognized!", // 42
			"No file(s) in the installation folder are ready to be patched\\nThe installation cannot proceed, make sure you selected the right folder (ex. not \"win\" or \"data\" in the case of Danganronpa V3)", // 43
			"VAR_PATCH_FORMAT files were found, but they aren't \"VAR_PATCH_SPECIFIC_STRING\" files, as they should instead be (rename them manually?)\\nAlso, they must NOT contain two extensions (for example, \"myfile.SPCVAR_PATCH_SPECIFIC_STRING\" is not a valid name), only \"VAR_PATCH_SPECIFIC_STRING\" (correct example: \"myfileVAR_PATCH_SPECIFIC_STRING\")", // 44
			"Couldn't identify the game (please report this)", // 45
			"Couldn't create an instance for the game (please report this)", // 46
			"Couldn't find VAR_SOME_FILE", // 47
			"The executable (\"VAR_UNITY_EXE_NAME\") couldn't be found (normal for non-Unity installations)!", // 48
			"SpecialK or Reloaded-II were found.\\nThings might not work as expected.\\nProceed anyway?", // 49
			"The installation command is empty (please report this)", // 50
			"Missing value for --install-dir.", // 51
			"Missing value for --patch-folder.", // 52
			"Could not associate every patch file with its corresponding installation file.", // 53
			"A file was not installed correctly. The installation was cancelled and modified files were restored.", // 54
			"Backup created: VAR_BACKUP_FILE", // 55
			"Installation started. Files to process: VAR_FILE_COUNT.", // 56
			"Processing file VAR_FILE_INDEX/VAR_FILE_COUNT: VAR_OUTPUT_FILE", // 57
			"Executable skipped: VAR_OUTPUT_FILE (EXE access is disabled).", // 58
			"Original backup already exists: VAR_BACKUP_FILE", // 59
			"Original backup created: VAR_BACKUP_FILE", // 60
			"Applying patch: VAR_PATCH_FILE -> VAR_OUTPUT_FILE", // 61
			"Patch command: VAR_COMMAND", // 62
			"Patch tool finished with exit code VAR_EXIT_CODE for VAR_OUTPUT_FILE.", // 63
			"Patch tool output: VAR_TOOL_OUTPUT", // 64
			"Patch tool stderr: VAR_TOOL_ERROR", // 65
			"Patched file successfully: VAR_OUTPUT_FILE", // 66
			"Verifying file VAR_FILE_INDEX/VAR_FILE_COUNT: VAR_OUTPUT_FILE", // 67
			"Installation completed successfully. Files modified: VAR_MODIFIED_COUNT/VAR_FILE_COUNT.", // 68
			"Restoring file: VAR_OUTPUT_FILE", // 69
			"Operation failed for VAR_OUTPUT_FILE: VAR_EXCEPTION", // 70
			"Creating backup snapshot: VAR_BACKUP_DIRECTORY", // 71
			"Backup snapshot completed: VAR_BACKUP_DIRECTORY", // 72
			"Patch association: VAR_PATCH_FILE -> VAR_OUTPUT_FILE", // 73
			"Found VAR_PATCH_COUNT patch files; associated VAR_MATCHED_COUNT files with the installation.", // 74
			"Patch summary ready. Files to install: VAR_FILE_COUNT.", // 75
			"Backup failed: an empty file path was found in the installation list.", // 76
			"Backup failed: source file was not found: VAR_SOURCE_FILE", // 77
			"Backup failed: a file outside the verified installation was requested: VAR_SOURCE_FILE", // 78
			"Backup failed: the destination path could not be determined for VAR_SOURCE_FILE", // 79
			"Backup failed: duplicate backup destination: VAR_BACKUP_FILE", // 80
			"Backup snapshot could not be published: VAR_BACKUP_DIRECTORY", // 81
			"Installation could not start: the verified installation folder is invalid.", // 82
			"Installation could not start: the patch folder is invalid.", // 83
			"Installation could not start: no files are ready to be patched.", // 84
			"Installation could not start: patch/file count mismatch. Patches: VAR_PATCH_COUNT; files: VAR_FILE_COUNT.", // 85
			"Installation could not start: patch tool was not found: VAR_PATCH_TOOL", // 86
			"Installation aborted: an empty target file path was found.", // 87
			"Installation aborted: target file was not found: VAR_OUTPUT_FILE", // 88
			"Installation aborted: patch file was not found: VAR_PATCH_FILE", // 89
			"Installation aborted: patch tool reported success but produced no output file: VAR_OUTPUT_FILE", // 90
			"Installation aborted: the patch tool could not be started.", // 91
			"Installation aborted: unsupported patch format.", // 92
			"Installation verification failed: target file or original backup is missing: VAR_OUTPUT_FILE", // 93
			"Preparing original-file backups before starting the patch tool.", // 94
			"Install/update requested.", // 95
			"Pre-installation backup failed. The patch tool was not started.", // 96
			"Installation phase failed. The post-installation snapshot was not created.", // 97
			"Post-installation snapshot failed. The patched game files remain installed.", // 98
			"An install/update/uninstall operation is already in progress. Wait for it to finish before starting another one.", // 99
		};

		ui_messages_en = new List<string>()
		{
			"WARNING", // 0
			"There seems to be a folder with the exact same name as the file. Continue?", // 1
			"Choose installation folder", // 2
			"Choose patch folder", // 3
			"Install / Update", // 4
			"Uninstall", // 5
			"POST-INSTALL - File does not exist: VAR_OUTPUT_FILE, creating manually from backup... (please report this)", // 6
			"Somebody forgot to update IT or EN translations 👀 (please report this)", // 7
		};
	}

	private void InitLanguages()
	{
		string localizationDirectory = Path.Combine(
			AppContext.BaseDirectory,
			"Localization");

		// The directory and the built-in files are best-effort only.
		// The embedded arrays above remain the authoritative fallback.
		try
		{
			Directory.CreateDirectory(localizationDirectory);
			EnsureLocalizationFiles(localizationDirectory, "Italiano", info_it, ui_messages_it);
			EnsureLocalizationFiles(localizationDirectory, "English", info_en, ui_messages_en);
		}
		catch
		{
			// External localization files are optional.
		}

		if (LanguageComboBox == null)
		{
			return;
		}

		LanguageComboBox.BeginUpdate();

		try
		{
			LanguageComboBox.Items.Clear();

			// The default languages always exist in the executable itself.
			AddLanguageIfMissing(LanguageComboBox, "Italiano");
			AddLanguageIfMissing(LanguageComboBox, "English");

			try
			{
				foreach (string directory in Directory.EnumerateDirectories(
					localizationDirectory,
					"*",
					SearchOption.TopDirectoryOnly))
				{
					string languageName = Path.GetFileName(
						directory.TrimEnd(
							Path.DirectorySeparatorChar,
							Path.AltDirectorySeparatorChar));

					if (string.IsNullOrWhiteSpace(languageName) ||
						FindLanguageIndex(LanguageComboBox, languageName) >= 0)
					{
						continue;
					}

					string infoFile = Path.Combine(directory, "info.txt");
					string uiFile = Path.Combine(directory, "ui.txt");

					// Do not expose incomplete/broken translations in the UI.
					if (!File.Exists(infoFile) || !File.Exists(uiFile))
					{
						continue;
					}

					if (!TryReadLocalizationFile(infoFile, out List<string>? externalInfo) ||
						!TryReadLocalizationFile(uiFile, out List<string>? externalUi) ||
						externalInfo == null ||
						externalUi == null ||
						externalInfo.Count == 0 ||
						externalUi.Count == 0 ||
						externalInfo.Count > info_en.Count ||
						externalUi.Count > ui_messages_en.Count)
					{
						continue;
					}

					// Older custom translations may legitimately contain fewer entries
					// than the current version. Keep those languages available: missing
					// trailing entries are filled from English when the language is loaded.
					AddLanguageIfMissing(LanguageComboBox, languageName);
				}
			}
			catch
			{
				// Keep the built-in languages when a custom localization folder
				// cannot be enumerated.
			}

			List<string> languages = LanguageComboBox.Items
				.Cast<object>()
				.OfType<string>()
				.OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
				.ToList();

			LanguageComboBox.Items.Clear();

			foreach (string language in languages)
			{
				LanguageComboBox.Items.Add(language);
			}

			LanguageComboBox.Enabled = LanguageComboBox.Items.Count > 0;

			int englishIndex = FindLanguageIndex(LanguageComboBox, "English");

			if (englishIndex >= 0)
			{
				LanguageComboBox.SelectedIndex = englishIndex;
			}
			else if (LanguageComboBox.Items.Count > 0)
			{
				LanguageComboBox.SelectedIndex = 0;
			}
		}
		finally
		{
			LanguageComboBox.EndUpdate();
		}

		// The designer attaches this handler. We removed it in the constructor
		// so that initialization is silent; now it can safely be restored.
		LanguageComboBox.SelectedIndexChanged -= LanguageComboBox_SelectedIndexChanged;
		LanguageComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;

		// Apply the initial selection explicitly because the event was disabled
		// while the ComboBox was populated.
		CheckIndexChange();
	}

	private static void EnsureLocalizationFiles(
		string localizationDirectory,
		string languageName,
		IEnumerable<string> infoMessages,
		IEnumerable<string> uiMessages)
	{
		string languageDirectory = Path.Combine(
			localizationDirectory,
			languageName);

		Directory.CreateDirectory(languageDirectory);

		WriteLocalizationFileIfMissing(
			Path.Combine(languageDirectory, "info.txt"),
			infoMessages);

		WriteLocalizationFileIfMissing(
			Path.Combine(languageDirectory, "ui.txt"),
			uiMessages);
	}

	private static void WriteLocalizationFileIfMissing(
		string filePath,
		IEnumerable<string> messages)
	{
		if (File.Exists(filePath))
		{
			return;
		}

		// CreateNew prevents accidental overwrites if another process creates
		// the file between the existence check and file creation.
		try
		{
			using FileStream stream = new FileStream(
				filePath,
				FileMode.CreateNew,
				FileAccess.Write,
				FileShare.Read);

			using StreamWriter writer = new StreamWriter(
				stream,
				new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

			foreach (string message in messages ?? Array.Empty<string>())
			{
				writer.WriteLine(message);
			}
		}
		catch (IOException)
		{
			// The file may have been created concurrently; leave it untouched.
		}
	}

	private static bool TryReadLocalizationFile(
		string filePath,
		out List<string>? messages)
	{
		messages = null;

		try
		{
			// Throw on invalid UTF-8 instead of silently replacing corrupted
			// bytes with the replacement character.
			Encoding encoding = new UTF8Encoding(
				encoderShouldEmitUTF8Identifier: false,
				throwOnInvalidBytes: true);

			messages = File.ReadAllLines(filePath, encoding).ToList();
			return true;
		}
		catch
		{
			return false;
		}
	}

	private void CheckIndexChange()
	{
		if (LanguageComboBox == null ||
			LanguageComboBox.Items.Count == 0 ||
			LanguageComboBox.SelectedItem is not string languageName ||
			string.IsNullOrWhiteSpace(languageName))
		{
			return;
		}

		// Built-in languages do not depend on the filesystem at all.
		if (string.Equals(languageName, "Italiano", StringComparison.OrdinalIgnoreCase))
		{
			info = info_it;
			ui_messages = ui_messages_it;
			ApplyLocalizedInterface();
			return;
		}

		if (string.Equals(languageName, "English", StringComparison.OrdinalIgnoreCase))
		{
			info = info_en;
			ui_messages = ui_messages_en;
			ApplyLocalizedInterface();
			return;
		}

		string localizationDirectory = Path.Combine(
			AppContext.BaseDirectory,
			"Localization");

		string languageDirectory;
		string infoFile;
		string uiFile;

		try
		{
			languageDirectory = Path.Combine(localizationDirectory, languageName);
			infoFile = Path.Combine(languageDirectory, "info.txt");
			uiFile = Path.Combine(languageDirectory, "ui.txt");
		}
		catch
		{
			FallbackToEnglish(languageName);
			return;
		}

		if (!Directory.Exists(languageDirectory) ||
			!File.Exists(infoFile) ||
			!File.Exists(uiFile))
		{
			FallbackToEnglish(languageName);
			return;
		}

		if (!TryReadLocalizationFile(
			infoFile,
			out List<string>? newInfo) ||
			newInfo == null ||
			newInfo.Count == 0 ||
			newInfo.Count > info_en.Count)
		{
			FallbackToEnglish(languageName);
			return;
		}

		if (!TryReadLocalizationFile(
			uiFile,
			out List<string>? newUi) ||
			newUi == null ||
			newUi.Count == 0 ||
			newUi.Count > ui_messages_en.Count)
		{
			FallbackToEnglish(languageName);
			return;
		}

		// Preserve older/custom translations when new localization entries were
		// added in a later version. Missing trailing entries use the built-in
		// English text instead of making the entire language unusable.
		while (newInfo.Count < info_en.Count)
		{
			newInfo.Add(info_en[newInfo.Count]);
		}

		while (newUi.Count < ui_messages_en.Count)
		{
			newUi.Add(ui_messages_en[newUi.Count]);
		}

		info = newInfo;
		ui_messages = newUi;

		ApplyLocalizedInterface();
	}

	private void FallbackToEnglish(string invalidLanguage)
	{
		// Remove the known-invalid entry and change the selected item while the
		// event handler is detached. Otherwise removing/changing the selected
		// item can recursively invoke CheckIndexChange().
		LanguageComboBox.SelectedIndexChanged -= LanguageComboBox_SelectedIndexChanged;

		try
		{
			LanguageComboBox.Items.Remove(invalidLanguage);

			info = info_en;
			ui_messages = ui_messages_en;

			int englishIndex = FindLanguageIndex(
				LanguageComboBox,
				"English");

			if (englishIndex >= 0)
			{
				LanguageComboBox.SelectedIndex = englishIndex;
			}
			else if (LanguageComboBox.Items.Count > 0)
			{
				LanguageComboBox.SelectedIndex = 0;
			}
		}
		finally
		{
			LanguageComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;
		}

		LanguageComboBox.Enabled = LanguageComboBox.Items.Count > 0;
		ApplyLocalizedInterface();
	}

	private void ApplyLocalizedInterface()
	{
		if (ui_messages == null)
		{
			return;
		}

		if (SelectInstallationFolderButton != null &&
			ui_messages.Count > 2)
		{
			SelectInstallationFolderButton.Text =
				FormatLocalizedText(ui_messages[2]);
		}

		if (SelectPatchFolderButton != null &&
			ui_messages.Count > 3)
		{
			SelectPatchFolderButton.Text =
				FormatLocalizedText(ui_messages[3]);
		}

		if (InstallButton != null &&
			ui_messages.Count > 4)
		{
			InstallButton.Text =
				FormatLocalizedText(ui_messages[4]);
		}

		if (UninstallButton != null &&
			ui_messages.Count > 5)
		{
			UninstallButton.Text =
				FormatLocalizedText(ui_messages[5]);
		}
	}

	private static string FormatLocalizedText(string text)
	{
		return string.IsNullOrEmpty(text)
			? string.Empty
			: text.Replace("\\n", Environment.NewLine);
	}

	private static void AddLanguageIfMissing(
		ComboBox comboBox,
		string languageName)
	{
		if (FindLanguageIndex(comboBox, languageName) >= 0)
		{
			return;
		}

		comboBox.Items.Add(languageName);
	}

	private static int FindLanguageIndex(
		ComboBox comboBox,
		string languageName)
	{
		for (int i = 0; i < comboBox.Items.Count; i++)
		{
			if (comboBox.Items[i] is string existingLanguage &&
				string.Equals(
					existingLanguage,
					languageName,
					StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}

		return -1;
	}
}
