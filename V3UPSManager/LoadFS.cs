using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3UPSManager
{
    public partial class Form1 : Form
    {
        List<string> couldnt_be_found = new List<string>();
        bool IsLegacy = false;

        private void LoadInstallationFolder()
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

            if (!CheckLegacyConfiguration())
            {
                if (!CheckUnityConfiguration())
                {
                    return;
                }
                else
                {
                    IsLegacy = false;
                    data_folder = Path.Combine(installation_folder, "Data");
                    data_folder = Path.Combine(data_folder, "StreamingAssets");
                    string platform = GetUnityPlatformByExclusion(data_folder);
                    if (platform != null && !string.IsNullOrWhiteSpace(platform) && platform.Length > 0)
                    {
                        data_folder = Path.Combine(installation_folder, platform);
                    }
                }
            }
            else
            {
                IsLegacy = true;
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

        private void LoadPatchFolder()
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

            string upsfolder = "";
            if (IsLegacy)
            {
                // Check if /data/win/ exists in the patch folder
                string upswindata = Path.Combine(ups_folder, "data");
                upswindata = Path.Combine(upswindata, "win");
                if (!Directory.Exists(upswindata))
                {
                    DisplayInfo.Print(info[21]);
                    return;
                }
                upsfolder = upswindata;
            }
            else
            {
                // Check if /Data/StreaminAssets exists in the patch folder
                string upsdatasa = Path.Combine(ups_folder, "Data");
                upsdatasa = Path.Combine(upsdatasa, "StreamingAssets");
                if (!Directory.Exists(upsdatasa))
                {
                    DisplayInfo.Print(info[35]);
                    return;
                }
                upsfolder = ups_folder;
            }

            // Count and get all .ups files in the patch folder
            string[] files = Directory.GetFiles(upsfolder, "*.ups", SearchOption.AllDirectories);
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

            GetToApply(files);

            foreach (string file in couldnt_be_found)
            {
                DisplayInfo.Print("Couldn't find: " + file);
            }

            DisplayInfo.Print(info[23]);

            textBox2.Text = ups_folder;
        }

        private void GetToApply(string[] ups_files)
        {

            to_apply = new List<string>();

            TryToApplySPCFiles(ups_files);
            TryToApplyABFiles(ups_files);
            TryToApplyAssetsFiles(ups_files);

            foreach(string f in to_apply)
            {
                if (to_apply.Contains(f))
                {
                    couldnt_be_found.Remove(Path.GetFileNameWithoutExtension(f).ToLowerInvariant());
                }
            }
        }

        private void TryToApplySPCFiles(string[] ups_files)
        {
            // Calculate which SPC files correspond to the UPS files
            // and add them to a vector
            foreach (string file in ups_files)
            {
                if(to_apply.Contains(file))
                {
                    continue;
                }
                // Remove the part before the UPS folder
                string second_half = file.Substring(ups_folder.Length + 1);

                // Get the "approximate" .spc file
                string approximate_spc = Path.Combine(verified_installation_folder, second_half);

                // _patch.ups (length: 10)
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

                    // and, if so, which one exists
                    exists_lower = FileExistsCaseSensitive(try_approximate_spc_lower);
                    exists_upper = FileExistsCaseSensitive(try_approximate_spc_upper);

                    if (!exists_lower && !exists_upper)
                    {
                        // None exists
                        if (!to_apply.Contains(try_approximate_spc_lower) &&
                            !couldnt_be_found.Contains(Path.GetFileNameWithoutExtension(try_approximate_spc_lower).ToLowerInvariant()))
                        {
                            couldnt_be_found.Add(Path.GetFileNameWithoutExtension(try_approximate_spc_lower).ToLowerInvariant());
                        }
                        continue;
                    }
                    else
                    {
                        // One has to exist
                        if (exists_lower)
                        {
                            // Lower exists
                            // Restore bak
                            string nobak = try_approximate_spc_lower.Substring(0, try_approximate_spc_lower.Length - 4);
                            System.IO.File.Copy(try_approximate_spc_lower, nobak, true);
                            try_approximate_spc_lower = nobak;
                        }
                        else
                        {
                            // Upper exists
                            string nobak = try_approximate_spc_upper.Substring(0, try_approximate_spc_upper.Length - 4);
                            System.IO.File.Copy(try_approximate_spc_upper, nobak, true);
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

                if (to_apply.Contains(try_approximate_spc_lower))
                {
                    couldnt_be_found.Remove(Path.GetFileNameWithoutExtension(try_approximate_spc_lower).ToLowerInvariant());
                }
            }
        }

        private void TryToApplyABFiles(string[] ups_files)
        {
            // Calculate which AB files correspond to the UPS files
            // and add them to a vector
            foreach (string file in ups_files)
            {
                if (to_apply.Contains(file))
                {
                    continue;
                }

                // Remove the part before the UPS folder
                string second_half = file.Substring(ups_folder.Length + 1);

                // Get the "approximate" .ab file
                string approximate_ab = Path.Combine(verified_installation_folder, second_half);

                // _patch.ups (length: 10)
                approximate_ab = approximate_ab.Substring(0, approximate_ab.Length - 10);

                approximate_ab += ".ab";

                if (!File.Exists(approximate_ab))
                {
                    // Does not exist

                    approximate_ab += "_bak";

                    if (!File.Exists(approximate_ab))
                    {
                        if (!to_apply.Contains(approximate_ab) &&
                            !couldnt_be_found.Contains(Path.GetFileNameWithoutExtension(approximate_ab).ToLowerInvariant()))
                        {
                            couldnt_be_found.Add(Path.GetFileNameWithoutExtension(approximate_ab).ToLowerInvariant());
                        }
                        continue;
                    }
                    else
                    {
                        // restore backup
                        string nobak = approximate_ab.Substring(0, approximate_ab.Length - 4);
                        System.IO.File.Copy(approximate_ab, nobak, true);
                        approximate_ab = nobak;
                    }
                }

                to_apply.Add(approximate_ab);

                if (to_apply.Contains(approximate_ab))
                {
                    couldnt_be_found.Remove(Path.GetFileNameWithoutExtension(approximate_ab).ToLowerInvariant());
                }
            }
        }

        private void TryToApplyAssetsFiles(string[] ups_files)
        {
            // Calculate which Assets files correspond to the UPS files
            // and add them to a vector
            foreach (string file in ups_files)
            {
                if (to_apply.Contains(file))
                {
                    continue;
                }
                // Remove the part before the UPS folder
                string second_half = file.Substring(ups_folder.Length + 1);

                // Get the "approximate" .assets file
                string approximate_assets = Path.Combine(verified_installation_folder, second_half);

                // _patch.ups (length: 10)
                approximate_assets = approximate_assets.Substring(0, approximate_assets.Length - 10);

                approximate_assets += ".assets";

                if (!File.Exists(approximate_assets))
                {
                    // Does not exist

                    approximate_assets += "_bak";

                    if (!File.Exists(approximate_assets))
                    {
                        if (!to_apply.Contains(approximate_assets) &&
                            !couldnt_be_found.Contains(Path.GetFileNameWithoutExtension(approximate_assets).ToLowerInvariant()))
                        {
                            couldnt_be_found.Add(Path.GetFileNameWithoutExtension(approximate_assets).ToLowerInvariant());
                        }
                        continue;
                    }
                    else
                    {
                        // restore backup
                        string nobak = approximate_assets.Substring(0, approximate_assets.Length - 4);
                        System.IO.File.Copy(approximate_assets, nobak, true);
                        approximate_assets = nobak;
                    }
                }

                to_apply.Add(approximate_assets);

                if (to_apply.Contains(approximate_assets))
                {
                    couldnt_be_found.Remove(Path.GetFileNameWithoutExtension(approximate_assets).ToLowerInvariant());
                }
            }
        }
    }
}
