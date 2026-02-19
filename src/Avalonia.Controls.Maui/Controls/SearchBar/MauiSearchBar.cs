using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Avalonia.Controls.Maui.Extensions;
using AvaloniaTextAlignment = Avalonia.Media.TextAlignment;

namespace Avalonia.Controls.Maui.Controls;

/// <summary>
/// A search bar control that provides text input with search-related functionality.
/// Supports placeholder text, clear button, and search button pressed events.
/// </summary>
public partial class MauiSearchBar : TemplatedControl
{
    internal TextBox? _textBox;
    private TextBlock? _placeholder;
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

    public static readonly StyledProperty<AvaloniaTextAlignment> HorizontalTextAlignmentProperty =
        AvaloniaProperty.Register<MauiSearchBar, AvaloniaTextAlignment>(nameof(HorizontalTextAlignment), defaultValue: AvaloniaTextAlignment.Left);

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        AvaloniaProperty.Register<MauiSearchBar, VerticalAlignment>(nameof(VerticalContentAlignment), defaultValue: VerticalAlignment.Center);

    public static readonly StyledProperty<IBrush?> SearchIconColorProperty =
        AvaloniaProperty.Register<MauiSearchBar, IBrush?>(nameof(SearchIconColor));

    public static readonly StyledProperty<object?> SearchIconProperty =
        AvaloniaProperty.Register<MauiSearchBar, object?>(nameof(SearchIcon));

    public static readonly StyledProperty<object?> ClearIconProperty =
        AvaloniaProperty.Register<MauiSearchBar, object?>(nameof(ClearIcon));

    public static readonly StyledProperty<System.Windows.Input.ICommand?> ClearCommandProperty =
        AvaloniaProperty.Register<MauiSearchBar, System.Windows.Input.ICommand?>(nameof(ClearCommand));

    public static readonly StyledProperty<object?> ClearCommandParameterProperty =
        AvaloniaProperty.Register<MauiSearchBar, object?>(nameof(ClearCommandParameter));

    public static readonly StyledProperty<bool> IsClearEnabledProperty =
        AvaloniaProperty.Register<MauiSearchBar, bool>(nameof(IsClearEnabled), defaultValue: true);

    public static readonly StyledProperty<Keyboard?> KeyboardProperty =
        AvaloniaProperty.Register<MauiSearchBar, Keyboard?>(nameof(Keyboard));

    public static readonly StyledProperty<SearchBoxVisibility> SearchBoxVisibilityProperty =
        AvaloniaProperty.Register<MauiSearchBar, SearchBoxVisibility>(nameof(SearchBoxVisibility), defaultValue: SearchBoxVisibility.Expanded);

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<MauiSearchBar, bool>(nameof(IsExpanded), defaultValue: true);

    public static readonly StyledProperty<string?> QueryIconHelpTextProperty =
        AvaloniaProperty.Register<MauiSearchBar, string?>(nameof(QueryIconHelpText));

    public static readonly StyledProperty<string?> ClearIconHelpTextProperty =
        AvaloniaProperty.Register<MauiSearchBar, string?>(nameof(ClearIconHelpText));

    public static readonly StyledProperty<string?> ClearPlaceholderHelpTextProperty =
        AvaloniaProperty.Register<MauiSearchBar, string?>(nameof(ClearPlaceholderHelpText));

