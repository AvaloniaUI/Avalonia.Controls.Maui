using Avalonia.Controls.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace Avalonia.Controls.Maui.Handlers
{
    /// <summary>
    /// A handler that connects the AvaloniaView control to its platform-specific implementations, allowing Avalonia content to be hosted within a .NET MAUI application across multiple platforms.
    /// </summary>
    public partial class NativeAvaloniaViewHandler
    {
        /// <summary>
        /// Defines the property mapper that maps properties from the AvaloniaView to the platform-specific views. This mapper is used by the handler to update the platform view whenever a property on the AvaloniaView changes. In this case, it maps the Content property to the MapContent method, which updates the content displayed in the platform-specific view accordingly.
        /// </summary>
        public static IPropertyMapper<AvaloniaView, NativeAvaloniaViewHandler> PropertyMapper = new PropertyMapper<AvaloniaView, NativeAvaloniaViewHandler>(Microsoft.Maui.Handlers.ViewHandler.ViewMapper)
        {
            [nameof(AvaloniaView.Content)] = MapContent,
            [nameof(IView.Background)] = MapBackground
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeAvaloniaViewHandler"/> class. This constructor calls the base constructor of the ViewHandler class, passing in the PropertyMapper defined above, which sets up the property mappings for this handler.
        /// </summary>
        public NativeAvaloniaViewHandler() : base(PropertyMapper)
        {
        }
    }
}