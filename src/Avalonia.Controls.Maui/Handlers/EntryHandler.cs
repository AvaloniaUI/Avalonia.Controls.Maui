using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Microsoft.Maui;
using Avalonia.Controls.Maui.Controls;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IEntry"/>.</summary>
public class EntryHandler : ViewHandler<IEntry, MauiEntry>
{
    /// <summary>Property mapper for <see cref="EntryHandler"/>.</summary>
    public static IPropertyMapper<IEntry, EntryHandler> Mapper = new PropertyMapper<IEntry, EntryHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IEntry.Background)] = MapBackground,
        [nameof(IEntry.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(IEntry.ClearButtonVisibility)] = MapClearButtonVisibility,
        [nameof(IEntry.Font)] = MapFont,
        [nameof(IEntry.IsPassword)] = MapIsPassword,
        [nameof(IEntry.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
        [nameof(IEntry.VerticalTextAlignment)] = MapVerticalTextAlignment,
        [nameof(IEntry.IsReadOnly)] = MapIsReadOnly,
        [nameof(IEntry.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
        [nameof(IEntry.IsSpellCheckEnabled)] = MapIsSpellCheckEnabled,
        [nameof(IEntry.Keyboard)] = MapKeyboard,
        [nameof(IEntry.MaxLength)] = MapMaxLength,
        [nameof(IEntry.Placeholder)] = MapPlaceholder,
        [nameof(IEntry.PlaceholderColor)] = MapPlaceholderColor,
        [nameof(IEntry.ReturnType)] = MapReturnType,
        [nameof(IEntry.Text)] = MapText,
        [nameof(IEntry.TextColor)] = MapTextColor,
        [nameof(IEntry.CursorPosition)] = MapCursorPosition,
        [nameof(IEntry.SelectionLength)] = MapSelectionLength,
        [nameof(Microsoft.Maui.Controls.Entry.TextTransform)] = MapTextTransform
    };
    
    /// <summary>Command mapper for <see cref="EntryHandler"/>.</summary>
    public static CommandMapper<IEntry, EntryHandler> CommandMapper = new(ViewCommandMapper);

    /// <summary>Initializes a new instance of <see cref="EntryHandler"/>.</summary>
    public EntryHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="EntryHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    public EntryHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="EntryHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    public EntryHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
        : base(mapper, commandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override MauiEntry CreatePlatformView()
    {
        return new MauiEntry();
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(MauiEntry platformView)
    {
        base.ConnectHandler(platformView);
        platformView.TextChanged += OnTextChanged;
        platformView.KeyDown += OnKeyDown;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(MauiEntry platformView)
    {
        platformView.TextChanged -= OnTextChanged;
        platformView.KeyDown -= OnKeyDown;
        base.DisconnectHandler(platformView);
    }

    /// <summary>Gets a value indicating whether this handler requires a container view.</summary>
    public override bool NeedsContainer => false;

    /// <summary>Maps the Background property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapBackground(EntryHandler handler, IEntry entry)
    {
        handler.UpdateValue(nameof(IViewHandler.ContainerView));
        handler.PlatformView?.UpdateBackground(entry);
    }
    
    /// <summary>Maps the Text property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapText(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateText(entry);

    /// <summary>Maps the TextColor property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapTextColor(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateTextColor(entry);

    /// <summary>Maps the CharacterSpacing property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapCharacterSpacing(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateCharacterSpacing(entry);

    /// <summary>Maps the Font property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapFont(EntryHandler handler, IEntry entry)
    {
        var fontManager = handler.GetRequiredService<IFontManager>();
        handler.PlatformView?.UpdateFont(entry, fontManager);
    }

    /// <summary>Maps the HorizontalTextAlignment property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateHorizontalTextAlignment(entry);

    /// <summary>Maps the VerticalTextAlignment property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapVerticalTextAlignment(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateVerticalTextAlignment(entry);

    /// <summary>Maps the IsPassword property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapIsPassword(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateIsPassword(entry);

    /// <summary>Maps the IsReadOnly property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapIsReadOnly(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateIsReadOnly(entry);

    /// <summary>Maps the IsTextPredictionEnabled property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry)
    {
        if (handler.PlatformView is MauiEntry textBox)
        {
            TextInputOptions.SetShowSuggestions(textBox, entry.IsTextPredictionEnabled);
        }
    }

    /// <summary>Maps the IsSpellCheckEnabled property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapIsSpellCheckEnabled(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateIsSpellCheckEnabled(entry);

    /// <summary>Maps the Keyboard property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapKeyboard(EntryHandler handler, IEntry entry)
    {
        handler.PlatformView?.UpdateKeyboard(entry.Keyboard);
    }

    /// <summary>Maps the MaxLength property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapMaxLength(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateMaxLength(entry);

    /// <summary>Maps the Placeholder property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapPlaceholder(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdatePlaceholder(entry);

    /// <summary>Maps the PlaceholderColor property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapPlaceholderColor(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdatePlaceholderColor(entry);

    /// <summary>Maps the ReturnType property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapReturnType(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateReturnType(entry);

    /// <summary>Maps the ClearButtonVisibility property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateClearButtonVisibility(entry);

    /// <summary>Maps the CursorPosition property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapCursorPosition(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateCursorPosition(entry);

    /// <summary>Maps the SelectionLength property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapSelectionLength(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateSelectionLength(entry);

    /// <summary>Maps the TextTransform property to the platform view.</summary>
    /// <param name="handler">The handler for the entry.</param>
    /// <param name="entry">The virtual view.</param>
    public static void MapTextTransform(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateTextTransform(entry);
    
    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (VirtualView == null || PlatformView == null)
            return;

        VirtualView.Text = PlatformView.Text ?? string.Empty;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && VirtualView != null)
        {
            VirtualView.Completed();
        }
    }
}