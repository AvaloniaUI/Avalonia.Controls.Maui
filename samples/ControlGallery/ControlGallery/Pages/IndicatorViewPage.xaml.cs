using System.Collections.ObjectModel;

namespace ControlGallery.Pages;

public partial class IndicatorViewPage : ContentPage
{
    private readonly ObservableCollection<string> _items = new();

    public IndicatorViewPage()
    {
        InitializeComponent();
        
        // Initialize ItemsSource sample
        ItemsSourceIndicator.ItemsSource = _items;
        UpdateItemCountLabel();
        
        // Subscribe to programmatic indicator position changes
        ProgrammaticIndicator.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IndicatorView.Position))
            {
                UpdatePositionLabel();
            }
        };
        
        // Subscribe to template indicator position changes
        TemplatedIndicator.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IndicatorView.Position))
            {
                UpdateTemplatePositionLabel();
            }
        };
    }

    private void OnPreviousClicked(object sender, EventArgs e)
    {
        if (ProgrammaticIndicator.Position > 0)
        {
            ProgrammaticIndicator.Position--;
            UpdatePositionLabel();
        }
    }

    private void OnNextClicked(object sender, EventArgs e)
    {
        if (ProgrammaticIndicator.Position < ProgrammaticIndicator.Count - 1)
        {
            ProgrammaticIndicator.Position++;
            UpdatePositionLabel();
        }
    }

    private void OnResetClicked(object sender, EventArgs e)
    {
        ProgrammaticIndicator.Position = 0;
        UpdatePositionLabel();
    }

    private void UpdatePositionLabel()
    {
        PositionLabel.Text = $"Current Position: {ProgrammaticIndicator.Position}";
    }

    private void UpdateTemplatePositionLabel()
    {
        TemplatePositionLabel.Text = $"Selected: {TemplatedIndicator.Position}";
    }

    // ItemsSource sample handlers
    private void OnAddItemClicked(object sender, EventArgs e)
    {
        _items.Add($"Item {_items.Count + 1}");
        UpdateItemCountLabel();
    }

    private void OnRemoveItemClicked(object sender, EventArgs e)
    {
        if (_items.Count > 0)
        {
            _items.RemoveAt(_items.Count - 1);
            UpdateItemCountLabel();
        }
    }

    private void OnClearItemsClicked(object sender, EventArgs e)
    {
        _items.Clear();
        UpdateItemCountLabel();
    }

    private void UpdateItemCountLabel()
    {
        ItemCountLabel.Text = $"Items: {_items.Count}";
    }
}
