namespace V3UPSManager.Core.Wizard;

/// <summary>Owns temporary extracted data for the complete wizard installation. Never deletes a user's package folder.</summary>
public sealed class PackageSession : IDisposable
{
    private readonly string workspace = Path.Combine(Path.GetTempPath(), "V3UPSWizard_" + Guid.NewGuid().ToString("N"));
    private readonly SevenZipExtractor extractor;
    private bool disposed;
    private bool openedSuccessfully;
    private string? nestedArchive;
    public string Root { get; private set; } = "";
    public string PatchFolder { get; private set; } = "";
    public string Readme { get; private set; } = "";
    public string? GameName { get; private set; }

    public PackageSession(string sevenZipExecutable) => extractor = new SevenZipExtractor(sevenZipExecutable);

    public async Task OpenAsync(string source, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (Root.Length != 0) throw new InvalidOperationException("Create a new session to open another package.");
        if (Directory.Exists(source)) Root = Path.GetFullPath(source);
        else if (File.Exists(source) && source.EndsWith(".DAIX.7z", StringComparison.OrdinalIgnoreCase))
        {
            Root = Path.Combine(workspace, "package");
            await extractor.ExtractAsync(source, Root, progress, cancellationToken).ConfigureAwait(false);
        }
        else throw new InvalidDataException("Select an existing patch folder or a .DAIX.7z archive.");

        cancellationToken.ThrowIfCancellationRequested();
        string[] files = EnumerateFiles(Root, cancellationToken).ToArray();
        string[] gameFiles = files.Where(path => path.EndsWith(".game", StringComparison.OrdinalIgnoreCase)).ToArray();
        if (gameFiles.Length > 1) throw new InvalidDataException("The package contains multiple .game files.");
        if (gameFiles.Length == 1)
        {
            string name = ReadSmallText(gameFiles[0]).Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? "";
            if (!SupportedGameNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                throw new InvalidDataException("The .game file does not name a supported game.");
            GameName = SupportedGameNames.Single(game => game.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        string? readme = files.Where(path => Path.GetFileName(path).Equals("LEGGIMI.txt", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.OrdinalIgnoreCase).FirstOrDefault()
            ?? files.Where(path => Path.GetFileName(path).Equals("README.txt", StringComparison.OrdinalIgnoreCase))
                .Order(StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        Readme = readme is null ? "No README.txt or LEGGIMI.txt was included. Check the mod author's instructions before continuing." : ReadSmallText(readme);
        string[] nested = files.Where(path => Path.GetFileName(path).Equals("PATCH.7z", StringComparison.OrdinalIgnoreCase)).ToArray();
        if (nested.Length > 1) throw new InvalidDataException("The package contains multiple PATCH.7z archives.");
        nestedArchive = nested.SingleOrDefault();
        if (nestedArchive is null) ValidatePatchFiles(files);
        cancellationToken.ThrowIfCancellationRequested();
        openedSuccessfully = true;
    }

    public async Task PreparePatchesAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!openedSuccessfully) throw new InvalidOperationException("Open a valid package successfully before preparing patches.");
        cancellationToken.ThrowIfCancellationRequested();
        if (PatchFolder.Length != 0) return;
        string prepared = Root;
        if (nestedArchive is null) ValidatePatchFiles(EnumerateFiles(Root, cancellationToken));
        else
        {
            // Use a fresh destination on retries; a failed attempt never becomes a successful package.
            string destination = Path.Combine(workspace, "patches_" + Guid.NewGuid().ToString("N"));
            await extractor.ExtractAsync(nestedArchive, destination, progress, cancellationToken).ConfigureAwait(false);
            ValidatePatchFiles(EnumerateFiles(destination, cancellationToken));
            prepared = destination;
        }
        cancellationToken.ThrowIfCancellationRequested();
        PatchFolder = prepared;
    }

    public static IReadOnlyList<string> SupportedGameNames { get; } =
        GameCatalog.All.Select(game => game.GameID.ToString()).ToArray();

    private static void ValidatePatchFiles(IEnumerable<string> files)
    {
        if (!files.Any(path => path.EndsWith(".ups", StringComparison.OrdinalIgnoreCase) ||
                               path.EndsWith(".xdelta", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidDataException("No UPS or XDelta patches were found in the package.");
    }

    private static string ReadSmallText(string path)
    {
        if (new FileInfo(path).Length > 2 * 1024 * 1024)
            throw new InvalidDataException("Package metadata exceeds the 2 MiB limit.");
        return File.ReadAllText(path);
    }

    public static IEnumerable<string> EnumerateFiles(string root, CancellationToken cancellationToken = default)
    {
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string current = pending.Pop();
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Package folders must not contain links or reparse points.");
            foreach (string path in Directory.EnumerateFileSystemEntries(current))
            {
                cancellationToken.ThrowIfCancellationRequested();
                FileAttributes attributes = File.GetAttributes(path);
                if ((attributes & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Package folders must not contain links or reparse points.");
                if ((attributes & FileAttributes.Directory) != 0) pending.Push(path);
                else yield return path;
            }
        }
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        try { if (Directory.Exists(workspace)) Directory.Delete(workspace, recursive: true); }
        catch (IOException) { } // A locked temporary file is safer to retain than deleting user data.
        catch (UnauthorizedAccessException) { }
        GC.SuppressFinalize(this);
    }
}
