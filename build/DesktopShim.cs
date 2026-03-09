// Shim for Avalonia.Desktop source builds.
// Provides UsePlatformDetect() without referencing Avalonia.Desktop.csproj,
// which transitively pulls in X11 → FreeDesktop.AtSpi and its DBus source
// generator that doesn't propagate across repo boundaries.
//
// Only the current platform's backend is compiled in, controlled by
// AVALONIA_DESKTOP_OSX / _WINDOWS / _LINUX defines set in SourceBuild.targets.

using System.Runtime.CompilerServices;

namespace Avalonia
{
    internal static class AppBuilderDesktopExtensions
    {
        public static AppBuilder UsePlatformDetect(this AppBuilder builder)
        {
            builder.UseHarfBuzz();

#if AVALONIA_DESKTOP_WINDOWS
            LoadWin32(builder);
#elif AVALONIA_DESKTOP_OSX
            LoadAvaloniaNative(builder);
#elif AVALONIA_DESKTOP_LINUX
            LoadX11(builder);
#endif

            builder.UseSkia();
            return builder;
        }

#if AVALONIA_DESKTOP_WINDOWS
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void LoadWin32(AppBuilder builder) => builder.UseWin32();
#endif

#if AVALONIA_DESKTOP_OSX
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void LoadAvaloniaNative(AppBuilder builder) => builder.UseAvaloniaNative();
#endif

#if AVALONIA_DESKTOP_LINUX
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void LoadX11(AppBuilder builder) => builder.UseX11();
#endif
    }
}
