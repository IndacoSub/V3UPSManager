namespace V3UPSManager.Core.Tests;

public sealed class PatchInstallerTests
{
    [Fact]
    public void SuccessfulReinstallUsesCleanBackupAndRemovesTemporaryFiles()
    {
        using var fixture = new PatchFixture();
        string target = fixture.Game.Write("asset.spc", "previous mod");
        fixture.Game.Write("asset.spc_bak", "clean original");
        string patch = fixture.Patches.Write("asset.spc_patch.ups");
        var tool = new RecordingTool();

        Assert.True(new PatchInstaller(tool).Install(fixture.Request(new PatchTarget(target, patch))));

        Assert.Equal("patched:clean original", File.ReadAllText(target));
        Assert.Equal("clean original", File.ReadAllText(target + "_bak"));
        Assert.Equal(["clean original"], tool.Inputs);
        Assert.Empty(Directory.GetFiles(fixture.Game.Path, "*.v3ups_*"));
    }

    [Fact]
    public void FailedSecondPatchPreservesPreviouslyModdedLiveFiles()
    {
        using var fixture = new PatchFixture();
        string first = fixture.Game.Write("first.spc", "first existing mod");
        string second = fixture.Game.Write("second.spc", "second existing mod");
        fixture.Game.Write("first.spc_bak", "first clean");
        fixture.Game.Write("second.spc_bak", "second clean");
        string patch = fixture.Patches.Write("patch.ups");
        var tool = new RecordingTool(failAt: 2);

        Assert.False(new PatchInstaller(tool).Install(fixture.Request(new PatchTarget(first, patch), new PatchTarget(second, patch))));

        Assert.Equal("first existing mod", File.ReadAllText(first));
        Assert.Equal("second existing mod", File.ReadAllText(second));
        Assert.Equal("first clean", File.ReadAllText(first + "_bak"));
        Assert.Equal("second clean", File.ReadAllText(second + "_bak"));
        Assert.Empty(Directory.GetFiles(fixture.Game.Path, "*.v3ups_*"));
    }

    [Fact]
    public void PublicationFailureRollsBackToPreviousLiveContent()
    {
        using var fixture = new PatchFixture();
        string first = fixture.Game.Write("first.spc", "existing mod");
        string second = fixture.Game.Write("second.spc", "second existing mod");
        fixture.Game.Write("first.spc_bak", "clean original");
        fixture.Game.Write("second.spc_bak", "second clean");
        string patch = fixture.Patches.Write("patch.ups");
        bool faultInjected = false;
        void FailSecondPublication(OperationMessage message)
        {
            if (message.MessageId != 66 || faultInjected) return;
            faultInjected = true;
            // Reproduce a filesystem change between staging and publishing.
            File.Delete(second);
            Directory.CreateDirectory(second);
        }

        Assert.False(new PatchInstaller(new RecordingTool()).Install(
            fixture.Request(new PatchTarget(first, patch), new PatchTarget(second, patch)), FailSecondPublication));

        Assert.True(faultInjected);
        Assert.Equal("existing mod", File.ReadAllText(first));
        Assert.Equal("clean original", File.ReadAllText(first + "_bak"));
        // The second destination is unavailable; keep its recovery copy for manual recovery.
        Assert.Contains(Directory.GetFiles(fixture.Game.Path, "second.spc.v3ups_rollback.*"),
            path => File.ReadAllText(path) == "second existing mod");
    }

    [Fact]
    public void MissingOutputIsFailureEvenWhenToolReportsSuccess()
    {
        using var fixture = new PatchFixture();
        string target = fixture.Game.Write("asset.spc", "clean");
        string patch = fixture.Patches.Write("asset.ups");

        Assert.False(new PatchInstaller(new RecordingTool(writeOutput: false)).Install(fixture.Request(new PatchTarget(target, patch))));
        Assert.Equal("clean", File.ReadAllText(target));
        Assert.Empty(Directory.GetFiles(fixture.Game.Path, "*.v3ups_*"));
    }

    [Fact]
    public void OutsideTargetIsRejectedBeforeAnyBackupOrToolInvocation()
    {
        using var fixture = new PatchFixture();
        using var outside = new TestDirectory();
        string safe = fixture.Game.Write("safe.spc", "clean");
        string unsafeTarget = outside.Write("outside.spc", "outside");
        string patch = fixture.Patches.Write("asset.ups");
        var tool = new RecordingTool();

        Assert.False(new PatchInstaller(tool).Install(fixture.Request(new PatchTarget(safe, patch), new PatchTarget(unsafeTarget, patch))));
        Assert.Empty(tool.Inputs);
        Assert.False(File.Exists(safe + "_bak"));
        Assert.False(File.Exists(unsafeTarget + "_bak"));
        Assert.Equal("outside", File.ReadAllText(unsafeTarget));
    }

    [Fact]
    public void OverlappingTargetAndBackupAreRejected()
    {
        using var fixture = new PatchFixture();
        string target = fixture.Game.Write("asset.spc", "clean");
        string backup = fixture.Game.Write("asset.spc_bak", "clean");
        string patch = fixture.Patches.Write("asset.ups");
        var tool = new RecordingTool();

        Assert.False(new PatchInstaller(tool).Install(fixture.Request(new PatchTarget(target, patch), new PatchTarget(backup, patch))));
        Assert.Empty(tool.Inputs);
        Assert.False(File.Exists(backup + "_bak"));
    }

    [LinuxFact]
    public void LinkedTargetIsRejected()
    {
        using var fixture = new PatchFixture();
        using var outside = new TestDirectory();
        string outsideFile = outside.Write("outside.spc", "outside");
        string link = System.IO.Path.Combine(fixture.Game.Path, "linked.spc");
        File.CreateSymbolicLink(link, outsideFile);
        string patch = fixture.Patches.Write("asset.ups");
        var tool = new RecordingTool();

        Assert.False(new PatchInstaller(tool).Install(fixture.Request(new PatchTarget(link, patch))));
        Assert.Empty(tool.Inputs);
        Assert.Equal("outside", File.ReadAllText(outsideFile));
    }

    private sealed class RecordingTool(int failAt = -1, bool writeOutput = true) : IPatchTool
    {
        public List<string> Inputs { get; } = [];

        public PatchToolResult Apply(string toolPath, FormatType format, string baseFile, string patchFile, string outputFile)
        {
            string source = File.ReadAllText(baseFile);
            Inputs.Add(source);
            if (Inputs.Count == failAt) return new(1, "", "invalid patch");
            if (writeOutput) File.WriteAllText(outputFile, "patched:" + source);
            return new(0, "ok", "");
        }
    }

    private sealed class PatchFixture : IDisposable
    {
        public TestDirectory Game { get; } = new();
        public TestDirectory Patches { get; } = new();
        private readonly TestDirectory tools = new();
        private readonly string toolPath;
        public PatchFixture() => toolPath = tools.Write("fake-tool");
        public PatchInstallRequest Request(params PatchTarget[] targets)
            => new(Game.Path, Patches.Path, toolPath, FormatType.UPS, targets);
        public void Dispose() { Game.Dispose(); Patches.Dispose(); tools.Dispose(); }
    }
}
