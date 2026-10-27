namespace V3UPSManager.Core;

public sealed record PatchTarget(string FilePath, string PatchPath);

public sealed record PatchInstallRequest(
    string InstallationRoot,
    string PatchRoot,
    string ToolPath,
    FormatType Format,
    IReadOnlyList<PatchTarget> Targets,
    bool CanAccessExecutable = true);

/// <summary>
/// Applies a validated plan using permanent clean backups and separate rollback copies.
/// A failed reinstall restores the live files from before this attempt, including earlier mods.
/// </summary>
public sealed class PatchInstaller(IPatchTool? tool = null)
{
    private readonly IPatchTool _tool = tool ?? new ProcessPatchTool();

    public bool Install(PatchInstallRequest request, Action<OperationMessage>? log = null)
    {
        var staged = new List<StagedTarget>();
        var modified = new List<StagedTarget>();
        string current = request.InstallationRoot;
        bool completed = false;
        try
        {
            string root = PatchPathSafety.ExistingRoot(request.InstallationRoot);
            string patches = PatchPathSafety.ExistingRoot(request.PatchRoot);
            if (!File.Exists(request.ToolPath))
            {
                OperationLog.Report(log, 86, true, ("VAR_PATCH_TOOL", request.ToolPath));
                return false;
            }
            if (!Enum.IsDefined(request.Format)) throw new ArgumentOutOfRangeException(nameof(request.Format));
            PatchTarget[] targets = request.Targets.ToArray();
            if (targets.Length == 0) throw new InvalidOperationException("No patches were selected.");
            var unique = new HashSet<string>(PatchPathSafety.Comparer);
            var allPaths = new HashSet<string>(PatchPathSafety.Comparer);
            foreach (PatchTarget target in targets)
            {
                current = target.FilePath;
                string file = PatchPathSafety.Within(root, target.FilePath);
                string patch = PatchPathSafety.Within(patches, target.PatchPath);
                string backup = PatchPathSafety.Within(root, file + "_bak");
                if (!unique.Add(file)) throw new IOException($"Duplicate patch target: {file}");
                // A target may not overwrite another target's permanent backup.
                if (!allPaths.Add(file) || !allPaths.Add(backup))
                    throw new IOException($"Patch targets overlap their backup paths: {file}");
                if (!File.Exists(patch)) throw new FileNotFoundException("Patch not found.", patch);
                if (!File.Exists(file) && !File.Exists(backup))
                    throw new FileNotFoundException("Neither the game file nor its clean backup exists.", file);
                if (!request.CanAccessExecutable && string.Equals(Path.GetExtension(file), ".exe", StringComparison.OrdinalIgnoreCase))
                {
                    OperationLog.Report(log, 58, false, ("VAR_OUTPUT_FILE", file));
                    continue;
                }
                string id = Guid.NewGuid().ToString("N");
                staged.Add(new(file, patch, backup, file + ".v3ups_tmp." + id, file + ".v3ups_rollback." + id, File.Exists(file)));
            }
            if (staged.Count == 0) throw new InvalidOperationException("No accessible patch targets were selected.");
            OperationLog.Report(log, 56, false, ("VAR_FILE_COUNT", staged.Count.ToString()));
            OperationLog.Report(log, 94);
            // Validate the complete plan before creating any permanent backups.
            foreach (StagedTarget target in staged)
            {
                current = target.File;
                if (target.HadLiveFile) File.Copy(target.File, target.Rollback, false);
                bool exists = File.Exists(target.Backup);
                if (!exists)
                {
                    // Publish clean backups atomically so a failed copy cannot leave a partial _bak.
                    File.Copy(target.File, target.Output, false);
                    File.Move(target.Output, target.Backup, false);
                }
                OperationLog.Report(log, exists ? 59 : 60, false, ("VAR_BACKUP_FILE", target.Backup));
            }
            // Produce every output before modifying any live game file.
            foreach (StagedTarget target in staged)
            {
                current = target.File;
                OperationLog.Report(log, 61, false, ("VAR_PATCH_FILE", target.Patch), ("VAR_OUTPUT_FILE", target.File));
                PatchToolResult result = _tool.Apply(request.ToolPath, request.Format, target.Backup, target.Patch, target.Output);
                OperationLog.Report(log, 63, result.ExitCode != 0,
                    ("VAR_EXIT_CODE", result.ExitCode.ToString()), ("VAR_OUTPUT_FILE", target.File));
                if (!string.IsNullOrWhiteSpace(result.StandardOutput))
                    OperationLog.Report(log, 64, false, ("VAR_TOOL_OUTPUT", result.StandardOutput.Trim()));
                if (!string.IsNullOrWhiteSpace(result.StandardError))
                    OperationLog.Report(log, 65, true, ("VAR_TOOL_ERROR", result.StandardError.Trim()));
                if (result.ExitCode != 0 || !File.Exists(target.Output))
                    throw new IOException($"Patch tool failed for {target.File} (exit code {result.ExitCode}).");
                PatchPathSafety.Within(root, target.Output);
            }
            foreach (StagedTarget target in staged)
            {
                current = target.File;
                PatchPathSafety.Within(root, target.File);
                // Register first so rollback also covers a publication failure after partial replacement.
                modified.Add(target);
                File.Move(target.Output, target.File, true);
                OperationLog.Report(log, 66, false, ("VAR_OUTPUT_FILE", target.File));
            }
            OperationLog.Report(log, 68, false,
                ("VAR_MODIFIED_COUNT", modified.Count.ToString()), ("VAR_FILE_COUNT", staged.Count.ToString()));
            completed = true;
            return true;
        }
        catch (Exception exception)
        {
            OperationLog.Failure(log, current, exception);
            foreach (StagedTarget target in modified.AsEnumerable().Reverse())
            {
                try
                {
                    PatchPathSafety.Within(request.InstallationRoot, target.File);
                    if (target.HadLiveFile)
                    {
                        // Restore via a sibling file so a failed copy cannot truncate the live file.
                        File.Copy(target.Rollback, target.Output, true);
                        File.Move(target.Output, target.File, true);
                    }
                    else
                        File.Delete(target.File);
                    target.Restored = true;
                    OperationLog.Report(log, 69, false, ("VAR_OUTPUT_FILE", target.File));
                }
                catch (Exception rollbackException)
                {
                    // Keep the rollback copy when automatic restoration fails.
                    OperationLog.Failure(log, target.Rollback, rollbackException);
                }
            }
            OperationLog.Report(log, 54, true);
            return false;
        }
        finally
        {
            foreach (StagedTarget target in staged)
            {
                PatchPathSafety.DeleteFile(target.Output);
                if (completed || !modified.Contains(target) || target.Restored)
                    PatchPathSafety.DeleteFile(target.Rollback);
            }
        }
    }

    private sealed record StagedTarget(string File, string Patch, string Backup, string Output, string Rollback, bool HadLiveFile)
    {
        public bool Restored { get; set; }
    }
}
