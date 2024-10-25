using System.Media;

namespace V3UPSManager;

internal class DisplayInfo
{
	internal static string FormatString(string str, GameBase gb = null)
	{
		if(gb != null)
		{
			str = str.Replace("VAR_LEGACY_EXE_NAME", gb.LEGACY_EXE_NAME);
			str = str.Replace("VAR_ANNIVERSARY_EXE_NAME", gb.ANNIVERSARY_EXE_NAME);
			str = str.Replace("VAR_PATCH_SPECIFIC_STRING", gb.PatchSpecificString);
		}
		return str.Replace("\\n", "\n");
	}

	internal static DialogResult Print(string line, GameBase gb = null)
	{
		MessageBoxButtons btn = new MessageBoxButtons();
		return MessageBox.Show(FormatString(line, gb), "Prompt", btn, MessageBoxIcon.Asterisk);
	}

	internal static DialogResult Ask(string line, GameBase gb = null)
	{
		return MessageBox.Show(FormatString(line, gb), "Prompt", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
	}
}