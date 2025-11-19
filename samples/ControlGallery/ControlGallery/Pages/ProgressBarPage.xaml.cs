using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ControlGallery.Pages;

public partial class ProgressBarPage : ContentPage, INotifyPropertyChanged
{
    private double _progress = 0.5;
    private Color _progressColor = Colors.Blue;

    public ProgressBarPage()
    {
        InitializeComponent();

        SetProgressCommand = new Command<string>(percent =>
        {
            if (int.TryParse(percent, out int value))
            {
                ProgressPercent = value;
            }
        });

        SetColorCommand = new Command<string>(colorName =>
        {
            ProgressColor = colorName switch
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

    public double Progress
    {
        get => _progress;
        set
        {
            if (_progress != value)
            {
                _progress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressPercent));
            }
        }
    }

    public double ProgressPercent
    {
        get => _progress * 100;
        set
        {
            var newProgress = value / 100.0;
            if (_progress != newProgress)
            {
                _progress = newProgress;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Progress));
            }
        }
    }

    public Color ProgressColor
    {
        get => _progressColor;
        set
        {
            if (_progressColor != value)
            {
                _progressColor = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SetProgressCommand { get; }
    public ICommand SetColorCommand { get; }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
