using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;

namespace Avalonia.Controls.Maui.Tests.Controls;

public class MauiComboBoxTests
{
    [AvaloniaFact(DisplayName = "MauiComboBox Has Header Property")]
    public void MauiComboBoxHasHeaderProperty()
    {
        var comboBox = new MauiComboBox();
        Assert.NotNull(MauiComboBox.HeaderProperty);
        Assert.Null(comboBox.Header);
    }

    [AvaloniaFact(DisplayName = "MauiComboBox Has HeaderTemplate Property")]
    public void MauiComboBoxHasHeaderTemplateProperty()
    {
        var comboBox = new MauiComboBox();
        Assert.NotNull(MauiComboBox.HeaderTemplateProperty);
        Assert.Null(comboBox.HeaderTemplate);
    }

    [AvaloniaFact(DisplayName = "MauiComboBox Can Set Header As String")]
    public void MauiComboBoxCanSetHeaderAsString()
    {
        var comboBox = new MauiComboBox
        {
            Header = "Test Header"
        };

        Assert.Equal("Test Header", comboBox.Header);
    }

    [AvaloniaFact(DisplayName = "MauiComboBox Can Set HeaderTemplate")]
    public void MauiComboBoxCanSetHeaderTemplate()
    {
        var comboBox = new MauiComboBox();
        var template = new FuncDataTemplate<object?>((data, _) =>
        {
            return new TextBlock { Text = "Custom Header" };
        });

        comboBox.HeaderTemplate = template;

        Assert.Equal(template, comboBox.HeaderTemplate);
    }

    [AvaloniaFact(DisplayName = "MauiComboBox Inherits From ComboBox")]
    public void MauiComboBoxInheritsFromComboBox()
    {
        var comboBox = new MauiComboBox();
        Assert.IsAssignableFrom<ComboBox>(comboBox);
    }

    [AvaloniaFact(DisplayName = "MauiComboBox Supports Standard ComboBox Properties")]
    public void MauiComboBoxSupportsStandardComboBoxProperties()
    {
        var comboBox = new MauiComboBox
        {
            SelectedIndex = 1,
            PlaceholderText = "Select an item"
        };

        Assert.Equal(1, comboBox.SelectedIndex);
        Assert.Equal("Select an item", comboBox.PlaceholderText);
    }
}