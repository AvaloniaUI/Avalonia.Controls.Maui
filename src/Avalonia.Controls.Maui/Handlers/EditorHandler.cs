using Avalonia.Controls.Maui.Controls;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Handlers
{
    public class EditorHandler : ViewHandler<IEditor, MauiEditor>
    {
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

        public static CommandMapper<IEditor, EditorHandler> CommandMapper = new(ViewCommandMapper)
        {
        };

        public EditorHandler() : base(Mapper, CommandMapper)
        {
        }

        public EditorHandler(IPropertyMapper? mapper)
            : base(mapper ?? Mapper, CommandMapper)
        {
        }

        public EditorHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
            : base(mapper, commandMapper)
        {
        }
        
        protected override MauiEditor CreatePlatformView()
        {
            return new MauiEditor();
        }

        protected override void ConnectHandler(MauiEditor platformView)
        {
            base.ConnectHandler(platformView);
            platformView.TextChanged += OnTextChanged;
            platformView.LostFocus += OnLostFocus;
        }

        protected override void DisconnectHandler(MauiEditor platformView)
        {
            platformView.TextChanged -= OnTextChanged;
            platformView.LostFocus -= OnLostFocus;
            base.DisconnectHandler(platformView);
        }

        public override bool NeedsContainer => false;

        public static void MapBackground(EditorHandler handler, IEditor editor)
        {
            handler.UpdateValue(nameof(IViewHandler.ContainerView));
            handler.PlatformView?.UpdateBackground(editor);
        }

        public static void MapText(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorText(editor);

        public static void MapTextColor(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorTextColor(editor);

        public static void MapCharacterSpacing(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorCharacterSpacing(editor);

        public static void MapFont(EditorHandler handler, IEditor editor)
        {
            var fontManager = handler.GetRequiredService<IFontManager>();
            handler.PlatformView?.UpdateEditorFont(editor, fontManager);
        }

        public static void MapHorizontalTextAlignment(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorHorizontalTextAlignment(editor);

        public static void MapVerticalTextAlignment(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorVerticalTextAlignment(editor);

        public static void MapIsReadOnly(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorIsReadOnly(editor);

        [NotImplemented("Avalonia TextBox does not currently support disabling text prediction.")]
        public static void MapIsTextPredictionEnabled(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorIsTextPredictionEnabled(editor);

        [NotImplemented("Avalonia TextBox does not currently support disabling spell check.")]
        public static void MapIsSpellCheckEnabled(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorIsSpellCheckEnabled(editor);

        [NotImplemented("Custom keyboard mapping is not yet implemented for Avalonia Editor.")]
        public static void MapKeyboard(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorKeyboard(editor);

        public static void MapMaxLength(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorMaxLength(editor);

        public static void MapPlaceholder(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorPlaceholder(editor);

        public static void MapPlaceholderColor(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorPlaceholderColor(editor);

        public static void MapCursorPosition(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorCursorPosition(editor);

        public static void MapSelectionLength(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorSelectionLength(editor);

        public static void MapTextTransform(EditorHandler handler, IEditor editor) =>
            handler.PlatformView?.UpdateEditorTextTransform(editor);

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
    }
}