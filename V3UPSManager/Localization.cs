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
			"The installation folder doesn't exist!",
			"The executable (\"VAR_LEGACY_EXE_NAME\") couldn't be found (normal for Unity-version or Xbox (Anniversary) installations)!",
			"The file \"language.txt\" couldn't be found!\\nPlease boot the game at least once\\n(and make sure it's working).",
			"Your game language is not set to English.\\nPlease uninstall and reinstall the game\\nwith the English language instead.",
			"The \"data\" and/or \"win\" (or \"WIN\") folders couldn't be found!",
			"CPK file(s) found in \"data\\win\"!\\nYou were supposed to extract them\\nand move them elsewhere.",
			"Non-English CPK file(s) found in \"data\\win\"!\\nPlease uninstall and reinstall the game\\nwith the English language instead.",
			"Couldn't compute MD5 hash for \"VAR_LEGACY_EXE_NAME\"! (Did a new update come out?)",
			"You aren't using the latest version of the game,\\nmaybe you chose the Demo version folder,\\nor you might be using a pirated copy.\\nPlease note that we don't support piracy and you won't\\nreceive support for this installation, if so.\\nThings might not work as expected while using an older version of the game.\\nProceed anyway?",
			"A ReShade installation was found.\\nThings might not work as expected while using ReShade.\\nProceed anyway?",
			"VAR_GAME_FIX was found.\\nThings might not work as expected while using VAR_GAME_FIX.\\nProceed anyway?",
			"The \"data\\win\\boot\" folder couldn't be found!",
			"The \"data\\win\\flash\" folder couldn't be found!",
			"The \"data\\win\\game_resident\" folder couldn't be found!",
			"The \"data\\win\\minigame\" folder couldn't be found!",
			"The \"data\\win\\trial_font\" folder couldn't be found!",
			"The \"data\\win\\wrd_data\" folder couldn't be found!",
			"The \"data\\win\\wrd_script\" folder couldn't be found!",
			"No problems found with your installation!\\nHooray!",
			"The installation folder is not valid!",
			"The patch folder doesn't exist!",
			"The \"data\" and/or \"data\\win\" folders couldn't be found!",
			"Couldn't find any VAR_PATCH_FORMAT_EXTENSION file(s)!",
			"File information loaded! Ready to apply!",
			"\"VAR_PATCH_FORMAT_INSTALLER_EXE\" not found!\\nRead the instructions in the README file!",
			"The installation folder and/or the patch folder couldn't be loaded!",
			"No SPC file info found!",
			"No VAR_PATCH_FORMAT file info found!",
			"Something is wrong with the SPC files!",
			"Something is wrong with the VAR_PATCH_FORMAT files!",
			"Done! Have fun!",
			"Unknown/Unsupported game version.",
			"No file to be uninstalled was found.",
			"Done uninstalling!",
			"Some files couldn't be installed...",
			"The \"Data\" and/or \"Data\\StreamingAssets\" folders couldn't be found!",
			"No SPC/AB/Assets file info found!",
			"The executable (\"VAR_ANNIVERSARY_EXE_NAME\") couldn't be found (normal for Switch and \"Legacy\" (Steam) installations)!",
			"The game's folder (\"Legacy\" (Steam) version) contains files from the Anniversary Edition.",
			"The game's folder (\"Anniversary Edition\") contains files from the \"Legacy\" (Steam) version.",
			"ARC file(s) found in \"data\\win\"!\\nYou were supposed to extract them\\nand move them elsewhere.\\n(At this time we can't extract them, unfortunately...)",
			"The \"Uninstall\" button only works with the Legacy (Steam) and Xbox versions!\\nIt's much easier to uninstall mods on Switch/Emulators, you just need to delete the mod folder(s)!", // 41
			"Switch version (Anniversary Edition) recognized!",
			"No file(s) in the installation folder are ready to be patched\\nThe installation cannot proceed, make sure you selected the right folder (ex. not \"win\" or \"data\" in the case of Danganronpa V3)",
			"VAR_PATCH_FORMAT files were found, but they aren't \"VAR_PATCH_SPECIFIC_STRING\" files, as they should instead be (rename them manually?)\\nAlso, they must NOT contain two extensions (for example, \"myfile.SPCVAR_PATCH_SPECIFIC_STRING\" is not a valid name), only \"VAR_PATCH_SPECIFIC_STRING\" (correct example: \"myfileVAR_PATCH_SPECIFIC_STRING\")",
			"Couldn't identify the game (please report this)",
			"Couldn't create an instance for the game (please report this)",
			"Couldn't find VAR_SOME_FILE",
			"The executable (\"VAR_UNITY_EXE_NAME\") couldn't be found (normal for non-Unity installations)!",
			"SpecialK or Reloaded-II were found.\\nThings might not work as expected.\\nProceed anyway?",
			"The installation command is empty (please report this)"
		};

		ui_messages_en = new List<string>()
		{
			"WARNING",
			"There seems to be a folder with the exact same name as the file. Continue?",
			"Choose installation folder",
			"Choose patch folder",
			"Install / Update",
			"Uninstall",
			"POST-INSTALL - File does not exist: VAR_OUTPUT_FILE, creating manually from backup... (please report this)",
			"Somebody forgot to update IT or EN translations 👀 (please report this)",
		};
	}

	private void CheckIndexChange()
	{
		// Change the GUI strings based on the selected language

		if (LanguageComboBox == null)
		{
			Log("LanguageComboBox is null.");
			return;
		}

		if (LanguageComboBox.Items.Count == 0)
		{
			Log("LanguageComboBox has no items.");
			return;
		}

		if (LanguageComboBox.SelectedIndex == -1)
		{
			Log("No item is selected in LanguageComboBox.");
			return;
		}

		if (LanguageComboBox.SelectedItem == null)
		{
			Log("SelectedItem is null.");
			return;
		}

		if (LanguageComboBox.SelectedIndex >= LanguageComboBox.Items.Count)
		{
			Log($"SelectedIndex {LanguageComboBox.SelectedIndex} is out of range.");
			return;
		}

		var index_string = LanguageComboBox.SelectedItem.ToString() as string;
		if (index_string == null)
		{
			return;
		}
		var cur = Directory.GetCurrentDirectory();
		var locfolder = Path.Combine(cur, "Localization");
		var langfolder = Path.Combine(locfolder, index_string);
		var infofile = Path.Combine(langfolder, "info.txt");
		var uifile = Path.Combine(langfolder, "ui.txt");
		bool valid = false;

		if (Directory.Exists(langfolder))
		{
			if (File.Exists(infofile) && File.Exists(uifile))
			{
				var new_info = File.ReadAllLines(infofile).ToList();
				var new_ui = File.ReadAllLines(uifile).ToList();

				if (new_info.Count == info_it.Count && new_ui.Count == ui_messages_it.Count)
				{
					info = new_info;
					ui_messages = new_ui;
					valid = true;
				}
				else
				{
					//MessageBox.Show("Lines is no: " + new_info.Count + " / " + info_it.Count + " and " + new_ui.Count + " / " + ui_messages_it.Count);
				}
			}
			else
			{
				//MessageBox.Show("Files is no");
			}
		}
		else
		{
			//MessageBox.Show("Langfolder is no");
		}

		if (!valid)
		{
			LanguageComboBox.SelectedIndex = 1;
			info = info_en;
			ui_messages = ui_messages_en;
			LanguageComboBox.Items.Remove(index_string);
		}

		SelectInstallationFolderButton.Text = ui_messages[2].Replace("\\n", "\n");
		SelectPatchFolderButton.Text = ui_messages[3].Replace("\\n", "\n");
		InstallButton.Text = ui_messages[4].Replace("\\n", "\n");
		UninstallButton.Text = ui_messages[5].Replace("\\n", "\n");
	}
}