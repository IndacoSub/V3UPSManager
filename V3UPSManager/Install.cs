using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3UPSManager
{
    public partial class Form1 : Form
    {
        List<string> to_apply;
        List<string> to_be_applied;
        private void Install()
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
                if (IsLegacy)
                {
                    DisplayInfo.Print(info[26]);
                } else
                {
                    DisplayInfo.Print(info[36]);
                }
                return;
            }

            if (to_apply.Count <= 0)
            {
                DisplayInfo.Print(info[27]);
                return;
            }

            to_apply.Sort();
            to_be_applied.Sort();

            // *If they don't exist already*, copy the SPCs/Unity-files
            // as .*_bak files (backup)
            // This allows us to maintain compatibility
            // with future translation updates
            // but *will* break for any new
            // game update which updates the
            // base files (hasn't happened yet)

            foreach (string file in to_apply)
            {
                if (to_apply.Contains("_bak"))
                {
                    continue;
                }
                if (!File.Exists(file + "_bak") && File.Exists(file))
                {
                    // Only create one when there isn't one
                    System.IO.File.Copy(file, file + "_bak", false);
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

                // Output .spc/.ab/.assets file
                string outfile = to_apply[j];

                // Base .spc/.ab/.assets file
                string before = outfile + "_bak";

                // .ups file
                string after = to_be_applied[j];

                string command = "apply --base " + $"\"{before}\"" + " --patch " + $"\"{after}\"" + " --output " + $"\"{outfile}\"";

                //Process.Start(ups, "apply --base " + $"\"{before}\"" + " --patch " + $"\"{after}\"" + " --output " + $"\"{outfile}\"").WaitForExit();

                var p = new System.Diagnostics.Process();
                //p.StartInfo.WorkingDirectory = cur;
                p.StartInfo.FileName = "ups.exe";
                p.StartInfo.Arguments = command;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();

                // If the outfile (not _bak) file does not exist while the backup does
                if (!File.Exists(outfile) && File.Exists(before))
                {
                    // Then copy the backup
                    File.Copy(before, outfile, false);
                }
            }

            // Count backup files in the installation folder
            string[] files = Directory.GetFiles(installation_folder, "*.*_bak", SearchOption.AllDirectories);
            if (files != null && files.Length > 0 && files.Length == to_apply.Count)
            {
                DisplayInfo.Print(info[30]);
            }
            else
            {
                DisplayInfo.Print(info[34]);
            }
        }
    }
}
