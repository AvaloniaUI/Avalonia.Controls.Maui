using Avalonia.Controls.Maui.Controls;

namespace Avalonia.Controls.Maui.Platforms.Windows.Handlers
{
    /// <summary>
    /// A platform-specific view for Windows that hosts Avalonia content within a .NET MAUI application.
    /// Inherits from <see cref="Avalonia.WinUI.AvaloniaSwapChainPanel"/>, which renders the Avalonia
    /// content into a WinUI 3 swap chain and bridges input, IME, cursors and drag-and-drop.
    /// </summary>
    public partial class MauiAvaloniaView : Avalonia.WinUI.AvaloniaSwapChainPanel
    {
        readonly AvaloniaView _mauiView;
        readonly Avalonia.Controls.Border _backgroundBorder = new();
        readonly EmbeddedPopupHost _popupHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="MauiAvaloniaView"/> class.
        /// </summary>
        /// <param name="mauiView">The Avalonia view to host within the .NET MAUI application.</param>
        public MauiAvaloniaView(AvaloniaView mauiView)
        {
            _mauiView = mauiView;
            // Embedded views are not at the window edges, so skip the safe area padding.
            Avalonia.Controls.TopLevel.SetAutoSafeAreaPadding(_backgroundBorder, false);
            Content = _backgroundBorder;
            _popupHost = EmbeddedPopupHost.Attach(this, _backgroundBorder);
            AvaloniaPanelFocus.Attach(this);

            // The panel creates its top level when it is loaded, during a WinUI layout pass after MAUI first
            // measured the content. The content gets its template and styles then, so it is measured again after.
            _backgroundBorder.AttachedToVisualTree += (_, _) =>
                DispatcherQueue.TryEnqueue(InvalidateMauiMeasure);
        }

        private void InvalidateMauiMeasure()
        {
            ((Microsoft.Maui.IView)_mauiView).InvalidateMeasure();

            // The MAUI layouts above the panel ask the handler for the size of the content. The desired size of the
            // panel itself does not depend on its content, so they are invalidated directly.
            Microsoft.UI.Xaml.DependencyObject? element = this;
            while (element is not null)
            {
                if (element is Microsoft.UI.Xaml.UIElement uiElement)
                    uiElement.InvalidateMeasure();
                element = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(element);
            }
        }

        /// <summary>
        /// Updates the content of the Avalonia view. This method sets the hosted content to the
        /// content of the AvaloniaView, allowing the Avalonia content to be displayed within the
        /// WinUI visual tree.
        /// </summary>
        public void UpdateContent()
        {
            _backgroundBorder.Child = _mauiView.Content as Avalonia.Controls.Control;
        }

        /// <summary>
        /// Detaches the hosted Avalonia content so it can be re-parented elsewhere.
        /// </summary>
        public void ClearContent()
        {
            _backgroundBorder.Child = null;
        }

        /// <summary>
        /// Updates the background of the Avalonia content area.
        /// </summary>
        /// <param name="brush">The Avalonia brush to use as the background, or <see langword="null"/> to use the default theme background.</param>
        public void UpdateBackground(Avalonia.Media.IBrush? brush)
        {
            _backgroundBorder.Background = brush;
        }
    }
}
