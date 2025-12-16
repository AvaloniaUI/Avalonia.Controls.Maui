using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace Avalonia.Controls.Maui.Controls;

/// <summary>
/// A search bar control that provides text input with search-related functionality.
/// Supports placeholder text, clear button, and search button pressed events.
/// </summary>
public partial class MauiSearchBar : TemplatedControl
{
    internal TextBox? _textBox;
    private Button? _clearButton;
    private Button? _searchButton;
    private Button? _searchIconButton;
    private PathIcon? _clearIcon;
    private PathIcon? _searchIcon;

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<MauiSearchBar, string>(nameof(Text), defaultValue: string.Empty);

    public static readonly StyledProperty<string> PlaceholderProperty =
        AvaloniaProperty.Register<MauiSearchBar, string>(nameof(Placeholder), defaultValue: string.Empty);

    public static readonly StyledProperty<IBrush?> PlaceholderForegroundProperty =
        AvaloniaProperty.Register<MauiSearchBar, IBrush?>(nameof(PlaceholderForeground));

    public static readonly StyledProperty<IBrush?> CancelButtonColorProperty =
        AvaloniaProperty.Register<MauiSearchBar, IBrush?>(nameof(CancelButtonColor));

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<MauiSearchBar, bool>(nameof(IsReadOnly), defaultValue: false);

    public static readonly StyledProperty<int> MaxLengthProperty =
        AvaloniaProperty.Register<MauiSearchBar, int>(nameof(MaxLength), defaultValue: 0);

    public static readonly StyledProperty<double> CharacterSpacingProperty =
        AvaloniaProperty.Register<MauiSearchBar, double>(nameof(CharacterSpacing), defaultValue: 0.0);

    public static readonly StyledProperty<int> CursorPositionProperty =
        AvaloniaProperty.Register<MauiSearchBar, int>(nameof(CursorPosition), defaultValue: 0);

    public static readonly StyledProperty<int> SelectionLengthProperty =
        AvaloniaProperty.Register<MauiSearchBar, int>(nameof(SelectionLength), defaultValue: 0);

    public static readonly StyledProperty<TextAlignment> HorizontalTextAlignmentProperty =
        AvaloniaProperty.Register<MauiSearchBar, TextAlignment>(nameof(HorizontalTextAlignment), defaultValue: TextAlignment.Left);

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        AvaloniaProperty.Register<MauiSearchBar, VerticalAlignment>(nameof(VerticalContentAlignment), defaultValue: VerticalAlignment.Center);

    public static readonly StyledProperty<IBrush?> SearchIconColorProperty =
        AvaloniaProperty.Register<MauiSearchBar, IBrush?>(nameof(SearchIconColor));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public IBrush? PlaceholderForeground
    {
        get => GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }

