namespace V3UPSManager.Core.Tests;

public sealed class GameCatalogTests
{
    [Theory]
    [InlineData("Danganronpa V3 Killing Harmony", Game.DanganronpaV3)]
    [InlineData("AI The Somnium Files", Game.AITheSomniumFiles)]
    [InlineData("東京サイコデミック", Game.TokyoPsychodemic)]
    [InlineData("010063F014176000", Game.DanganronpaV3)]
    [InlineData("My Danganronpa V3 Killing Harmony", Game.None)]
    [InlineData("Danganronpa V3 Killing Harmony backup", Game.None)]
    public void DetectionRequiresCompleteGameFolderSegments(string folder, Game expected)
        => Assert.Equal(expected, GameCatalog.Detect(System.IO.Path.Combine(System.IO.Path.GetTempPath(), folder)));

    [Fact]
    public void DetectionHandlesWindowsPathsAndCase()
        => Assert.Equal(Game.AITheSomniumFiles, GameCatalog.Detect(@"C:\Games\ai- the somnium files\Content\"));

    [Fact]
    public void UnsupportedIdCannotCreateAnotherGame()
        => Assert.Equal(Game.None, GameCatalog.Create((Game)999).GameID);

    [Fact]
    public void InaccessibleExecutablesAreExcludedFromPatchTargets()
    {
        var game = GameCatalog.Create(Game.AITheSomniumFiles);
        Assert.Contains(".exe", game.GetPatchTargetExtensions(isUnity: true, canAccessExe: true));
        Assert.DoesNotContain(".exe", game.GetPatchTargetExtensions(isUnity: true, canAccessExe: false));
        Assert.Contains("", game.GetPatchTargetExtensions(isUnity: true, canAccessExe: false));
    }
}
