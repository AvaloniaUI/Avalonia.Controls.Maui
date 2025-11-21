using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ControlGallery.Pages;

public partial class StepperPage : ContentPage, INotifyPropertyChanged
{
    private double _basicValue = 5;
    private double _customIncrementValue = 50;
    private double _decimalValue = 2.5;
    private double _temperature = 22.0;
    private double _volume = 50;
    private double _styledValue = 25;
    private double _quantity = 1;
    private const double ItemPrice = 19.99;

    public StepperPage()
    {
        InitializeComponent();

        ResetCommand = new Command(() => BasicValue = 5);
        SetMinCommand = new Command(() => BasicValue = 0);
        SetMidCommand = new Command(() => BasicValue = 5);
        SetMaxCommand = new Command(() => BasicValue = 10);

        BindingContext = this;
    }

    public double BasicValue
    {
        get => _basicValue;
        set
        {
            if (_basicValue != value)
            {
                _basicValue = value;
                OnPropertyChanged();
            }
        }
    }

    public double CustomIncrementValue
    {
        get => _customIncrementValue;
        set
        {
            if (_customIncrementValue != value)
            {
                _customIncrementValue = value;
                OnPropertyChanged();
            }
        }
    }

    public double DecimalValue
    {
        get => _decimalValue;
        set
        {
            if (_decimalValue != value)
            {
                _decimalValue = value;
                OnPropertyChanged();
            }
        }
    }

    public double Temperature
    {
        get => _temperature;
        set
        {
            if (_temperature != value)
            {
                _temperature = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TemperatureColor));
                OnPropertyChanged(nameof(TemperatureDescription));
            }
        }
    }

    public Color TemperatureColor
    {
        get
        {
            return Temperature switch
            {
                < 18 => Colors.Blue,
                >= 18 and < 22 => Colors.LightBlue,
                >= 22 and < 26 => Colors.Orange,
                _ => Colors.Red
            };
        }
    }

    public string TemperatureDescription
    {
        get
        {
            return Temperature switch
            {
                < 18 => "Cold",
                >= 18 and < 22 => "Cool",
                >= 22 and < 26 => "Warm",
                _ => "Hot"
            };
        }
    }

    public double Volume
    {
        get => _volume;
        set
        {
            if (_volume != value)
            {
                _volume = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(VolumeProgress));
                OnPropertyChanged(nameof(VolumeDisplay));
            }
        }
    }

    public double VolumeProgress => Volume / 100.0;

    public string VolumeDisplay
    {
        get
        {
            string bar = new string('▮', (int)(Volume / 10));
            string empty = new string('▯', 10 - (int)(Volume / 10));
            return $"Volume: {Volume:F0}% [{bar}{empty}]";
        }
    }

    public double StyledValue
    {
        get => _styledValue;
        set
        {
            if (_styledValue != value)
            {
                _styledValue = value;
                OnPropertyChanged();
            }
        }
    }

    public double Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
    }

    public double TotalPrice => Quantity * ItemPrice;

    public ICommand ResetCommand { get; }
    public ICommand SetMinCommand { get; }
    public ICommand SetMidCommand { get; }
    public ICommand SetMaxCommand { get; }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
