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

	// Checks whether a directory exists with exactly the requested casing.
	// Directory.Exists() alone is insufficient when the source directory is
	// copied from a case-sensitive filesystem and the exact on-disk name matters.
	public static bool DirExistsMatchCase(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return false;
		}

		try
		{
			string fullPath = Path.GetFullPath(path);

			// Root directories have no parent/name pair to compare.
			DirectoryInfo info = new DirectoryInfo(fullPath);
			if (info.Parent == null)
			{
				return info.Exists;
			}

			if (!info.Parent.Exists || string.IsNullOrEmpty(info.Name))
			{
				return false;
			}

			foreach (DirectoryInfo directory in info.Parent.EnumerateDirectories())
			{
				if (string.Equals(
					directory.FullName,
					fullPath,
					StringComparison.Ordinal))
				{
					return true;
				}
			}
		}
		catch
		{
			// Invalid paths, missing parents or filesystem errors are simply
			// treated as "not found" by callers.
		}

		return false;
	}

	// Checks whether a file exists with exactly the requested casing.
	// This avoids false positives on case-insensitive filesystems while still
	// behaving correctly when running from a normal Windows installation.
	public static bool FileExistsCaseSensitive(string filename)
	{
		if (string.IsNullOrWhiteSpace(filename))
		{
			return false;
		}

		try
		{
			string fullPath = Path.GetFullPath(filename);
			string? directoryPath = Path.GetDirectoryName(fullPath);
			string fileName = Path.GetFileName(fullPath);

			if (string.IsNullOrEmpty(directoryPath) ||
				string.IsNullOrEmpty(fileName) ||
				!Directory.Exists(directoryPath))
			{
				return false;
			}

			foreach (string file in Directory.EnumerateFiles(directoryPath))
			{
				if (string.Equals(
					file,
					fullPath,
					StringComparison.Ordinal))
				{
					return true;
				}
			}
		}
		catch
		{
			// Treat inaccessible/invalid paths as non-existent.
		}

		return false;
	}

	/// <summary>
	/// Computes an MD5 hash using the same lowercase hexadecimal representation
	/// historically used by the game validators. An empty result means the file
	/// could not be read.
	/// </summary>
	public static string ComputeMd5(string filename)
	{
		if (string.IsNullOrWhiteSpace(filename))
		{
			return string.Empty;
		}

		try
		{
			using MD5 md5 = MD5.Create();
			using FileStream stream = File.OpenRead(filename);
			return ToHex(md5.ComputeHash(stream), upperCase: false);
		}
		catch
		{
			return string.Empty;
		}
	}

	public static string ToHex(byte[] bytes, bool upperCase)
	{
		if (bytes == null)
		{
			return string.Empty;
		}

		StringBuilder result = new StringBuilder(bytes.Length * 2);

		for (int i = 0; i < bytes.Length; i++)
		{
			result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
		}

		return result.ToString();
	}

	public static bool IsDirectory(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return false;
		}

		try
		{
			FileAttributes attributes = File.GetAttributes(path);
			return attributes.HasFlag(FileAttributes.Directory);
		}
		catch
		{
			// Missing, invalid or inaccessible paths are not treated as folders.
			return false;
		}
	}
}
