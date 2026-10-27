namespace V3UPSManager.Core.Tests;

public sealed class ValidationTests
{
    [Fact]
    public void DecliningLegacyHashWarningDoesNotProbeOtherEditions()
    {
        using var directory = new TestDirectory();
        var game = new DRV3();
        CreateLegacy(directory, game);
        var events = new List<ValidationMessage>();
        var result = new InstallationValidator().Validate(game, directory.Path, message =>
        {
            events.Add(message);
            return !message.RequiresConfirmation;
        });
        Assert.Null(result);
        Assert.Single(events);
        Assert.Equal(8, events[0].MessageId);
        Assert.True(events[0].RequiresConfirmation);
    }

    [Fact]
    public void LegacyLanguageFileAcceptsWhitespaceAndRequiredDirectoriesAreChecked()
    {
        using var directory = new TestDirectory();
        var game = new DRV3();
        CreateLegacy(directory, game);
        var validator = new InstallationValidator();
        var result = validator.Validate(game, directory.Path, _ => true);
        Assert.NotNull(result);
        Assert.True(result.IsLegacy);
        Assert.Equal(System.IO.Path.Combine(directory.Path, "data", "win"), result.DataFolder);
        Directory.Delete(System.IO.Path.Combine(result.DataFolder, "boot"));
        var events = new List<ValidationMessage>();
        Assert.Null(validator.Validate(game, directory.Path, message => { events.Add(message); return true; }));
        Assert.Contains(events, message => message.MessageId == 11 && message.IsError);
    }

    [Fact]
    public void SomniumUsesValidatedAssetBundlePathRegardlessOfDirectoryOrder()
    {
        using var directory = new TestDirectory();
        var game = new AITSF();
        directory.Write(game.UNITY_EXE_NAME);
        Directory.CreateDirectory(System.IO.Path.Combine(directory.Path, game.UnityDataFolder, "StreamingAssets", "Other"));
        string data = System.IO.Path.Combine(game.UnityDataFolder, "StreamingAssets", "AssetBundles", "StandaloneWindows64");
        foreach (string name in new[] { "fonts", "luabytecode", "etc" })
            directory.Write(System.IO.Path.Combine(data, name));
        var result = new InstallationValidator().Validate(game, directory.Path, _ => true);
        Assert.NotNull(result);
        Assert.Equal(System.IO.Path.Combine(directory.Path, data), result.DataFolder);
        Assert.True(result.CanAccessExecutable);
    }

    [Fact]
    public void SomniumXboxDisablesExecutablePatchingWithoutRequestingHashConfirmation()
    {
        using var directory = new TestDirectory();
        var game = new AITSF();
        directory.Write(game.UNITY_EXE_NAME);
        Directory.CreateDirectory(System.IO.Path.Combine(directory.Path, game.UnityDataFolder, "StreamingAssets", "Movie"));
        foreach (string name in new[] { "fonts", "luabytecode", "etc" })
            directory.Write(System.IO.Path.Combine(game.UnityDataFolder, "StreamingAssets", "AssetBundles", "StandaloneWindows64", name));
        var result = new InstallationValidator().Validate(game, directory.Path);
        Assert.NotNull(result);
        Assert.False(result.CanAccessExecutable);
    }

    [Fact]
    public void SwitchFindsPlatformByContentAndRejectsAmbiguousPlatforms()
    {
        using var directory = new TestDirectory();
        var game = new DRV3();
        string title = game.SupportedUnityTitleIds[0];
        string root = System.IO.Path.Combine(directory.Path, title);
        string streaming = System.IO.Path.Combine(title, game.UnityDataFolder, "StreamingAssets");
        Directory.CreateDirectory(System.IO.Path.Combine(directory.Path, streaming, "Unrelated"));
        foreach (string required in game.RequiredDataDirectories.Append("all"))
            Directory.CreateDirectory(System.IO.Path.Combine(directory.Path, streaming, "Switch", required));
        directory.Write(System.IO.Path.Combine(streaming, "Switch", "all", "content.assets"));
        var validator = new InstallationValidator();
        var result = validator.Validate(game, root);
        Assert.NotNull(result);
        Assert.True(result.IsUnity);
        Assert.Equal(title, result.TitleId);
        Assert.EndsWith("Switch", result.DataFolder);
        Directory.CreateDirectory(System.IO.Path.Combine(directory.Path, streaming, "OtherPlatform", "all"));
        Assert.Null(validator.Validate(game, root));
    }

    [Fact]
    public void UnknownGameIsNeverAcceptedByExistingDirectory()
    {
        using var directory = new TestDirectory();
        Assert.Null(new InstallationValidator().Validate(new NoneGame(), directory.Path));
    }

    private static void CreateLegacy(TestDirectory directory, DRV3 game)
    {
        directory.Write(game.LEGACY_EXE_NAME);
        directory.Write(game.LegacyLanguageFile, " US\r\n");
        foreach (string required in game.RequiredDataDirectories)
            Directory.CreateDirectory(System.IO.Path.Combine(directory.Path, "data", "win", required));
    }
}
