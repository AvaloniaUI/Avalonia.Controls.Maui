using AvaloniaControl = Avalonia.Controls.Control;

namespace Avalonia.Controls.Maui.Controls;

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