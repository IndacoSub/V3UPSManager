
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
		// Let the user select the folder manually
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
			Log(info[0], null, Verbosity.Error);
			return;
		}

		string data_folder = installation_folder;

		Game CurrentGameID = GetGameByFolder(installation_folder);

		if(CurrentGameID == Game.None)
		{
			Log(info[45], null, Verbosity.Error);

			return;
		}

		CurrentGame = SpawnGameByID(CurrentGameID);

		if(CurrentGame == null || CurrentGame.GameID == Game.None)
		{
			Log(info[46], null, Verbosity.Error);

			return;
		}

		Log("Game recognized: " + CurrentGameID.ToString(), null, Verbosity.Info, LogType.ConsoleOnly);

		this.Text = "V3 UPS Manager" + " - " + CurrentGameID.ToString();

		// Detect game edition/platform
		if (!CheckLegacyConfiguration())
		{
			// At this point: not Legacy (Steam)
			if (!CheckUnityConfiguration())
			{
				// At this point: not Legacy (Steam) or Unity (Switch, Anniversary Edition)
				if (!CheckXboxConfiguration())
				{
					// At this point: not Legacy (Steam), Unity (Switch, Anniversary Edition) or Xbox (Microsoft Store, Anniversary Edition)
					// So... Android/iOS/PS4/PSVita?
					// We currently don't have any plans to support any other ports

					return;
				}

				// Xbox (Microsoft Store, AE) version:
				// Yes, the "WIN" is in uppercase

				switch(CurrentGame.GameID)
				{
					case Game.DanganronpaV3:
						data_folder = Path.Combine(installation_folder, "data");
						data_folder = Path.Combine(data_folder, "WIN");
						break;
					default:
						break;
				}
				

				IsLegacy = false;
				IsUnity = false;
			} else
			{
				// This wasn't an else, before October 30th 2024
				// This was right after the Xbox version checks, which means data_folder would get overwritten

				// Unity (Switch, AE) version:
				// Just like most Unity games, V3 AE's Switch port also has the "Data" and "StreamingAssets" folders

				data_folder = Path.Combine(installation_folder, CurrentGame.UnityDataFolder);
				if (!File.Exists(data_folder))
				{
					// TODO: Implement
				}
				data_folder = Path.Combine(data_folder, "StreamingAssets");
				string platform = GetUnityPlatformByExclusion(data_folder);
				if (platform != null && !string.IsNullOrWhiteSpace(platform) && platform.Length > 0)
				{
					data_folder = Path.Combine(installation_folder, platform);
				}

				IsLegacy = false;
				IsUnity = true;
			}
		}
		else
		{

			switch(CurrentGame.GameID)
			{
				case Game.DanganronpaV3:
					// Legacy (Steam) version:
					// Yes, the "win" is in lowercase

					data_folder = Path.Combine(installation_folder, "data");
					data_folder = Path.Combine(data_folder, "win");
					break;
				default:
					break;
			}

			IsLegacy = true;
			IsUnity = false;
		}

		if (!CheckInstall(data_folder))
		{
			return;
		}

		Log(info[18]);
		verified_installation_folder = installation_folder;
		InstallationPathPreviewTextbox.Text = verified_installation_folder;
	}

	private void LoadPatchFolder()
	{
		// No installation folder? Go back
		if (!Directory.Exists(verified_installation_folder) || verified_installation_folder == null ||
			verified_installation_folder.Length == 0)
		{
			// Maybe they deleted it?
			Log(info[19], null, Verbosity.Error);
			return;
		}

		// Let the user select the patch (ups) folder manually
		using (var fold = new FolderBrowserDialog())
		{
			DialogResult res = fold.ShowDialog();

			if (res == DialogResult.OK && !string.IsNullOrWhiteSpace(fold.SelectedPath))
			{
				ups_folder = fold.SelectedPath;
			}
		}

		// No patch folder? Go back
		if (!Directory.Exists(ups_folder) || ups_folder.Length == 0)
		{
			// Maybe they deleted it?
			Log(info[20], null, Verbosity.Error);
			return;
		}

		string upsfolder = "";

		if (!IsUnity)
		{
			switch(CurrentGame.GameID)
			{
				case Game.DanganronpaV3:
					// If you're here: NOT Unity!
					// It could be Legacy (Steam) or Xbox (Microsoft Store, AE)
					// ...or weird versions which we won't support (such as mobile or PS4/Vita)

					string upswindata = Path.Combine(ups_folder, "data");

					// Check if /data/win/ exists in the patch folder
					// AKA: Find out if the user is actually installing the patch for the Legacy or Xbox version(s)
					// (yes, with most certainty)
					// You never know, less "techy" users might have downloaded the wrong version of the mod

					if (IsLegacy)
					{
						upswindata = Path.Combine(upswindata, "win");
					}
					else
					{
						upswindata = Path.Combine(upswindata, "WIN");
					}

					if (!Directory.Exists(upswindata))
					{
						Log(info[21], null, Verbosity.Error);
						return;
					}

					// Safe to assume that the /data/win/ folder exists
					// They are empty, outside of the CPKs/ARCs, so we can just "skip" them
					upsfolder = upswindata;
					break;
				default:
					break;
			}
		}
		else
		{
			// If you're here: this is Unity! Good luck with debugging

			// Check if CurrentUnityDataFolder/StreamingAssets exists in the patch folder
			// AKA: Find out if the user is actually installing the patch for a Unity version
			// (yes, with most certainty)
			// You never know, less "techy" users might have downloaded the wrong version of the mod

			string upsdatasa = Path.Combine(ups_folder, CurrentGame.UnityDataFolder);
			upsdatasa = Path.Combine(upsdatasa, "StreamingAssets");
			if (!Directory.Exists(upsdatasa))
			{
				Log(info[35], null, Verbosity.Error);
				return;
			}

			upsfolder = ups_folder;
		}

		// Count and get all .ups files inside the patch folder
		// and do it recursively
		// TODO: Does this also count the patch files?
		string patch_format = CurrentGame.PatchFormatExtension;
		string[] files = Directory.GetFiles(upsfolder, "*" + patch_format, SearchOption.AllDirectories);
		if (files == null || files.Length == 0)
		{
			Log(info[22], new Dictionary<string, string>()
			{
				{ "VAR_PATCH_FORMAT_EXTENSION", CurrentGame.PatchFormatExtension },
			}, Verbosity.Error);
			return;
		}

		// Clear (or "reset") the to_be_applied list
		to_be_applied = new List<string>();

		List<string> actual_ups_files = new List<string>();

		// Look for any ups/xdelta file
		// that is not a folder (obviously) which contains "_patch.ups" ("patch specific" string)
		// TODO: Why? Because the user might have random non-patch .ups files in the patch folder?
		foreach (string file in files)
		{
			if (IsDirectory(file))
			{
				continue;
			}

			actual_ups_files.Add(file);

			if (!file.Contains(CurrentGame.PatchSpecificString))
			{
				continue;
			}

			to_be_applied.Add(file);
		}

		if(to_be_applied.Count <= 0 && actual_ups_files.Count > 0)
		{
			Log(info[44], new Dictionary<string, string>(){
				{ "VAR_PATCH_FORMAT", CurrentGame.PatchFormat.ToString() },
				{ "VAR_PATCH_SPECIFIC_STRING", CurrentGame.PatchSpecificString },
			});
			return;
		}

		// Prepare... something???
		// TODO: Explain this better
		GetToApply(files);

		if(to_be_applied.Count > 0)
		{
			Log("", null, Verbosity.Info, LogType.ConsoleOnly);
			foreach (string file in to_be_applied)
			{
				Log("OK ( ⮜ ): VAR_SOME_FILE", new Dictionary<string, string>()
				{
					{ "VAR_SOME_FILE", file },
				}, Verbosity.Info, LogType.ConsoleOnly);
			}
			Log("", null, Verbosity.Info, LogType.ConsoleOnly);
		}
		

		if (to_apply.Count > 0)
		{
			Log("", null, Verbosity.Info, LogType.ConsoleOnly);
			foreach (string file in to_apply)
			{
				Log("OK ( ⮞ ): VAR_SOME_FILE", new Dictionary<string, string>()
				{
					{ "VAR_SOME_FILE", file },
				}, Verbosity.Info, LogType.ConsoleOnly);
			}
			Log("", null, Verbosity.Info, LogType.ConsoleOnly);
		}

		if (couldnt_be_found.Count > 0)
		{
			Log("", null, Verbosity.Info, LogType.ConsoleOnly);
			foreach (string file in couldnt_be_found)
			{
				Log(info[47], new Dictionary<string, string>()
				{
					{ "VAR_SOME_FILE", file },
				}, Verbosity.Info, LogType.ConsoleOnly);
			}
			Log("", null, Verbosity.Info, LogType.ConsoleOnly);
		}

		Log(info[23]);

		PatchPathPreviewTextbox.Text = ups_folder;
	}

	private void RemoveFromMissingList(string str)
	{
		// Remove a file? from the list of files? that could not be found
		// ...Because apparently they were just found???

		while (couldnt_be_found.IndexOf(str) >= 0) couldnt_be_found.RemoveAt(couldnt_be_found.IndexOf(str));
		while (couldnt_be_found.Any(str.Contains)) couldnt_be_found.Remove(str);
		couldnt_be_found.RemoveAll(x => x == str);
	}

	private void GetToApply(string[] ups_files)
	{
		to_apply = new List<string>();
		already_checked = new List<string>();

		// WHY were those all different functions?

		// REMEMBER TO UPDATE BELOW AS WELL
		switch(CurrentGame.GameID) {
			case Game.DanganronpaV3:
				TryToApplyFiles(ups_files, ".spc");
				TryToApplyFiles(ups_files, ".awb");
				if (IsUnity)
				{
					// .ab, .pb e .asset files are only in the Unity version(s?)
					TryToApplyFiles(ups_files, ".pb");
					TryToApplyFiles(ups_files, ".ab");
					TryToApplyFiles(ups_files, ".assets");
				}
				break;
			case Game.AITheSomniumFiles:
				TryToApplyFiles(ups_files, "");
				TryToApplyFiles(ups_files, ".assets");
				TryToApplyFiles(ups_files, ".exe");
				break;
			default:
				break;
		}

		foreach (string f in to_apply)
		{
			switch (CurrentGame.GameID)
			{
				case Game.DanganronpaV3:
					if (!Path.HasExtension(f))
					{
						// If it doesn't have an extension, we don't care about this file
						continue;
					}
					break;
				default:
					break;
			}

			// Remove the extension

			string no_ext = f.Substring(0, f.Length - Path.GetExtension(f).Length);

			// All "possible" extensions, backups and UPS included

			// REMEMBER TO UPDATE ABOVE AS WELL

			List<string> all_installable_extensions = new List<string>();

			List<string> DRV3_all_installable_extensions = new List<string>()
			{
				// Legacy and Xbox
				".spc",
				".awb",

				// Unity
				".ab",
				".pb",
				".assets",

				// Patch
				".ups",

				// EXE
				".exe",
			};

			// REMEMBER TO UPDATE ABOVE AS WELL

			List<string> AITSF_all_installable_extensions = new List<string>()
			{
				"",

				// Unity
				".ab",
				".pb",
				".assets",
				
				// Patch
				".xdelta",

				// EXE
				".exe",
			};

			switch(CurrentGame.GameID)
			{
				case Game.DanganronpaV3:
					all_installable_extensions = DRV3_all_installable_extensions;
					break;
				case Game.AITheSomniumFiles:
					all_installable_extensions = AITSF_all_installable_extensions;
					break;
				default:
					break;
			}

			string bak = "_bak";

			// Remove all extension versions of the file from the "could not find" list
			// TODO: Any scenario where this is actually useful?
			// There must be some, or otherwise these functions below wouldn't be there, but which ones?

			foreach (string extension in all_installable_extensions)
			{
				string file = no_ext + extension;
				string filebak = no_ext + extension + bak;
				RemoveFromMissingList(file);
				RemoveFromMissingList(filebak);
			}
            RemoveFromMissingList(no_ext.ToLowerInvariant());
            RemoveFromMissingList(no_ext + bak.ToLowerInvariant());
        }
	}

	private void TryToApplyFiles(string[] ups_files, string extension)
	{
		// Calculate which (extension, ex. SPC) files correspond to the UPS files
		// and add them to a vector

		// Find out which file corresponds to
		// ex. C:\Patch\data\win\game_resident\game_resident_US.spc
		// the solution would be
		// ex. C:\SomeFolders\V3Folder\data\win\game_resident\game_resident_US.spc

		foreach (string file in ups_files)
		{
			string file_noext = Path.GetFileName(file);

			// We don't want duplicates
			if (to_apply.Contains(file) || to_apply.Contains(file_noext))
			{
				continue;
			}

			// We don't want folders
			if (IsDirectory(file))
			{
				continue;
			}

			// If it doesn't contain the patch-specific string, then it's a random .ups file?
			// We don't care about it
			if (!file.Contains(CurrentGame.PatchSpecificString))
			{
				continue;
			}

			// Remove the part before (and) the UPS folder
			// ...So, like,
			// ex. C:\Patch\data\win\game_resident\game_resident_US_patch.ups
			// would become
			// ex. data\win\game_resident\game_resident_US_patch.ups
			// right?
			string second_half = file.Substring(ups_folder.Length + 1);

			// If it's already in the "already_checked" vector, we already checked it(?)
			if (already_checked.Contains(second_half.ToLowerInvariant()))
			{
				continue;
			}

			// Get the "approximate" (extension, ex. .spc) path
			// By combining the installation folder and the relative path of the .ups file
			// ex. data\win\game_resident\game_resident_US_patch.ups
			// would become
			// ex. C:\SomeFolders\V3Folder\data\win\game_resident\game_resident_US_patch.ups
			string approximate_spc = Path.Combine(verified_installation_folder, second_half);

            // If it's a directory, ...which it most certainly isn't
            // UPDATE (September 2023)
            // Some users complained that files would not install
            // if there was a folder name that had the same name as the file
            // So let's just ask the user what they want to do, in this case
            if (IsDirectory(approximate_spc))
			{
				var res = MessageBox.Show(approximate_spc + ": " + ui_messages[1], ui_messages[0], MessageBoxButtons.YesNo);
				if (res != DialogResult.Yes)
				{
					continue;
				}
			}

			// Remove the patch-specific string from the file
			// PatchSpecificString is, currently at least, "_patch.ups" (length: 10)
			// So,
			// ex. C:\SomeFolders\V3Folder\data\win\game_resident\game_resident_US_patch.ups
			// would become
			// // ex. C:\SomeFolders\V3Folder\data\win\game_resident\game_resident_US

			approximate_spc = approximate_spc.Substring(0, approximate_spc.Length - CurrentGame.PatchSpecificString.Length);

			// If the string is now empty, because apparently it was literally just the patch-specific string?
			if (approximate_spc.Length <= 0)
			{
				continue;
			}

			// The objective is now to find the (extension, ex. .spc) file
			// Try to see if the (extension, ex. .spc) file exists

			if (extension.Length > 0)
			{
				if(!approximate_spc.Contains(extension))
				{
					continue;
				}

				/*
				Log("\"VAR_EXT\" - Approximate file: VAR_SOME_FILE", new Dictionary<string, string>()
				{
					{ "VAR_EXT", extension },
					{ "VAR_SOME_FILE", approximate_spc },
				}, Verbosity.Debug, LogType.ConsoleOnly);
				*/

				bool approx_has_extension = Path.HasExtension(approximate_spc) && extension == Path.GetExtension(approximate_spc);
				if (approx_has_extension)
				{
					/*
					Log("Approximate SPC has extension: VAR_VALUE", new Dictionary<string, string>()
					{
						{ "VAR_VALUE", approx_has_extension.ToString() },
					});
					*/
				}

				/*
				Log("\"VAR_EXT\" - File Exists: VAR_SOME_FILE - VAR_RESULT", new Dictionary<string, string>()
				{
					{ "VAR_EXT", extension },
					{ "VAR_SOME_FILE", approximate_spc },
					{ "VAR_RESULT", File.Exists(approximate_spc).ToString() },
				}, Verbosity.Debug, LogType.ConsoleOnly);
				*/

				if (!approx_has_extension)
				{

					string try_approximate_ext_lower = approximate_spc + (extension.ToLowerInvariant());
					string try_approximate_ext_upper = approximate_spc + (extension.ToUpperInvariant());

					string og_no_bak = try_approximate_ext_lower;

					bool exists_lower = FileExistsCaseSensitive(try_approximate_ext_lower);
					bool exists_upper = FileExistsCaseSensitive(try_approximate_ext_upper);
					if (!exists_lower && !exists_upper)
					{
						// try to see if the _bak exists
						try_approximate_ext_lower = try_approximate_ext_lower + "_bak";
						try_approximate_ext_upper = try_approximate_ext_upper + "_bak";

						// and, if so, which one exists
						exists_lower = FileExistsCaseSensitive(try_approximate_ext_lower);
						exists_upper = FileExistsCaseSensitive(try_approximate_ext_upper);

						if (!exists_lower && !exists_upper)
						{
							// None exists
							if (!to_apply.Contains(og_no_bak) &&
								!couldnt_be_found.Contains(og_no_bak.ToLowerInvariant()))
							{
								// DEBUG
								/*
								Log("Situation 1 for VAR_SOME_FILE", new Dictionary<string, string>()
								{
									{ "VAR_SOME_FILE", og_no_bak },
								});
								*/
								couldnt_be_found.Add(og_no_bak.ToLowerInvariant());
							}

							continue;
						}

						// One has to exist, if we're here
						// Lower checked first as that's the rule, not the exception
						if (exists_lower)
						{
							// Lower exists
							// Restore bak to extension (ex. bak -> spc)
							string nobak = try_approximate_ext_lower.Substring(0, try_approximate_ext_lower.Length - extension.Length);
							File.Copy(try_approximate_ext_lower, nobak, true);
							try_approximate_ext_lower = nobak;
						}
						else
						{
							// Upper exists
							// Restore bak to extension (ex. bak -> spc)
							string nobak = try_approximate_ext_upper.Substring(0, try_approximate_ext_upper.Length - extension.Length);
							File.Copy(try_approximate_ext_upper, nobak, true);
							try_approximate_ext_upper = nobak;
						}

					}

					// Add the right version to to_apply
					if (exists_lower)
					{
						to_apply.Add(try_approximate_ext_lower);
					}
					else
					{
						to_apply.Add(try_approximate_ext_upper);
					}

					// Add the relative path to the list of paths we already checked
					already_checked.Add(second_half.ToLowerInvariant());

					// If we found it, then it's not "not found"
					if (to_apply.Contains(try_approximate_ext_lower))
					{
						couldnt_be_found.Remove(try_approximate_ext_lower.ToLowerInvariant());
						couldnt_be_found.Remove(og_no_bak.ToLowerInvariant());
					}
				} else
				{
					// Attempt at fixing a bug regarding Launcher EXE files
					// Please don't break anything...
					bool exists_asis = FileExistsCaseSensitive(approximate_spc);
					if(!exists_asis)
					{
						// TODO: Implement (???)
						continue;
					}

					if (to_apply.Contains(approximate_spc))
					{
						couldnt_be_found.Remove(approximate_spc.ToLowerInvariant());
						continue;
					}

					to_apply.Add(approximate_spc);
					already_checked.Add(second_half.ToLowerInvariant());
					// If we found it, then it's not "not found"
					if (to_apply.Contains(approximate_spc))
					{
						couldnt_be_found.Remove(approximate_spc.ToLowerInvariant());
					}
				}
			} else
			{
				if(to_apply.Contains(approximate_spc))
				{
					couldnt_be_found.Remove(approximate_spc.ToLowerInvariant());
					continue;
				}

				// Examples of file without extension:
				// pretty much all AI: The Somnium Files... files

				string backup = file + "_bak";
				if (File.Exists(backup))
				{
					File.Copy(backup, approximate_spc, true);
				}

				if(!File.Exists(approximate_spc))
				{
					// DEBUG
					/*
					Log("Situation 2 for VAR_SOME_FILE", new Dictionary<string, string>()
					{
						{ "VAR_SOME_FILE", approximate_spc },
					});
					*/
					couldnt_be_found.Add(approximate_spc);
				} else
				{
					to_apply.Add(approximate_spc);
				}
			}
		}
	}
}