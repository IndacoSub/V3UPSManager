using System.Media;

namespace V3UPSManager;

// https://stackoverflow.com/questions/1926264/color-different-parts-of-a-richtextbox-string
public static class RichTextBoxExtensions
{
	public static void AppendText(this RichTextBox box, string text, Color color)
	{
		box.SelectionStart = box.TextLength;
		box.SelectionLength = 0;

		box.SelectionColor = color;
		box.AppendText(text);
		box.SelectionColor = box.ForeColor;
	}
}

public partial class MainWindow : Form
{

	internal enum Verbosity {
		Debug,
		Info,
		Error,
	};

	internal enum LogType {
		
		ConsoleOnly,
		MessageBoxOnly,
		Print,
		Ask,
	}

	internal void AddConsoleLogLine(string str, Color c)
	{
		LogConsole.AppendText(str + Environment.NewLine, c);
		LogConsole.ScrollToCaret();
	}

	private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

	internal DialogResult Log(string str, Dictionary<string, string> args = null, Verbosity verbosity = Verbosity.Info, LogType logtype = LogType.Print)
	{
		if (args != null && args.Count > 0)
		{
			foreach (var arg in args)
			{
				if (!arg.Key.StartsWith("VAR_") || arg.Value.StartsWith("VAR_"))
				{
					continue;
				}
				str = str.Replace(arg.Key, arg.Value);
			}
		}

		try
		{
			semaphore.Wait();

			using (FileStream fs = new FileStream("all_log.txt", FileMode.Append, FileAccess.Write, FileShare.None))
			{
				using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default))
				{
					sw.WriteLine(str.Replace("\\", "/"));
					sw.Flush();
				}
			}
		}
		catch (Exception ex)
		{
			// Log or handle the exception
			Console.WriteLine(ex.Message);
		}
		finally
		{
			semaphore.Release();
		}

		Color logcolor = verbosity == Verbosity.Error ? Color.Red : Color.Black;

		DialogResult ret = DialogResult.OK;

		switch (logtype)
		{
			case LogType.ConsoleOnly:
				AddConsoleLogLine(str, logcolor);
				return DialogResult.OK;
			case LogType.MessageBoxOnly:
			case LogType.Print:
				MessageBoxButtons btn = new MessageBoxButtons();
				ret = MessageBox.Show(str, "Prompt", btn, MessageBoxIcon.Asterisk);
				break;
			case LogType.Ask:
				ret = MessageBox.Show(str, "Prompt", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
				break;
			default:
				break;
		}

		if (logtype != LogType.MessageBoxOnly)
		{
			AddConsoleLogLine(str, logcolor);
		}

		return ret;
	}

}