    public IBrush? CancelButtonColor
    {
        get => GetValue(CancelButtonColorProperty);
        set => SetValue(CancelButtonColorProperty, value);
    }

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public int MaxLength
    {
        get => GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public double CharacterSpacing
    {
        get => GetValue(CharacterSpacingProperty);
        set => SetValue(CharacterSpacingProperty, value);
    }

    public int CursorPosition
    {
        get => GetValue(CursorPositionProperty);
        set => SetValue(CursorPositionProperty, value);
    }

    public int SelectionLength
    {
        get => GetValue(SelectionLengthProperty);
        set => SetValue(SelectionLengthProperty, value);
    }

    public TextAlignment HorizontalTextAlignment
    {
        get => GetValue(HorizontalTextAlignmentProperty);
        set => SetValue(HorizontalTextAlignmentProperty, value);
    }

    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    public IBrush? SearchIconColor
    {
        get => GetValue(SearchIconColorProperty);
        set => SetValue(SearchIconColorProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? SearchButtonPressed;
    public event EventHandler<TextChangedEventArgs>? TextChanged;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        if (_textBox != null)
        {
            _textBox.TextChanged -= OnTextBoxTextChanged;
            _textBox.KeyDown -= OnTextBoxKeyDown;
        }

        if (_clearButton != null)
        {
            _clearButton.Click -= OnClearButtonClick;
        }

        if (_searchButton != null)
        {
            _searchButton.Click -= OnSearchButtonClick;
        }

        if (_searchIconButton != null)
        {
            _searchIconButton.Click -= OnSearchIconButtonClick;
        }

        _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
        _clearButton = e.NameScope.Find<Button>("PART_ClearButton");
        _searchButton = e.NameScope.Find<Button>("PART_SearchButton");
        _searchIconButton = e.NameScope.Find<Button>("PART_SearchIconButton");
        _clearIcon = e.NameScope.Find<PathIcon>("PART_ClearIcon");
        _searchIcon = e.NameScope.Find<PathIcon>("PART_SearchIcon");

        if (_textBox != null)
        {
            _textBox.TextChanged += OnTextBoxTextChanged;
            _textBox.KeyDown += OnTextBoxKeyDown;
            UpdateTextBoxProperties();
        }

        if (_clearButton != null)
        {
            _clearButton.Click += OnClearButtonClick;
        }

        if (_searchButton != null)
        {
            _searchButton.Click += OnSearchButtonClick;
        }

        if (_searchIconButton != null)
        {
            _searchIconButton.Click += OnSearchIconButtonClick;
        }

        // Icon colors are handled via TemplateBinding in the AXAML template
        // No code-behind assignment needed, local values would override bindings
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // Handle Text property separately to avoid caret position issues
        if (change.Property == TextProperty)
        {
            // Only update TextBox if the change came from outside (not from user typing)
            if (_textBox != null && _textBox.Text != Text)
            {
                _textBox.Text = Text;
            }
            UpdateClearButtonVisibility();
        }

        else if (change.Property == IsReadOnlyProperty)
        {
            if (_textBox != null)
                _textBox.IsReadOnly = IsReadOnly;
        }
        else if (change.Property == MaxLengthProperty)
        {
            if (_textBox != null)
                _textBox.MaxLength = MaxLength;
        }
        else if (change.Property == HorizontalTextAlignmentProperty)
        {
            if (_textBox != null)
                _textBox.TextAlignment = HorizontalTextAlignment;
        }
        else if (change.Property == CursorPositionProperty)
        {
            if (_textBox != null && CursorPosition >= 0 && CursorPosition <= (Text?.Length ?? 0))
            {
                _textBox.CaretIndex = CursorPosition;
            }
        }
        else if (change.Property == SelectionLengthProperty)
        {
            if (_textBox != null && SelectionLength > 0)
            {
                _textBox.SelectionStart = CursorPosition;
                _textBox.SelectionEnd = CursorPosition + SelectionLength;
            }
        }

        // SearchIconColor and CancelButtonColor are handled via TemplateBinding
        // No code-behind needed, TemplateBinding automatically syncs on property changes
    }

    private void UpdateTextBoxProperties()
    {
        if (_textBox == null)
            return;

        if (_textBox.Text != Text)
        {
            _textBox.Text = Text;
        }

        _textBox.IsReadOnly = IsReadOnly;
        _textBox.MaxLength = MaxLength;
        _textBox.TextAlignment = HorizontalTextAlignment;

        if (CursorPosition >= 0 && CursorPosition <= (Text?.Length ?? 0))
        {
            _textBox.CaretIndex = CursorPosition;
        }
    }

    private void OnTextBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_textBox != null && _textBox.Text != Text)
        {
            Text = _textBox.Text ?? string.Empty;
            TextChanged?.Invoke(this, e);
        }
        UpdateClearButtonVisibility();
    }

    private void UpdateClearButtonVisibility()
    {
        if (_clearButton != null)
        {
            _clearButton.IsVisible = !string.IsNullOrEmpty(Text);
        }
    }

    private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchButtonPressed?.Invoke(this, new RoutedEventArgs());
            e.Handled = true;
        }
    }

    private void OnClearButtonClick(object? sender, RoutedEventArgs e)
    {
        Text = string.Empty;
        _textBox?.Focus();
    }

    private void OnSearchButtonClick(object? sender, RoutedEventArgs e)
    {
        SearchButtonPressed?.Invoke(this, e);
    }

    private void OnSearchIconButtonClick(object? sender, RoutedEventArgs e)
    {
        SearchButtonPressed?.Invoke(this, e);
    }

    public void Focus()
    {
        _textBox?.Focus();
    }
}