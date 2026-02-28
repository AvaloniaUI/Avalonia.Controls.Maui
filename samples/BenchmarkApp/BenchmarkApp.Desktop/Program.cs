// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Avalonia;
using Avalonia.Controls.Maui;

namespace BenchmarkApp;

class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var options = BenchmarkOptions.Parse(args);

        if (options.ListTests || options.TestName is null)
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

        if (BenchmarkRegistry.CreateTest(options.TestName) is null)
        {
            Console.Error.WriteLine($"Error: Unknown benchmark test '{options.TestName}'.");
            Console.Error.WriteLine("Use --list to see available tests.");
            return 1;
        }

        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
