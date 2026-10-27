using Microsoft.Win32;
using System.Security.Cryptography;
using System.Text;

namespace V3UPSManager;

public partial class MainWindow : Form
{
	// Shows the native folder picker while asking Wine/Proton to expose Linux
	// dot-files and dot-directories. Steam Deck uses the Linux convention of
	// hiding entries whose names start with '.', while Windows applications
	// normally receive them through the Wine shell as hidden files.
	//
	// The Wine setting is changed only for the lifetime of this dialog and the
	// previous value is restored afterwards. This avoids permanently changing
	// the user's Wine prefix just because they opened the folder picker.
	private static DialogResult ShowFolderBrowserDialogWithHiddenFiles(
		FolderBrowserDialog dialog)
	{
		const string wineRegistryPath = @"Software\Wine";
		const string showDotFilesValueName = "ShowDotFiles";

		RegistryKey? wineKey = null;
		object? previousValue = null;
		RegistryValueKind previousValueKind = RegistryValueKind.String;
		bool hadPreviousValue = false;
		bool changedWineSetting = false;

		try
		{
			// A native Windows installation normally has no Software\Wine key.
			// Do not create one merely because the folder picker was opened. On
			// Wine/Proton the key belongs to the existing Wine prefix, where this
			// setting is meaningful.
			wineKey = Registry.CurrentUser.OpenSubKey(
				wineRegistryPath,
				writable: true);

			if (wineKey != null)
			{
				try
				{
					previousValue = wineKey.GetValue(
						showDotFilesValueName,
						null,
						RegistryValueOptions.DoNotExpandEnvironmentNames);

					hadPreviousValue = previousValue != null;

					if (hadPreviousValue)
					{
						previousValueKind = wineKey.GetValueKind(
							showDotFilesValueName);
					}

					// Winetricks/Wine tooling uses this Wine-specific value to expose
					// Unix dot-files and dot-directories to Windows file pickers.
					wineKey.SetValue(
						showDotFilesValueName,
						"Y",
						RegistryValueKind.String);

					changedWineSetting = true;
				}
				catch
				{
					// Registry access is best-effort. The native dialog remains usable
					// even when the current runtime does not expose Wine's registry.
				}
			}

			return dialog.ShowDialog();
		}
		finally
		{
			if (wineKey != null && changedWineSetting)
			{
				try
				{
					if (hadPreviousValue)
					{
						wineKey.SetValue(
							showDotFilesValueName,
							previousValue!,
							previousValueKind);
					}
					else
					{
						wineKey.DeleteValue(
							showDotFilesValueName,
							throwOnMissingValue: false);
					}
				}
				catch
				{
					// Best-effort restoration: never turn a successful folder selection
					// into an application failure because the registry became unavailable.
				}
			}

			wineKey?.Dispose();
		}
	}

}
