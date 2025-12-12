using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace Avalonia.Controls.Maui;

/// <summary>
/// A ComboBox control with Header support for MAUI compatibility.
/// Provides Header and HeaderTemplate properties to display a label/title above the ComboBox.
/// </summary>
public partial class MauiComboBox : ComboBox
{
    /// <summary>
    /// Defines the <see cref="Header"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<MauiComboBox, object?>(nameof(Header));

    /// <summary>
    /// Defines the <see cref="HeaderTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<MauiComboBox, IDataTemplate?>(nameof(HeaderTemplate));

    /// <summary>
    /// Gets or sets the header content.
    /// This is typically used to display a label or title for the ComboBox.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template used to display the header.
    /// </summary>
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public MauiComboBox()
    {
        InitializeComponent();
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
    }
}
