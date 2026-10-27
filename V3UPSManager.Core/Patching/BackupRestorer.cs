namespace V3UPSManager.Core;

public sealed record RestoreResult(int RestoredCount, int FailedCount)
{
    public bool Succeeded => RestoredCount > 0 && FailedCount == 0;
}

/// <summary>Restores permanent clean backups, retaining recovery data when an operation fails.</summary>
public sealed class BackupRestorer
{
    public RestoreResult Restore(string installationRoot, IReadOnlyList<string> extensions,
        Action<OperationMessage>? log = null)
    {
        int restored = 0;
        int failed = 0;
        string root;
        string[] backups;
        try
        {
            root = PatchPathSafety.ExistingRoot(installationRoot);
            // Materialize inside the try: recursive enumeration can fail after its creation.
            // Skip junctions/symlinks rather than recursing outside the selected tree.
            var options = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = false,
                AttributesToSkip = FileAttributes.ReparsePoint,
                MatchCasing = MatchCasing.CaseInsensitive,
            };
            backups = Directory.EnumerateFiles(root, "*_bak", options)
                .Where(path => extensions.Any(extension => extension == "*" ||
                    path.EndsWith(extension + "_bak", StringComparison.OrdinalIgnoreCase)))
                .Distinct(PatchPathSafety.Comparer).ToArray();
        }
        catch (Exception exception)
        {
            OperationLog.Failure(log, installationRoot, exception);
            return new(0, 1);
        }
        foreach (string backup in backups)
        {
            string output = backup[..^4];
            string temporary = output + ".v3ups_restore." + Guid.NewGuid().ToString("N");
            try
            {
                PatchPathSafety.Within(root, backup);
                PatchPathSafety.Within(root, output);
                File.Copy(backup, temporary, false);
                File.Move(temporary, output, true);
                // Only remove the recovery copy after the complete restored output is published.
                File.Delete(backup);
                restored++;
            }
            catch (Exception exception)
            {
                failed++;
                OperationLog.Failure(log, output, exception);
            }
            finally { PatchPathSafety.DeleteFile(temporary); }
        }
        return new(restored, failed);
    }
}
