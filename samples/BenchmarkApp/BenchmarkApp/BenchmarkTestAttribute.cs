// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BenchmarkApp;

/// <summary>
/// Marks a <see cref="BenchmarkTestPage"/> for discovery by the benchmark registry.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class BenchmarkTestAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BenchmarkTestAttribute"/> class.
    /// </summary>
    /// <param name="name">The unique name used to invoke this benchmark from the CLI.</param>
    public BenchmarkTestAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the unique name used to invoke this benchmark from the CLI.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets an optional description of what the benchmark measures.
    /// </summary>
    public string? Description { get; set; }
}
