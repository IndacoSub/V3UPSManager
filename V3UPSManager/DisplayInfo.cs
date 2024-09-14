using System.Media;

namespace V3UPSManager;

internal class DisplayInfo
{
	internal static DialogResult Prompt(MessageBoxButtons btn, params string[] lines)
	{
		SystemSounds.Question.Play();
		string msg = string.Join(Environment.NewLine + Environment.NewLine, lines);
		return MessageBox.Show(msg.Replace("\\n", "\n"), "Prompt", btn, MessageBoxIcon.Asterisk);
	}

	internal static DialogResult Print(string line)
	{
		MessageBoxButtons btn = new MessageBoxButtons();
		return MessageBox.Show(line.Replace("\\n", "\n"), "Prompt", btn, MessageBoxIcon.Asterisk);
	}

	internal static DialogResult Ask(string line)
	{
		return MessageBox.Show(line.Replace("\\n", "\n"), "Prompt", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
	}
}