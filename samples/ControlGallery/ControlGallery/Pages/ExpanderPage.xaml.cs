using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui.Core;

namespace ControlGallery.Pages;

// Expander binds to its own IsExpanded property by name, which TrimmerRoots.xml keeps
[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The property Expander binds to is kept by TrimmerRoots.xml")]
public partial class ExpanderPage : ContentPage
{
    public ExpanderPage()
    {
        InitializeComponent();
    }

    private void OnBorderExpanderExpandedChanged(object? sender, ExpandedChangedEventArgs e)
    {
        BorderExpanderIndicator.Text = e.IsExpanded ? "Hide" : "Show";
    }

    private void OnExpandClicked(object? sender, EventArgs e)
    {
        ProgrammaticExpander.IsExpanded = true;
    }

    private void OnCollapseClicked(object? sender, EventArgs e)
    {
        ProgrammaticExpander.IsExpanded = false;
    }
}
