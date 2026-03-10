using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace ControlGallery.Pages;

public class CarouselItem
{
    public string Title { get; set; } = string.Empty;
    public Color Color { get; set; } = Colors.Transparent;
}

public partial class IndicatorViewPage : ContentPage, INotifyPropertyChanged
{
    private int _position;
    private const int PageCount = 5;

    public IndicatorViewPage()
    {
        InitializeComponent();

        Items = new ObservableCollection<CarouselItem>
        {
            new CarouselItem { Title = "Item 1", Color = Color.FromArgb("#E91E63") },
            new CarouselItem { Title = "Item 2", Color = Color.FromArgb("#2196F3") },
            new CarouselItem { Title = "Item 3", Color = Color.FromArgb("#4CAF50") },
            new CarouselItem { Title = "Item 4", Color = Color.FromArgb("#FF9800") },
            new CarouselItem { Title = "Item 5", Color = Color.FromArgb("#9C27B0") },
        };

        PreviousCommand = new Command(
            () => Position = Math.Max(0, Position - 1),
            () => Position > 0);

        NextCommand = new Command(
            () => Position = Math.Min(Count - 1, Position + 1),
            () => Position < Count - 1);

        ResetCommand = new Command(() => Position = 0);

        numberedIndicator.IndicatorTemplate = BuildNumberedTemplate();

        BindingContext = this;
    }

    public ObservableCollection<CarouselItem> Items { get; }

    public int Count => PageCount;

    public int Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PositionText));
                ((Command)PreviousCommand).ChangeCanExecute();
                ((Command)NextCommand).ChangeCanExecute();
            }
        }
    }

    public string PositionText => $"Page {Position + 1} of {Count}";

    public ICommand PreviousCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand ResetCommand { get; }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static DataTemplate BuildNumberedTemplate()
    {
        return new DataTemplate(() =>
        {
            var border = new Border
            {
                WidthRequest = 32,
                HeightRequest = 32,
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                BackgroundColor = Color.FromArgb("#6200EE"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };
            var label = new Label
            {
                TextColor = Colors.White,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };
            label.SetBinding(Label.TextProperty, ".");
            border.Content = label;
            return border;
        });
    }
}
