using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Windows.Input;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

public class RadioButtonStub : StubBase, IRadioButton
{
    private bool _isChecked;
    private object? _content;
    private IView? _presentedContent;
    private Color _strokeColor = Colors.Transparent;
    private double _strokeThickness;
    private int _cornerRadius;
    private string _groupName = string.Empty;
    private double _characterSpacing;
    private Microsoft.Maui.Font _font = Microsoft.Maui.Font.Default;
    private Color _textColor = Colors.Black;
    private object? _value;
    private DataTemplate? _contentTemplate;
    private TextTransform _textTransform = TextTransform.Default;
    private ICommand? _command;
    private object? _commandParameter;
    private Microsoft.Maui.Thickness _padding;

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (SetProperty(ref _isChecked, value))
                CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(value));
        }
    }

    public object? Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    public IView? PresentedContent
    {
        get => _presentedContent;
        set => SetProperty(ref _presentedContent, value);
    }

    public Color StrokeColor
    {
        get => _strokeColor;
        set => SetProperty(ref _strokeColor, value);
    }

    public double StrokeThickness
    {
        get => _strokeThickness;
        set => SetProperty(ref _strokeThickness, value);
    }

    public int CornerRadius
    {
        get => _cornerRadius;
        set => SetProperty(ref _cornerRadius, value);
    }

    public string GroupName
    {
        get => _groupName;
        set => SetProperty(ref _groupName, value);
    }

    public double CharacterSpacing
    {
        get => _characterSpacing;
        set => SetProperty(ref _characterSpacing, value);
    }

    public Microsoft.Maui.Font Font
    {
        get => _font;
        set => SetProperty(ref _font, value);
    }

    public Color TextColor
    {
        get => _textColor;
        set => SetProperty(ref _textColor, value);
    }

    public Microsoft.Maui.Thickness Padding
    {
        get => _padding;
        set => SetProperty(ref _padding, value);
    }
    
    // kept here to support handler tests.
    public object? Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public DataTemplate? ContentTemplate
    {
        get => _contentTemplate;
        set => SetProperty(ref _contentTemplate, value);
    }

    public TextTransform TextTransform
    {
        get => _textTransform;
        set => SetProperty(ref _textTransform, value);
    }

    public ICommand? Command
    {
        get => _command;
        set => SetProperty(ref _command, value);
    }

    public object? CommandParameter
    {
        get => _commandParameter;
        set => SetProperty(ref _commandParameter, value);
    }

    // IContentView layout methods
    public MauiGraphics.Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
    {
        return new MauiGraphics.Size(100, 30); // Default stub size
    }

    public MauiGraphics.Size CrossPlatformArrange(MauiGraphics.Rect bounds)
    {
        return bounds.Size;
    }

    public event EventHandler<CheckedChangedEventArgs>? CheckedChanged;
}
