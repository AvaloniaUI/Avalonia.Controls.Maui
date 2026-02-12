using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ControlGallery.Pages;

public partial class SliderPage : ContentPage, INotifyPropertyChanged
{
    private double _sliderValue = 50;
    private double _customRangeValue = 5;
    private double _minimumTrackSliderValue = 75;
    private double _maximumTrackSliderValue = 60;
    private double _bothTrackColorsValue = 40;
    private double _thumbColorValue = 65;
    private double _allColorsValue = 55;
    private double _interactiveValue = 50;
    private Color _selectedMinimumTrackColor = Colors.Red;
    private Color _selectedMaximumTrackColor = Colors.LightGray;
    private string _lastEventText = string.Empty;

    public SliderPage()
    {
        InitializeComponent();

        SetMinimumTrackColorCommand = new Command<string>(colorName =>
        {
            SelectedMinimumTrackColor = colorName switch
            {
                "Red" => Colors.Red,
                "Green" => Colors.Green,
                "Blue" => Colors.Blue,
                _ => Colors.Red
            };
        });

        ResetValueCommand = new Command(() => InteractiveValue = 50);

        BindingContext = this;
    }

    public double SliderValue
    {
        get => _sliderValue;
        set
        {
            if (Math.Abs(_sliderValue - value) > 0.001)
            {
                _sliderValue = value;
                OnPropertyChanged();
            }
        }
    }

    public double CustomRangeValue
    {
        get => _customRangeValue;
        set
        {
            if (Math.Abs(_customRangeValue - value) > 0.001)
            {
                _customRangeValue = value;
                OnPropertyChanged();
            }
        }
    }

    public double MinimumTrackSliderValue
    {
        get => _minimumTrackSliderValue;
        set
        {
            if (Math.Abs(_minimumTrackSliderValue - value) > 0.001)
            {
                _minimumTrackSliderValue = value;
                OnPropertyChanged();
            }
        }
    }

    public double MaximumTrackSliderValue
    {
        get => _maximumTrackSliderValue;
        set
        {
            if (Math.Abs(_maximumTrackSliderValue - value) > 0.001)
            {
                _maximumTrackSliderValue = value;
                OnPropertyChanged();
            }
        }
    }

    public double BothTrackColorsValue
    {
        get => _bothTrackColorsValue;
        set
        {
            if (Math.Abs(_bothTrackColorsValue - value) > 0.001)
            {
                _bothTrackColorsValue = value;
                OnPropertyChanged();
            }
        }
    }

    public double ThumbColorValue
    {
        get => _thumbColorValue;
        set
        {
            if (Math.Abs(_thumbColorValue - value) > 0.001)
            {
                _thumbColorValue = value;
                OnPropertyChanged();
            }
        }
    }

    public double AllColorsValue
    {
        get => _allColorsValue;
        set
        {
            if (Math.Abs(_allColorsValue - value) > 0.001)
            {
                _allColorsValue = value;
                OnPropertyChanged();
            }
        }
    }

    public double InteractiveValue
    {
        get => _interactiveValue;
        set
        {
            if (Math.Abs(_interactiveValue - value) > 0.001)
            {
                _interactiveValue = value;
                OnPropertyChanged();
            }
        }
    }

    public Color SelectedMinimumTrackColor
    {
        get => _selectedMinimumTrackColor;
        set
        {
            if (_selectedMinimumTrackColor != value)
            {
                _selectedMinimumTrackColor = value;
                OnPropertyChanged();
            }
        }
    }

    public Color SelectedMaximumTrackColor
    {
        get => _selectedMaximumTrackColor;
        set
        {
            if (_selectedMaximumTrackColor != value)
            {
                _selectedMaximumTrackColor = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SetMinimumTrackColorCommand { get; }
    public ICommand ResetValueCommand { get; }

    public string LastEventText
    {
        get => _lastEventText;
        set
        {
            if (_lastEventText != value)
            {
                _lastEventText = value;
                OnPropertyChanged();
            }
        }
    }

    void OnColorVariationValueChanged(object? sender, ValueChangedEventArgs e)
    {
        if (sender is Slider slider)
            LastEventText = $"ValueChanged: {slider.MinimumTrackColor} -> {e.NewValue:F2}";
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
