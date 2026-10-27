using System.Security.Cryptography;
using V3UPSManager.Core.Wizard;

namespace V3UPSManager.Core.Tests;

public sealed class GameInstallationDiscoveryTests
{
    [Fact]
    public void FindsOnlySupportedGamesInKnownSteamLocations()
    {
        using var drive = new TestDirectory();
        string dangan = CreateGame(drive, "Program Files (x86)/Steam/steamapps/common/Danganronpa V3 Killing Harmony", "Dangan3Win.exe");
        string somnium = CreateGame(drive, "SteamLibrary/steamapps/common/AI The Somnium Files", "AI_TheSomniumFiles.exe");
        CreateGame(drive, "SteamLibrary/steamapps/common/Unrelated Game", "Unrelated.exe");

        var results = GameInstallationDiscovery.Discover(drive.Path);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, result => result.Game == Game.DanganronpaV3 && result.InstallationFolder == dangan && result.Source == "Steam");
        Assert.Contains(results, result => result.Game == Game.AITheSomniumFiles && result.InstallationFolder == somnium && result.Source == "Steam");
    }

    [Fact]
    public void EmptyNamedGameFolderIsNotAnInstalledGame()
    {
        using var drive = new TestDirectory();
        Directory.CreateDirectory(System.IO.Path.Combine(drive.Path, "SteamLibrary/steamapps/common/Danganronpa V3 Killing Harmony"));

        Assert.Empty(GameInstallationDiscovery.Discover(drive.Path));
    }

    [Fact]
    public void DirectSelectedLibraryIsSupportedAndFilterDoesNotChooseAnotherGame()
    {
        using var library = new TestDirectory();
        CreateGame(library, "steamapps/common/Danganronpa V3 Killing Harmony", "Dangan3Win.exe");
        string expected = CreateGame(library, "steamapps/common/AI The Somnium Files", "AI_TheSomniumFiles.exe");

        var result = Assert.Single(GameInstallationDiscovery.Discover(library.Path, Game.AITheSomniumFiles));

        Assert.Equal(Game.AITheSomniumFiles, result.Game);
        Assert.Equal(expected, result.InstallationFolder);
    }

    [Fact]
    public void ManifestCanLocateACustomInstallationFolder()
    {
        using var drive = new TestDirectory();
        string expected = CreateGame(drive, "Steam/steamapps/common/Translated game folder", "Dangan3Win.exe");
        drive.Write("Steam/steamapps/appmanifest_567640.acf", Manifest("Danganronpa V3: Killing Harmony", "Translated game folder"));

        var result = Assert.Single(GameInstallationDiscovery.Discover(drive.Path));

        Assert.Equal(Game.DanganronpaV3, result.Game);
        Assert.Equal(expected, result.InstallationFolder);
    }

    [Fact]
    public void CommentedMetadataKeepsQuotedBracesAndEscapedLibraryPathsAsText()
    {
        using var drive = new TestDirectory();
        string library = System.IO.Path.Combine(drive.Path, "Custom {Library}");
        string expected = CreateGame(drive, "Custom {Library}/steamapps/common/Renamed {Game}", "Dangan3Win.exe");
        drive.Write("Steam/steamapps/libraryfolders.vdf", $$"""
            // Steam may include comments between its metadata fields.
            "libraryfolders"
            {
                "0" { "path" "{{EscapeVdf(library)}}" }
            }
            "unrelated" "}"
            "support" "https://example.invalid/instructions"
            """
        );
        drive.Write("Custom {Library}/steamapps/appmanifest_567640.acf", """
            "AppState"
            {
                "name" "Danganronpa V3: Killing Harmony" // An ordinary inline comment.
                "installdir" "Renamed {Game}"
                "unrelated" "}"
            }
            """
        );

        var result = Assert.Single(GameInstallationDiscovery.Discover(drive.Path));

        Assert.Equal(Game.DanganronpaV3, result.Game);
        Assert.Equal(expected, result.InstallationFolder);
    }

    [Fact]
    public void LibraryMetadataFindsAdditionalLibrariesInsideSelectedRootWithoutDuplicates()
    {
        using var drive = new TestDirectory();
        string customLibrary = System.IO.Path.Combine(drive.Path, "Custom library");
        string expected = CreateGame(drive, "Custom library/steamapps/common/AI The Somnium Files", "AI_TheSomniumFiles.exe");
        string escaped = EscapeVdf(customLibrary);
        drive.Write("Steam/steamapps/libraryfolders.vdf", $"\"libraryfolders\" {{ \"0\" {{ \"path\" \"{escaped}\" }} \"1\" {{ \"path\" \"{escaped}\" }} }}");

        var result = Assert.Single(GameInstallationDiscovery.Discover(drive.Path));

        Assert.Equal(Game.AITheSomniumFiles, result.Game);
        Assert.Equal(expected, result.InstallationFolder);
    }

    [Fact]
    public void LibraryMetadataCannotExpandSearchOutsideSelectedRoot()
    {
        using var drive = new TestDirectory();
        using var outside = new TestDirectory();
        CreateGame(outside, "steamapps/common/Danganronpa V3 Killing Harmony", "Dangan3Win.exe");
        drive.Write("Steam/steamapps/libraryfolders.vdf", $"\"libraryfolders\" {{ \"0\" {{ \"path\" \"{EscapeVdf(outside.Path)}\" }} }}");

        Assert.Empty(GameInstallationDiscovery.Discover(drive.Path));
    }

    [Fact]
    public void LegacyLibraryMetadataStillFindsAnInScopeLibrary()
    {
        using var drive = new TestDirectory();
        string library = System.IO.Path.Combine(drive.Path, "Legacy Library");
        string expected = CreateGame(drive, "Legacy Library/steamapps/common/AI The Somnium Files", "AI_TheSomniumFiles.exe");
        drive.Write("Steam/steamapps/libraryfolders.vdf", $"\"libraryfolders\" {{ \"0\" \"{EscapeVdf(library)}\" }}");

        Assert.Equal(expected, Assert.Single(GameInstallationDiscovery.Discover(drive.Path)).InstallationFolder);
    }

    [Fact]
    public void LibraryWithSelectedRootNamePrefixIsStillOutsideSearchScope()
    {
        using var drive = new TestDirectory();
        string selected = System.IO.Path.Combine(drive.Path, "selected");
        string sibling = System.IO.Path.Combine(drive.Path, "selected-other");
        CreateGame(drive, "selected-other/steamapps/common/Danganronpa V3 Killing Harmony", "Dangan3Win.exe");
        drive.Write("selected/Steam/steamapps/libraryfolders.vdf", $"\"libraryfolders\" {{ \"0\" {{ \"path\" \"{EscapeVdf(sibling)}\" }} }}");

        Assert.Empty(GameInstallationDiscovery.Discover(selected));
    }

    [Fact]
    public void OversizedMetadataCannotExpandTheSearch()
    {
        using var drive = new TestDirectory();
        string library = System.IO.Path.Combine(drive.Path, "Custom library");
        CreateGame(drive, "Custom library/steamapps/common/Danganronpa V3 Killing Harmony", "Dangan3Win.exe");
        drive.Write("Steam/steamapps/libraryfolders.vdf", new string(' ', 2 * 1024 * 1024) +
            $"\"libraryfolders\" {{ \"0\" {{ \"path\" \"{EscapeVdf(library)}\" }} }}");

        Assert.Empty(GameInstallationDiscovery.Discover(drive.Path));
    }

    [Theory]
    [InlineData("../Outside game")]
    [InlineData("nested/../../Outside game")]
    [InlineData(@"..\Outside game")]
    public void ManifestInstallationFolderCannotEscapeSteamCommon(string unsafeFolder)
    {
        using var drive = new TestDirectory();
        CreateGame(drive, "Steam/steamapps/Outside game", "Dangan3Win.exe");
        drive.Write("Steam/steamapps/appmanifest_567640.acf", Manifest("Danganronpa V3: Killing Harmony", unsafeFolder));

        Assert.Empty(GameInstallationDiscovery.Discover(drive.Path));
    }

    [Fact]
    public void MalformedMetadataDoesNotPreventKnownFolderSuggestions()
    {
        using var drive = new TestDirectory();
        string expected = CreateGame(drive, "SteamLibrary/steamapps/common/Danganronpa V3 Killing Harmony", "Dangan3Win.exe");
        drive.Write("SteamLibrary/steamapps/libraryfolders.vdf", "\"libraryfolders\" { \"0\" { \"path\" \"unterminated");
        drive.Write("SteamLibrary/steamapps/appmanifest_567640.acf", "\"AppState\" { \"name\"");

        var result = Assert.Single(GameInstallationDiscovery.Discover(drive.Path));

        Assert.Equal(expected, result.InstallationFolder);
    }

    [Fact]
    public void FindsSupportedXboxContentWithoutSearchingOtherFolders()
    {
        using var drive = new TestDirectory();
        string expected = CreateGame(drive, "XboxGames/Danganronpa V3- Killing Harmony Anniversary Edition/Content", "Dangan3Desktop.exe");
        CreateGame(drive, "XboxGames/Unrelated Game/Content", "Unrelated.exe");
        CreateGame(drive, "Documents/Nested/XboxGames/AI- THE SOMNIUM FILES/Content", "AI_TheSomniumFiles.exe");

        var result = Assert.Single(GameInstallationDiscovery.Discover(drive.Path));

        Assert.Equal(Game.DanganronpaV3, result.Game);
        Assert.Equal(expected, result.InstallationFolder);
        Assert.Equal("Xbox", result.Source);
    }

    [Fact]
    public void SearchDoesNotWriteMetadataOrModifyGameFiles()
    {
        using var drive = new TestDirectory();
        CreateGame(drive, "Steam/steamapps/common/Danganronpa V3 Killing Harmony", "Dangan3Win.exe");
        drive.Write("Steam/steamapps/appmanifest_567640.acf", Manifest("Danganronpa V3: Killing Harmony", "Danganronpa V3 Killing Harmony"));
        drive.Write("Documents/private.txt", "This file must not be touched.");
        var before = Snapshot(drive.Path);

        Assert.NotEmpty(GameInstallationDiscovery.Discover(drive.Path));

        Assert.Equal(before, Snapshot(drive.Path));
    }

    [Fact]
    public void CanceledSearchDoesNotReturnPartialSuggestions()
    {
        using var drive = new TestDirectory();
        CreateGame(drive, "SteamLibrary/steamapps/common/Danganronpa V3 Killing Harmony", "Dangan3Win.exe");

        Assert.ThrowsAny<OperationCanceledException>(() => GameInstallationDiscovery.Discover(drive.Path,
            cancellationToken: new CancellationToken(canceled: true)));
    }

    [LinuxFact]
    public void LinkedLibraryCannotReadOrSuggestAnOutsideGame()
    {
        using var drive = new TestDirectory();
        using var outside = new TestDirectory();
        CreateGame(outside, "steamapps/common/Danganronpa V3 Killing Harmony", "Dangan3Win.exe");
        Directory.CreateSymbolicLink(System.IO.Path.Combine(drive.Path, "SteamLibrary"), outside.Path);

        Assert.Empty(GameInstallationDiscovery.Discover(drive.Path));
    }

    [LinuxFact]
    public void LinkedExecutableDoesNotTurnAnExternalFileIntoASuggestion()
    {
        using var drive = new TestDirectory();
        using var outside = new TestDirectory();
        string executable = outside.Write("Dangan3Win.exe");
        string folder = System.IO.Path.Combine(drive.Path, "SteamLibrary/steamapps/common/Danganronpa V3 Killing Harmony");
        Directory.CreateDirectory(folder);
        File.CreateSymbolicLink(System.IO.Path.Combine(folder, "Dangan3Win.exe"), executable);

        Assert.Empty(GameInstallationDiscovery.Discover(drive.Path));
    }

    [LinuxFact]
    public void LinkedMetadataIsNotReadEvenWhenItAdvertisesAnInScopeLibrary()
    {
        using var drive = new TestDirectory();
        using var outside = new TestDirectory();
        string library = System.IO.Path.Combine(drive.Path, "Custom library");
        CreateGame(drive, "Custom library/steamapps/common/Danganronpa V3 Killing Harmony", "Dangan3Win.exe");
        string metadata = outside.Write("private-metadata.vdf", $"\"libraryfolders\" {{ \"0\" {{ \"path\" \"{EscapeVdf(library)}\" }} }}");
        string steamapps = System.IO.Path.Combine(drive.Path, "Steam/steamapps");
        Directory.CreateDirectory(steamapps);
        File.CreateSymbolicLink(System.IO.Path.Combine(steamapps, "libraryfolders.vdf"), metadata);

        Assert.Empty(GameInstallationDiscovery.Discover(drive.Path));
    }

    private static string CreateGame(TestDirectory directory, string relativeFolder, string executable)
    {
        directory.Write(System.IO.Path.Combine(relativeFolder, executable), "representative game executable");
        return System.IO.Path.Combine(directory.Path, relativeFolder.Replace('/', System.IO.Path.DirectorySeparatorChar));
    }

    private static string Manifest(string name, string folder) =>
        $"\"AppState\" {{ \"appid\" \"567640\" \"name\" \"{EscapeVdf(name)}\" \"installdir\" \"{EscapeVdf(folder)}\" }}";

    private static string EscapeVdf(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static string[] Snapshot(string root) => Directory.GetFiles(root, "*", SearchOption.AllDirectories)
        .Order(StringComparer.Ordinal)
        .Select(path => System.IO.Path.GetRelativePath(root, path) + ":" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))))
        .ToArray();
}
