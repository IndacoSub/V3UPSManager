﻿using System.Diagnostics;

namespace V3UPSManager;

public partial class MainWindow : Form
{
	// UPS Files to apply (so the "patch" section)
	private List<string> to_apply = new List<string>();

	// SPC/AB/etc. files to mod (so the "installation folder" section)
	private List<string> to_be_applied = new List<string>();

	private void Install()
	{
		string cur = Directory.GetCurrentDirectory();
		string current_dir = Path.GetFullPath(cur);

		string patch_tool_exe = CurrentGame.PatchFormatInstaller + ".exe";

		string ups = Path.Combine(current_dir, patch_tool_exe);
		if (!File.Exists(ups))
		{
			Log(info[24], new Dictionary<string, string>()
			{
				{ "VAR_PATCH_FORMAT_INSTALLER_EXE", CurrentGame.PatchFormatInstaller },
			}, Verbosity.Error);
			return;
		}

		// Check if the installation folder is not valid
		if (string.IsNullOrWhiteSpace(verified_installation_folder) || verified_installation_folder.Length <= 0 ||
			!Directory.Exists(verified_installation_folder))
		{
			Log(info[25], null, Verbosity.Error);
			return;
		}

		// Check if the UPS folder is not valid
		if (string.IsNullOrWhiteSpace(ups_folder) || ups_folder.Length <= 0 || !Directory.Exists(ups_folder))
		{
			Log(info[25], null, Verbosity.Error);
			return;
		}

		if (to_be_applied.Count <= 0)
		{
			Log(info[43], null, Verbosity.Error);

			switch (CurrentGame.GameID)
			{
				case Game.DanganronpaV3:
					if (!IsUnity)
					{
						Log(info[26], null, Verbosity.Error);
					}
					else
					{
						Log(info[36], null, Verbosity.Error);
					}
					break;
				case Game.AITheSomniumFiles:
					// TODO: Implement
					break;
				default:
					break;
			}

			return;
		}

		if (to_apply.Count <= 0)
		{
			Log(info[27], new Dictionary<string, string>()
			{
				{ "VAR_PATCH_FORMAT", CurrentGame.PatchFormat.ToString() },
			}, Verbosity.Error);
			return;
		}

		// DO NOT SORT THE STRING ARRAYS
		/*
			string logFileApplyBefore = "to_apply_before.txt";
			string logFileApplyAfter = "to_apply_after.txt";
			string logFileToBeAppliedBefore = "to_be_applied_before.txt";
			string logFileToBeAppliedAfter = "to_be_applied_after.txt";
			File.WriteAllLines(logFileApplyBefore, to_apply);
			File.WriteAllLines(logFileToBeAppliedBefore, to_be_applied);
			to_apply.Sort();
			to_be_applied.Sort();
			File.WriteAllLines(logFileApplyAfter, to_apply);
			File.WriteAllLines(logFileToBeAppliedAfter, to_be_applied);
		*/

		// *If they don't exist already*, copy the SPCs/Unity-files
		// as .*_bak files (backup)
		// This allows us to maintain compatibility
		// with future translation updates
		// but *will* break for any new
		// game update which updates the
		// base files (hasn't happened yet)
		// As of June 2023, the latest game version for the Legacy (Steam) version is 1.01
		// It's safe to assume that there won't be any update anytime soon

		// Foreach in to_apply (patch files?)
		foreach (string file in to_apply)
		{
			if (file.EndsWith(".exe") && !CanAccessExe)
			{
				continue;
			}

			// Ignore bak files in to_apply
			if (to_apply.Contains("_bak"))
			{
				continue;
			}

			// If ex. game_resident_US.spc_bak DOES NOT exist while game_resident_US.spc DOES exist
			if (!File.Exists(file + "_bak") && File.Exists(file))
			{
				// Make a backup of the .spc file
				// Only create a backup when there isn't one
				File.Copy(file, file + "_bak", false);
			}

			// If ex. game_resident_US.spc DOES exist while game_resident_US.spc_bak ALSO DOES exist: 
			if (File.Exists(file) && File.Exists(file + "_bak"))
			{
				// Delete game_resident_US.spc
				File.Delete(file);
			}

			// October 2024:
			// Copy the _bak to the file

			File.Copy(file + "_bak", file, true);
		}

		// Apply the patches

		string logFile = Path.Combine(Directory.GetCurrentDirectory(), "patch_log.txt");
		if (File.Exists(logFile))
		{
			File.Delete(logFile);
		}

		string logFileApply = "to_apply.txt";
		string logFileToBeApplied = "to_be_applied.txt";
		string bak_files_log = "bak_files.txt";
		if (File.Exists(logFileApply))
		{
			File.Delete(logFileApply);
		}
		if (File.Exists(logFileToBeApplied))
		{
			File.Delete(logFileToBeApplied);
		}
		if(File.Exists(bak_files_log))
		{
			File.Delete(bak_files_log);
		}


		if (to_apply.Count != to_be_applied.Count || false)
		{
			Log("🥶", null, Verbosity.Error, LogType.ConsoleOnly);
			// Oh my God
			File.WriteAllLines(logFileApply, to_apply);
			File.WriteAllLines(logFileToBeApplied, to_be_applied);
			return;
		}

		if(to_apply.Count <= 0 || to_be_applied.Count <= 0)
		{
			Log("👺", null, Verbosity.Error, LogType.ConsoleOnly);
			return;
		}

		for (int j = 0; j < to_apply.Count(); j++)
		{
			if (to_apply.Count <= j || to_apply[j] == null)
			{
				Log(info[28], null, Verbosity.Error);
				return;
			}

			if (to_be_applied.Count <= j || to_be_applied[j] == null)
			{
				Log(info[29], null, Verbosity.Error);
				return;
			}

			// Output .spc/.awb/.pb/.ab/.assets file
			string outfile = to_apply[j];

			if(outfile.EndsWith(".exe") && !CanAccessExe)
			{
				continue;
			}

			// Base .spc/.awb/.pb/.ab/.assets file
			string before = outfile + "_bak";

			// .ups file
			string after = to_be_applied[j];

			string command = "";

			switch(CurrentGame.PatchFormat)
			{
				case FormatType.UPS:
					command = "apply --base " + $"\"{before}\"" + " --patch " + $"\"{after}\"" + " --output " + $"\"{outfile}\"";
					break;
				case FormatType.XDelta:
					command = "-d -f -s " + $"\"{before}\"" + " " + $"\"{after}\"" + " " + $"\"{outfile}\""; 
					break;
				default:
					break;
			}

			if(command.Length == 0)
			{
				// TODO: Implement
			}

			//Process.Start(ups, "apply --base " + $"\"{before}\"" + " --patch " + $"\"{after}\"" + " --output " + $"\"{outfile}\"").WaitForExit();

			var p = new Process();
			p.StartInfo.FileName = patch_tool_exe;
			p.StartInfo.Arguments = command;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = true;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.CreateNoWindow = true;

			if (false)
			{
				Log(patch_tool_exe + " used: " + command);
				Clipboard.SetText(patch_tool_exe + " " + command);
			}
			//Log(patch_tool_exe + " " + command + Environment.NewLine, null, Verbosity.Info, LogType.ConsoleOnly);
			File.AppendAllText(logFile, patch_tool_exe + " " + command + Environment.NewLine);

			p.Start();

			string output = p.StandardOutput.ReadToEnd();
			string error = p.StandardError.ReadToEnd();

			p.WaitForExit();

			if (output.Length > 0)
			{
				//Log("Output:\n" + output);
			}

			if (error.Length > 0)
			{
				Log("Error while installing " + after + " to " + outfile + ":\n" + error, null, Verbosity.Error);
			}

			// WHY IS THIS AFTER INSTALLING? (October 2024)
			// If the outfile (not _bak) file does not exist while the backup does
			if (!File.Exists(outfile) && File.Exists(before))
			{
				Log(ui_messages[6], new Dictionary<string, string>()
				{
					{ "VAR_OUTPUT_FILE", outfile },
				});
				// Then copy the backup

				if (outfile.EndsWith(".exe") && !CanAccessExe)
				{
					continue;
				}
				File.Copy(before, outfile, false);
			}
		}
	}
}