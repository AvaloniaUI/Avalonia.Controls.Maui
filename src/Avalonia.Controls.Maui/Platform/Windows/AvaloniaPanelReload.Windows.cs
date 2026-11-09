using System.Runtime.CompilerServices;
using Avalonia.Input;
using Avalonia.WinUI;

namespace Avalonia.Controls.Maui.Platforms.Windows.Handlers
{
    /// <summary>
    /// Lets an <see cref="AvaloniaSwapChainPanel"/> show its content and handle input again when it is loaded again,
    /// for example when a page that contains it is navigated back to.
    /// </summary>
    internal static class AvaloniaPanelReload
    {
        private static readonly ConditionalWeakTable<AvaloniaSwapChainPanel, object> s_attached = new();

        /// <summary>
        /// When <paramref name="panel"/> is unloaded, removes <paramref name="content"/> from its top level, so that
        /// the top level the panel creates when it is loaded again can show it, and gives the panel new pointer
        /// devices for its next top level. Does nothing if <paramref name="panel"/> was already attached.
        /// </summary>
        public static void Attach(AvaloniaSwapChainPanel panel, Control content)
        {
            if (!s_attached.TryAdd(panel, s_attached))
                return;

            // Runs after the handler of the panel, which subscribed first.
            panel.Unloaded += (_, _) =>
            {
                if (TopLevel.GetTopLevel(content) is ContentControl topLevel && topLevel.Content == content)
                    topLevel.Content = null;

                GetMouseDevice(panel) = CreateMouseDevice(null);
                GetTouchDevice(panel) = CreateTouchDevice();
                GetPenDevice(panel) = CreatePenDevice(false);
            };
        }

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_mouseDevice")]
        private static extern ref MouseDevice GetMouseDevice(AvaloniaSwapChainPanel panel);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_touchDevice")]
        private static extern ref TouchDevice GetTouchDevice(AvaloniaSwapChainPanel panel);

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_penDevice")]
        private static extern ref PenDevice GetPenDevice(AvaloniaSwapChainPanel panel);

        // The constructors are not in the reference assemblies of Avalonia.
        [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
        private static extern MouseDevice CreateMouseDevice(Avalonia.Input.Pointer? pointer);

        [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
        private static extern TouchDevice CreateTouchDevice();

        [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
        private static extern PenDevice CreatePenDevice(bool releasePointerOnPenUp);
    }
}
