using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.Controls.Embedding;
using Avalonia.WinUI;
using Microsoft.UI.Xaml;

namespace Avalonia.Controls.Maui.Platforms.Windows.Handlers
{
    /// <summary>
    /// Keeps <see cref="AvaloniaSwapChainPanel"/> from clearing the Avalonia focus of other embedded views.
    /// </summary>
    internal static class AvaloniaPanelFocus
    {
        private static readonly MethodInfo? s_onLostFocus = typeof(AvaloniaSwapChainPanel).GetMethod(
            "OnLostFocus", BindingFlags.Instance | BindingFlags.NonPublic, [typeof(object), typeof(RoutedEventArgs)]);

        /// <summary>Replaces the LostFocus handler of <paramref name="panel"/>.</summary>
        [DynamicDependency("OnLostFocus", typeof(AvaloniaSwapChainPanel))]
        public static void Attach(AvaloniaSwapChainPanel panel)
        {
            if (s_onLostFocus is null)
                return;

            var onLostFocus = s_onLostFocus.CreateDelegate<RoutedEventHandler>(panel);
            panel.LostFocus -= onLostFocus;
            panel.LostFocus += (sender, e) =>
            {
                ref var root = ref GetRoot(panel);
                var focused = root?.FocusManager?.GetFocusedElement() as Visual;
                if (root is null || focused is null || TopLevel.GetTopLevel(focused) == root)
                {
                    onLostFocus(sender, e);
                    return;
                }

                // The focus is in another top level: let the panel update its text input, but keep the focus.
                var savedRoot = root;
                root = null;
                try
                {
                    onLostFocus(sender, e);
                }
                finally
                {
                    root = savedRoot;
                }
            };
        }

        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_root")]
        private static extern ref EmbeddableControlRoot? GetRoot(AvaloniaSwapChainPanel panel);
    }
}
