using V3UPSManager.Core;

namespace V3UPSManager;

public partial class MainWindow : Form
{
    private bool Uninstall()
    {
        if (!CurrentGame.SupportsBackupUninstall(_isUnity))
        {
            Log(info[41]);
            return false;
        }
        RestoreResult result = new BackupRestorer().Restore(
            _verifiedInstallationFolder, CurrentGame.UninstallExtensions, ReportCoreOperation);
        Log(result.Succeeded ? info[33] : info[32], null,
            result.Succeeded ? Verbosity.Info : Verbosity.Error);
        return result.Succeeded;
    }
}
