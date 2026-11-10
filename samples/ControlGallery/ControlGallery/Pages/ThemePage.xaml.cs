using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace ControlGallery.Pages;

public partial class ThemePage : ContentPage
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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Application.Current is { } app)
            app.RequestedThemeChanged += OnRequestedThemeChanged;

        UpdateCurrentTheme();
    }

    protected override void OnDisappearing()
    {
        if (Application.Current is { } app)
            app.RequestedThemeChanged -= OnRequestedThemeChanged;

        base.OnDisappearing();
    }

    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        UpdateCurrentTheme();
    }

    private void OnToggleThemeClicked(object? sender, EventArgs e)
    {
        var app = Application.Current;
        if (app == null)
            return;

        // Get the current effective theme (considering both UserAppTheme and RequestedTheme)
        var currentTheme = app.UserAppTheme != AppTheme.Unspecified
            ? app.UserAppTheme
            : app.RequestedTheme;

        // Toggle between Light and Dark themes using MAUI's UserAppTheme
        app.UserAppTheme = currentTheme == AppTheme.Light
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


}
