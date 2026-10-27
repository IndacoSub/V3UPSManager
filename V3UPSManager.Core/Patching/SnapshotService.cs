namespace V3UPSManager.Core;

public sealed record SnapshotRequest(
    string InstallationRoot,
    string DestinationDirectory,
    IReadOnlyList<string> Files,
    bool UseCleanBackups = true,
    bool CanAccessExecutable = true,
    string? TitleId = null);

/// <summary>Publishes complete snapshots without discarding a previous valid snapshot on failure.</summary>
public sealed class SnapshotService
{
    private static bool Contains(string parent, string path)
    {
        string relative = Path.GetRelativePath(parent, path);
        return relative == "." || !Path.IsPathRooted(relative) && relative != ".." &&
            !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal);
    }

    public bool Create(SnapshotRequest request, Action<OperationMessage>? log = null)
    {
        string destination = Path.GetFullPath(request.DestinationDirectory);
        string id = Guid.NewGuid().ToString("N");
        string staging = destination + ".tmp." + id;
        string previous = destination + ".old." + id;
        string current = request.InstallationRoot;
        bool published = false;
        try
        {
            string root = PatchPathSafety.ExistingRoot(request.InstallationRoot);
            PatchPathSafety.NoLinks(destination);
            // Renaming either overlapping tree could remove live files unrelated to this snapshot.
            if (Contains(destination, root) || Contains(root, destination))
                throw new IOException("The snapshot destination and installation must not overlap.");
            string[] files = request.Files.ToArray();
            if (files.Length == 0) throw new InvalidOperationException("No files were selected for the snapshot.");
            string actualRoot = staging;
            if (!string.IsNullOrEmpty(request.TitleId))
            {
                if (request.TitleId is "." or ".." || request.TitleId.IndexOfAny(['/', '\\', ':']) >= 0)
                    throw new IOException("Invalid title ID.");
                actualRoot = Path.Combine(staging, request.TitleId, "romfs");
            }
            var plan = new List<(string Source, string Destination)>();
            var unique = new HashSet<string>(PatchPathSafety.Comparer);
            foreach (string file in files)
            {
                current = file;
                string fullFile = PatchPathSafety.Within(root, file);
                if (!request.CanAccessExecutable && string.Equals(Path.GetExtension(fullFile), ".exe", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!unique.Add(fullFile)) throw new IOException($"Duplicate snapshot target: {file}");
                string backup = PatchPathSafety.Within(root, fullFile + "_bak");
                string source = request.UseCleanBackups && File.Exists(backup) ? backup : fullFile;
                if (!File.Exists(source)) throw new FileNotFoundException("Snapshot source not found.", source);
                string relative = Path.GetRelativePath(root, fullFile);
                plan.Add((source, Path.Combine(actualRoot, relative)));
            }
            if (plan.Count == 0) throw new InvalidOperationException("No accessible snapshot files were selected.");
            OperationLog.Report(log, 71, false, ("VAR_BACKUP_DIRECTORY", destination));
            Directory.CreateDirectory(actualRoot);
            foreach (var item in plan)
            {
                current = item.Source;
                Directory.CreateDirectory(Path.GetDirectoryName(item.Destination)!);
                File.Copy(item.Source, item.Destination, false);
                OperationLog.Report(log, 55, false,
                    ("VAR_BACKUP_FILE", Path.Combine(destination, Path.GetRelativePath(staging, item.Destination))));
            }
            if (Directory.Exists(destination)) Directory.Move(destination, previous);
            try
            {
                Directory.Move(staging, destination);
                published = true;
            }
            catch
            {
                if (Directory.Exists(previous) && !Directory.Exists(destination)) Directory.Move(previous, destination);
                throw;
            }
            OperationLog.Report(log, 72, false, ("VAR_BACKUP_DIRECTORY", destination));
            return true;
        }
        catch (Exception exception)
        {
            OperationLog.Failure(log, current, exception);
            OperationLog.Report(log, 81, true, ("VAR_BACKUP_DIRECTORY", destination));
            return false;
        }
        finally
        {
            PatchPathSafety.DeleteDirectory(staging);
            // If publication/restoration failed, retain the old snapshot for manual recovery.
            if (published) PatchPathSafety.DeleteDirectory(previous);
        }
    }
}
