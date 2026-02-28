

using System.Diagnostics;

namespace BenchmarkApp.Diagnostics;

/// <summary>
/// Manages dotnet-gcdump subprocess lifecycle for benchmark diagnostics.
/// All methods are synchronous and intended to be called from <c>Main</c>, outside the Avalonia event loop.
/// </summary>
public static class DiagnosticsCollector
{
    /// <summary>
    /// Validates that dotnet-gcdump is installed.
    /// </summary>
    /// <returns>A list of missing tool names, empty if all required tools are available.</returns>
    public static List<string> ValidateTools()
    {
        var missing = new List<string>();

        if (!IsToolAvailable("dotnet-gcdump"))
        {
            missing.Add("dotnet-gcdump");
        }

        return missing;
    }

    /// <summary>
    /// Collects a GC heap dump of the current process.
    /// Blocks until the dump is complete or a timeout is reached.
    /// </summary>
    /// <param name="outputPath">Path for the output .gcdump file.</param>
    public static void CollectGcDump(string outputPath)
    {
        var pid = Environment.ProcessId;

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet-gcdump",
            ArgumentList = { "collect", "-p", pid.ToString(), "-o", outputPath },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start dotnet-gcdump process.");

        // Drain stdout/stderr to prevent pipe buffer deadlock
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        process.WaitForExit(TimeSpan.FromMinutes(5));

        // Ensure pipe readers finish
        stdoutTask.Wait(TimeSpan.FromSeconds(5));
        stderrTask.Wait(TimeSpan.FromSeconds(5));
    }

    private static bool IsToolAvailable(string toolName)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = toolName,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process is null)
                return false;

            process.WaitForExit(TimeSpan.FromSeconds(10));
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
