using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace V3UPSManager
{
    public partial class Form1 : Form
    {
        string verified_installation_folder;
        string installation_folder;
        string ups_folder;
        List<string> to_apply;
        List<string> to_be_applied;

        string[] info_it = new string[30];
        string[] info_en = new string[30];
        string[] info = new string[30];

        public Form1()
        {
            InitializeComponent();
            LoadLanguages();
            info = new string[30];
            comboBox1.SelectedIndex = 1; // Default to Italian
            CheckIndexChange();
        }

        private void LoadLanguages()
        {
            info_it = new string[] {
                "La cartella di installazione non esiste!",
                "L'eseguibile (\"Dangan3Win.exe\") non è stato trovato!",
                "Il file \"language.txt\" non è stato trovato!\nPer favore avvia il gioco almeno una volta\n(e controlla che funzioni).",
                "Il gioco non è in inglese.\nPer favore disinstalla e installa nuovamente il gioco\ncon la lingua inglese.",
                "La/e cartella/e \"data\" e \"win\" non sono state trovate!",
                "Sono stati trovati dei file CPK in \"data\\win\"!\nAvresti dovuto estrarli e spostarli altrove.",
                "Sono stati trovati dei file CPK non-inglesi in \"data\\win\"!\nPer favore disinstalla e installa nuovamente il gioco\ncon la lingua inglese.",
                "Non è stato possibile calcolare l'hash MD5 per Dangan3Win.exe!",
                "Non hai l'ultima versione del gioco,\nhai selezionato la versione demo, oppure\nstai usando una copia pirata del gioco.\nNota: noi non supportiamo la pirateria e non\nriceverai supporto/aiuti per l'installazione, nel caso.\nIl gioco potrebbe non funzionare a dovere usando una versione vecchia del gioco.\nProcedere comunque?",
                "E' stata trovata un'installazione di ReShade.\nIl gioco potrebbe non funzionare a dovere con ReShade attivo.\nProcedere comunque?",
                "E' stato trovato DR3Fix.\nIl gioco potrebbe non funzionare a dovere con DR3Fix attivo.\nProcedere comunque?",
                "La cartella \"data\\win\\boot\" non è stata trovata!",
                "La cartella \"data\\win\\flash\" non è stata trovata!",
                "La cartella \"data\\win\\game_resident\" non è stata trovata!",
                "La cartella \"data\\win\\minigame\" non è stata trovata!",
                "La cartella \"data\\win\\trial_font\" non è stata trovata!",
                "La cartella \"data\\win\\wrd_data\" non è stata trovata!",
                "La cartella \"data\\win\\wrd_script\" non è stata trovata!",
                "Non è stato trovato alcun problema!\nEvviva!",
                "La cartella di installazione non è valida!",
                "La cartella contenente le patch non è valida!",
                "La/e cartella/e \"data\" e \"data\\win\" non sono state trovate!",
                "Non è stato trovato alcun file .ups!",
                "Informazioni sui file caricate! Ci siamo!",
                "\"ups.exe\" non trovato!\nLeggi le istruzioni nel file README!",
                "Impossibile caricare la cartella di installazione o la cartella delle patch!",
                "Non sono state trovate informazioni sui file SPC!",
                "Non sono state trovate informazioni sui file UPS!",
                "Qualcosa non quadra coi file SPC!",
                "Qualcosa non quadra coi file UPS!",
                "Fatto! Buon divertimento!",
                "Versione Switch non (ancora) supportata.",
                "Non è stato trovato alcun file da disinstallare.",
                "Disinstallazione completata!",
            };

            info_en = new string[] {
                "The installation folder doesn't exist!",
                "The executable (\"Dangan3Win.exe\") couldn't be found!",
                "The file \"language.txt\" couldn't be found!\nPlease boot the game at least once\n(and make sure it's working).",
                "Your game language is not set to English.\nPlease uninstall and reinstall the game\nwith the English language instead.",
                "The \"data\" and/or \"data\\win\" folders couldn't be found!",
                "CPK file(s) found in \"win\\data\"!\nYou were supposed to extract them\nand move them elsewhere.",
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
                "Switch version not (yet) supported.",
                "No file to be uninstalled was found.",
                "Done uninstalling!",
            };
        }

        // From https://stackoverflow.com/questions/2435695/converting-a-md5-hash-byte-array-to-a-string
        public static string ToHex(byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

            return result.ToString();
        }

        // From https://stackoverflow.com/questions/16183788/case-sensitive-directory-exists-file-exists
        // (edited)
        static public bool DirExistsMatchCase(string path)
        {

            // Figure out if the case (of the final part) is the same
            string thisDir = Path.GetFileName(path);
            thisDir = thisDir.Substring(thisDir.LastIndexOf('\\') + 1);
            string actualDir = Path.GetFileName(Directory.GetDirectories(Path.GetDirectoryName(path), thisDir)[0]);
            //DisplayInfo.Print("thisDir: " + thisDir + ", actualDir: " + actualDir);
            return thisDir == actualDir;
        }

        // From https://stackoverflow.com/questions/16183788/case-sensitive-directory-exists-file-exists
        public static bool FileExistsCaseSensitive(string filename)
        {
            string name = Path.GetDirectoryName(filename);

            return name != null
                   && Array.Exists(Directory.GetFiles(name), s => s == Path.GetFullPath(filename));
        }
        bool CheckSwitchConfiguration()
        {
            // Switch version NOT SUPPORTED
            // So we're going to return FALSE

            string switch_root = "Data";

            string switch_find = Path.Combine(installation_folder, switch_root);

            // NOTE: Only for the Switch version
            // Check if the "Data" folder exists

            if (!Directory.Exists(switch_find))
            {
                DisplayInfo.Print(info[31]);
                return true;
            }

            if (DirExistsMatchCase(switch_find))
            {
                DisplayInfo.Print(info[31]);
                return true;
            }

            // NOTE: Only for the Switch version
            // Check if the "StreamingAssets" folder exists
            switch_find = Path.Combine(installation_folder, "StreamingAssets");
            if (Directory.Exists(switch_find))
            {
                DisplayInfo.Print(info[31]);
                return true;
            }

            // NOTE: Only for the Switch version
            // Check if the "all" folder exists
            switch_find = Path.Combine(installation_folder, "all");
            if (Directory.Exists(switch_find))
            {
                DisplayInfo.Print(info[31]);
                return true;
            }

            // Count .ab (Unity) files in the installation folder
            string[] files = Directory.GetFiles(installation_folder, "*.ab", SearchOption.AllDirectories);
            if (files != null && files.Length > 0)
            {
                DisplayInfo.Print(info[31]);
                return true;
            }

            return false;
        }

        bool CheckPCConfiguration()
        {
            // Check if Dangan3Win.exe exists
            string exe = Path.Combine(installation_folder, "Dangan3Win.exe");
            if (!File.Exists(exe))
            {
                // Maybe they deleted it?
                DisplayInfo.Print(info[1]);
                return false;
            }

            // Check if language.txt exists
            string config_language = Path.Combine(installation_folder, "language.txt");
            if (!File.Exists(config_language))
            {
                DisplayInfo.Print(info[2]);
                return false;
            }

            // Read language.txt
            string language = File.ReadAllText(config_language);
            if (string.IsNullOrWhiteSpace(language) || language != "US")
            {
                DisplayInfo.Print(info[3]);
                return false;
            }

            // Check if /data/win/ exists
            string windata = Path.Combine(installation_folder, "data");
            windata = Path.Combine(windata, "win");
            if (!Directory.Exists(windata))
            {
                DisplayInfo.Print(info[4]);
                return false;
            }

            // NOTE: Only for the PC version
            // Check if the CPKs are in /data/win/
            string cpk_01_en = Path.Combine(windata, "partition_data_win.cpk");
            string cpk_02_en = Path.Combine(windata, "partition_data_win_us.cpk");
            string cpk_03_en = Path.Combine(windata, "partition_resident_win.cpk");
            bool cpk_01_en_exists = File.Exists(cpk_01_en);
            bool cpk_02_en_exists = File.Exists(cpk_02_en);
            bool cpk_03_en_exists = File.Exists(cpk_03_en);
            if (cpk_01_en_exists || cpk_02_en_exists || cpk_03_en_exists)
            {
                DisplayInfo.Print(info[5]);
                return false;
            }

            // NOTE: Only for the PC version
            // Count the number of CPKs in /data/win/
            var count_lowercase = Directory.GetFiles(windata, "*.cpk", SearchOption.TopDirectoryOnly).Length;
            var count_uppercase = Directory.GetFiles(windata, "*.CPK", SearchOption.TopDirectoryOnly).Length;
            if (count_lowercase + count_uppercase > 0)
            {
                DisplayInfo.Print(info[6]);
                return false;
            }

            // Calculate MD5 hash of Dangan3Win.exe
            string exe_md5 = "";
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(exe))
                {
                    var hash = md5.ComputeHash(stream);
                    if (hash != null)
                    {
                        exe_md5 = ToHex(hash, false);
                    }
                }
            }

            // Check if the MD5 hash is valid
            if (exe_md5.Length == 0 || string.IsNullOrWhiteSpace(exe_md5))
            {
                DisplayInfo.Print(info[7]);
                return false;
            }

            // Check if the calculated hash corresponds to the expected hash
            const string expected_hash = "ff2dd8163a9b2f5b018339fbee69f5ea";
            if (exe_md5.ToLower() != expected_hash)
            {
                var proceed = DisplayInfo.Ask(info[8]);
                if (proceed == DialogResult.No || proceed == DialogResult.Cancel)
                {
                    return false;
                }
            }

            // Check if ReShade is present
            string reshade_ini = Path.Combine(installation_folder, "ReShade.ini");
            if (File.Exists(reshade_ini))
            {
                var proceed = DisplayInfo.Ask(info[9]);
                if (proceed == DialogResult.No || proceed == DialogResult.Cancel)
                {
                    return false;
                }
            }

            // Check if DR3Fix is present
            string dr3fix_cfg = Path.Combine(installation_folder, "dr3fix.cfg");
            if (File.Exists(dr3fix_cfg))
            {
                var proceed = DisplayInfo.Ask(info[10]);
                if (proceed == DialogResult.No || proceed == DialogResult.Cancel)
                {
                    return false;
                }
            }

            return true;
        }

        bool CheckInstall(string data_folder)
        {

            // Dome some checks about the folder where the .ups files will be installed

            if (data_folder == null || string.IsNullOrEmpty(data_folder) || !Directory.Exists(data_folder))
            {
                DisplayInfo.Print(info[25]);
                return false;
            }

            string check_boot = Path.Combine(data_folder, "boot");
            if (!Directory.Exists(check_boot))
            {
                DisplayInfo.Print(info[11]);
                return false;
            }

            string check_flash = Path.Combine(data_folder, "flash");
            if (!Directory.Exists(check_flash))
            {
                DisplayInfo.Print(info[12]);
                return false;
            }

            string check_game_resident = Path.Combine(data_folder, "game_resident");
            if (!Directory.Exists(check_game_resident))
            {
                DisplayInfo.Print(info[13]);
                return false;
            }

            string check_minigame = Path.Combine(data_folder, "minigame");
            if (!Directory.Exists(check_minigame))
            {
                DisplayInfo.Print(info[14]);
                return false;
            }

            string check_trial_font = Path.Combine(data_folder, "trial_font");
            if (!Directory.Exists(check_trial_font))
            {
                DisplayInfo.Print(info[15]);
                return false;
            }

            string check_wrd_data = Path.Combine(data_folder, "wrd_data");
            if (!Directory.Exists(check_wrd_data))
            {
                DisplayInfo.Print(info[16]);
                return false;
            }

            string check_wrd_script = Path.Combine(data_folder, "wrd_script");
            if (!Directory.Exists(check_wrd_script))
            {
                DisplayInfo.Print(info[17]);
                return false;
            }

            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var fold = new FolderBrowserDialog())
            {
                DialogResult res = fold.ShowDialog();

                if (res == DialogResult.OK && !string.IsNullOrWhiteSpace(fold.SelectedPath))
                {
                    installation_folder = fold.SelectedPath;
                }
            }

            if (!Directory.Exists(installation_folder) || installation_folder == null)
            {
                // Maybe they deleted it?
                DisplayInfo.Print(info[0]);
                return;
            }

            string data_folder = installation_folder;

            // Check if it's the Switch version
            // Switch version NOT SUPPORTED
            // So we're going to return FALSE
            if (CheckSwitchConfiguration())
            {
                return;
            }
            else
            {
                // TODO
            }

            if (!CheckPCConfiguration())
            {
                return;
            }
            else
            {
                data_folder = Path.Combine(installation_folder, "data");
                data_folder = Path.Combine(data_folder, "win");
            }

            if (!CheckInstall(data_folder))
            {
                return;
            }

            DisplayInfo.Print(info[18]);
            verified_installation_folder = installation_folder;
            textBox1.Text = verified_installation_folder;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (!Directory.Exists(verified_installation_folder) || verified_installation_folder == null || verified_installation_folder.Length == 0)
            {
                // Maybe they deleted it?
                DisplayInfo.Print(info[19]);
                return;
            }

            using (var fold = new FolderBrowserDialog())
            {
                DialogResult res = fold.ShowDialog();

                if (res == DialogResult.OK && !string.IsNullOrWhiteSpace(fold.SelectedPath))
                {
                    ups_folder = fold.SelectedPath;
                }
            }

            if (!Directory.Exists(ups_folder) || ups_folder == null)
            {
                // Maybe they deleted it?
                DisplayInfo.Print(info[20]);
                return;
            }

            // Check if /data/win/ exists in the patch folder
            string upswindata = Path.Combine(ups_folder, "data");
            upswindata = Path.Combine(upswindata, "win");
            if (!Directory.Exists(upswindata))
            {
                DisplayInfo.Print(info[21]);
                return;
            }

            // Count and get all .ups files in the patch folder
            string[] files = Directory.GetFiles(upswindata, "*.ups", SearchOption.AllDirectories);
            if (files == null || files.Length == 0)
            {
                DisplayInfo.Print(info[22]);
                return;
            }
            else
            {
                to_be_applied = new List<string>();
                foreach (string file in files)
                {
                    to_be_applied.Add(file);
                }
            }

            to_apply = new List<string>();

            List<string> couldnt_be_found = new List<string>();

            // Calculate which SPC files correspond to the UPS files
            // and add them to a vector
            foreach (string file in files)
            {
                // Remove the part before the UPS folder
                string second_half = file.Substring(ups_folder.Length + 1);

                // Get the "approximate" .spc file
                string approximate_spc = Path.Combine(verified_installation_folder, second_half);

                // I don't know
                approximate_spc = approximate_spc.Substring(0, approximate_spc.Length - 10);

                string try_approximate_spc_lower = approximate_spc + ".spc";
                string try_approximate_spc_upper = approximate_spc + ".SPC";

                bool exists_lower = FileExistsCaseSensitive(try_approximate_spc_lower);
                bool exists_upper = FileExistsCaseSensitive(try_approximate_spc_upper);
                if (!exists_lower && !exists_upper)
                {
                    // try to see if the _bak exists
                    try_approximate_spc_lower = try_approximate_spc_lower + "_bak";
                    try_approximate_spc_upper = try_approximate_spc_upper + "_bak";

                    // and which one exists
                    exists_lower = FileExistsCaseSensitive(try_approximate_spc_lower);
                    exists_upper = FileExistsCaseSensitive(try_approximate_spc_upper);

                    if (!exists_lower && !exists_upper)
                    {
                        couldnt_be_found.Add(try_approximate_spc_lower);
                        continue;
                    }
                    else
                    {

                        if (exists_lower)
                        {
                            string nobak = try_approximate_spc_lower.Substring(0, try_approximate_spc_lower.Length - 4);
                            System.IO.File.Move(try_approximate_spc_lower, nobak, true);
                            try_approximate_spc_lower = nobak;
                        }
                        else
                        {
                            string nobak = try_approximate_spc_upper.Substring(0, try_approximate_spc_upper.Length - 4);
                            System.IO.File.Move(try_approximate_spc_upper, nobak, true);
                            try_approximate_spc_upper = nobak;
                        }
                    }
                }

                if (exists_lower)
                {
                    to_apply.Add(try_approximate_spc_lower);
                }
                else
                {
                    to_apply.Add(try_approximate_spc_upper);
                }
            }

            DisplayInfo.Print(info[23]);

            textBox2.Text = ups_folder;

            /*
            foreach (string file in to_be_applied)
            {
                DisplayInfo.Print(file);
            }

            foreach(string file in to_apply)
            {
                DisplayInfo.Print("Will be \"installed\": " + file);
            }
            */

            foreach (string file in couldnt_be_found)
            {
                DisplayInfo.Print("Couldn't find: " + file);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string cur = Directory.GetCurrentDirectory();
            string current_dir = Path.GetFullPath(cur);

            // Check if "ups.exe" from "UPS Tools" exists
            // UPS Tools is licensed under the MIT license
            // https://github.com/rameshvarun/ups
            // https://github.com/rameshvarun/ups/LICENSE
            string ups = Path.Combine(current_dir, "ups.exe");
            if (!File.Exists(ups))
            {
                DisplayInfo.Print(info[24]);
                return;
            }

            // Check if the installation folder is not valid
            if (string.IsNullOrWhiteSpace(verified_installation_folder) || verified_installation_folder.Length <= 0 || !Directory.Exists(verified_installation_folder))
            {
                DisplayInfo.Print(info[25]);
                return;
            }

            // Check if the UPS folder is not valid
            if (string.IsNullOrWhiteSpace(ups_folder) || ups_folder.Length <= 0 || !Directory.Exists(ups_folder))
            {
                DisplayInfo.Print(info[25]);
                return;
            }

            if (to_be_applied.Count <= 0)
            {
                DisplayInfo.Print(info[26]);
                return;
            }

            if (to_apply.Count <= 0)
            {
                DisplayInfo.Print(info[27]);
                return;
            }

            // *If they don't exist already*, copy the SPCs
            // as .spc_bak files (backup)
            // This allows us to maintain compatibility
            // with future translation updates
            // but *will* break for any new
            // game update which updates the
            // SPC files (hasn't happened yet)

            foreach (string file in to_apply)
            {
                if(to_apply.Contains("_bak"))
                {
                    continue;
                } 
                if (!File.Exists(file + "_bak") && File.Exists(file))
                {
                    System.IO.File.Copy(file, file + "_bak");
                }
                if (File.Exists(file) && File.Exists(file + "_bak"))
                {
                    File.Delete(file);
                }
            }

            // Apply the patches

            for (int j = 0; j < to_apply.Count(); j++)
            {
                if (to_apply[j] == null)
                {
                    DisplayInfo.Print(info[28]);
                    return;
                }
                if (to_be_applied[j] == null)
                {
                    DisplayInfo.Print(info[29]);
                    return;
                }

                // Output .spc file
                string outfile = to_apply[j];

                // Base .spc file
                string before = outfile + "_bak";

                // .ups file
                string after = to_be_applied[j];

                string command = "apply --base " + $"\"{before}\"" + " --patch " + $"\"{after}\"" + " --output " + $"\"{outfile}\"";

                //Process.Start(ups, "apply --base " + $"\"{before}\"" + " --patch " + $"\"{after}\"" + " --output " + $"\"{outfile}\"").WaitForExit();

                var p = new System.Diagnostics.Process();
                //p.StartInfo.WorkingDirectory = cur;
                p.StartInfo.FileName = "ups.exe";
                p.StartInfo.Arguments = command;
                p.StartInfo.RedirectStandardOutput = false;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();

                if(!File.Exists(outfile) && File.Exists(before))
                {
                    File.Move(before, outfile, false);
                }
            }

            DisplayInfo.Print(info[30]);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        void CheckIndexChange()
        {
            // Change the language of the program
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    info = info_en;
                    button1.Text = "Choose installation folder";
                    button2.Text = "Choose patch folder";
                    button3.Text = "Install / Update";
                    button4.Text = "Uninstall";
                    break;
                case 1:
                default:
                    info = info_it;
                    button1.Text = "Seleziona cartella di installazione";
                    button2.Text = "Seleziona cartella patch";
                    button3.Text = "Installa / Aggiorna";
                    button4.Text = "Disinstalla";
                    break;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckIndexChange();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Check if the installation folder is not valid
            if (string.IsNullOrWhiteSpace(verified_installation_folder) || verified_installation_folder.Length <= 0 || !Directory.Exists(verified_installation_folder))
            {
                DisplayInfo.Print(info[25]);
                return;
            }

            List<string> filelist = new List<string>();
            // Count and get all .spc backup files in the patch folder
            string[] files1 = Directory.GetFiles(verified_installation_folder, ("*.spc_bak"), SearchOption.AllDirectories);
            string[] files2 = Directory.GetFiles(verified_installation_folder, ("*.SPC_bak"), SearchOption.AllDirectories);
            if (files1 == null || files2 == null || files1.Length == 0 || files2.Length == 0)
            {
                DisplayInfo.Print(info[32]);
                return;
            }

            foreach (string file in files1)
            {
                filelist.Add(file);
            }

            foreach (string file in files2)
            {
                filelist.Add(file);
            }

            int count_uninstalled = 0;
            foreach (string file in filelist)
            {
                // .spc_bak
                string actual_file = file;
                // .spc
                string normal_file = actual_file.Substring(0, actual_file.Length - 4);
                if (!File.Exists(normal_file) || string.IsNullOrWhiteSpace(normal_file))
                {
                    continue;
                }

                if (!File.Exists(actual_file) || !File.Exists(file))
                {
                    continue;
                }

                // Delete the .spc file
                File.Delete(normal_file);

                // Copy the bak file to the .spc file
                File.Move(actual_file, normal_file, true);

                if (File.Exists(actual_file))
                {
                    // Delete the .bak file
                    File.Delete(actual_file);
                }

                //DisplayInfo.Print("Normal: " + normal_file + ", actual: " + actual_file + "...");
                count_uninstalled++;
            }

            if (count_uninstalled > 0)
            {
                DisplayInfo.Print(info[33]);
            }
            else
            {
                DisplayInfo.Print(info[32]);
            }
        }
    }
}