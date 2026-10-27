namespace V3UPSManager.Core.Tests;

public sealed class BackupTests
{
    [Fact]
    public void CleanSnapshotUsesOriginalBackupAndKeepsLiveFilename()
    {
        using var game = new TestDirectory();
        using var snapshots = new TestDirectory();
        string target = game.Write("data/file.spc", "existing mod");
        game.Write("data/file.spc_bak", "original");
        string destination = System.IO.Path.Combine(snapshots.Path, "Backup");

        Assert.True(new SnapshotService().Create(new(game.Path, destination, [target])));

        Assert.Equal("original", File.ReadAllText(System.IO.Path.Combine(destination, "data/file.spc")));
        Assert.False(File.Exists(System.IO.Path.Combine(destination, "data/file.spc_bak")));
        Assert.Equal("existing mod", File.ReadAllText(target));
    }

    [Fact]
    public void ModdedSnapshotCopiesLiveContent()
    {
        using var game = new TestDirectory();
        using var snapshots = new TestDirectory();
        string target = game.Write("file.spc", "current mod");
        game.Write("file.spc_bak", "original");
        string destination = System.IO.Path.Combine(snapshots.Path, "ModdedFiles");

        Assert.True(new SnapshotService().Create(new(game.Path, destination, [target], UseCleanBackups: false)));

        Assert.Equal("current mod", File.ReadAllText(System.IO.Path.Combine(destination, "file.spc")));
    }

    [Fact]
    public void FailedSnapshotKeepsPreviousCompleteSnapshot()
    {
        using var game = new TestDirectory();
        using var snapshots = new TestDirectory();
        string target = game.Write("file.spc", "current");
        string previous = snapshots.Write("Backup/previous.spc", "previous snapshot");
        string destination = System.IO.Path.Combine(snapshots.Path, "Backup");

        Assert.False(new SnapshotService().Create(new(game.Path, destination,
            [target, System.IO.Path.Combine(game.Path, "missing.spc")])));

        Assert.Equal("previous snapshot", File.ReadAllText(previous));
        Assert.Single(Directory.GetFiles(destination));
        Assert.Empty(Directory.GetDirectories(snapshots.Path, "Backup.tmp.*"));
    }

    [Fact]
    public void SnapshotCannotReplaceInstallationOrItsAncestor()
    {
        using var game = new TestDirectory();
        string target = game.Write("file.spc", "current");

        Assert.False(new SnapshotService().Create(new(game.Path, game.Path, [target])));
        Assert.Equal("current", File.ReadAllText(target));
    }

    [Theory]
    [InlineData(".")]
    [InlineData("game/data")]
    public void SnapshotCannotReplaceAnAncestorOrDescendantOfInstallation(string relativeDestination)
    {
        using var root = new TestDirectory();
        string target = root.Write("game/data/file.spc", "current");
        string installation = System.IO.Path.Combine(root.Path, "game");
        string destination = System.IO.Path.GetFullPath(System.IO.Path.Combine(root.Path, relativeDestination));

        Assert.False(new SnapshotService().Create(new(installation, destination, [target])));
        Assert.Equal("current", File.ReadAllText(target));
    }

    [Fact]
    public void RestoreHandlesWildcardOnceAndDeletesOnlySuccessfulBackups()
    {
        using var game = new TestDirectory();
        string target = game.Write("file.assets", "modded");
        game.Write("file.assets_bak", "original");
        game.Write("blocked.assets_bak", "recovery");
        Directory.CreateDirectory(System.IO.Path.Combine(game.Path, "blocked.assets"));

        var result = new BackupRestorer().Restore(game.Path, [".assets", "*"]);

        Assert.Equal(1, result.RestoredCount);
        Assert.Equal(1, result.FailedCount);
        Assert.False(result.Succeeded);
        Assert.Equal("original", File.ReadAllText(target));
        Assert.False(File.Exists(target + "_bak"));
        Assert.Equal("recovery", File.ReadAllText(System.IO.Path.Combine(game.Path, "blocked.assets_bak")));
        Assert.Empty(Directory.GetFiles(game.Path, "*.v3ups_restore.*"));
    }
}
