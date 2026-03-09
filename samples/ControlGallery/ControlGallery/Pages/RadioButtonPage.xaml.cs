using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace ControlGallery.Pages;

public partial class RadioButtonPage : ContentPage, INotifyPropertyChanged
{
    private bool _isOptionAChecked = true;
    private string _selectedSize = "Small";
    private bool _isStateRadioEnabled = true;
    private bool _isStateRadioChecked;

    public RadioButtonPage()
    {
        InitializeComponent();

        ToggleCheckedCommand = new Command(() => IsStateRadioChecked = !IsStateRadioChecked);
        ToggleEnabledCommand = new Command(() => IsStateRadioEnabled = !IsStateRadioEnabled);

        TemplateOption1 = new OptionModel("First", Colors.CornflowerBlue);
        TemplateOption2 = new OptionModel("Second", Colors.OrangeRed);
        TemplateOption3 = new OptionModel("Third", Colors.MediumSeaGreen);

        BindingContext = this;
    }

    public bool IsOptionAChecked
    {
        get => _isOptionAChecked;
        set
        {
            if (_isOptionAChecked != value)
            {
                _isOptionAChecked = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BasicSelectedText));
            }
        }
    }

    public string BasicSelectedText => IsOptionAChecked ? "Selected: Option A" : "Selected: Option B/C";

    public string SelectedSize
    {
        get => _selectedSize;
        set
        {
            if (_selectedSize != value)
            {
                _selectedSize = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsStateRadioEnabled
    {
        get => _isStateRadioEnabled;
        set
        {
            if (_isStateRadioEnabled != value)
            {
                _isStateRadioEnabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StateRadioStatus));
            }
        }
    }

    public bool IsStateRadioChecked
    {
        get => _isStateRadioChecked;
        set
        {
            if (_isStateRadioChecked != value)
            {
                _isStateRadioChecked = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StateRadioStatus));
            }
        }
    }

    public string StateRadioStatus =>
        $"Checked: {IsStateRadioChecked}, Enabled: {IsStateRadioEnabled}";

    public OptionModel TemplateOption1 { get; }
    public OptionModel TemplateOption2 { get; }
    public OptionModel TemplateOption3 { get; }

    public ICommand ToggleCheckedCommand { get; }
    public ICommand ToggleEnabledCommand { get; }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public record OptionModel(string Text, Color Color);
}