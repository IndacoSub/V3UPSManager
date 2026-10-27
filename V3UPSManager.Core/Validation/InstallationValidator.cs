namespace V3UPSManager.Core;

public enum GameEdition { Legacy, Unity, Xbox }

public sealed record InstallationValidationResult(
    GameEdition Edition, string DataFolder, bool CanAccessExecutable = true, string TitleId = "")
{
    public bool IsLegacy => Edition == GameEdition.Legacy;
    public bool IsUnity => Edition == GameEdition.Unity;
}

/// <summary>A validation event; the presentation layer owns the localized text.</summary>
public sealed record ValidationMessage(
    int MessageId, Dictionary<string, string>? Arguments = null,
    bool IsError = false, bool RequiresConfirmation = false);

/// <summary>Validates supported game layouts without depending on a window or control.</summary>
public sealed class InstallationValidator
{
    public InstallationValidationResult? Validate(
        GameBase game, string installationFolder, Func<ValidationMessage, bool>? report = null)
    {
        bool Notify(int id, bool error = false, bool ask = false,
            Dictionary<string, string>? arguments = null) =>
            report?.Invoke(new(id, arguments, error, ask)) ?? !ask;

        bool Reject(int id)
        {
            Notify(id, error: true);
            return false;
        }

        bool ConfirmFile(string name, int id, Dictionary<string, string>? args = null) =>
            !File.Exists(Path.Combine(installationFolder, name)) || Notify(id, ask: true, arguments: args);

        bool ConfirmHash(string executable, string? expected)
        {
            string actual = FileSystemUtilities.ComputeMd5(executable);
            if (actual.Length == 0)
            {
                Notify(7, error: true, arguments: new() { ["VAR_LEGACY_EXE_NAME"] = Path.GetFileName(executable) });
                return false;
            }
            return string.IsNullOrWhiteSpace(expected) ||
                actual.Equals(expected, StringComparison.OrdinalIgnoreCase) || Notify(8, ask: true);
        }

        bool ConfirmCommonConflicts() => ConfirmFile("ReShade.ini", 9);

        try
        {
            if (game.GameID == Game.None || string.IsNullOrWhiteSpace(installationFolder) ||
                !Directory.Exists(installationFolder))
            {
                Reject(25);
                return null;
            }

            installationFolder = Path.GetFullPath(installationFolder);
            InstallationValidationResult? result = game.GameID switch
            {
                Game.DanganronpaV3 => ValidateDanganronpa(),
                Game.AITheSomniumFiles => ValidateSomnium(),
                Game.TokyoPsychodemic => ValidateTokyo(),
                _ => null,
            };

            if (result == null)
                return null;
            if (!Directory.Exists(result.DataFolder))
            {
                Reject(25);
                return null;
            }

            foreach (string required in game.RequiredDataDirectories)
            {
                if (Directory.Exists(Path.Combine(result.DataFolder, required)))
                    continue;
                Reject(required switch
                {
                    "boot" => 11, "flash" => 12, "game_resident" => 13,
                    "minigame" => 14, "trial_font" => 15, "wrd_data" => 16,
                    "wrd_script" => 17, _ => 25,
                });
                return null;
            }
            return result;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException
            or NotSupportedException or System.Security.SecurityException)
        {
            Notify(25, error: true);
            return null;
        }

        InstallationValidationResult? ValidateDanganronpa()
        {
            // Select an edition from identifying files before validating it.
            // Rejecting a confirmation must not fall through to another edition.
            string legacyExe = Path.Combine(installationFolder, game.LEGACY_EXE_NAME);
            if (game.SupportsLegacyEdition && File.Exists(legacyExe))
                return ValidateLegacy(legacyExe);
            string xboxExe = Path.Combine(installationFolder, game.ANNIVERSARY_EXE_NAME);
            if (game.SupportsXboxEdition && File.Exists(xboxExe))
                return ValidateXbox();
            return ValidateSwitch();
        }

        InstallationValidationResult? ValidateLegacy(string executable)
        {
            string languageFile = Path.Combine(installationFolder, game.LegacyLanguageFile);
            if (!File.Exists(languageFile)) { Reject(2); return null; }
            if (!File.ReadAllText(languageFile).Trim().Equals(game.LegacyLanguageValue, StringComparison.OrdinalIgnoreCase))
            { Reject(3); return null; }
            string data = game.GetLegacyDataFolder(installationFolder);
            if (!Directory.Exists(data)) { Reject(4); return null; }
            if (game.LegacyArchiveFiles.Any(name => File.Exists(Path.Combine(data, name))))
            { Reject(5); return null; }
            if (game.AnniversaryArchiveFiles.Any(name => File.Exists(Path.Combine(data, name))))
            { Reject(38); return null; }
            if (Directory.EnumerateFiles(data).Any(file => Path.GetExtension(file).Equals(".cpk", StringComparison.OrdinalIgnoreCase)))
            { Reject(6); return null; }
            if (!ConfirmHash(executable, game.LegacyExecutableHash) || !ConfirmCommonConflicts() ||
                !ConfirmFile("dr3fix.cfg", 10, new() { ["VAR_GAME_FIX"] = "DR3Fix" }) || !ConfirmFile("dinput8.ini", 49))
                return null;
            return new(GameEdition.Legacy, data);
        }

        InstallationValidationResult? ValidateXbox()
        {
            string data = game.GetXboxDataFolder(installationFolder);
            if (!Directory.Exists(data)) { Reject(4); return null; }
            if (game.LegacyArchiveFiles.Any(name => File.Exists(Path.Combine(data, name))))
            { Reject(39); return null; }
            if (game.AnniversaryArchiveFiles.Any(name => File.Exists(Path.Combine(data, name))))
            { Reject(40); return null; }
            if (!ConfirmCommonConflicts()) return null;
            return new(GameEdition.Xbox, data, CanAccessExecutable: false);
        }

        InstallationValidationResult? ValidateSwitch()
        {
            string name = Path.GetFileName(Path.TrimEndingDirectorySeparator(installationFolder));
            string? titleId = game.SupportedUnityTitleIds.FirstOrDefault(id => id.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (titleId == null) { Reject(31); return null; }
            string unityRoot = Path.Combine(installationFolder, game.UnityDataFolder);
            if (!FileSystemUtilities.DirExistsMatchCase(unityRoot)) { Reject(31); return null; }
            string streaming = Path.Combine(unityRoot, "StreamingAssets");
            if (!Directory.Exists(streaming)) { Reject(31); return null; }

            // Find platforms by their required content, not enumeration order.
            string[] platforms = Directory.GetDirectories(streaming)
                .Where(path => !new[] { "map", "master", "static" }.Contains(Path.GetFileName(path), StringComparer.OrdinalIgnoreCase))
                .Where(path => Directory.Exists(Path.Combine(path, "all")))
                .ToArray();
            if (platforms.Length != 1) { Reject(31); return null; }
            string data = game.GetUnityDataFolderPath(installationFolder, Path.GetFileName(platforms[0]));
            bool hasContent = Directory.EnumerateFiles(data, "*", SearchOption.AllDirectories)
                .Any(path => game.UnityValidationExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase));
            if (!hasContent) { Reject(31); return null; }
            Notify(42);
            return new(GameEdition.Unity, data, TitleId: titleId);
        }

        InstallationValidationResult? ValidateSomnium()
        {
            string executable = Path.Combine(installationFolder, game.UNITY_EXE_NAME);
            if (!File.Exists(executable))
            {
                Notify(48, error: true, arguments: new() { ["VAR_UNITY_EXE_NAME"] = game.UNITY_EXE_NAME });
                return null;
            }
            string streaming = Path.Combine(installationFolder, game.UnityDataFolder, "StreamingAssets");
            string data = Path.Combine(streaming, "AssetBundles", "StandaloneWindows64");
            if (!Directory.Exists(data) || new[] { "luabytecode", "etc", "fonts" }.Any(file => !File.Exists(Path.Combine(data, file))))
            { Reject(31); return null; }
            bool isXbox = Directory.Exists(Path.Combine(streaming, "Movie"));
            if ((!isXbox && !ConfirmHash(executable, game.UnityExecutableHash)) || !ConfirmCommonConflicts())
                return null;
            if (Directory.Exists(Path.Combine(installationFolder, "BepInEx")) &&
                !Notify(10, ask: true, arguments: new() { ["VAR_GAME_FIX"] = "AISomniumFilesFix" }))
                return null;
            if (!ConfirmFile(Path.Combine("Mods", "SomniumDebugEnabler.dll"), 10,
                new() { ["VAR_GAME_FIX"] = "SomniumDebugEnabler" }))
                return null;
            return new(GameEdition.Unity, data, CanAccessExecutable: !isXbox);
        }

        InstallationValidationResult? ValidateTokyo()
        {
            string executable = Path.Combine(installationFolder, game.UNITY_EXE_NAME);
            if (!File.Exists(executable))
            {
                Notify(48, error: true, arguments: new() { ["VAR_UNITY_EXE_NAME"] = game.UNITY_EXE_NAME });
                return null;
            }
            string data = game.GetUnityDataFolderPath(installationFolder, string.Empty);
            if (!Directory.Exists(data)) { Reject(31); return null; }
            if (!ConfirmHash(executable, game.UnityExecutableHash) || !ConfirmCommonConflicts() || !ConfirmFile("dinput8.ini", 49))
                return null;
            return new(GameEdition.Unity, data);
        }
    }
}
