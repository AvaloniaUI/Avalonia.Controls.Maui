using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ControlGallery.Pages;

public partial class BoxViewPage : ContentPage, INotifyPropertyChanged
{
    private Color _selectedColor = Colors.Red;

    public BoxViewPage()
    {
        InitializeComponent();

        SetColorCommand = new Command<string>(OnSetColor);

        BindingContext = this;
    }

    public Color SelectedColor
    {
        get => _selectedColor;
        set
        {
            if (_selectedColor != value)
            {
                _selectedColor = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand SetColorCommand { get; }

    private void OnSetColor(string colorName)
    {
        SelectedColor = colorName switch
        {
            "Red" => Colors.Red,
            "Green" => Colors.Green,
            "Blue" => Colors.Blue,
            "Purple" => Colors.Purple,
            "Orange" => Colors.Orange,
            "Yellow" => Colors.Yellow,
            _ => Colors.Red
        };
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
