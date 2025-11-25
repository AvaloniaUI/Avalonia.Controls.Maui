using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace ControlGallery.Pages;

public partial class ThemePage : ContentPage, INotifyPropertyChanged
{
    private string _currentTheme = "Unknown";

    public ThemePage()
    {
        InitializeComponent();
        BindingContext = this;
        UpdateCurrentTheme();
    }

    public string CurrentTheme
    {
        get => _currentTheme;
        set
        {
            if (_currentTheme != value)
            {
                _currentTheme = value;
                OnPropertyChanged();
            }
        }
    }

    private void OnToggleThemeClicked(object? sender, EventArgs e)
    {
        var app = Application.Current;
        if (app == null)
            return;

        // Toggle between Light and Dark themes using MAUI's UserAppTheme
        app.UserAppTheme = app.UserAppTheme == AppTheme.Light
            ? AppTheme.Dark
            : AppTheme.Light;

        UpdateCurrentTheme();
    }

    private void UpdateCurrentTheme()
    {
        var app = Application.Current;
        if (app == null)
        {
            CurrentTheme = "Unknown";
            return;
        }

        CurrentTheme = app.UserAppTheme switch
        {
            AppTheme.Light => "Light",
            AppTheme.Dark => "Dark",
            _ => AppInfo.Current.RequestedTheme.ToString()
        };
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
