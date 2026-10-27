namespace V3UPSManager.Core;

/// <summary>A UI-independent notification; the GUI resolves the existing localization ID.</summary>
public sealed record OperationMessage(
    int MessageId,
    Dictionary<string, string>? Variables = null,
    bool IsError = false);

internal static class OperationLog
{
    public static void Report(Action<OperationMessage>? log, int id,
        bool error = false, params (string Key, string Value)[] values)
    {
        try { log?.Invoke(new(id, values.ToDictionary(value => value.Key, value => value.Value), error)); }
        catch
        {
            // Notifications are optional. A UI/logging failure must never interrupt rollback.
        }
    }

    public static void Failure(Action<OperationMessage>? log, string path, Exception exception)
        => Report(log, 70, true, ("VAR_OUTPUT_FILE", path), ("VAR_EXCEPTION", exception.Message));
}
