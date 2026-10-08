namespace V3UPSManager;

public static class RichTextBoxExtensions
{
	public static void AppendText(this RichTextBox box, string text, Color color)
	{
		if (box == null || box.IsDisposed)
		{
			return;
		}

		box.SelectionStart = box.TextLength;
		box.SelectionLength = 0;
		box.SelectionColor = color;
		box.AppendText(text ?? string.Empty);
		box.SelectionColor = box.ForeColor;
	}
}

public partial class MainWindow : Form
{
	internal enum Verbosity
	{
		Debug,
		Info,
		Error,
	};

	internal enum LogType
	{
		ConsoleOnly,
		MessageBoxOnly,
		Print,
		Ask,
	}

	internal void AddConsoleLogLine(string str, Color c)
	{
		if (LogConsole == null || LogConsole.IsDisposed)
		{
			return;
		}

		LogConsole.AppendText(str + Environment.NewLine, c);
		LogConsole.ScrollToCaret();
	}

	internal void RefreshLogConsole()
	{
		if (LogConsole == null || LogConsole.IsDisposed)
		{
			return;
		}

		// Refresh repaints the control synchronously without pumping the WinForms
		// message queue. This keeps progress visible while preventing re-entrant
		// button clicks and other handlers from running in the middle of an install.
		LogConsole.Refresh();
	}

	private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

	internal DialogResult Log(
		string str,
		Dictionary<string, string>? args = null,
		Verbosity verbosity = Verbosity.Info,
		LogType logtype = LogType.Print)
	{
		str ??= string.Empty;

		if (args != null && args.Count > 0)
		{
			foreach (KeyValuePair<string, string> arg in args)
			{
				if (string.IsNullOrEmpty(arg.Key) ||
					!arg.Key.StartsWith("VAR_", StringComparison.Ordinal) ||
					string.IsNullOrEmpty(arg.Value) ||
					arg.Value.StartsWith("VAR_", StringComparison.Ordinal))
				{
					continue;
				}

				str = str.Replace(arg.Key, arg.Value, StringComparison.Ordinal);
			}
		}
		str = str.Replace("\\n", "\n", StringComparison.OrdinalIgnoreCase);

		bool lockTaken = false;

		try
		{
			semaphore.Wait();
			lockTaken = true;

			string logPath = Path.Combine(
				AppContext.BaseDirectory,
				"all_log.txt");

			using FileStream fs = new FileStream(
				logPath,
				FileMode.Append,
				FileAccess.Write,
				FileShare.Read);

			using StreamWriter sw = new StreamWriter(
				fs,
				new System.Text.UTF8Encoding(
					encoderShouldEmitUTF8Identifier: false));

			sw.WriteLine(str.Replace('\\', '/'));
		}
		catch
		{
			// Logging must never be able to crash the application.
		}
		finally
		{
			if (lockTaken)
			{
				semaphore.Release();
			}
		}

		Color logColor = verbosity == Verbosity.Error
			? Color.Red
			: Color.Black;

		DialogResult result = DialogResult.OK;

		switch (logtype)
		{
			case LogType.ConsoleOnly:
				AddConsoleLogLine(str, logColor);
				break;

			case LogType.MessageBoxOnly:
				result = MessageBox.Show(
					str,
					GetMessageBoxTitle(),
					MessageBoxButtons.OK,
					MessageBoxIcon.Asterisk);
				break;

			case LogType.Print:
				result = MessageBox.Show(
					str,
					GetMessageBoxTitle(),
					MessageBoxButtons.OK,
					MessageBoxIcon.Asterisk);
				AddConsoleLogLine(str, logColor);
				break;

			case LogType.Ask:
				result = MessageBox.Show(
					str,
					GetMessageBoxTitle(),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Asterisk);
				AddConsoleLogLine(str, logColor);
				break;
		}

		return result;
	}

	private string GetMessageBoxTitle()
	{
		// The localization table already owns the warning/title text, so
		// dialogs do not need another hard-coded translated string.
		if (ui_messages != null &&
			ui_messages.Count > 0 &&
			!string.IsNullOrWhiteSpace(ui_messages[0]))
		{
			return ui_messages[0];
		}

		return Text ?? string.Empty;
	}
}
