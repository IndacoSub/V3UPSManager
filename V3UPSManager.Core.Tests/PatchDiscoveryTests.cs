namespace V3UPSManager.Core.Tests;

public sealed class PatchDiscoveryTests
{
    [Fact]
    public void DiscoveryCanUseCleanBackupWithoutRestoringMissingLiveFile()
    {
        using var game = new TestDirectory();
        using var patches = new TestDirectory();
        game.Write("data/win/dialogue.spc_bak", "original");
        string patch = patches.Write("data/win/dialogue_patch.ups");

        var result = PatchDiscovery.Discover(new DRV3(), game.Path, patches.Path, false, true, true);

        Assert.True(result.IsComplete);
        var target = Assert.Single(result.Targets);
        Assert.Equal(System.IO.Path.Combine(game.Path, "data", "win", "dialogue.spc"), target.FilePath);
        Assert.Equal(patch, target.PatchPath);
        Assert.False(File.Exists(target.FilePath));
        Assert.Equal("original", File.ReadAllText(target.FilePath + "_bak"));
    }

    [Fact]
    public void DiscoveryRejectsAmbiguousExtensionMatches()
    {
        using var game = new TestDirectory();
        using var patches = new TestDirectory();
        game.Write("data/win/dialogue.spc");
        game.Write("data/win/dialogue.awb");
        patches.Write("data/win/dialogue_patch.ups");

        var result = PatchDiscovery.Discover(new DRV3(), game.Path, patches.Path, false, true, true);

        Assert.False(result.IsComplete);
        Assert.Empty(result.Targets);
        Assert.Single(result.MissingFiles);
    }

    [Fact]
    public void ExecutableCannotMatchExtensionlessRuleWhenAccessIsDisabled()
    {
        using var game = new TestDirectory();
        using var patches = new TestDirectory();
        game.Write("AI_TheSomniumFiles.exe");
        patches.Write("AI_TheSomniumFiles_Data/StreamingAssets/marker.txt");
        patches.Write("AI_TheSomniumFiles.exe_patch.xdelta");

        var result = PatchDiscovery.Discover(new AITSF(), game.Path, patches.Path, true, false, false);

        Assert.False(result.IsComplete);
        Assert.Empty(result.Targets);
    }

    [Fact]
    public void SameTargetCannotReceiveTwoPatchesInOnePlan()
    {
        using var game = new TestDirectory();
        using var patches = new TestDirectory();
        game.Write("data/win/dialogue.spc");
        patches.Write("data/win/dialogue_patch.ups");
        patches.Write("data/win/dialogue.spc_patch.ups");

        var result = PatchDiscovery.Discover(new DRV3(), game.Path, patches.Path, false, true, true);

        Assert.False(result.IsComplete);
        Assert.Single(result.Targets);
        Assert.Equal(2, result.CandidateCount);
    }
}
