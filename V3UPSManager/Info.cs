namespace V3UPSManager;

public partial class MainWindow : Form
{
    private string[] info = new string[30];
    private string[] info_en = new string[30];
    private string[] info_it = new string[30];

    private string ANNIVERSARY_EXE_NAME = "Dangan3Desktop.exe";

    private void LoadLanguages()
    {
        info_it = new[]
        {
            "La cartella di installazione non esiste!", // 0
            "L'eseguibile (\"Dangan3Win.exe\") non è stato trovato (normale per installazioni versione Unity (Anniversary))!", // 1
            "Il file \"language.txt\" non è stato trovato!\nPer favore avvia il gioco almeno una volta\n(e controlla che funzioni).", // 2
            "Il gioco non è in inglese.\nPer favore disinstalla e installa nuovamente il gioco\ncon la lingua inglese.", // 3
            "La/e cartella/e \"data\" e \"win\" (o \"WIN\") non sono state trovate!", // 4
            "Sono stati trovati dei file CPK in \"data\\win\"!\nAvresti dovuto estrarli e spostarli altrove.", // 5
            "Sono stati trovati dei file CPK non-inglesi in \"data\\win\"!\nPer favore disinstalla e installa nuovamente il gioco\ncon la lingua inglese.", // 6
            "Non è stato possibile calcolare l'hash MD5 per Dangan3Win.exe!", // 7
            "Non hai l'ultima versione del gioco,\nhai selezionato la versione demo, oppure\nstai usando una copia pirata del gioco.\nNota: noi non supportiamo la pirateria e non\nriceverai supporto/aiuti per l'installazione, nel caso.\nIl gioco potrebbe non funzionare a dovere usando una versione vecchia del gioco.\nProcedere comunque?", // 8
            "E' stata trovata un'installazione di ReShade.\nIl gioco potrebbe non funzionare a dovere con ReShade attivo.\nProcedere comunque?", // 9
            "E' stato trovato DR3Fix.\nIl gioco potrebbe non funzionare a dovere con DR3Fix attivo.\nProcedere comunque?", // 10
            "La cartella \"data\\win\\boot\" non è stata trovata!", // 11
            "La cartella \"data\\win\\flash\" non è stata trovata!", // 12
            "La cartella \"data\\win\\game_resident\" non è stata trovata!", // 13
            "La cartella \"data\\win\\minigame\" non è stata trovata!", // 14
            "La cartella \"data\\win\\trial_font\" non è stata trovata!", // 15
            "La cartella \"data\\win\\wrd_data\" non è stata trovata!", // 16
            "La cartella \"data\\win\\wrd_script\" non è stata trovata!", // 17
            "Non è stato trovato alcun problema!\nEvviva!", // 18
            "La cartella di installazione non è valida!", // 19
            "La cartella contenente le patch non è valida!", // 20
            "La/e cartella/e \"data\" e/o \"data\\win\" non sono state trovate!", // 21
            "Non è stato trovato alcun file .ups!", // 22
            "Informazioni sui file caricate! Ci siamo!", // 23
            "\"ups.exe\" non trovato!\nLeggi le istruzioni nel file README!", // 24
            "Impossibile caricare la cartella di installazione o la cartella delle patch!", // 25
            "Non sono state trovate informazioni sui file SPC!", // 26
            "Non sono state trovate informazioni sui file UPS!", // 27
            "Qualcosa non quadra coi file SPC!", // 28
            "Qualcosa non quadra coi file UPS!", // 29
            "Fatto! Buon divertimento!", // 30
            "Versione del gioco sconosciuta/non supportata.", // 31
            "Non è stato trovato alcun file da disinstallare.", // 32
            "Disinstallazione completata!", // 33
            "Non è stato possibile installare alcuni file...", // 34
            "La/e cartella/e \"Data\" e/o \"Data\\StreamingAssets\" non sono state trovate!", // 35
            "Non sono state trovate informazioni sui file SPC/AB/Assets!", // 36
            "Non è stato trovato l'eseguibile (\"" + ANNIVERSARY_EXE_NAME + "\") (normale per installazioni versione Switch o \"Legacy\" (Steam))!", // 37
            "La cartella del gioco (versione \"Legacy\" (Steam)) contiene dei file provenienti dalla Anniversary Edition.", // 38
            "La cartella del gioco (\"Anniversary Edition\") contiene dei file provenienti dalla versione \"Legacy\" (Steam).", // 39
            "Sono stati trovati dei file ARC in \"data\\WIN\"!\nAvresti dovuto estrarli e spostarli altrove.", // 40
        };

        info_en = new[]
        {
            "The installation folder doesn't exist!",
            "The executable (\"Dangan3Win.exe\") couldn't be found (normal for Unity-version (Anniversary) installations)!",
            "The file \"language.txt\" couldn't be found!\nPlease boot the game at least once\n(and make sure it's working).",
            "Your game language is not set to English.\nPlease uninstall and reinstall the game\nwith the English language instead.",
            "The \"data\" and/or \"win\" (or \"WIN\") folders couldn't be found!",
            "CPK file(s) found in \"data\\win\"!\nYou were supposed to extract them\nand move them elsewhere.",
            "Non-English CPK file(s) found in \"data\\win\"!\nPlease uninstall and reinstall the game\nwith the English language instead.",
            "Couldn't compute MD5 hash for Dangan3Win.exe!",
            "You aren't using the latest version of the game,\nmaybe you chose the Demo version folder,\nor you might be using a pirated copy.\nPlease note that we don't support piracy and you won't\nreceive support for this installation, if so.\nThings might not work as expected while using an older version of the game.\nProceed anyway?",
            "A ReShade installation was found.\nThings might not work as expected while using ReShade.\nProceed anyway?",
            "DR3Fix was found.\nThings might not work as expected while using DR3Fix.\nProceed anyway?",
            "The \"data\\win\\boot\" folder couldn't be found!",
            "The \"data\\win\\flash\" folder couldn't be found!",
            "The \"data\\win\\game_resident\" folder couldn't be found!",
            "The \"data\\win\\minigame\" folder couldn't be found!",
            "The \"data\\win\\trial_font\" folder couldn't be found!",
            "The \"data\\win\\wrd_data\" folder couldn't be found!",
            "The \"data\\win\\wrd_script\" folder couldn't be found!",
            "No problems found with your installation!\nHooray!",
            "The installation folder is not valid!",
            "The patch folder doesn't exist!",
            "The \"data\" and/or \"data\\win\" folders couldn't be found!",
            "Couldn't find any .ups file(s)!",
            "File information loaded! Ready to apply!",
            "\"ups.exe\" not found!\nRead the instructions in the README file!",
            "The installation folder and/or the patch folder couldn't be loaded!",
            "No SPC file info found!",
            "No UPS file info found!",
            "Something is wrong with the SPC files!",
            "Something is wrong with the UPS files!",
            "Done! Have fun!",
            "Unknown/Unsupported game version.",
            "No file to be uninstalled was found.",
            "Done uninstalling!",
            "Some files couldn't be installed...",
            "The \"Data\" and/or \"Data\\StreamingAssets\" folders couldn't be found!",
            "No SPC/AB/Assets file info found!",
            "The executable (\"" + ANNIVERSARY_EXE_NAME + "\") couldn't be found (normal for non-Switch and non-\"Legacy\" (Steam) installations)!",
            "The game's folder (\"Legacy\" (Steam) version) contains files from the Anniversary Edition.",
            "The game's folder (\"Anniversary Edition\") contains files from the \"Legacy\" (Steam) version.",
            "ARC file(s) found in \"data\\win\"!\nYou were supposed to extract them\nand move them elsewhere.",
        };
    }

    private void CheckIndexChange()
    {
        // Change the language of the program
        switch (LanguageComboBox.SelectedIndex)
        {
            case 0:
                info = info_en;
                SelectInstallationFolderButton.Text = "Choose installation folder";
                SelectPatchFolderButton.Text = "Choose patch folder";
                InstallButton.Text = "Install / Update";
                UninstallButton.Text = "Uninstall";
                break;
            case 1:
            default:
                info = info_it;
                SelectInstallationFolderButton.Text = "Seleziona cartella di installazione";
                SelectPatchFolderButton.Text = "Seleziona cartella patch";
                InstallButton.Text = "Installa / Aggiorna";
                UninstallButton.Text = "Disinstalla";
                break;
        }
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
        CheckIndexChange();
    }
}