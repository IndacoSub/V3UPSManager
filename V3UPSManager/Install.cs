using System.Diagnostics;
using System.Text;

namespace V3UPSManager;

public partial class MainWindow : Form
{
	// Files in the installation which will receive the patched data.
	private List<string> to_apply = new List<string>();

	// Patch files corresponding to to_apply. The two lists are kept in exactly
	// the same order so each patch is always applied to its matching base file.
	private List<string> to_be_applied = new List<string>();

	private bool Install()
	{
		if (string.IsNullOrWhiteSpace(verified_installation_folder) ||
			!Directory.Exists(verified_installation_folder))
		{
			Log(info[82], null, Verbosity.Error, LogType.ConsoleOnly);
			RefreshLogConsole();
			return false;
		}

		if (string.IsNullOrWhiteSpace(ups_folder) ||
			!Directory.Exists(ups_folder))
		{
			Log(info[83], null, Verbosity.Error, LogType.ConsoleOnly);
			RefreshLogConsole();
			return false;
		}

		if (to_apply == null ||
			to_be_applied == null ||
			to_apply.Count == 0 ||
			to_be_applied.Count == 0)
		{
			Log(info[84], null, Verbosity.Error, LogType.ConsoleOnly);
			RefreshLogConsole();
			return false;
		}

		// A count mismatch is not just cosmetic: using two independently ordered
		// lists could apply one patch to a different game's file.
		if (to_apply.Count != to_be_applied.Count)
		{
			WriteDiagnosticFiles();
			Log(
				info[85],
				new Dictionary<string, string>()
				{
					{ "VAR_PATCH_COUNT", to_be_applied.Count.ToString() },
					{ "VAR_FILE_COUNT", to_apply.Count.ToString() },
				},
				Verbosity.Error,
				LogType.ConsoleOnly);
			RefreshLogConsole();
			return false;
		}

		// The application is a Windows WinForms program and is distributed with
		// the Windows patch tools. Keep patch-tool selection deterministic and
		// tied to the application distribution.
		string patchToolName =
			CurrentGame.PatchFormatInstaller + ".exe";

		string patchToolPath = Path.Combine(
			AppContext.BaseDirectory,
			patchToolName);

		if (!File.Exists(patchToolPath))
		{
			Log(
				info[86],
				new Dictionary<string, string>()
				{
					{ "VAR_PATCH_TOOL", patchToolPath },
				},
				Verbosity.Error,
				LogType.ConsoleOnly);
			RefreshLogConsole();
			return false;
		}

		// Snapshot both aligned lists before any filesystem work begins. This makes
		// the transaction independent from mutable UI state for its entire lifetime.
		List<string> filesToApply = new List<string>(to_apply);
		List<string> patchesToApply = new List<string>(to_be_applied);

		// Clean diagnostic files from previous attempts.
		ClearDiagnosticFiles();

		List<string> modifiedFiles = new List<string>();
		string currentOutputFile = string.Empty;

		// Show the actual installation plan before any patch tool is started.
		// The message is console-only so it is visible immediately without adding
		// another modal dialog to the installation flow.
		Log(
			info[56],
			new Dictionary<string, string>()
			{
				{ "VAR_FILE_COUNT", filesToApply.Count.ToString() },
			},
			Verbosity.Info,
			LogType.ConsoleOnly);
		RefreshLogConsole();

		try
		{
			// Explicitly mark the pre-patch phase so an instant failure can be
			// distinguished from a failure after the patch tool starts.
			Log(info[94], null, Verbosity.Info, LogType.ConsoleOnly);
			RefreshLogConsole();

			// Create or validate the original-file backups before any patch tool
			// is executed. Existing backups are never overwritten.
			for (int i = 0; i < filesToApply.Count; i++)
			{
				string file = filesToApply[i];
				currentOutputFile = file ?? string.Empty;

				Log(
					info[57],
					new Dictionary<string, string>()
					{
						{ "VAR_FILE_INDEX", (i + 1).ToString() },
						{ "VAR_FILE_COUNT", filesToApply.Count.ToString() },
						{ "VAR_OUTPUT_FILE", file ?? string.Empty },
					},
					Verbosity.Info,
					LogType.ConsoleOnly);
				RefreshLogConsole();

				if (string.IsNullOrWhiteSpace(file))
				{
					RestoreModifiedFiles(modifiedFiles);
					Log(info[87], null, Verbosity.Error, LogType.ConsoleOnly);
					RefreshLogConsole();
					Log(info[54], null, Verbosity.Error);
					return false;
				}

				bool isExe = string.Equals(
					Path.GetExtension(file),
					".exe",
					StringComparison.OrdinalIgnoreCase);

				if (isExe && !CanAccessExe)
				{
					Log(
						info[58],
						new Dictionary<string, string>()
						{
							{ "VAR_OUTPUT_FILE", file },
						},
						Verbosity.Info,
						LogType.ConsoleOnly);
					RefreshLogConsole();
					continue;
				}

				string backupFile = file + "_bak";

				if (!File.Exists(backupFile))
				{
					if (!File.Exists(file))
					{
						RestoreModifiedFiles(modifiedFiles);
						Log(
							info[88],
							new Dictionary<string, string>()
							{ { "VAR_OUTPUT_FILE", file } },
							Verbosity.Error,
							LogType.ConsoleOnly);
						RefreshLogConsole();
						Log(info[54], null, Verbosity.Error);
						return false;
					}

					File.Copy(file, backupFile, false);

					Log(
						info[60],
						new Dictionary<string, string>()
						{
							{ "VAR_BACKUP_FILE", backupFile },
						},
						Verbosity.Info,
						LogType.ConsoleOnly);
					RefreshLogConsole();
				}
				else
				{
					Log(
						info[59],
						new Dictionary<string, string>()
						{
							{ "VAR_BACKUP_FILE", backupFile },
						},
						Verbosity.Info,
						LogType.ConsoleOnly);
					RefreshLogConsole();
				}

				if (!File.Exists(backupFile))
				{
					RestoreModifiedFiles(modifiedFiles);
					Log(info[54], null, Verbosity.Error);
					return false;
				}
			}

			for (int j = 0; j < filesToApply.Count; j++)
			{
				string outfile = filesToApply[j];
				string patchFile = patchesToApply[j];
				currentOutputFile = outfile ?? string.Empty;

				Log(
					info[61],
					new Dictionary<string, string>()
					{
						{ "VAR_PATCH_FILE", patchFile ?? string.Empty },
						{ "VAR_OUTPUT_FILE", outfile ?? string.Empty },
					},
					Verbosity.Info,
					LogType.ConsoleOnly);
				RefreshLogConsole();

				if (string.IsNullOrWhiteSpace(outfile) ||
					string.IsNullOrWhiteSpace(patchFile))
				{
					RestoreModifiedFiles(modifiedFiles);
					Log(info[54], null, Verbosity.Error);
					return false;
				}

				if (string.Equals(
					Path.GetExtension(outfile),
					".exe",
					StringComparison.OrdinalIgnoreCase) &&
					!CanAccessExe)
				{
					Log(
						info[58],
						new Dictionary<string, string>()
						{
							{ "VAR_OUTPUT_FILE", outfile },
						},
						Verbosity.Info,
						LogType.ConsoleOnly);
					RefreshLogConsole();
					continue;
				}

				string before = outfile + "_bak";

				if (!File.Exists(before) || !File.Exists(patchFile))
				{
					RestoreModifiedFiles(modifiedFiles);
					Log(
						info[89],
						new Dictionary<string, string>()
						{ { "VAR_PATCH_FILE", patchFile } },
						Verbosity.Error,
						LogType.ConsoleOnly);
					RefreshLogConsole();
					Log(info[54], null, Verbosity.Error);
					return false;
				}

				// Every patch gets its own unique temporary output file. This prevents
				// stale output from a crashed run from being reused accidentally.
				string temporaryOutput =
					outfile + ".v3ups_tmp." + Guid.NewGuid().ToString("N");

				string commandDiagnostic = BuildDiagnosticCommand(
					patchToolPath,
					CurrentGame.PatchFormat,
					before,
					patchFile,
					temporaryOutput);

				AppendDiagnostic(commandDiagnostic);

				Log(
					info[62],
					new Dictionary<string, string>()
					{
						{ "VAR_COMMAND", commandDiagnostic },
					},
					Verbosity.Info,
					LogType.ConsoleOnly);
				RefreshLogConsole();

				// Never write the patch output directly over the live game file.
				// A failed patch therefore leaves the existing installation intact.
				if (File.Exists(temporaryOutput))
				{
					File.Delete(temporaryOutput);
				}

				using Process process = new Process();

				process.StartInfo = new ProcessStartInfo
				{
					FileName = patchToolPath,
					WorkingDirectory = AppContext.BaseDirectory,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true,
				};

				switch (CurrentGame.PatchFormat)
				{
					case FormatType.UPS:
						process.StartInfo.ArgumentList.Add("apply");
						process.StartInfo.ArgumentList.Add("--base");
						process.StartInfo.ArgumentList.Add(before);
						process.StartInfo.ArgumentList.Add("--patch");
						process.StartInfo.ArgumentList.Add(patchFile);
						process.StartInfo.ArgumentList.Add("--output");
						process.StartInfo.ArgumentList.Add(temporaryOutput);
						break;

					case FormatType.XDelta:
						process.StartInfo.ArgumentList.Add("-d");
						process.StartInfo.ArgumentList.Add("-f");
						process.StartInfo.ArgumentList.Add("-s");
						process.StartInfo.ArgumentList.Add(before);
						process.StartInfo.ArgumentList.Add(patchFile);
						process.StartInfo.ArgumentList.Add(temporaryOutput);
						break;

					default:
						RestoreModifiedFiles(modifiedFiles);
						Log(info[92], null, Verbosity.Error, LogType.ConsoleOnly);
						RefreshLogConsole();
						Log(info[54], null, Verbosity.Error);
						return false;
				}

				try
				{
					if (!process.Start())
					{
						RestoreModifiedFiles(modifiedFiles);
						Log(info[91], null, Verbosity.Error, LogType.ConsoleOnly);
						RefreshLogConsole();
						Log(info[54], null, Verbosity.Error);
						return false;
					}

					// Read stdout/stderr concurrently. Reading one stream completely
					// before the other can deadlock when a tool fills the first buffer.
					Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
					Task<string> errorTask = process.StandardError.ReadToEndAsync();

					process.WaitForExit();

					string output = outputTask.GetAwaiter().GetResult();
					string error = errorTask.GetAwaiter().GetResult();

					AppendDiagnostic(BuildDiagnosticResult(
						patchFile,
						outfile,
						process.ExitCode,
						output,
						error));

					Log(
						info[63],
						new Dictionary<string, string>()
						{
							{ "VAR_EXIT_CODE", process.ExitCode.ToString() },
							{ "VAR_OUTPUT_FILE", outfile },
						},
						process.ExitCode == 0 ? Verbosity.Info : Verbosity.Error,
						LogType.ConsoleOnly);

					if (!string.IsNullOrWhiteSpace(output))
					{
						Log(
							info[64],
							new Dictionary<string, string>()
							{
								{ "VAR_TOOL_OUTPUT", output.Trim() },
							},
							Verbosity.Info,
							LogType.ConsoleOnly);
					}

					if (!string.IsNullOrWhiteSpace(error))
					{
						Log(
							info[65],
							new Dictionary<string, string>()
							{
								{ "VAR_TOOL_ERROR", error.Trim() },
							},
							Verbosity.Error,
							LogType.ConsoleOnly);
					}
					RefreshLogConsole();

					if (process.ExitCode != 0 ||
						!File.Exists(temporaryOutput))
					{
						if (process.ExitCode == 0 && !File.Exists(temporaryOutput))
						{
							Log(
								info[90],
								new Dictionary<string, string>()
								{ { "VAR_OUTPUT_FILE", temporaryOutput } },
								Verbosity.Error,
								LogType.ConsoleOnly);
							RefreshLogConsole();
						}

						try
						{
							if (File.Exists(temporaryOutput))
							{
								File.Delete(temporaryOutput);
							}
						}
						catch
						{
							// Cleanup is best-effort.
						}

						RestoreModifiedFiles(modifiedFiles);
						Log(info[54], null, Verbosity.Error);
						return false;
					}
				}
				catch (Exception ex)
				{
					Log(
						info[70],
						new Dictionary<string, string>()
						{
							{ "VAR_OUTPUT_FILE", outfile },
							{ "VAR_EXCEPTION", ex.Message },
						},
						Verbosity.Error,
						LogType.ConsoleOnly);
					RefreshLogConsole();

					try
					{
						if (File.Exists(temporaryOutput))
						{
							File.Delete(temporaryOutput);
						}
					}
					catch
					{
						// Cleanup is best-effort.
					}

					RestoreModifiedFiles(modifiedFiles);
					Log(info[54], null, Verbosity.Error);
					return false;
				}

				try
				{
					// Replace the live target only after the patch tool has
					// successfully produced its complete output.
					File.Move(temporaryOutput, outfile, true);
					modifiedFiles.Add(outfile);

					Log(
						info[66],
						new Dictionary<string, string>()
						{
							{ "VAR_OUTPUT_FILE", outfile },
						},
						Verbosity.Info,
						LogType.ConsoleOnly);
					RefreshLogConsole();
				}
				catch (Exception ex)
				{
					Log(
						info[70],
						new Dictionary<string, string>()
						{
							{ "VAR_OUTPUT_FILE", outfile },
							{ "VAR_EXCEPTION", ex.Message },
						},
						Verbosity.Error,
						LogType.ConsoleOnly);
					RefreshLogConsole();

					try
					{
						if (File.Exists(temporaryOutput))
						{
							File.Delete(temporaryOutput);
						}
					}
					catch
					{
						// Cleanup is best-effort.
					}

					RestoreModifiedFiles(modifiedFiles);
					Log(info[54], null, Verbosity.Error);
					return false;
				}
			}

			// Verify every expected output before declaring success.
			for (int i = 0; i < filesToApply.Count; i++)
			{
				string file = filesToApply[i];

				Log(
					info[67],
					new Dictionary<string, string>()
					{
						{ "VAR_FILE_INDEX", (i + 1).ToString() },
						{ "VAR_FILE_COUNT", filesToApply.Count.ToString() },
						{ "VAR_OUTPUT_FILE", file ?? string.Empty },
					},
					Verbosity.Info,
					LogType.ConsoleOnly);
				RefreshLogConsole();
				bool isExe = string.Equals(
					Path.GetExtension(file),
					".exe",
					StringComparison.OrdinalIgnoreCase);

				if (isExe && !CanAccessExe)
				{
					continue;
				}

				if (!File.Exists(file) || !File.Exists(file + "_bak"))
				{
					RestoreModifiedFiles(modifiedFiles);
					Log(
						info[93],
						new Dictionary<string, string>()
						{ { "VAR_OUTPUT_FILE", file } },
						Verbosity.Error,
						LogType.ConsoleOnly);
					RefreshLogConsole();
					Log(info[54], null, Verbosity.Error);
					return false;
				}
			}

			Log(
				info[68],
				new Dictionary<string, string>()
				{
					{ "VAR_MODIFIED_COUNT", modifiedFiles.Count.ToString() },
					{ "VAR_FILE_COUNT", filesToApply.Count.ToString() },
				},
				Verbosity.Info,
				LogType.ConsoleOnly);
			RefreshLogConsole();

			return true;
		}
		catch (Exception ex)
		{
			Log(
				info[70],
				new Dictionary<string, string>()
				{
					{ "VAR_OUTPUT_FILE", currentOutputFile },
					{ "VAR_EXCEPTION", ex.Message },
				},
				Verbosity.Error,
				LogType.ConsoleOnly);
			RefreshLogConsole();
			RestoreModifiedFiles(modifiedFiles);
			Log(info[54], null, Verbosity.Error);
			return false;
		}
	}

