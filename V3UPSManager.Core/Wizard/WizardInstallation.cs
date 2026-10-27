namespace V3UPSManager.Core.Wizard;

public sealed record PreparedInstallation(
    GameBase Game,
    InstallationValidationResult Validation,
    PatchDiscoveryResult Discovery,
    PatchInstallRequest Request);

/// <summary>Shares validation, patch planning and transactional installation with the manager GUI.</summary>
public static class WizardInstallation
{
    public static PreparedInstallation Prepare(Game selectedGame, string installationFolder, string patchFolder,
        string toolsDirectory, Func<ValidationMessage, bool>? report = null)
    {
        GameBase game = GameCatalog.Create(selectedGame);
        if (game.GameID == Game.None) throw new InvalidDataException("Select a supported game.");
        string installation = Path.GetFullPath(installationFolder);
        string patches = Path.GetFullPath(patchFolder);
        InstallationValidationResult validation = new InstallationValidator().Validate(game, installation, report)
            ?? throw new InvalidDataException("The game installation could not be validated. Review the reported errors and select the correct game installation root.");
        PatchDiscoveryResult discovery = PatchDiscovery.Discover(game, installation, patches,
            validation.IsUnity, validation.IsLegacy, validation.CanAccessExecutable);
        if (!discovery.IsComplete)
            throw new InvalidDataException(discovery.CandidateCount == 0
                ? "No correctly named patches for this game and edition were found in the package."
                : $"Not every patch could be matched to a game file ({discovery.Targets.Count}/{discovery.CandidateCount}). " +
                  "Check that this package matches the selected game and edition.");
        string tool = Path.GetFullPath(Path.Combine(toolsDirectory, game.PatchFormatInstaller + ".exe"));
        if (!File.Exists(tool))
            throw new FileNotFoundException($"Required patch tool not found: {Path.GetFileName(tool)}. Place it beside V3UPSWizard.exe.", tool);
        var request = new PatchInstallRequest(installation, patches, tool, game.PatchFormat,
            discovery.Targets, validation.CanAccessExecutable);
        return new(game, validation, discovery, request);
    }

    // Mutex ownership is thread-affine. Call this whole synchronous method from the worker thread.
    public static bool Install(PreparedInstallation prepared, IPatchTool? tool = null,
        Action<OperationMessage>? report = null)
    {
        using var mutex = new Mutex(false, FileOperationLock.Name);
        if (!FileOperationLock.TryAcquire(mutex))
            throw new IOException("Another installation or uninstallation is running. Wait for it to finish and retry.");
        try { return new PatchInstaller(tool).Install(prepared.Request, report); }
        finally { mutex.ReleaseMutex(); }
    }
}
