using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Avalonia.Controls.Maui.Controls;

internal class MauiSearchBar : TemplatedControl
{
    internal TextBox? _textBox;
    private Button? _clearButton;
    private Button? _searchButton;

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

        _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
        _clearButton = e.NameScope.Find<Button>("PART_ClearButton");
        _searchButton = e.NameScope.Find<Button>("PART_SearchButton");

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
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty ||
            change.Property == PlaceholderProperty ||
            change.Property == PlaceholderForegroundProperty ||
            change.Property == IsReadOnlyProperty ||
            change.Property == MaxLengthProperty ||
            change.Property == CharacterSpacingProperty ||
            change.Property == HorizontalTextAlignmentProperty ||
            change.Property == CursorPositionProperty ||
            change.Property == SelectionLengthProperty)
        {
            UpdateTextBoxProperties();
        }

        if (change.Property == CancelButtonColorProperty && _clearButton != null)
        {
            _clearButton.Foreground = CancelButtonColor;
        }
    }

    private void UpdateTextBoxProperties()
    {
        if (_textBox == null)
            return;

        _textBox.Text = Text;
        _textBox.Watermark = Placeholder;
        _textBox.IsReadOnly = IsReadOnly;
        _textBox.MaxLength = MaxLength;
        _textBox.TextAlignment = HorizontalTextAlignment;

        if (PlaceholderForeground != null)
        {
            // Note: Avalonia doesn't have direct PlaceholderForeground on TextBox
            // This would need to be handled through styles
        }

        if (CursorPosition >= 0 && CursorPosition <= Text.Length)
        {
            _textBox.CaretIndex = CursorPosition;
        }

        if (SelectionLength > 0)
        {
            _textBox.SelectionStart = CursorPosition;
            _textBox.SelectionEnd = CursorPosition + SelectionLength;
        }
    }

    private void OnTextBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_textBox != null && _textBox.Text != Text)
        {
            Text = _textBox.Text ?? string.Empty;
            TextChanged?.Invoke(this, e);
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

    public void FocusSearchBar()
    {
        _textBox?.Focus();
    }
}
