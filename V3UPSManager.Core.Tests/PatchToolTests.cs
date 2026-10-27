namespace V3UPSManager.Core.Tests;

public sealed class PatchToolTests
{
    [LinuxFact]
    public async Task RealProcessPreservesArgumentsClosesInputAndDrainsBothPipes()
    {
        using var directory = new TestDirectory();
        string executable = directory.Write("patch tool", """
            #!/bin/sh
            set -eu
            if IFS= read -r value; then exit 13; fi
            if [ "$1" = apply ]; then
              [ "$2" = --base ] && [ "$4" = --patch ] && [ "$6" = --output ]
              [ -f "$5" ]
              cat "$3" > "$7"
            else
              [ "$1" = -d ] && [ "$2" = -f ] && [ "$3" = -s ]
              [ -f "$5" ]
              cat "$4" > "$6"
            fi
            i=0
            while [ "$i" -lt 3000 ]; do
              printf 'stdout: patch tool progress text that fills the output pipe\n'
              printf 'stderr: patch tool diagnostic text that fills the error pipe\n' >&2
              i=$((i+1))
            done
            """ + "\n");
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(executable, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        string original = directory.Write("game data/original file.spc", "clean original");
        string patch = directory.Write("mod data/translation patch.ups");
        foreach (FormatType format in new[] { FormatType.UPS, FormatType.XDelta })
        {
            string output = System.IO.Path.Combine(directory.Path, format + " output.spc");
            var result = await Task.Run(() => new ProcessPatchTool().Apply(executable, format, original, patch, output))
                .WaitAsync(TimeSpan.FromSeconds(15));
            Assert.Equal(0, result.ExitCode);
            Assert.Equal("clean original", File.ReadAllText(output));
            Assert.Equal(3000, result.StandardOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
            Assert.Equal(3000, result.StandardError.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
        }
    }
}
