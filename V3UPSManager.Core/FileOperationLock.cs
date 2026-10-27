namespace V3UPSManager.Core;

/// <summary>Shared by both front ends to serialize game-file changes in the Windows session.</summary>
public static class FileOperationLock
{
    public const string Name = "Local\\V3UPSManager.FileOperation";

    // Acquire and release on the same thread: named mutex ownership is thread-affine.
    public static bool TryAcquire(Mutex mutex)
    {
        try { return mutex.WaitOne(0); }
        catch (AbandonedMutexException) { return true; }
    }
}
