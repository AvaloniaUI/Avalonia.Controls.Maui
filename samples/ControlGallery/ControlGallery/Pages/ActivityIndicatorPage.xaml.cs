using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ControlGallery.Pages;

public partial class ActivityIndicatorPage : ContentPage, INotifyPropertyChanged
{
    private bool _isRunning = true;
    private Color _indicatorColor = Colors.Blue;

    public ActivityIndicatorPage()
    {
        InitializeComponent();

        StartCommand = new Command(() => IsRunning = true);
        StopCommand = new Command(() => IsRunning = false);
        SetColorCommand = new Command<string>(colorName =>
        {
            IndicatorColor = colorName switch
            {
                "Red" => Colors.Red,
                "Green" => Colors.Green,
                "Blue" => Colors.Blue,
                "Orange" => Colors.Orange,
                _ => Colors.Blue
            };
        });

        BindingContext = this;
    }

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (_isRunning != value)
            {
                _isRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    public Color IndicatorColor
    {
        get => _indicatorColor;
        set
        {
            if (_indicatorColor != value)
            {
                _indicatorColor = value;
                OnPropertyChanged();
            }
        }
    }

    public string StatusText => IsRunning ? "Running" : "Stopped";

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand SetColorCommand { get; }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
