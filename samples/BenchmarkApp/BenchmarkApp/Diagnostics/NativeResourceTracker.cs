using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BenchmarkApp.Diagnostics;

/// <summary>
/// Tracks platform-specific native resource counts (GDI objects, USER objects)
/// that are invisible to managed memory tracking.
/// </summary>
internal static partial class NativeResourceTracker
{
    private const uint GR_GDIOBJECTS = 0;
    private const uint GR_USEROBJECTS = 1;
    private const uint GR_GDIOBJECTS_PEAK = 2;
    private const uint GR_USEROBJECTS_PEAK = 4;

    [LibraryImport("user32.dll")]
    private static partial uint GetGuiResources(IntPtr hProcess, uint uiFlags);

    /// <summary>
    /// Captures current GDI and USER object counts for the current process.
    /// Returns zeroes on non-Windows platforms.
    /// </summary>
    public static HandleCounts GetHandleCounts()
    {
        if (!OperatingSystem.IsWindows())
            return default;

        using var proc = Process.GetCurrentProcess();
        return new HandleCounts(
            GetGuiResources(proc.Handle, GR_GDIOBJECTS),
            GetGuiResources(proc.Handle, GR_USEROBJECTS),
            GetGuiResources(proc.Handle, GR_GDIOBJECTS_PEAK),
            GetGuiResources(proc.Handle, GR_USEROBJECTS_PEAK));
    }
}

/// <summary>
/// GDI and USER object counts for a process at a point in time.
/// </summary>
public readonly record struct HandleCounts(
    uint GdiObjects,
    uint UserObjects,
    uint GdiObjectsPeak,
    uint UserObjectsPeak);
