namespace Avalonia.Controls.Maui.Handlers
{
    /// <summary>
    /// Shared interface implemented by platform-specific host views that render an Avalonia control
    /// inside a MAUI handler. Enables the generic <see cref="AvaloniaControlHandler{TVirtualView,TControl}"/>
    /// to manipulate the hosted control and its background without platform-specific code.
    /// </summary>
    public interface IAvaloniaControlHost
    {
        /// <summary>
        /// Sets the Avalonia control to be hosted in this view.
        /// </summary>
        /// <param name="control">The Avalonia control to display, or <see langword="null"/> to clear the content.</param>
        void SetControl(Avalonia.Controls.Control? control);

        /// <summary>
        /// Updates the background brush of the hosted content area.
        /// </summary>
        /// <param name="brush">The Avalonia brush to use as the background, or <see langword="null"/> to clear it.</param>
        void UpdateBackground(Avalonia.Media.IBrush? brush);
    }
}
