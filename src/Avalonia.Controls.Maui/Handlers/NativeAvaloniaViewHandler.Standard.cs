
#if !ANDROID && !IOS && !WINDOWS && !MACOS && !MACCATALYST
using Avalonia.Controls;
using Avalonia.Controls.Maui.Controls;
using AvaloniaControl = Avalonia.Controls.Control;

namespace Avalonia.Controls.Maui.Handlers
{
    /// <summary>
    /// A handler that connects the AvaloniaView control to its platform-specific implementations, allowing Avalonia content to be hosted within a .NET MAUI application across multiple platforms.
    /// </summary>
    public partial class NativeAvaloniaViewHandler : Microsoft.Maui.Handlers.ViewHandler<AvaloniaView, Avalonia.Controls.Maui.Platforms.Standard.MauiAvaloniaView>
    {
        /// <summary>
        /// Creates the platform-specific view for platforms that do not have specific implementations (like Android or iOS). This method is called when the handler is initialized and is responsible for creating an instance of the MauiAvaloniaView, which serves as a fallback for hosting Avalonia content in a cross-platform manner, allowing developers to use the AvaloniaView control in their .NET MAUI applications regardless of the target platform. It is designed to work with the AvaloniaViewHandler to ensure that the content from the AvaloniaView is properly displayed and updated within the MauiAvaloniaView.
        /// </summary>
        /// <returns>The platform-specific view for standard platforms.</returns>
        protected override Avalonia.Controls.Maui.Platforms.Standard.MauiAvaloniaView CreatePlatformView()
        {
            return new Avalonia.Controls.Maui.Platforms.Standard.MauiAvaloniaView(VirtualView);
        }

        /// <summary>
        /// Connects the handler to the platform-specific view. This method is called after the platform view has been created and is responsible for setting up any necessary event handlers or bindings between the AvaloniaView and the MauiAvaloniaView.
        /// </summary>
        /// <param name="platformView">The platform-specific view for standard platforms.</param>
        protected override void ConnectHandler(Avalonia.Controls.Maui.Platforms.Standard.MauiAvaloniaView platformView)
        {
            base.ConnectHandler(platformView);

            platformView.Content = VirtualView.Content as AvaloniaControl;
        }
        
        /// <summary>
        /// Disconnects the handler from the platform-specific view. This method is called when the handler is being disposed and is responsible for cleaning up any event handlers or bindings that were set up in the ConnectHandler method, as well as disposing of the platform view if necessary.
        /// </summary>
        /// <param name="handler">The handler that manages the connection between the AvaloniaView and the MauiAvaloniaView.</param>
        /// <param name="view">The AvaloniaView instance whose content is being mapped.</param>
        public static void MapContent(NativeAvaloniaViewHandler handler, AvaloniaView view)
        {
            handler.PlatformView?.UpdateContent();
        }
    }
}


namespace Avalonia.Controls.Maui.Platforms.Standard
{
    /// <summary>
    /// A view that can host Avalonia content within a .NET MAUI application on platforms that do not have specific implementations (like Android or iOS). This view serves as a fallback for hosting Avalonia content in a cross-platform manner, allowing developers to use the AvaloniaView control in their .NET MAUI applications regardless of the target platform. It is designed to work with the AvaloniaViewHandler to ensure that the content from the AvaloniaView is properly displayed and updated within the MauiAvaloniaView.
    /// </summary>
    public class MauiAvaloniaView : ContentControl
    {
        readonly AvaloniaView _mauiView;

        /// <summary>
        /// Initializes a new instance of the <see cref="MauiAvaloniaView"/> class. This constructor takes an instance of the AvaloniaView from the .NET MAUI application and stores it in a private field. The MauiAvaloniaView will use this reference to access the content of the AvaloniaView and update its own content accordingly when changes occur.
        /// </summary>
        /// <param name="mauiView"></param>
        public MauiAvaloniaView(AvaloniaView mauiView)
        {
            _mauiView = mauiView;
        }

        /// <summary>
        /// Updates the content of the MauiAvaloniaView based on the content of the AvaloniaView. This method is called whenever the Content property of the AvaloniaView changes, and it is responsible for ensuring that the content displayed in the MauiAvaloniaView is always in sync with the content of the AvaloniaView. It casts the content of the AvaloniaView to an AvaloniaControl and sets it as the content of the MauiAvaloniaView, allowing the Avalonia content to be rendered correctly within the .NET MAUI application.
        /// </summary>
        public void UpdateContent()
        {
            Content = _mauiView.Content as AvaloniaControl;
        }
    }
}
#endif