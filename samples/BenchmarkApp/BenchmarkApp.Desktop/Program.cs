

using Avalonia;
using Avalonia.Controls.Maui;
using BenchmarkApp.Diagnostics;

namespace BenchmarkApp;

class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var options = BenchmarkOptions.Parse(args);

        if (options.ListTests || (options.TestName is null && !options.RunAll))
        {
            var tests = BenchmarkRegistry.GetTests();
            Console.WriteLine("Available benchmark tests:");
            Console.WriteLine();
            foreach (var (name, (description, _)) in tests)
            {
                Console.WriteLine($"  {name,-30} {description ?? string.Empty}");
            }

            return 0;
        }

        if (!options.RunAll && BenchmarkRegistry.CreateTest(options.TestName!) is null)
        {
            Console.Error.WriteLine($"Error: Unknown benchmark test '{options.TestName}'.");
            Console.Error.WriteLine("Use --list to see available tests.");
            return 1;
        }

        if (options.GcDump)
        {
            var missing = DiagnosticsCollector.ValidateTools();
            if (missing.Count > 0)
            {
                Console.Error.WriteLine("Required diagnostic tools are not installed:");
                foreach (var tool in missing)
                {
                    Console.Error.WriteLine($"  {tool}");
                    Console.Error.WriteLine($"    Install with: dotnet tool install -g {tool}");
                }

                return 1;
            }

            Directory.CreateDirectory(options.DiagnosticsOutputDir);
        }

        if (options.Trace)
        {
            var missing = DiagnosticsCollector.ValidateTraceTools();
            if (missing.Count > 0)
            {
                Console.Error.WriteLine("Required diagnostic tools are not installed:");
                foreach (var tool in missing)
                {
                    Console.Error.WriteLine($"  {tool}");
                    Console.Error.WriteLine($"    Install with: dotnet tool install -g {tool}");
                }

                return 1;
            }

            Directory.CreateDirectory(options.DiagnosticsOutputDir);
        }

        // Build a file name prefix from the test name and current timestamp.
        var testLabel = options.RunAll ? "all" : options.TestName ?? "unknown";
        var safeTestLabel = string.Join("_", testLabel.Split(Path.GetInvalidFileNameChars()));
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var filePrefix = $"{safeTestLabel}_{timestamp}";

        var exitCode = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        // Collect GC dump after the app exits (captures post-shutdown heap state).
        if (options.GcDump)
        {
            var gcDumpPath = Path.Combine(options.DiagnosticsOutputDir, $"{filePrefix}.gcdump");
            Console.WriteLine($"Collecting GC dump...");
            DiagnosticsCollector.CollectGcDump(gcDumpPath);
            Console.WriteLine($"GC dump saved to {gcDumpPath}");
        }

        return exitCode;
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
