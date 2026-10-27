namespace V3UPSManager.Core.Wizard;

/// <summary>Extracts only ordinary relative archive entries into a fresh directory.</summary>
public sealed class SevenZipExtractor(string executable)
{
    public async Task ExtractAsync(string archive, string destination, IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var listing = new List<string>();
        await ExternalTool.RunAsync(executable, ["l", "-slt", "--", Path.GetFullPath(archive)],
            new InlineProgress(line => { lock (listing) listing.Add(line); }), cancellationToken).ConfigureAwait(false);
        ValidateListing(listing);
        if (Directory.Exists(destination) && Directory.EnumerateFileSystemEntries(destination).Any())
            throw new IOException("Extraction requires an empty destination directory.");
        Directory.CreateDirectory(destination);
        await ExternalTool.RunAsync(executable,
            ["x", "-y", "-o" + Path.GetFullPath(destination), "--", Path.GetFullPath(archive)],
            progress, cancellationToken).ConfigureAwait(false);
        // Defense in depth: never hand links/reparse points to the installer.
        PackageSession.EnumerateFiles(destination, cancellationToken).ToArray();
    }

    public static void ValidateListing(IEnumerable<string> lines)
    {
        bool entries = false;
        bool foundEntry = false;
        foreach (string line in lines)
        {
            if (line == "----------") { entries = true; continue; }
            if (!entries) continue; // The first Path field describes the archive itself.
            if (line.StartsWith("Path = ", StringComparison.Ordinal))
            {
                ValidateEntryPath(line[7..]);
                foundEntry = true;
            }
            if (line.StartsWith("Symbolic Link =", StringComparison.Ordinal) ||
                line.StartsWith("Hard Link =", StringComparison.Ordinal) ||
                (line.StartsWith("Attributes = ", StringComparison.Ordinal) &&
                 (line[13..].Contains('L') || line[13..].Split(' ').Any(value => value.StartsWith('l')))))
                throw new InvalidDataException("Package archives must not contain symbolic or hard links.");
        }
        if (!foundEntry) throw new InvalidDataException("The archive contains no files or could not be inspected.");
    }

    public static void ValidateEntryPath(string entry)
    {
        string normalized = entry.Replace('\\', '/');
        if (string.IsNullOrWhiteSpace(normalized) || normalized.StartsWith('/') || normalized.Contains(':'))
            throw new InvalidDataException("The archive contains an absolute or invalid entry path.");
        foreach (string segment in normalized.Split('/'))
        {
            if (segment is ".." or "." || segment.EndsWith(' ') || segment.EndsWith('.') ||
                segment.IndexOfAny(['<', '>', '"', '|', '?', '*', '\0']) >= 0)
                throw new InvalidDataException("The archive contains an unsafe entry path.");
            string device = segment.Split('.')[0];
            if (device.Equals("CON", StringComparison.OrdinalIgnoreCase) ||
                device.Equals("PRN", StringComparison.OrdinalIgnoreCase) ||
                device.Equals("AUX", StringComparison.OrdinalIgnoreCase) ||
                device.Equals("NUL", StringComparison.OrdinalIgnoreCase) ||
                (device.Length == 4 && (device.StartsWith("COM", StringComparison.OrdinalIgnoreCase) ||
                 device.StartsWith("LPT", StringComparison.OrdinalIgnoreCase)) && device[3] is >= '1' and <= '9'))
                throw new InvalidDataException("The archive contains a reserved Windows device name.");
        }
    }

    private sealed class InlineProgress(Action<string> report) : IProgress<string>
    {
        public void Report(string value) => report(value);
    }
}