	private void RestoreModifiedFiles(List<string> modifiedFiles)
	{
		foreach (string file in modifiedFiles.AsEnumerable().Reverse())
		{
			try
			{
				string backup = file + "_bak";

				if (File.Exists(backup))
				{
					File.Copy(backup, file, true);

					Log(
						info[69],
						new Dictionary<string, string>()
						{
							{ "VAR_OUTPUT_FILE", file },
						},
						Verbosity.Info,
						LogType.ConsoleOnly);
					RefreshLogConsole();
				}
			}
			catch (Exception ex)
			{
				Log(
					info[70],
					new Dictionary<string, string>()
					{
						{ "VAR_OUTPUT_FILE", file },
						{ "VAR_EXCEPTION", ex.Message },
					},
					Verbosity.Error,
					LogType.ConsoleOnly);
				RefreshLogConsole();

				// Keep attempting to restore the remaining files. The generic
				// failure message is emitted by the caller.
			}
		}
	}

	private void ClearDiagnosticFiles()
	{
		try
		{
			string[] files =
			{
				"patch_log.txt",
				"to_apply.txt",
				"to_be_applied.txt",
				"bak_files.txt",
			};

			foreach (string name in files)
			{
				string path = Path.Combine(AppContext.BaseDirectory, name);

				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
		}
		catch
		{
			// Diagnostics are optional.
		}
	}

	private void WriteDiagnosticFiles()
	{
		try
		{
			File.WriteAllLines(
				Path.Combine(AppContext.BaseDirectory, "to_apply.txt"),
				to_apply ?? new List<string>(),
				Encoding.UTF8);

			File.WriteAllLines(
				Path.Combine(AppContext.BaseDirectory, "to_be_applied.txt"),
				to_be_applied ?? new List<string>(),
				Encoding.UTF8);
		}
		catch
		{
			// Diagnostics are optional.
		}
	}

	private void AppendDiagnostic(string text)
	{
		try
		{
			File.AppendAllText(
				Path.Combine(AppContext.BaseDirectory, "patch_log.txt"),
				text + Environment.NewLine,
				Encoding.UTF8);
		}
		catch
		{
			// Diagnostic logging must never interrupt installation.
		}
	}

	private static string BuildDiagnosticCommand(
		string executable,
		FormatType format,
		string before,
		string patchFile,
		string temporaryOutput)
	{
		string safeExecutable = executable.Replace("\\", "/");
		string safeBefore = before.Replace("\\", "/");
		string safePatch = patchFile.Replace("\\", "/");
		string safeOutput = temporaryOutput.Replace("\\", "/");

		return format switch
		{
			FormatType.UPS =>
				$"{safeExecutable} apply --base \"{safeBefore}\" --patch \"{safePatch}\" --output \"{safeOutput}\"",

			FormatType.XDelta =>
				$"{safeExecutable} -d -f -s \"{safeBefore}\" \"{safePatch}\" \"{safeOutput}\"",

			_ => safeExecutable,
		};
	}

	private static string BuildDiagnosticResult(
		string patchFile,
		string outputFile,
		int exitCode,
		string stdout,
		string stderr)
	{
		StringBuilder result = new StringBuilder();

		result.Append("ExitCode=");
		result.Append(exitCode);
		result.Append(" Patch=");
		result.Append(patchFile.Replace("\\", "/"));
		result.Append(" Output=");
		result.Append(outputFile.Replace("\\", "/"));

		if (!string.IsNullOrWhiteSpace(stdout))
		{
			result.AppendLine();
			result.Append(stdout.Trim());
		}

		if (!string.IsNullOrWhiteSpace(stderr))
		{
			result.AppendLine();
			result.Append(stderr.Trim());
		}

		return result.ToString();
	}
}
