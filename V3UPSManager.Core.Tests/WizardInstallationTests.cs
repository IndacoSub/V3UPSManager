using V3UPSManager.Core.Wizard;

namespace V3UPSManager.Core.Tests;

public sealed class WizardInstallationTests
{
    [Fact]
    public async Task FolderPackageIsValidatedPlannedAndInstalledDirectly()
    {
        using var fixture = new WizardFixture();
        using var package = new PackageSession("not-needed-for-loose-patches");
        await package.OpenAsync(fixture.Patches.Path);
        await package.PreparePatchesAsync();
        Game selected = Enum.Parse<Game>(package.GameName!);

        PreparedInstallation prepared = WizardInstallation.Prepare(selected, fixture.Game.Path,
            package.PatchFolder, fixture.Tools.Path, _ => true);
        var tool = new FakePatchTool();
        Assert.True(WizardInstallation.Install(prepared, tool));

        Assert.True(prepared.Validation.IsLegacy);
        Assert.True(prepared.Discovery.IsComplete);
        Assert.Equal("patched:clean first", File.ReadAllText(fixture.FirstFile));
        Assert.Equal("clean first", File.ReadAllText(fixture.FirstFile + "_bak"));
        Assert.Equal(2, tool.Calls);
        Assert.Empty(Directory.GetFiles(fixture.Game.Path, "*.v3ups_*", SearchOption.AllDirectories));
    }

    [Fact]
    public void FailedSetupPreservesEarlierModsAndCanBeRetried()
    {
        using var fixture = new WizardFixture();
        File.WriteAllText(fixture.FirstFile + "_bak", "clean first");
        File.WriteAllText(fixture.SecondFile + "_bak", "clean second");
        File.WriteAllText(fixture.FirstFile, "earlier mod first");
        File.WriteAllText(fixture.SecondFile, "earlier mod second");
        var prepared = fixture.Prepare();

        Assert.False(WizardInstallation.Install(prepared, new FakePatchTool(failOnSecond: true)));
        Assert.Equal("earlier mod first", File.ReadAllText(fixture.FirstFile));
        Assert.Equal("earlier mod second", File.ReadAllText(fixture.SecondFile));
        Assert.Equal("clean first", File.ReadAllText(fixture.FirstFile + "_bak"));
        Assert.Equal("clean second", File.ReadAllText(fixture.SecondFile + "_bak"));

        // A failed transaction must release the operation mutex as well as clean up outputs.
        Assert.True(WizardInstallation.Install(prepared, new FakePatchTool()));
        Assert.Equal("patched:clean first", File.ReadAllText(fixture.FirstFile));
    }

    [Fact]
    public void IncompletePackageIsRejectedBeforeAnyGameFileIsChanged()
    {
        using var fixture = new WizardFixture();
        fixture.Patches.Write("data/win/boot/missing.spc_patch.ups");

        Assert.Throws<InvalidDataException>(() => fixture.Prepare());

        Assert.Equal("clean first", File.ReadAllText(fixture.FirstFile));
        Assert.False(File.Exists(fixture.FirstFile + "_bak"));
        Assert.False(File.Exists(fixture.SecondFile + "_bak"));
    }

    [Fact]
    public void DecliningGameBuildConfirmationPreventsSetup()
    {
        using var fixture = new WizardFixture();

        Assert.Throws<InvalidDataException>(() => WizardInstallation.Prepare(Game.DanganronpaV3,
            fixture.Game.Path, fixture.Patches.Path, fixture.Tools.Path,
            message => !message.RequiresConfirmation));

        Assert.Equal("clean first", File.ReadAllText(fixture.FirstFile));
        Assert.False(File.Exists(fixture.FirstFile + "_bak"));
    }

    [Fact]
    public void MissingBundledPatchToolPreventsSetup()
    {
        using var fixture = new WizardFixture();
        File.Delete(System.IO.Path.Combine(fixture.Tools.Path, "ups.exe"));

        Assert.Throws<FileNotFoundException>(() => fixture.Prepare());

        Assert.False(File.Exists(fixture.FirstFile + "_bak"));
    }

    [Fact]
    public void SetupUsesTheSameOperationMutexAsTheManager()
    {
        using var fixture = new WizardFixture();
        var prepared = fixture.Prepare();
        using var acquired = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        bool ownsMutex = false;
        Exception? workerException = null;
        var worker = new Thread(() =>
        {
            try
            {
                using var mutex = new Mutex(false, FileOperationLock.Name);
                ownsMutex = FileOperationLock.TryAcquire(mutex);
                acquired.Set();
                if (!ownsMutex) return;
                try { release.Wait(TimeSpan.FromSeconds(10)); }
                finally { mutex.ReleaseMutex(); }
            }
            catch (Exception exception) { workerException = exception; acquired.Set(); }
        });
        worker.Start();
        try
        {
            Assert.True(acquired.Wait(TimeSpan.FromSeconds(5)), "The lock fixture did not start.");
            Assert.Null(workerException);
            Assert.True(ownsMutex);
            var tool = new FakePatchTool();

            Assert.Throws<IOException>(() => WizardInstallation.Install(prepared, tool));

            Assert.Equal(0, tool.Calls);
            Assert.False(File.Exists(fixture.FirstFile + "_bak"));
        }
        finally
        {
            release.Set();
            Assert.True(worker.Join(TimeSpan.FromSeconds(5)), "The lock fixture did not stop.");
        }
        Assert.True(WizardInstallation.Install(prepared, new FakePatchTool()));
    }

    private sealed class FakePatchTool(bool failOnSecond = false) : IPatchTool
    {
        public int Calls { get; private set; }
        public PatchToolResult Apply(string toolPath, FormatType format, string baseFile, string patchFile, string outputFile)
        {
            Calls++;
            if (failOnSecond && Calls == 2) return new(1, "", "Patch does not match the game build.");
            File.WriteAllText(outputFile, "patched:" + File.ReadAllText(baseFile));
            return new(0, "Patch applied.", "");
        }
    }

    private sealed class WizardFixture : IDisposable
    {
        public TestDirectory Game { get; } = new();
        public TestDirectory Patches { get; } = new();
        public TestDirectory Tools { get; } = new();
        public string FirstFile { get; }
        public string SecondFile { get; }

        public WizardFixture()
        {
            var game = new DRV3();
            Game.Write(game.LEGACY_EXE_NAME, "representative executable");
            Game.Write(game.LegacyLanguageFile, "US");
            foreach (string directory in game.RequiredDataDirectories)
                Directory.CreateDirectory(System.IO.Path.Combine(Game.Path, "data", "win", directory));
            FirstFile = Game.Write("data/win/boot/first.spc", "clean first");
            SecondFile = Game.Write("data/win/boot/second.spc", "clean second");
            Patches.Write("data/win/boot/first.spc_patch.ups", "patch first");
            Patches.Write("data/win/boot/second.spc_patch.ups", "patch second");
            Patches.Write("target.game", "DanganronpaV3");
            Patches.Write("README.txt", "Install this package into a supported clean game build.");
            Tools.Write("ups.exe");
        }

        public PreparedInstallation Prepare() => WizardInstallation.Prepare(global::V3UPSManager.Game.DanganronpaV3,
            Game.Path, Patches.Path, Tools.Path, _ => true);

        public void Dispose() { Game.Dispose(); Patches.Dispose(); Tools.Dispose(); }
    }
}
