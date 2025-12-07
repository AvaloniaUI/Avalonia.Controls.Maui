using System.Windows.Input;
using Microsoft.Maui;
using MauiGraphics = Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

/// <summary>
/// Stub implementation of IImageButton for testing ImageButtonHandler.
/// </summary>
public class ImageButtonStub : StubBase, IImageButton, IImage, IButtonStroke, IImageSourcePart, IPadding
{
    private IImageSource? _source;
    private Aspect _aspect = Aspect.AspectFit;
    private bool _isOpaque;
    private Microsoft.Maui.Thickness _padding = Microsoft.Maui.Thickness.Zero;
    private MauiGraphics.Color _strokeColor = MauiGraphics.Colors.Transparent;
    private double _strokeThickness;
    private int _cornerRadius;
    private MauiGraphics.Paint? _background;
    private ICommand? _command;
    private object? _commandParameter;

    // Event counters for testing
    public int ClickedCount { get; private set; }
    public int PressedCount { get; private set; }
    public int ReleasedCount { get; private set; }
    public int IsLoadingUpdatedCount { get; private set; }
    public bool IsLoading { get; private set; }
    public int CommandExecutedCount { get; private set; }
    public object? LastCommandParameter { get; private set; }

    public IImageSource? Source
    {
        get => _source;
        set => SetProperty(ref _source, value);
    }

    public Aspect Aspect
    {
        get => _aspect;
        set => SetProperty(ref _aspect, value);
    }

    public bool IsOpaque
    {
        get => _isOpaque;
        set => SetProperty(ref _isOpaque, value);
    }

    public Microsoft.Maui.Thickness Padding
    {
        get => _padding;
        set => SetProperty(ref _padding, value);
    }

    public MauiGraphics.Color StrokeColor
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

    public new MauiGraphics.Paint? Background
    {
        get => _background;
        set => SetProperty(ref _background, value);
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

    public void Clicked()
    {
        ClickedCount++;

        // Execute command like MAUI's ButtonElement.ElementClicked does
        if (Command?.CanExecute(CommandParameter) == true)
        {
            Command.Execute(CommandParameter);
            CommandExecutedCount++;
            LastCommandParameter = CommandParameter;
        }
    }

    public void Pressed()
    {
        PressedCount++;
    }

    public void Released()
    {
        ReleasedCount++;
    }

    public void UpdateIsLoading(bool isLoading)
    {
        IsLoading = isLoading;
        IsLoadingUpdatedCount++;
    }

    bool IImageSourcePart.IsAnimationPlaying => false;
}
