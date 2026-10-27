using System.Globalization;
using System.Text;

namespace V3UPSManager.Core.Wizard;

public sealed record DiscoveredGame(Game Game, string InstallationFolder, string Source);

/// <summary>
/// Read-only suggestions from known Steam/Xbox locations inside one explicitly selected root.
/// Calling this service is an opt-in UI action; it never enumerates drives or scans recursively.
/// Suggestions still require normal installation validation before patching.
/// </summary>
public static class GameInstallationDiscovery
{
    private const int MaximumMetadataBytes = 2 * 1024 * 1024;
    private const int MaximumManifestFiles = 1024;
    private const int MaximumLibraries = 32;

    public static IReadOnlyList<DiscoveredGame> Discover(string selectedRoot, Game? filter = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (OperatingSystem.IsWindows())
        {
            // Inspect only the chosen drive's type. UNC and mapped network drives are
            // excluded before opening directories, matching this feature's local-only scope.
            string fullRoot = Path.GetFullPath(selectedRoot);
            string volume = Path.GetPathRoot(fullRoot)!;
            if (volume.StartsWith("\\\\", StringComparison.Ordinal) ||
                new DriveInfo(volume).DriveType is not (DriveType.Fixed or DriveType.Removable or DriveType.Ram))
                throw new ArgumentException("Choose a local fixed or removable drive. Network locations are not searched.", nameof(selectedRoot));
        }
        string root = PatchPathSafety.ExistingRoot(selectedRoot);
        GameBase[] games = GameCatalog.All.Where(game => filter is null || game.GameID == filter).ToArray();
        if (games.Length == 0) return [];
        var results = new Dictionary<string, DiscoveredGame>(PatchPathSafety.Comparer);
        var libraries = new Queue<string>();
        var scheduledLibraries = new HashSet<string>(PatchPathSafety.Comparer);

        foreach (string relative in new[] { "", "SteamLibrary", "Steam", "Program Files (x86)/Steam", "Program Files/Steam" })
            ScheduleLibrary(Path.Combine(root, Native(relative)));
        InspectXbox(root);
        InspectXbox(Path.Combine(root, "XboxGames"));

        while (libraries.TryDequeue(out string? library))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string steamapps = Path.Combine(library, "steamapps");
            if (!IsDirectory(steamapps)) continue;
            string common = Path.Combine(steamapps, "common");
            foreach (GameBase game in games)
            foreach (string identifier in game.FolderIdentifiers.Where(identifier => !identifier.Contains('/')))
                Suggest(game, Path.Combine(common, Native(identifier)), "Steam");

            foreach (string manifest in Manifests(steamapps))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var state = ReadMetadata(manifest)?.Object("AppState");
                string? installDirectory = state?.Text("installdir");
                if (!IsDirectoryName(installDirectory)) continue;
                GameBase? game = games.FirstOrDefault(game => MatchesName(game, state?.Text("name")) ||
                    MatchesName(game, installDirectory));
                if (game is not null)
                    Suggest(game, Path.Combine(common, installDirectory!), "Steam");
            }

