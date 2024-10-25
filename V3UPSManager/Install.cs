using System.Diagnostics;

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

		// Check if "ups.exe" from "UPS Tools" exists
		// UPS Tools is licensed under the MIT license
		// https://github.com/rameshvarun/ups
		// https://github.com/rameshvarun/ups/LICENSE

		string patch_tool_exe = CurrentGame.PatchFormatInstaller + ".exe";

		string ups = Path.Combine(current_dir, patch_tool_exe);
		if (!File.Exists(ups))
		{
			DisplayInfo.Print(info[24]);
			return;
		}

		// Check if the installation folder is not valid
		if (string.IsNullOrWhiteSpace(verified_installation_folder) || verified_installation_folder.Length <= 0 ||
			!Directory.Exists(verified_installation_folder))
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
			DisplayInfo.Print(info[43]);

			if (!IsUnity)
			{
				DisplayInfo.Print(info[26]);
			}
			else
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
		// As of June 2023, the latest game version for the Legacy (Steam) version is 1.01
		// It's safe to assume that there won't be any update anytime soon

		foreach (string file in to_apply)
		{
			if (to_apply.Contains("_bak"))
			{
				continue;
			}

			if (!File.Exists(file + "_bak") && File.Exists(file))
			{
				// Only create one when there isn't one
				File.Copy(file, file + "_bak", false);
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

			// Output .spc/.awb/.pb/.ab/.assets file
			string outfile = to_apply[j];

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
			//p.StartInfo.WorkingDirectory = cur;
			p.StartInfo.FileName = patch_tool_exe;
			p.StartInfo.Arguments = command;
			p.StartInfo.RedirectStandardOutput = false;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.CreateNoWindow = true;
			p.Start();
			p.WaitForExit();

			// If the outfile (not _bak) file does not exist while the backup does
			if (!File.Exists(outfile) && File.Exists(before))
			{
				DisplayInfo.Print("File does not exist: " + outfile +
								  ", creating manually from backup... (please report this)");
				// Then copy the backup
				File.Copy(before, outfile, false);
			}
		}
	}
}