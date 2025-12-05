using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ControlGallery.Pages;

public partial class FramePage : ContentPage, INotifyPropertyChanged
{
    private static AppTheme CurrentTheme => Application.Current?.RequestedTheme ?? AppTheme.Light;
    private static Color DefaultFrameBorderColor => CurrentTheme == AppTheme.Dark ? Colors.LightGray : Colors.Navy;
    private static Color DefaultFrameBackgroundColor => CurrentTheme == AppTheme.Dark ? Color.FromArgb("#1E1E1E") : Colors.White;

    private Color _frameBorderColor = DefaultFrameBorderColor;
    private float _frameCornerRadius = 10;
    private bool _frameHasShadow = true;
    private Color _frameBackgroundColor = DefaultFrameBackgroundColor;

    public FramePage()
    {
        InitializeComponent();

        SetBorderColorCommand = new Command<string>(OnSetBorderColor);
        SetBackgroundColorCommand = new Command<string>(OnSetBackgroundColor);

        BindingContext = this;
    }

    public Color FrameBorderColor
    {
        get => _frameBorderColor;
        set
        {
            if (_frameBorderColor != value)
            {
                _frameBorderColor = value;
                OnPropertyChanged();
            }
        }
    }

    public float FrameCornerRadius
    {
        get => _frameCornerRadius;
        set
        {
            if (_frameCornerRadius != value)
            {
                _frameCornerRadius = value;
                OnPropertyChanged();
            }
        }
    }

    public bool FrameHasShadow
    {
        get => _frameHasShadow;
        set
        {
            if (_frameHasShadow != value)
            {
                _frameHasShadow = value;
                OnPropertyChanged();
            }
        }
    }

    public Color FrameBackgroundColor
    {
        get => _frameBackgroundColor;
        set
        {
            if (_frameBackgroundColor != value)
            {
                _frameBackgroundColor = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SetBorderColorCommand { get; }
    public ICommand SetBackgroundColorCommand { get; }

    private void OnSetBorderColor(string colorName)
    {
        FrameBorderColor = colorName switch
        {
            "Red" => Colors.Red,
            "Blue" => Colors.Blue,
            "Green" => Colors.Green,
            "Purple" => Colors.Purple,
            "Orange" => Colors.Orange,
            _ => DefaultFrameBorderColor
        };
    }

    private void OnSetBackgroundColor(string colorName)
    {
        FrameBackgroundColor = colorName switch
        {
            "White" => Colors.White,
            "LightBlue" => Colors.LightBlue,
            "LightGreen" => Colors.LightGreen,
            "LightPink" => Colors.LightPink,
            "LightYellow" => Colors.LightYellow,
            "LightGray" => Colors.LightGray,
            _ => DefaultFrameBackgroundColor
        };
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
