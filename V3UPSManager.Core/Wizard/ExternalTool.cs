using System.Diagnostics;

namespace V3UPSManager.Core.Wizard;

/// <summary>Runs bundled tools without shell quoting and drains both output streams.</summary>
public static class ExternalTool
{
    public static async Task RunAsync(string executable, IEnumerable<string> arguments,
        IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(executable))
            throw new FileNotFoundException("Required tool not found. Place it beside V3UPSWizard.", executable);

        var start = new ProcessStartInfo(Path.GetFullPath(executable))
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(executable))!
        };
        foreach (string argument in arguments) start.ArgumentList.Add(argument);
        cancellationToken.ThrowIfCancellationRequested();
        using var process = Process.Start(start) ?? throw new IOException("The external tool could not be started.");
        process.StandardInput.Close(); // Password/overwrite prompts must fail instead of hanging a hidden process.
        using var registration = cancellationToken.Register(() =>
        {
            try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) { }
            catch (System.ComponentModel.Win32Exception) { }
        });
        Task stdout = ReadOutputAsync(process.StandardOutput, progress);
        Task stderr = ReadOutputAsync(process.StandardError, progress);
        await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
        await Task.WhenAll(stdout, stderr).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (process.ExitCode != 0)
            throw new IOException($"{Path.GetFileName(executable)} failed (exit code {process.ExitCode}). See the extraction log.");
    }

    private static async Task ReadOutputAsync(StreamReader reader, IProgress<string>? progress)
    {
        while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
            progress?.Report(line);
    }
}