            foreach (string metadata in new[] { Path.Combine(steamapps, "libraryfolders.vdf"), Path.Combine(library, "config", "libraryfolders.vdf") })
            {
                var folders = ReadMetadata(metadata)?.Object("libraryfolders");
                if (folders is null) continue;
                foreach ((string key, object value) in folders.Values)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!int.TryParse(key, NumberStyles.None, CultureInfo.InvariantCulture, out _)) continue;
                    string? path = value is string legacy ? legacy : (value as KeyValues)?.Text("path");
                    // Reject other drives before any filesystem access. A VDF entry is not permission
                    // to wake or inspect another disk, even if Steam has registered that library.
                    if (path is not null && Path.IsPathFullyQualified(path)) ScheduleLibrary(path);
                }
            }
        }

        return results.Values.OrderBy(result => result.Game)
            .ThenBy(result => result.InstallationFolder, StringComparer.OrdinalIgnoreCase).ToArray();

        void ScheduleLibrary(string path)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (scheduledLibraries.Count >= MaximumLibraries || !TryConfine(path, out string full)) return;
            if (scheduledLibraries.Add(full) && IsDirectory(full)) libraries.Enqueue(full);
        }

        void InspectXbox(string xboxRoot)
        {
            if (!IsDirectory(xboxRoot)) return;
            foreach (GameBase game in games)
            foreach (string identifier in game.FolderIdentifiers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (identifier.Contains('/')) Suggest(game, Path.Combine(xboxRoot, Native(identifier)), "Xbox");
                else
                {
                    // Xbox packages usually put the selected game's files under Content.
                    Suggest(game, Path.Combine(xboxRoot, Native(identifier), "Content"), "Xbox");
                }
            }
        }

        void Suggest(GameBase game, string directory, string source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!TryConfine(directory, out string full) || !IsDirectory(full)) return;
            string[] executables = game.GameID == Game.DanganronpaV3
                ? [game.LEGACY_EXE_NAME, game.ANNIVERSARY_EXE_NAME] : [game.UNITY_EXE_NAME];
            if (executables.Any(name => IsFile(Path.Combine(full, name))))
                results.TryAdd(full, new(game.GameID, full, source));
        }

        bool TryConfine(string path, out string full)
        {
            full = "";
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                string absolute = Path.GetFullPath(path);
                full = PatchPathSafety.Comparer.Equals(root, absolute)
                    ? root : PatchPathSafety.Within(root, absolute);
                return true;
            }
            catch (Exception exception) when (IsFilesystemError(exception)) { return false; }
        }

        bool IsDirectory(string path) => TryConfine(path, out string full) && Directory.Exists(full);
        bool IsFile(string path) => TryConfine(path, out string full) && File.Exists(full);

        IEnumerable<string> Manifests(string steamapps)
        {
            string[] files;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var found = new List<string>();
                using var enumerator = Directory.EnumerateFiles(steamapps, "appmanifest_*.acf", SearchOption.TopDirectoryOnly).GetEnumerator();
                while (found.Count < MaximumManifestFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!enumerator.MoveNext()) break;
                    found.Add(enumerator.Current);
                }
                files = found.ToArray();
            }
            catch (Exception exception) when (IsFilesystemError(exception)) { yield break; }
            foreach (string file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return file;
            }
        }

        KeyValues? ReadMetadata(string path)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!TryConfine(path, out string full)) return null;
            try
            {
                using var stream = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                if (stream.Length > MaximumMetadataBytes) return null;
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                // Limit actual reads as well as initial size, since Steam can update this file.
                // A small reusable-size buffer avoids allocating megabytes for each tiny manifest.
                char[] characters = new char[4096];
                var text = new StringBuilder();
                int count;
                while ((count = reader.Read(characters, 0, characters.Length)) != 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (text.Length + count > MaximumMetadataBytes || stream.Position > MaximumMetadataBytes) return null;
                    text.Append(characters, 0, count);
                }
                return KeyValues.Parse(text.ToString());
            }
            catch (Exception exception) when (IsFilesystemError(exception) || exception is InvalidDataException) { return null; }
        }
    }

    private static string Native(string path) => path.Replace('/', Path.DirectorySeparatorChar);

    private static bool IsDirectoryName(string? name) => !string.IsNullOrWhiteSpace(name) &&
        name is not "." and not ".." && name.IndexOfAny(['/', '\\', ':', '\0']) < 0 &&
        !Path.IsPathRooted(name);

    private static bool MatchesName(GameBase game, string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        string normalized = Normalize(name);
        return game.FolderIdentifiers.Any(identifier => Normalize(identifier.Split('/')[0]) == normalized) ||
            Normalize(Path.GetFileNameWithoutExtension(game.UNITY_EXE_NAME)) == normalized;
    }

    private static string Normalize(string text) => new(text.Where(char.IsLetterOrDigit)
        .Select(char.ToUpperInvariant).ToArray());

    private static bool IsFilesystemError(Exception exception) => exception is IOException or
        UnauthorizedAccessException or ArgumentException or NotSupportedException or System.Security.SecurityException;

    /// <summary>Small bounded parser for Steam's quoted KeyValues metadata, including old library lists.</summary>
    private sealed class KeyValues
    {
        public Dictionary<string, object> Values { get; } = new(StringComparer.OrdinalIgnoreCase);
        public string? Text(string key) => Values.GetValueOrDefault(key) as string;
        public KeyValues? Object(string key) => Values.GetValueOrDefault(key) as KeyValues;

        public static KeyValues Parse(string text)
        {
            int position = 0;
            KeyValues result = ReadObject(0, nested: false);
            return result;

            KeyValues ReadObject(int depth, bool nested)
            {
                if (depth > 16) throw new InvalidDataException("Steam metadata is too deeply nested.");
                var result = new KeyValues();
                while (Token() is { } key)
                {
                    if (key.Structural && key.Text == "}")
                    {
                        if (nested) return result;
                        throw new InvalidDataException("Unexpected metadata closing brace.");
                    }
                    if (key.Structural) throw new InvalidDataException("Missing metadata key.");
                    KeyValueToken value = Token() ?? throw new InvalidDataException("Missing metadata value.");
                    object parsed = value.Structural && value.Text == "{" ? ReadObject(depth + 1, true) : value.Text;
                    if (value.Structural && value.Text == "}" || !result.Values.TryAdd(key.Text, parsed))
                        throw new InvalidDataException("Invalid or duplicate metadata key.");
                }
                if (nested) throw new InvalidDataException("Unclosed metadata object.");
                return result;
            }

            KeyValueToken? Token()
            {
                while (position < text.Length)
                {
                    if (char.IsWhiteSpace(text[position])) { position++; continue; }
                    if (text[position] == '/' && position + 1 < text.Length && text[position + 1] == '/')
                    {
                        while (position < text.Length && text[position] != '\n') position++;
                        continue;
                    }
                    break;
                }
                if (position == text.Length) return null;
                char first = text[position++];
                if (first is '{' or '}') return new(first.ToString(), true);
                var result = new StringBuilder();
                bool quoted = first == '"';
                if (!quoted) result.Append(first);
                while (position < text.Length)
                {
                    char current = text[position];
                    if (quoted && current == '"') { position++; return new(result.ToString(), false); }
                    if (!quoted && (char.IsWhiteSpace(current) || current is '{' or '}')) return new(result.ToString(), false);
                    position++;
                    if (quoted && current == '\\' && position < text.Length && text[position] is '\\' or '"')
                        current = text[position++];
                    result.Append(current);
                    if (result.Length > 4096) throw new InvalidDataException("Steam metadata token is too long.");
                }
                if (quoted) throw new InvalidDataException("Unclosed metadata string.");
                return new(result.ToString(), false);
            }
        }

        private readonly record struct KeyValueToken(string Text, bool Structural);
    }
}
