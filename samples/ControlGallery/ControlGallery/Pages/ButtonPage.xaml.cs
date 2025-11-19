using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ControlGallery.Pages;

public partial class ButtonPage : ContentPage, INotifyPropertyChanged
{
    private int _clickCount = 0;
    private Color _buttonColor = Colors.Blue;
    private int _buttonCornerRadius = 8;
    private int _interactiveClickCount = 0;

    public ButtonPage()
    {
        InitializeComponent();
        BindingContext = this;
        
        SetButtonColorCommand = new Command<string>(OnSetButtonColor);
        SetCornerRadiusCommand = new Command<string>(OnSetCornerRadius);
    }

    public Color ButtonColor
    {
        get => _buttonColor;
        set
        {
            if (_buttonColor != value)
            {
                _buttonColor = value;
                OnPropertyChanged();
            }
        }
    }

    public int ButtonCornerRadius
    {
        get => _buttonCornerRadius;
        set
        {
            if (_buttonCornerRadius != value)
            {
                _buttonCornerRadius = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SetButtonColorCommand { get; }
    public ICommand SetCornerRadiusCommand { get; }

    private void OnButtonClicked(object? sender, EventArgs e)
    {
        _clickCount++;
        ClickCountLabel.Text = $"Click count: {_clickCount}";
    }

    private void OnInteractiveButtonClicked(object? sender, EventArgs e)
    {
        _interactiveClickCount++;
        InteractionLabel.Text = $"Button clicked {_interactiveClickCount} time{(_interactiveClickCount == 1 ? "" : "s")}!";
        InteractionLabel.TextColor = Colors.Green;
    }

    private void OnSetButtonColor(string colorName)
    {
        ButtonColor = colorName switch
        {
            "Blue" => Colors.Blue,
            "Green" => Colors.Green,
            "Red" => Colors.Red,
            "Orange" => Colors.Orange,
            _ => Colors.Blue
        };
    }

    private void OnSetCornerRadius(string radiusValue)
    {
        if (int.TryParse(radiusValue, out int radius))
        {
            ButtonCornerRadius = radius;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}