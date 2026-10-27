using V3UPSManager.Core;

namespace V3UPSManager;

public partial class MainWindow : Form
{
    private InstallationValidationResult? ValidateInstallation() =>
        new InstallationValidator().Validate(CurrentGame, _installationFolder, message =>
        {
            DialogResult result = Log(
                info[message.MessageId], message.Arguments,
                message.IsError ? Verbosity.Error : Verbosity.Info,
                message.RequiresConfirmation ? LogType.Ask : LogType.Print);
            return !message.RequiresConfirmation || result == DialogResult.Yes;
        });
}
