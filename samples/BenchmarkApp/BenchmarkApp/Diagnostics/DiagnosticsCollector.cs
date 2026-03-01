

using System.Diagnostics;

namespace BenchmarkApp.Diagnostics;

/// <summary>
/// Manages dotnet-gcdump and dotnet-trace subprocess lifecycles for benchmark diagnostics.
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
    /// Validates that dotnet-trace is installed.
    /// </summary>
    /// <returns>A list of missing tool names, empty if all required tools are available.</returns>
    public static List<string> ValidateTraceTools()
    {
        var missing = new List<string>();

        if (!IsToolAvailable("dotnet-trace"))
        {
            missing.Add("dotnet-trace");
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

    /// <summary>
    /// Starts an EventPipe trace collection as a background process.
    /// </summary>
    /// <param name="outputPath">Path for the output .nettrace file.</param>
    /// <returns>The trace process handle. Pass to <see cref="StopTrace"/> to end collection.</returns>
    public static Process StartTrace(string outputPath)
    {
        var pid = Environment.ProcessId;

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet-trace",
            ArgumentList =
            {
                "collect",
                "-p", pid.ToString(),
                "--providers", "Microsoft-Windows-DotNETRuntime",
                "-o", outputPath,
            },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        return Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start dotnet-trace process.");
    }

    /// <summary>
    /// Stops a running trace collection gracefully by closing its stdin,
    /// which signals dotnet-trace to finish writing and exit.
    /// </summary>
    /// <param name="traceProcess">The process returned by <see cref="StartTrace"/>.</param>
    public static void StopTrace(Process traceProcess)
    {
        try
        {
            // Closing stdin signals dotnet-trace to stop
            traceProcess.StandardInput.Close();
            traceProcess.WaitForExit(TimeSpan.FromMinutes(2));
        }
        finally
        {
            if (!traceProcess.HasExited)
            {
                traceProcess.Kill();
            }

            traceProcess.Dispose();
        }
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
