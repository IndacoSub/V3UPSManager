using V3UPSManager.Core;

namespace V3UPSManager;

public partial class MainWindow : Form
{
    // Discovery keeps these aligned; the Core receives immutable target pairs.
    private List<string> _filesToPatch = new();
    private List<string> _patchesToApply = new();

    private bool Install()
    {
        if (_filesToPatch.Count != _patchesToApply.Count)
        {
            WriteDiagnosticFiles();
            ReportCoreOperation(new(85, new()
            {
                ["VAR_PATCH_COUNT"] = _patchesToApply.Count.ToString(),
                ["VAR_FILE_COUNT"] = _filesToPatch.Count.ToString(),
            }, true));
            return false;
        }
        string diagnostic = Path.Combine(AppContext.BaseDirectory, "patch_log.txt");
        try { File.Delete(diagnostic); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        var request = new PatchInstallRequest(
            _verifiedInstallationFolder,
            _patchFolder,
            Path.Combine(AppContext.BaseDirectory, CurrentGame.PatchFormatInstaller + ".exe"),
            CurrentGame.PatchFormat,
            _filesToPatch.Zip(_patchesToApply, (file, patch) => new PatchTarget(file, patch)).ToArray(),
            _canAccessExe);
        return new PatchInstaller().Install(request, ReportCoreOperation);
    }

    private void ReportCoreOperation(OperationMessage message)
    {
        Log(info[message.MessageId], message.Variables,
            message.IsError ? Verbosity.Error : Verbosity.Info, LogType.ConsoleOnly);
        RefreshLogConsole();
        try
        {
            string text = info[message.MessageId];
            if (message.Variables != null)
                foreach (var pair in message.Variables) text = text.Replace(pair.Key, pair.Value);
            File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "patch_log.txt"), text + Environment.NewLine);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private void WriteDiagnosticFiles()
    {
        try
        {
            File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "_filesToPatch.txt"), _filesToPatch);
            File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "_patchesToApply.txt"), _patchesToApply);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
