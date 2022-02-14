using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3UPSManager
{
    public partial class Form1 : Form
    {
        private void BackupChanges()
        {
            string cur = Directory.GetCurrentDirectory();
            string backupdir = Path.Combine(cur, "Backup");

            if(Directory.Exists(backupdir))
            {
                Directory.Delete(backupdir, true);
            } else
            {
                Directory.CreateDirectory(backupdir);
            }

            foreach (string file in to_apply)
            {
                string after_installdir = file.Substring(verified_installation_folder.Length + 1);
                string newpath = Path.Combine(backupdir, after_installdir);
                if(newpath == null || newpath.Length <= 0)
                {
                    continue;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(newpath));
                System.IO.File.Copy(file, newpath);
            }
        }

        private void DisplayStatus()
        {
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
