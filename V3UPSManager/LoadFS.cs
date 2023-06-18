namespace V3UPSManager;

public partial class MainWindow : Form
{
    private List<string> already_checked = new();
    private readonly List<string> couldnt_be_found = new();
    private bool IsLegacy;
    private bool IsUnity;
    private string TitleID = "";

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
                if(!CheckXboxConfiguration())
                {
                    return;
                }

                data_folder = Path.Combine(installation_folder, "data");
                data_folder = Path.Combine(data_folder, "WIN");

                IsLegacy = false;
                IsUnity = false;
            }

            data_folder = Path.Combine(installation_folder, "Data");
            data_folder = Path.Combine(data_folder, "StreamingAssets");
            string platform = GetUnityPlatformByExclusion(data_folder);
            if (platform != null && !string.IsNullOrWhiteSpace(platform) && platform.Length > 0)
            {
                data_folder = Path.Combine(installation_folder, platform);
            }

            IsLegacy = false;
            IsUnity = true;
        }
        else
        {
            data_folder = Path.Combine(installation_folder, "data");
            data_folder = Path.Combine(data_folder, "win");

            IsLegacy = true;
            IsUnity = false;
        }

        if (!CheckInstall(data_folder))
        {
            return;
        }

        DisplayInfo.Print(info[18]);
        verified_installation_folder = installation_folder;
        InstallationPathPreviewTextbox.Text = verified_installation_folder;
    }

    private void LoadPatchFolder()
    {
        if (!Directory.Exists(verified_installation_folder) || verified_installation_folder == null ||
            verified_installation_folder.Length == 0)
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

        if (!Directory.Exists(ups_folder) || ups_folder.Length == 0)
        {
            // Maybe they deleted it?
            DisplayInfo.Print(info[20]);
            return;
        }

        string upsfolder = "";
        if (!IsUnity)
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

        to_be_applied = new List<string>();
        foreach (string file in files)
        {
            if (IsDirectory(file))
            {
                continue;
            }

            if (!file.Contains("_patch.ups"))
            {
                continue;
            }

            to_be_applied.Add(file);
        }

        GetToApply(files);

        foreach (string file in couldnt_be_found)
        {
            DisplayInfo.Print("Couldn't find: " + file);
            //DisplayInfo.Print("Couldn't find: " + file.Substring(0, file.Length - Path.GetExtension(file).Length));
        }

        DisplayInfo.Print(info[23]);

        PatchPathPreviewTextbox.Text = ups_folder;
    }

    private void RemoveFromList(string str)
    {
        while (couldnt_be_found.IndexOf(str) >= 0) couldnt_be_found.RemoveAt(couldnt_be_found.IndexOf(str));
        while (couldnt_be_found.Any(str.Contains)) couldnt_be_found.Remove(str);
        couldnt_be_found.RemoveAll(x => x == str);
    }

    private void GetToApply(string[] ups_files)
    {
        to_apply = new List<string>();
        already_checked = new List<string>();

        TryToApplySPCFiles(ups_files);
        if (!IsLegacy)
        {
            TryToApplyPBFiles(ups_files);
            TryToApplyABFiles(ups_files);
            TryToApplyAssetsFiles(ups_files);
        }

        foreach (string f in to_apply)
        {
            if (!Path.HasExtension(f))
            {
                continue;
            }

            string no_ext = f.Substring(0, f.Length - Path.GetExtension(f).Length);
            string spc = no_ext + ".spc";
            string spcbak = spc + "_bak";
            string ab = no_ext + ".ab";
            string abbak = ab + "_bak";
            string assets = no_ext + ".assets";
            string assetsbak = assets + "_bak";
            string pb = no_ext + ".pb";
            string pbbak = pb + "_bak";
            string patchups = no_ext + ".ups";
            string no_extbak = no_ext + "_bak";
            RemoveFromList(f.ToLowerInvariant());
            RemoveFromList(spc.ToLowerInvariant());
            RemoveFromList(spcbak.ToLowerInvariant());
            RemoveFromList(ab.ToLowerInvariant());
            RemoveFromList(abbak.ToLowerInvariant());
            RemoveFromList(assets.ToLowerInvariant());
            RemoveFromList(assetsbak.ToLowerInvariant());
            RemoveFromList(pb.ToLowerInvariant());
            RemoveFromList(pbbak.ToLowerInvariant());
            RemoveFromList(patchups.ToLowerInvariant());
            RemoveFromList(no_ext.ToLowerInvariant());
            RemoveFromList(no_extbak.ToLowerInvariant());
        }
    }

    private void TryToApplySPCFiles(string[] ups_files)
    {
        // Calculate which SPC files correspond to the UPS files
        // and add them to a vector
        foreach (string file in ups_files)
        {
            if (to_apply.Contains(file))
            {
                continue;
            }

            if (IsDirectory(file))
            {
                continue;
            }

            if (!file.Contains("_patch.ups"))
            {
                continue;
            }

            // Remove the part before the UPS folder
            string second_half = file.Substring(ups_folder.Length + 1);

            if (already_checked.Contains(second_half.ToLowerInvariant()))
            {
                continue;
            }

            // Get the "approximate" .spc file
            string approximate_spc = Path.Combine(verified_installation_folder, second_half);

            if (IsDirectory(approximate_spc))
            {
                continue;
            }

            // _patch.ups (length: 10)
            approximate_spc = approximate_spc.Substring(0, approximate_spc.Length - 10);

            if (approximate_spc.Length <= 0)
            {
                continue;
            }

            string try_approximate_spc_lower = approximate_spc + ".spc";
            string try_approximate_spc_upper = approximate_spc + ".SPC";

            string og_no_bak = try_approximate_spc_lower;

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
                    if (!to_apply.Contains(og_no_bak) &&
                        !couldnt_be_found.Contains(og_no_bak.ToLowerInvariant()))
                    {
                        couldnt_be_found.Add(og_no_bak.ToLowerInvariant());
                    }

                    continue;
                }

                // One has to exist
                if (exists_lower)
                {
                    // Lower exists
                    // Restore bak
                    string nobak = try_approximate_spc_lower.Substring(0, try_approximate_spc_lower.Length - 4);
                    File.Copy(try_approximate_spc_lower, nobak, true);
                    try_approximate_spc_lower = nobak;
                }
                else
                {
                    // Upper exists
                    string nobak = try_approximate_spc_upper.Substring(0, try_approximate_spc_upper.Length - 4);
                    File.Copy(try_approximate_spc_upper, nobak, true);
                    try_approximate_spc_upper = nobak;
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

            already_checked.Add(second_half.ToLowerInvariant());

            if (to_apply.Contains(try_approximate_spc_lower))
            {
                couldnt_be_found.Remove(try_approximate_spc_lower.ToLowerInvariant());
                couldnt_be_found.Remove(og_no_bak.ToLowerInvariant());
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

            if (IsDirectory(file))
            {
                continue;
            }

            if (!file.Contains("_patch.ups"))
            {
                continue;
            }

            // Remove the part before the UPS folder
            string second_half = file.Substring(ups_folder.Length + 1);

            if (already_checked.Contains(second_half.ToLowerInvariant()))
            {
                continue;
            }

            // Get the "approximate" .ab file
            string approximate_ab = Path.Combine(verified_installation_folder, second_half);

            if (IsDirectory(approximate_ab))
            {
                continue;
            }

            string bak_approx = approximate_ab;

            // _patch.ups (length: 10)
            approximate_ab = approximate_ab.Substring(0, approximate_ab.Length - 10);

            if (approximate_ab.Length <= 0)
            {
                continue;
            }

            approximate_ab += ".ab";

            if (!File.Exists(approximate_ab))
            {
                // Does not exist

                approximate_ab += "_bak";

                if (!File.Exists(approximate_ab))
                {
                    if (!to_apply.Contains(approximate_ab) &&
                        !couldnt_be_found.Contains(approximate_ab.ToLowerInvariant()))
                    {
                        couldnt_be_found.Add(approximate_ab.ToLowerInvariant());
                    }

                    continue;
                }

                // restore backup
                string nobak = approximate_ab.Substring(0, approximate_ab.Length - 4);
                File.Copy(approximate_ab, nobak, true);
                approximate_ab = nobak;
            }

            to_apply.Add(approximate_ab);
            already_checked.Add(second_half.ToLowerInvariant());

            if (to_apply.Contains(approximate_ab))
            {
                couldnt_be_found.Remove(approximate_ab.ToLowerInvariant());
            }
        }
    }

    private void TryToApplyPBFiles(string[] ups_files)
    {
        // Calculate which PB files correspond to the UPS files
        // and add them to a vector
        foreach (string file in ups_files)
        {
            if (to_apply.Contains(file))
            {
                continue;
            }

            if (IsDirectory(file))
            {
                continue;
            }

            if (!file.Contains("_patch.ups"))
            {
                continue;
            }

            // Remove the part before the UPS folder
            string second_half = file.Substring(ups_folder.Length + 1);

            if (already_checked.Contains(second_half.ToLowerInvariant()))
            {
                continue;
            }

            // Get the "approximate" .pb file
            string approximate_pb = Path.Combine(verified_installation_folder, second_half);

            if (IsDirectory(approximate_pb))
            {
                continue;
            }

            string bak_approx = approximate_pb;

            // _patch.ups (length: 10)
            approximate_pb = approximate_pb.Substring(0, approximate_pb.Length - 10);

            if (approximate_pb.Length <= 0)
            {
                continue;
            }

            approximate_pb += ".pb";

            if (!File.Exists(approximate_pb))
            {
                // Does not exist

                approximate_pb += "_bak";

                if (!File.Exists(approximate_pb))
                {
                    if (!to_apply.Contains(approximate_pb) &&
                        !couldnt_be_found.Contains(approximate_pb.ToLowerInvariant()))
                    {
                        couldnt_be_found.Add(approximate_pb.ToLowerInvariant());
                    }

                    continue;
                }

                // restore backup
                string nobak = approximate_pb.Substring(0, approximate_pb.Length - 4);
                File.Copy(approximate_pb, nobak, true);
                approximate_pb = nobak;
            }

            to_apply.Add(approximate_pb);
            already_checked.Add(second_half.ToLowerInvariant());

            if (to_apply.Contains(approximate_pb))
            {
                couldnt_be_found.Remove(approximate_pb.ToLowerInvariant());
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

            if (IsDirectory(file))
            {
                continue;
            }

            if (!file.Contains("_patch.ups"))
            {
                continue;
            }

            // Remove the part before the UPS folder
            string second_half = file.Substring(ups_folder.Length + 1);

            if (already_checked.Contains(second_half))
            {
                continue;
            }

            // Get the "approximate" .assets file
            string approximate_assets = Path.Combine(verified_installation_folder, second_half);

            if (IsDirectory(approximate_assets))
            {
                continue;
            }

            string bak_approx = approximate_assets;

            // _patch.ups (length: 10)
            approximate_assets = approximate_assets.Substring(0, approximate_assets.Length - 10);

            if (approximate_assets.Length <= 0)
            {
                continue;
            }

            approximate_assets += ".assets";

            if (!File.Exists(approximate_assets))
            {
                // Does not exist

                approximate_assets += "_bak";

                if (!File.Exists(approximate_assets))
                {
                    if (!to_apply.Contains(approximate_assets) &&
                        !couldnt_be_found.Contains(approximate_assets.ToLowerInvariant()))
                    {
                        couldnt_be_found.Add(approximate_assets.ToLowerInvariant());
                    }

                    continue;
                }

                // restore backup
                string nobak = approximate_assets.Substring(0, approximate_assets.Length - 4);
                File.Copy(approximate_assets, nobak, true);
                approximate_assets = nobak;
            }

            to_apply.Add(approximate_assets);
            already_checked.Add(second_half.ToLowerInvariant());

            if (to_apply.Contains(approximate_assets))
            {
                couldnt_be_found.Remove(approximate_assets.ToLowerInvariant());
            }
        }
    }
}