// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BenchmarkApp;

/// <summary>
/// Command-line options for the benchmark runner.
/// </summary>
public sealed class BenchmarkOptions
{
    /// <summary>
    /// Gets the current options instance, set by <see cref="Parse"/>.
    /// </summary>
    public static BenchmarkOptions Current { get; private set; } = new();

    /// <summary>
    /// Gets the name of the benchmark test to run.
    /// </summary>
    public string? TestName { get; private set; }

    /// <summary>
    /// Gets a value indicating whether to list available tests and exit.
    /// </summary>
    public bool ListTests { get; private set; }

    /// <summary>
    /// Gets the number of iterations to run the benchmark.
    /// </summary>
    public int Iterations { get; private set; } = 1;

    /// <summary>
    /// Gets a value indicating whether to keep the window open after the benchmark completes.
    /// </summary>
    public bool KeepOpen { get; private set; }

    /// <summary>
    /// Parses command-line arguments and sets <see cref="Current"/>.
    /// </summary>
    public static BenchmarkOptions Parse(string[] args)
    {
        var options = new BenchmarkOptions();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--test" when i + 1 < args.Length:
                    options.TestName = args[++i];
                    break;
                case "--list":
                    options.ListTests = true;
                    break;
                case "--iterations" when i + 1 < args.Length:
                    if (int.TryParse(args[++i], out var iterations) && iterations > 0)
                    {
                        options.Iterations = iterations;
                    }

                    break;
                case "--keep-open":
                    options.KeepOpen = true;
                    break;
            }
        }

        Current = options;
        return options;
    }
}
