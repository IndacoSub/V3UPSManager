using System.Diagnostics;

namespace V3UPSManager.Core;

public sealed record PatchToolResult(int ExitCode, string StandardOutput, string StandardError);

public interface IPatchTool
{
    PatchToolResult Apply(string toolPath, FormatType format, string baseFile, string patchFile, string outputFile);
}

/// <summary>Runs a bundled patch tool directly, without a command shell.</summary>
public sealed class ProcessPatchTool : IPatchTool
{
    public PatchToolResult Apply(string toolPath, FormatType format, string baseFile, string patchFile, string outputFile)
    {
        var start = new ProcessStartInfo
        {
            FileName = Path.GetFullPath(toolPath),
            WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(toolPath))!,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
        };
        string[] arguments = format switch
        {
            FormatType.UPS => ["apply", "--base", baseFile, "--patch", patchFile, "--output", outputFile],
            FormatType.XDelta => ["-d", "-f", "-s", baseFile, patchFile, outputFile],
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };
        foreach (string argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new IOException("Could not start the patch tool.");
        process.StandardInput.Close();
        // Both pipes must drain concurrently, including when a tool emits a large error.
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        return new(process.ExitCode, output.GetAwaiter().GetResult(), error.GetAwaiter().GetResult());
    }
}
