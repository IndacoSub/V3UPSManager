namespace V3UPSManager.Core;

public sealed record PatchDiscoveryResult(
    IReadOnlyList<PatchTarget> Targets,
    IReadOnlyList<string> MissingFiles,
    int CandidateCount)
{
    public bool IsComplete => CandidateCount > 0 && Targets.Count == CandidateCount && MissingFiles.Count == 0;
}

/// <summary>Builds a complete patch plan without changing any game files.</summary>
public static class PatchDiscovery
{
    public static PatchDiscoveryResult Discover(GameBase game, string installationRoot,
        string patchFolder, bool isUnity, bool isLegacy, bool canAccessExecutable)
    {
        installationRoot = PatchPathSafety.ExistingRoot(installationRoot);
        patchFolder = PatchPathSafety.ExistingRoot(patchFolder);
        string root = game.GetPatchRoot(patchFolder, isUnity, isLegacy);
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            return new([], [], 0);

        var candidates = EnumeratePatchFiles(root, game.PatchSpecificString).Order(StringComparer.Ordinal).ToArray();
        var targets = new List<PatchTarget>();
        var missing = new List<string>();
        var used = new HashSet<string>(PatchPathSafety.Comparer);
        var extensions = game.GetPatchTargetExtensions(isUnity, canAccessExecutable);
        foreach (string patch in candidates)
        {
            PatchPathSafety.Within(patchFolder, patch);
            string relative = Path.GetRelativePath(patchFolder, patch);
            string stem = relative[..^game.PatchSpecificString.Length];
            string approximate = PatchPathSafety.Within(installationRoot, Path.Combine(installationRoot, stem));
            var matches = new HashSet<string>(PatchPathSafety.Comparer);
            string existingExtension = Path.GetExtension(approximate);
            foreach (string extension in extensions)
            {
                // An extensionless target must really have no extension. Otherwise an
                // executable could bypass the edition's executable-access restriction.
                string candidate;
                if (extension.Length == 0)
                {
                    if (existingExtension.Length != 0) continue;
                    candidate = approximate;
                }
                else
                {
                    candidate = existingExtension.Equals(extension, StringComparison.OrdinalIgnoreCase)
                        ? approximate : approximate + extension;
                }
                string? actual = FindExistingTarget(candidate);
                if (actual is not null)
                {
                    PatchPathSafety.Within(installationRoot, actual);
                    PatchPathSafety.Within(installationRoot, actual + "_bak");
                    matches.Add(actual);
                }
            }
            if (matches.Count == 1 && used.Add(matches.Single()))
                targets.Add(new(matches.Single(), patch));
            else
                missing.Add(approximate); // Missing, ambiguous or duplicate target: never guess.
        }
        return new(targets.AsReadOnly(), missing.AsReadOnly(), candidates.Length);
    }

    private static IEnumerable<string> EnumeratePatchFiles(string root, string suffix)
    {
        // Reject directory links instead of silently following them outside the package.
        foreach (string entry in Directory.EnumerateFileSystemEntries(root))
        {
            if ((File.GetAttributes(entry) & FileAttributes.ReparsePoint) != 0)
                throw new IOException($"Linked package paths are not supported: {entry}");
            if (Directory.Exists(entry))
            {
                foreach (string file in EnumeratePatchFiles(entry, suffix)) yield return file;
            }
            else if (entry.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) yield return entry;
        }
    }

    private static string? FindExistingTarget(string candidate)
    {
        if (File.Exists(candidate) || File.Exists(candidate + "_bak")) return candidate;
        // Preserve the package convention accepting uppercase target extensions on
        // case-sensitive volumes without changing the rest of the filename.
        string extension = Path.GetExtension(candidate);
        if (extension.Length == 0) return null;
        string uppercase = candidate[..^extension.Length] + extension.ToUpperInvariant();
        return File.Exists(uppercase) || File.Exists(uppercase + "_bak") ? uppercase : null;
    }
}
