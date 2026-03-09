using Avalonia.Controls.Maui.Controls;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Handlers
{
    /// <summary>Avalonia handler for <see cref="IEditor"/>.</summary>
    public class EditorHandler : ViewHandler<IEditor, MauiEditor>
    {
        /// <summary>Property mapper for <see cref="EditorHandler"/>.</summary>
        public static IPropertyMapper<IEditor, EditorHandler> Mapper = new PropertyMapper<IEditor, EditorHandler>(ViewHandler.ViewMapper)
        {
            [nameof(IEditor.Background)] = MapBackground,
            [nameof(IEditor.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(IEditor.Font)] = MapFont,
            [nameof(IEditor.IsReadOnly)] = MapIsReadOnly,
            [nameof(IEditor.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
            [nameof(IEditor.IsSpellCheckEnabled)] = MapIsSpellCheckEnabled,
            [nameof(IEditor.MaxLength)] = MapMaxLength,
            [nameof(IEditor.Placeholder)] = MapPlaceholder,
            [nameof(IEditor.PlaceholderColor)] = MapPlaceholderColor,
            [nameof(IEditor.Text)] = MapText,
            [nameof(IEditor.TextColor)] = MapTextColor,
            [nameof(IEditor.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
            [nameof(IEditor.VerticalTextAlignment)] = MapVerticalTextAlignment,
            [nameof(IEditor.Keyboard)] = MapKeyboard,
            [nameof(IEditor.CursorPosition)] = MapCursorPosition,
            [nameof(IEditor.SelectionLength)] = MapSelectionLength,
            [nameof(Microsoft.Maui.Controls.Editor.TextTransform)] = MapTextTransform,
            [nameof(Microsoft.Maui.Controls.Editor.AutoSize)] = MapAutoSize,
        };

        /// <summary>Command mapper for <see cref="EditorHandler"/>.</summary>
        public static CommandMapper<IEditor, EditorHandler> CommandMapper = new(ViewCommandMapper)
        {
        };

        /// <summary>Initializes a new instance of <see cref="EditorHandler"/>.</summary>
        public EditorHandler() : base(Mapper, CommandMapper)
        {
        }

        /// <summary>Initializes a new instance of <see cref="EditorHandler"/>.</summary>
        /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
        public EditorHandler(IPropertyMapper? mapper)
            : base(mapper ?? Mapper, CommandMapper)
        {
        }

        /// <summary>Initializes a new instance of <see cref="EditorHandler"/>.</summary>
        /// <param name="mapper">The property mapper to use.</param>
        /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
        public EditorHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
            : base(mapper, commandMapper)
        {
        }
        
        /// <summary>Creates the Avalonia platform view for this handler.</summary>
        protected override MauiEditor CreatePlatformView()
        {
            return new MauiEditor();
        }

        /// <summary>Maps the Background property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapBackground(EditorHandler handler, IEditor editor)
        {
            handler.UpdateValue(nameof(IViewHandler.ContainerView));

            if (handler.PlatformView != null)
            {
                Extensions.EntryExtensions.UpdateBackground(handler.PlatformView, editor);
            }
        }
        
        /// <inheritdoc/>
        protected override void ConnectHandler(MauiEditor platformView)
        {
            base.ConnectHandler(platformView);
            platformView.TextChanged += OnTextChanged;
            platformView.SelectionChanged += OnSelectionChanged;
            platformView.LostFocus += OnLostFocus;
        }

        /// <inheritdoc/>
        protected override void DisconnectHandler(MauiEditor platformView)
        {
            platformView.TextChanged -= OnTextChanged;
            platformView.SelectionChanged -= OnSelectionChanged;
            platformView.LostFocus -= OnLostFocus;
            base.DisconnectHandler(platformView);
        }

        /// <summary>Gets a value indicating whether this handler requires a container view.</summary>
        public override bool NeedsContainer => false;

        /// <summary>Maps the Text property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapText(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorText(editor);

        /// <summary>Maps the TextColor property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapTextColor(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorTextColor(editor);

        /// <summary>Maps the CharacterSpacing property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapCharacterSpacing(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorCharacterSpacing(editor);

        /// <summary>Maps the Font property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapFont(EditorHandler handler, IEditor editor)
        {
            var fontManager = handler.GetRequiredService<IFontManager>();
            handler.PlatformView?.UpdateEditorFont(editor, fontManager);
        }

        /// <summary>Maps the HorizontalTextAlignment property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapHorizontalTextAlignment(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorHorizontalTextAlignment(editor);

        /// <summary>Maps the VerticalTextAlignment property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapVerticalTextAlignment(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorVerticalTextAlignment(editor);

        /// <summary>Maps the IsReadOnly property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapIsReadOnly(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorIsReadOnly(editor);

        /// <summary>Maps the IsTextPredictionEnabled property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorIsTextPredictionEnabled(editor);

        /// <summary>Maps the IsSpellCheckEnabled property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        [Avalonia.Controls.Maui.Platform.NotImplementedAttribute("Avalonia TextBox does not currently support disabling spell check.")]
        public static void MapIsSpellCheckEnabled(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorIsSpellCheckEnabled(editor);

        /// <summary>Maps the Keyboard property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapKeyboard(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorKeyboard(editor);

        /// <summary>Maps the MaxLength property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapMaxLength(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorMaxLength(editor);

        /// <summary>Maps the Placeholder property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapPlaceholder(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorPlaceholder(editor);

        /// <summary>Maps the PlaceholderColor property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapPlaceholderColor(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorPlaceholderColor(editor);

        /// <summary>Maps the CursorPosition property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapCursorPosition(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorCursorPosition(editor);

        /// <summary>Maps the SelectionLength property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapSelectionLength(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorSelectionLength(editor);

        /// <summary>Maps the TextTransform property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapTextTransform(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorTextTransform(editor);

        /// <summary>Maps the AutoSize property to the platform view.</summary>
        /// <param name="handler">The handler for the editor.</param>
        /// <param name="editor">The virtual view.</param>
        public static void MapAutoSize(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorAutoSize(editor);
        
        private void OnTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (VirtualView == null || PlatformView == null)
                return;

            VirtualView.Text = PlatformView.Text ?? string.Empty;
        }

        private void OnLostFocus(object? sender, Interactivity.RoutedEventArgs e)
        {
            if (VirtualView is Microsoft.Maui.Controls.Editor editor)
            {
                editor.SendCompleted();
            }
        }

        private void OnSelectionChanged(object? sender, Interactivity.RoutedEventArgs e)
        {
            if (VirtualView is null || PlatformView is null)
                return;

            VirtualView.CursorPosition = PlatformView.SelectionStart;
            VirtualView.SelectionLength = PlatformView.SelectionEnd - PlatformView.SelectionStart;
        }
    }
}