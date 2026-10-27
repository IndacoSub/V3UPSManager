namespace V3UPSManager;

/// <summary>Supported games and folder detection, independent of any user interface.</summary>
public static class GameCatalog
{
    public static IReadOnlyList<GameBase> All { get; } = Array.AsReadOnly<GameBase>(
        [new DRV3(), new AITSF(), new TokyoPsychodemic()]);

    public static GameBase Create(Game game) => game switch
    {
        Game.DanganronpaV3 => new DRV3(),
        Game.AITheSomniumFiles => new AITSF(),
        Game.TokyoPsychodemic => new TokyoPsychodemic(),
        _ => new NoneGame()
    };

    public static Game Detect(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder)) return Game.None;
        // Recognize Windows package paths even when called by a non-Windows tool.
        string normalized = folder.TrimEnd('/', '\\').Replace('\\', '/');
        foreach (GameBase game in All)
        foreach (string identifier in game.FolderIdentifiers)
        {
            string segment = identifier.Trim().Replace('\\', '/').Trim('/');
            if (normalized.Equals(segment, StringComparison.OrdinalIgnoreCase) ||
                normalized.EndsWith("/" + segment, StringComparison.OrdinalIgnoreCase))
                return game.GameID;
        }
        return Game.None;
    }
}
