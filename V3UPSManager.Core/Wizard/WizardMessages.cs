namespace V3UPSManager.Core.Wizard;

/// <summary>English presentation for the standalone wizard; the manager keeps its existing localization.</summary>
public static class WizardMessages
{
    public static string Format(ValidationMessage message)
    {
        string text = message.MessageId switch
        {
            2 => "language.txt was not found. Start the game once and check that it works before installing a mod.",
            3 => "The game language is not English. This package requires the English game data.",
            4 => "The expected data/win (or data/WIN) directory was not found.",
            5 or 6 => "Packed CPK archives remain in data/win. Extract them and follow the mod author's instructions first.",
            7 => "The game executable could not be read: VAR_LEGACY_EXE_NAME.",
            8 => "The game executable differs from the expected version. This package may be incompatible. Continue anyway?",
            9 => "ReShade was detected. It may conflict with this mod. Continue anyway?",
            10 => "VAR_GAME_FIX was detected. It may conflict with this mod. Continue anyway?",
            11 => "The required boot directory was not found.",
            12 => "The required flash directory was not found.",
            13 => "The required game_resident directory was not found.",
            14 => "The required minigame directory was not found.",
            15 => "The required trial_font directory was not found.",
            16 => "The required wrd_data directory was not found.",
            17 => "The required wrd_script directory was not found.",
            25 => "Select a valid game installation root with its expected data directories.",
            31 => "The game version or Unity data layout is not supported. Check the selected game installation root.",
            38 => "Anniversary Edition files were found in a Legacy game installation.",
            39 => "Legacy game files were found in an Anniversary Edition installation.",
            40 => "Packed ARC archives remain in the game directory. Follow the mod author's preparation instructions first.",
            42 => "The supported Switch/Anniversary game layout was recognized.",
            48 => "The required game executable was not found: VAR_UNITY_EXE_NAME.",
            49 => "SpecialK or Reloaded-II was detected. It may conflict with this mod. Continue anyway?",
            _ => message.RequiresConfirmation ? "The game installation differs from the expected configuration. Continue anyway?"
                : message.IsError ? "The game installation is not supported or is incomplete." : "Game installation checked."
        };
        return Replace(text, message.Arguments);
    }

    public static string Format(OperationMessage message)
    {
        string text = message.MessageId switch
        {
            54 => "Installation failed. Automatic rollback was attempted; review any restoration errors below.",
            56 => "Installation started. Files to process: VAR_FILE_COUNT.",
            58 => "Executable skipped because this edition does not allow executable access: VAR_OUTPUT_FILE.",
            59 => "Using the existing original backup: VAR_BACKUP_FILE.",
            60 => "Original backup created: VAR_BACKUP_FILE.",
            61 => "Applying patch: VAR_PATCH_FILE → VAR_OUTPUT_FILE.",
            63 => "Patch tool exited with code VAR_EXIT_CODE for VAR_OUTPUT_FILE.",
            64 => "Patch tool output: VAR_TOOL_OUTPUT",
            65 => "Patch tool diagnostics: VAR_TOOL_ERROR",
            66 => "Installed patched file: VAR_OUTPUT_FILE.",
            68 => "Installation completed successfully. Files modified: VAR_MODIFIED_COUNT/VAR_FILE_COUNT.",
            69 => "Restored the previous file: VAR_OUTPUT_FILE.",
            70 => "Operation failed for VAR_OUTPUT_FILE: VAR_EXCEPTION",
            86 => "The required patch tool was not found: VAR_PATCH_TOOL.",
            94 => "Preparing original-file backups before applying patches.",
            _ => message.IsError ? "The installation encountered an error." : "Installation progress updated."
        };
        return Replace(text, message.Variables);
    }

    private static string Replace(string text, Dictionary<string, string>? variables)
    {
        if (variables is not null)
            foreach ((string key, string value) in variables.OrderByDescending(pair => pair.Key.Length))
                text = text.Replace(key, value, StringComparison.Ordinal);
        return text;
    }
}