    /// <summary>
    /// Gets or sets the text content of the search bar.
    /// </summary>
    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Gets or sets the placeholder text displayed when the search bar is empty.
    /// </summary>
    public string Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground brush for the placeholder text.
    /// </summary>
    public IBrush? PlaceholderForeground
    {
        get => GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the color of the cancel/clear button.
    /// </summary>
    public IBrush? CancelButtonColor
    {
        get => GetValue(CancelButtonColorProperty);
        set => SetValue(CancelButtonColorProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the search bar is read-only.
    /// </summary>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum number of characters allowed in the search bar.
    /// </summary>
    public int MaxLength
    {
        get => GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between characters in the search bar.
    /// </summary>
    public double CharacterSpacing
    {
        get => GetValue(CharacterSpacingProperty);
        set => SetValue(CharacterSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the current cursor position in the search bar.
    /// </summary>
    public int CursorPosition
    {
        get => GetValue(CursorPositionProperty);
        set => SetValue(CursorPositionProperty, value);
    }

    /// <summary>
    /// Gets or sets the length of the current selection in the search bar.
    /// </summary>
    public int SelectionLength
    {
        get => GetValue(SelectionLengthProperty);
        set => SetValue(SelectionLengthProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the text content.
    /// </summary>
    public AvaloniaTextAlignment HorizontalTextAlignment
    {
        get => GetValue(HorizontalTextAlignmentProperty);
        set => SetValue(HorizontalTextAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical alignment of the content.
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the color of the search icon.
    /// </summary>
    public IBrush? SearchIconColor
    {
        get => GetValue(SearchIconColorProperty);
        set => SetValue(SearchIconColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the content to display as the search icon.
    /// Can be a string (path data), an image, or any other Avalonia control.
    /// </summary>
    public object? SearchIcon
    {
        get => GetValue(SearchIconProperty);
        set => SetValue(SearchIconProperty, value);
    }

    /// <summary>
    /// Gets or sets the content to display as the clear icon.
    /// Can be a string (path data), an image, or any other Avalonia control.
    /// </summary>
    public object? ClearIcon
    {
        get => GetValue(ClearIconProperty);
        set => SetValue(ClearIconProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to execute when the clear button is clicked.
    /// </summary>
    public System.Windows.Input.ICommand? ClearCommand
    {
        get => GetValue(ClearCommandProperty);
        set => SetValue(ClearCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the <see cref="ClearCommand"/>.
    /// </summary>
    public object? ClearCommandParameter
    {
        get => GetValue(ClearCommandParameterProperty);
        set => SetValue(ClearCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the clear button is enabled.
    /// </summary>
    public bool IsClearEnabled
    {
        get => GetValue(IsClearEnabledProperty);
        set => SetValue(IsClearEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the keyboard type for the search bar.
    /// </summary>
    public Keyboard? Keyboard
    {
        get => GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    /// <summary>
    /// Gets or sets the search box visibility behavior.
    /// </summary>
    public SearchBoxVisibility SearchBoxVisibility
    {
        get => GetValue(SearchBoxVisibilityProperty);
        set => SetValue(SearchBoxVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the search box is currently expanded (relevant for Collapsible mode).
    /// </summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>
    /// Gets or sets the help text for the query icon.
    /// </summary>
    public string? QueryIconHelpText
    {
        get => GetValue(QueryIconHelpTextProperty);
        set => SetValue(QueryIconHelpTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the help text for the clear icon.
    /// </summary>
    public string? ClearIconHelpText
    {
        get => GetValue(ClearIconHelpTextProperty);
        set => SetValue(ClearIconHelpTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the help text for the clear placeholder button.
    /// </summary>
    public string? ClearPlaceholderHelpText
    {
        get => GetValue(ClearPlaceholderHelpTextProperty);
        set => SetValue(ClearPlaceholderHelpTextProperty, value);
    }

    /// <summary>
    /// Occurs when the search button is pressed or the Enter key is hit.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? SearchButtonPressed;

    /// <summary>
    /// Occurs when the text content of the search bar changes.
    /// </summary>
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
        _placeholder = e.NameScope.Find<TextBlock>("PART_Placeholder");
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
        else if (change.Property == CharacterSpacingProperty || change.Property == FontSizeProperty)
        {
            UpdateCharacterSpacing();
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
        else if (change.Property == KeyboardProperty)
        {
            if (_textBox != null)
                _textBox.UpdateKeyboard(Keyboard);
        }
        else if (change.Property == SearchBoxVisibilityProperty)
        {
            UpdateSearchBoxVisibility();
        }
        else if (change.Property == IsExpandedProperty)
        {
            UpdateSearchBoxVisibility();
        }
        // SearchIconColor and CancelButtonColor are handled via TemplateBinding
        // No code-behind needed, TemplateBinding automatically syncs on property changes
    }

    private void UpdateSearchBoxVisibility()
    {
        bool shouldExpand = SearchBoxVisibility == SearchBoxVisibility.Expanded || 
                           (SearchBoxVisibility == SearchBoxVisibility.Collapsible && IsExpanded);

        if (_textBox != null) _textBox.IsVisible = shouldExpand;
        if (_placeholder != null) _placeholder.IsVisible = shouldExpand && string.IsNullOrEmpty(Text);
        UpdateClearButtonVisibility();

        // If we are in search bar, we might want to adjust external margins or size
        // But for now, we just hide internal components.
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
        _textBox.UpdateKeyboard(Keyboard);
        UpdateCharacterSpacing();
        UpdateSearchBoxVisibility();

        if (CursorPosition >= 0 && CursorPosition <= (Text?.Length ?? 0))
        {
            _textBox.CaretIndex = CursorPosition;
        }
    }

    private void UpdateCharacterSpacing()
    {
        if (CharacterSpacing != 0)
        {
            LetterSpacing = CharacterSpacing;
        }
        else
        {
            ClearValue(LetterSpacingProperty);
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
            bool shouldExpand = SearchBoxVisibility == SearchBoxVisibility.Expanded || 
                               (SearchBoxVisibility == SearchBoxVisibility.Collapsible && IsExpanded);
            _clearButton.IsVisible = shouldExpand && !string.IsNullOrEmpty(Text);
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
        if (SearchBoxVisibility == SearchBoxVisibility.Collapsible)
        {
            IsExpanded = !IsExpanded;
            if (IsExpanded)
            {
                _textBox?.Focus();
            }
        }
        else
        {
            SearchButtonPressed?.Invoke(this, e);
        }
    }

    /// <summary>
    /// Sets focus to the search bar's input field.
    /// </summary>
    public void Focus()
    {
        _textBox?.Focus();
    }
}