using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ControlGallery.Pages;

public class CarouselItem
{
    public string Text { get; init; } = string.Empty;
    public Color Color { get; init; }
}

public partial class CarouselPage : ContentPage
{
    public IList<CarouselItem> Items { get; } = new List<CarouselItem>();

    private int _horizontalPosition;
    private int _verticalPosition;
    private bool _horizontalLoopEnabled = true;
    private bool _horizontalSwipeEnabled = true;
    private bool _verticalLoopEnabled = true;
    private bool _verticalSwipeEnabled = true;

    public int HorizontalPosition
    {
        get => _horizontalPosition;
        set
        {
            if (_horizontalPosition == value)
                return;
            _horizontalPosition = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasHorizontalPrevious));
            OnPropertyChanged(nameof(HasHorizontalNext));
        }
    }

    public int VerticalPosition
    {
        get => _verticalPosition;
        set
        {
            if (_verticalPosition == value)
                return;
            _verticalPosition = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasVerticalPrevious));
            OnPropertyChanged(nameof(HasVerticalNext));
        }
    }

    public bool HorizontalLoopEnabled
    {
        get => _horizontalLoopEnabled;
        set
        {
            if (_horizontalLoopEnabled == value)
                return;
            _horizontalLoopEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasHorizontalPrevious));
            OnPropertyChanged(nameof(HasHorizontalNext));
        }
    }

    public bool HorizontalSwipeEnabled
    {
        get => _horizontalSwipeEnabled;
        set
        {
            if (_horizontalSwipeEnabled == value)
                return;
            _horizontalSwipeEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasHorizontalPrevious));
            OnPropertyChanged(nameof(HasHorizontalNext));
        }
    }

    public bool VerticalLoopEnabled
    {
        get => _verticalLoopEnabled;
        set
        {
            if (_verticalLoopEnabled == value)
                return;
            _verticalLoopEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasVerticalPrevious));
            OnPropertyChanged(nameof(HasVerticalNext));
        }
    }

    public bool VerticalSwipeEnabled
    {
        get => _verticalSwipeEnabled;
        set
        {
            if (_verticalSwipeEnabled == value)
                return;
            _verticalSwipeEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasVerticalPrevious));
            OnPropertyChanged(nameof(HasVerticalNext));
        }
    }

    public bool HasHorizontalPrevious => HorizontalLoopEnabled || HorizontalPosition > 0;
    public bool HasHorizontalNext => HorizontalLoopEnabled || HorizontalPosition < Items.Count - 1;
    public bool HasVerticalPrevious => VerticalLoopEnabled || VerticalPosition > 0;
    public bool HasVerticalNext => VerticalLoopEnabled || VerticalPosition < Items.Count - 1;

    public CarouselPage()
    {
        InitializeComponent();
        PopulateItems();
        BindingContext = this;
    }

    void PopulateItems()
    {
        var random = new Random();
        const int totalItems = 10;

        for (int i = 1; i <= totalItems; i++)
        {
            Items.Add(new CarouselItem
            {
                Text = $"Item {i}",
                Color = Color.FromRgb(random.Next(256), random.Next(256), random.Next(256))
            });
        }

        OnPropertyChanged(nameof(HasHorizontalPrevious));
        OnPropertyChanged(nameof(HasHorizontalNext));
        OnPropertyChanged(nameof(HasVerticalPrevious));
        OnPropertyChanged(nameof(HasVerticalNext));
    }

    void OnHorizontalPreviousClicked(object sender, EventArgs e)
    {
        if (Items.Count == 0)
            return;

        if (HorizontalLoopEnabled)
        {
            HorizontalPosition = (HorizontalPosition - 1 + Items.Count) % Items.Count;
        }
        else if (HorizontalPosition > 0)
        {
            HorizontalPosition -= 1;
        }
    }

    void OnHorizontalNextClicked(object sender, EventArgs e)
    {
        if (Items.Count == 0)
            return;

        if (HorizontalLoopEnabled)
        {
            HorizontalPosition = (HorizontalPosition + 1) % Items.Count;
        }
        else if (HorizontalPosition < Items.Count - 1)
        {
            HorizontalPosition += 1;
        }
    }

    void OnVerticalPreviousClicked(object sender, EventArgs e)
    {
        if (Items.Count == 0)
            return;

        if (VerticalLoopEnabled)
        {
            VerticalPosition = (VerticalPosition - 1 + Items.Count) % Items.Count;
        }
        else if (VerticalPosition > 0)
        {
            VerticalPosition -= 1;
        }
    }

    void OnVerticalNextClicked(object sender, EventArgs e)
    {
        if (Items.Count == 0)
            return;

        if (VerticalLoopEnabled)
        {
            VerticalPosition = (VerticalPosition + 1) % Items.Count;
        }
        else if (VerticalPosition < Items.Count - 1)
        {
            VerticalPosition += 1;
        }
    }
}
