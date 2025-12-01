namespace ControlGallery.Pages;

public partial class PickerPage : ContentPage
{
    Picker? BasicPickerControl => FindByName("BasicPicker") as Picker;
    Label? SelectedFruitLabelControl => FindByName("SelectedFruitLabel") as Label;
    Picker? IndexedPickerControl => FindByName("IndexedPicker") as Picker;
    Label? PreselectedIndexLabelControl => FindByName("PreselectedIndexLabel") as Label;
    Picker? ItemPickerControl => FindByName("ItemPicker") as Picker;
    Label? SelectedItemLabelControl => FindByName("SelectedItemLabel") as Label;
    Picker? AlignmentPickerControl => FindByName("AlignmentPicker") as Picker;
    Label? AlignmentStatusLabelControl => FindByName("AlignmentStatusLabel") as Label;

    public PickerPage()
    {
        InitializeComponent();
        InitializeSelections();
    }

    void OnBasicPickerChanged(object? sender, EventArgs e) =>
        UpdateSelectedLabel(BasicPickerControl, SelectedFruitLabelControl);

    void OnItemPickerChanged(object? sender, EventArgs e) =>
        UpdateSelectedLabel(ItemPickerControl, SelectedItemLabelControl);

    void InitializeSelections()
    {
        SetInitialSelectionForItemPicker();
        UpdateSelectedLabel(IndexedPickerControl, PreselectedIndexLabelControl);
        UpdateSelectedLabel(ItemPickerControl, SelectedItemLabelControl);
        UpdateSelectedLabel(BasicPickerControl, SelectedFruitLabelControl);
        UpdateAlignmentLabel();
    }

    void UpdateSelectedLabel(Picker? picker, Label? label)
    {
        if (picker is null || label is null)
            return;

        if (picker.SelectedIndex >= 0 && picker.SelectedIndex < picker.Items.Count)
        {
            label.Text = $"Selected item: {picker.Items[picker.SelectedIndex]}";
        }
        else
        {
            label.Text = "Selected item: none";
        }
    }

    void SetInitialSelectionForItemPicker()
    {
        if (ItemPickerControl == null)
            return;

        var preferredItem = "Latte";
        var preferredIndex = ItemPickerControl.Items.IndexOf(preferredItem);
        ItemPickerControl.SelectedIndex = preferredIndex >= 0 ? preferredIndex : -1;
    }

    void OnAlignmentPickerChanged(object? sender, EventArgs e)
    {
        if (AlignmentPickerControl is null)
            return;

        AlignmentPickerControl.HorizontalTextAlignment = AlignmentPickerControl.SelectedIndex switch
        {
            0 => TextAlignment.Start,
            1 => TextAlignment.Center,
            2 => TextAlignment.End,
            _ => AlignmentPickerControl.HorizontalTextAlignment
        };

        UpdateAlignmentLabel();
    }

    void UpdateAlignmentLabel()
    {
        if (AlignmentPickerControl is null || AlignmentStatusLabelControl is null)
            return;

        AlignmentStatusLabelControl.Text = $"HorizontalTextAlignment: {AlignmentPickerControl.HorizontalTextAlignment}";
    }
}
