using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace V3UPSManager;

public partial class MainWindow : Form
{

	private bool CheckXboxConfiguration()
	{
		switch (CurrentGame.GameID)
		{
			case Game.DanganronpaV3:
				return CheckDRV3XboxConfiguration();
			case Game.AITheSomniumFiles:
				// The Steam and Xbox versions of AI are very similar (but not the same)
				// The main differences are the presence of the Movie folder inside StreamingAssets and PathID changes
				// But they are "similar enough" that we can get away with only checking once
				return false;
			default:
				return false;
		}
	}

	// Xbox = Microsoft Store "Anniversary Edition" version
	private bool CheckDRV3XboxConfiguration()
	{
		// Xbox version?
		string exe = Path.Combine(installation_folder, CurrentGame.ANNIVERSARY_EXE_NAME);
		if (!File.Exists(exe))
		{
			Log(info[37], new Dictionary<string, string>(){
				{ "VAR_ANNIVERSARY_EXE_NAME", CurrentGame.ANNIVERSARY_EXE_NAME },
			});
			return false;
		}

		// Check if /data/WIN/ exists
		// Yes, "WIN" is uppercase
		string windata = Path.Combine(installation_folder, "data");
		windata = Path.Combine(windata, "WIN");
		if (!Directory.Exists(windata))
		{
			Log(info[4], null, Verbosity.Error);
			return false;
		}

		// NOTE: Only for the Legacy PC version (Steam)
		// so they shouldn't be there
		// Check if the CPKs are in /data/WIN/
		string cpk_01_en = Path.Combine(windata, "partition_data_win.cpk");
		string cpk_02_en = Path.Combine(windata, "partition_data_win_us.cpk");
		string cpk_03_en = Path.Combine(windata, "partition_resident_win.cpk");
		bool cpk_01_en_exists = File.Exists(cpk_01_en);
		bool cpk_02_en_exists = File.Exists(cpk_02_en);
		bool cpk_03_en_exists = File.Exists(cpk_03_en);
		if (cpk_01_en_exists || cpk_02_en_exists || cpk_03_en_exists)
		{
			Log(info[39], null, Verbosity.Error);
			return false;
		}

		// Check if there are any .arc ("ARC0") files from the Anniversary Edition
		string arc_01 = Path.Combine(windata, "partition_data_win_us.arc");
		string arc_02 = Path.Combine(windata, "partition_data_win.arc");
		string arc_03 = Path.Combine(windata, "partition_resident_win.arc");
		string arc_04 = Path.Combine(windata, "partition_data_win_jp.arc");
		string arc_05 = Path.Combine(windata, "partition_data_win_zh.arc");
		bool arc_01_exists = File.Exists(arc_01);
		bool arc_02_exists = File.Exists(arc_02);
		bool arc_03_exists = File.Exists(arc_03);
		bool arc_04_exists = File.Exists(arc_04);
		bool arc_05_exists = File.Exists(arc_05);
		if (arc_01_exists || arc_02_exists || arc_03_exists)
		{
			Log(info[40], null, Verbosity.Error);
			return false;
		}

		// Cannot check SHA-256 for Anniversary Edition
		// (TODO: because of limitations with the Microsoft Store?)

		// Check if ReShade ( https://reshade.me/ ) is present
		string reshade_ini = Path.Combine(installation_folder, "ReShade.ini");
		if (File.Exists(reshade_ini))
		{
			var proceed = Log(info[9], null, Verbosity.Info, LogType.Ask);
			if (proceed == DialogResult.No || proceed == DialogResult.Cancel)
			{
				return false;
			}
		}

		return true;
	}
}