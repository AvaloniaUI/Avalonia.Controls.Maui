// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace BenchmarkApp;

/// <summary>
/// Base class for benchmark test pages. Subclasses set up their MAUI control tree
/// in the constructor and perform measurements in <see cref="RunAsync"/>.
/// </summary>
public abstract class BenchmarkTestPage : ContentPage
{
    /// <summary>
    /// Runs the benchmark and returns the result.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output during the run.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The benchmark result indicating pass/fail and any collected metrics.</returns>
    public abstract Task<BenchmarkResult> RunAsync(ILogger logger, CancellationToken cancellationToken);
}
