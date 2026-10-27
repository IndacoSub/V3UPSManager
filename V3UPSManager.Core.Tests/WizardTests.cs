using V3UPSManager.Core.Wizard;

namespace V3UPSManager.Core.Tests;

public sealed class WizardTests
{
    [Fact]
    public async Task FolderPackageLoadsMetadataAndNeverDeletesUsersFiles()
    {
        using var package = new TestDirectory();
        string patch = package.Write("data/file_patch.ups");
        package.Write("target.game", "danganronpav3\n");
        package.Write("LEGGIMI.txt", "Installation instructions");
        using (var session = new PackageSession("missing-7za-not-needed-for-folders"))
        {
            await session.OpenAsync(package.Path);
            Assert.Equal("DanganronpaV3", session.GameName);
            Assert.Equal("Installation instructions", session.Readme);
            Assert.Equal("", session.PatchFolder);
            await session.PreparePatchesAsync();
            Assert.Equal(package.Path, session.PatchFolder);
        }
        Assert.True(File.Exists(patch));
    }

    [Fact]
    public async Task InvalidGameMetadataCannotBecomeAPreparedPackage()
    {
        using var package = new TestDirectory();
        package.Write("file_patch.ups");
        package.Write("target.game", "UnsupportedGame");
        using var session = new PackageSession("unused");

        await Assert.ThrowsAsync<InvalidDataException>(() => session.OpenAsync(package.Path));
        Assert.Equal("", session.PatchFolder);
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.PreparePatchesAsync());
    }

    [Fact]
    public async Task MultipleGameFilesAreRejectedRatherThanSelectingArbitrarily()
    {
        using var package = new TestDirectory();
        package.Write("file_patch.ups");
        package.Write("first.game", "DanganronpaV3");
        package.Write("nested/second.game", "AITheSomniumFiles");
        using var session = new PackageSession("unused");

        await Assert.ThrowsAsync<InvalidDataException>(() => session.OpenAsync(package.Path));
    }

    [Fact]
    public async Task PackageWithoutPatchesCannotBecomeReady()
    {
        using var package = new TestDirectory();
        package.Write("README.txt", "Read me");
        using var session = new PackageSession("unused");

        await Assert.ThrowsAsync<InvalidDataException>(() => session.OpenAsync(package.Path));
        Assert.Equal("", session.PatchFolder);
    }

    [Fact]
    public async Task CanceledFolderPreparationDoesNotCacheReadyState()
    {
        using var package = new TestDirectory();
        package.Write("file_patch.ups");
        using var session = new PackageSession("unused");
        await session.OpenAsync(package.Path);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => session.PreparePatchesAsync(
            cancellationToken: new CancellationToken(canceled: true)));

        Assert.Equal("", session.PatchFolder);
        await session.PreparePatchesAsync();
        Assert.Equal(package.Path, session.PatchFolder);
    }

    [Fact]
    public async Task RemovedPatchesCannotLeaveAReadyFolderAfterPackageReview()
    {
        using var package = new TestDirectory();
        string patch = package.Write("file_patch.ups");
        using var session = new PackageSession("unused");
        await session.OpenAsync(package.Path);
        File.Delete(patch);

        await Assert.ThrowsAsync<InvalidDataException>(() => session.PreparePatchesAsync());

        Assert.Equal("", session.PatchFolder);
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("nested/../../outside.txt")]
    [InlineData(@"..\outside.txt")]
    [InlineData("/absolute.txt")]
    [InlineData(@"C:\outside.txt")]
    [InlineData(@"\\server\share\file")]
    [InlineData("file:stream")]
    [InlineData("folder/./file")]
    [InlineData("folder./file")]
    [InlineData("folder /file")]
    [InlineData("NUL.txt")]
    [InlineData("sub/COM1")]
    public void UnsafeArchivePathsAreRejectedBeforeExtraction(string path)
        => Assert.Throws<InvalidDataException>(() => SevenZipExtractor.ValidateEntryPath(path));

    [Theory]
    [InlineData("data/win/file_patch.ups")]
    [InlineData(@"data\win\file_patch.ups")]
    [InlineData("AI_TheSomniumFiles_Data/StreamingAssets/AssetBundles/fonts_patch.xdelta")]
    public void NormalArchivePathsAreAccepted(string path)
        => SevenZipExtractor.ValidateEntryPath(path);

    [Theory]
    [InlineData("Symbolic Link = ../outside")]
    [InlineData("Hard Link = other")]
    [InlineData("Attributes = lrwxrwxrwx")]
    public void ArchiveLinksAreRejected(string linkMetadata)
        => Assert.Throws<InvalidDataException>(() => SevenZipExtractor.ValidateListing(
            ["Path = package.DAIX.7z", "----------", "Path = file_patch.ups", linkMetadata]));

    [Fact]
    public void EmptyArchiveIsRejected()
        => Assert.Throws<InvalidDataException>(() => SevenZipExtractor.ValidateListing(
            ["Path = package.DAIX.7z", "----------"]));

    [LinuxFact]
    public async Task FailedNestedExtractionLeavesNoReadyPatchFolder()
    {
        using var package = new TestDirectory();
        using var tools = new TestDirectory();
        package.Write("PATCH.7z");
        string executable = WriteTool(tools,
            "if [ \"$1\" = l ]; then printf '%s\\n' ---------- 'Path = file_patch.ups'; exit 0; fi\n" +
            "printf 'archive is corrupt\\n' >&2\nexit 2\n");
        using var session = new PackageSession(executable);
        await session.OpenAsync(package.Path);

        await Assert.ThrowsAsync<IOException>(() => session.PreparePatchesAsync());

        Assert.Equal("", session.PatchFolder);
        Assert.True(File.Exists(System.IO.Path.Combine(package.Path, "PATCH.7z")));
    }

    [LinuxFact]
    public async Task ExtractedArchiveStaysAvailableUntilSessionIsDisposed()
    {
        using var package = new TestDirectory();
        using var tools = new TestDirectory();
        string archive = package.Write("package.DAIX.7z");
        string executable = WriteTool(tools,
            "if [ \"$1\" = l ]; then printf '%s\\n' ---------- 'Path = file_patch.ups'; exit 0; fi\n" +
            "for arg in \"$@\"; do case \"$arg\" in -o*) destination=${arg#-o};; esac; done\n" +
            "printf patch > \"$destination/file_patch.ups\"\n");
        string extracted;
        using (var session = new PackageSession(executable))
        {
            await session.OpenAsync(archive);
            await session.PreparePatchesAsync();
            extracted = session.PatchFolder;
            Assert.True(File.Exists(System.IO.Path.Combine(extracted, "file_patch.ups")));
        }
        Assert.False(Directory.Exists(extracted));
        Assert.True(File.Exists(archive));
    }

    [LinuxFact]
    public async Task ExternalToolDrainsBothOutputStreamsWithoutDeadlock()
    {
        using var tools = new TestDirectory();
        string executable = WriteTool(tools,
            "i=0; while [ \"$i\" -lt 3000 ]; do printf 'stdout line with enough text to fill a pipe\\n'; " +
            "printf 'stderr line with enough text to fill a pipe\\n' >&2; i=$((i+1)); done\n");
        var progress = new CountingProgress();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        await ExternalTool.RunAsync(executable, [], progress, timeout.Token);

        Assert.Equal(6000, progress.Count);
    }

    [LinuxFact]
    public async Task CancellationStopsAnExternalTool()
    {
        using var tools = new TestDirectory();
        string executable = WriteTool(tools, "sleep 30\n");
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            ExternalTool.RunAsync(executable, [], cancellationToken: cancellation.Token)
                .WaitAsync(TimeSpan.FromSeconds(10)));
    }

    [LinuxFact]
    public async Task ExternalToolClosesInputSoPasswordPromptsCannotHang()
    {
        using var tools = new TestDirectory();
        string executable = WriteTool(tools, "if IFS= read -r value; then exit 13; fi\nexit 0\n");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        await ExternalTool.RunAsync(executable, [], cancellationToken: timeout.Token);
    }

    private static string WriteTool(TestDirectory directory, string commands)
    {
        string executable = directory.Write("fake 7za", "#!/bin/sh\nset -eu\n" + commands);
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(executable, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        return executable;
    }

    private sealed class CountingProgress : IProgress<string>
    {
        private int count;
        public int Count => count;
        public void Report(string value) => Interlocked.Increment(ref count);
    }
}

internal sealed class LinuxFactAttribute : FactAttribute
{
    public LinuxFactAttribute()
    {
        if (!OperatingSystem.IsLinux()) Skip = "This process fixture uses a POSIX shell; portable package tests run on every platform.";
    }
}
