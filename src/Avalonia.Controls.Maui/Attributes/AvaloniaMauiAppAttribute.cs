using System;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Marks a static method that returns <c>MauiApp</c> as the entry point for Avalonia-based MAUI single-project applications.
/// The source generator will use this method to produce the Avalonia application class and entry point
/// for desktop (<c>net10.0</c>) and browser (<c>net10.0-browser</c>) target frameworks.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class AvaloniaMauiAppAttribute : Attribute
{
}
