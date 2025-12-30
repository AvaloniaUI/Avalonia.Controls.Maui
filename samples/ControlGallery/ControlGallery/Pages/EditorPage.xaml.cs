namespace ControlGallery.Pages
{
    public partial class EditorPage : ContentPage
    {
        public EditorPage()
        {
            InitializeComponent();
        }

        private void OnRedClicked(object? sender, EventArgs e)
        {
            DynamicEditor.BackgroundColor = Colors.Red;
        }

        private void OnGreenClicked(object? sender, EventArgs e)
        {
            DynamicEditor.BackgroundColor = Colors.Green;
        }

        private void OnBlueClicked(object? sender, EventArgs e)
        {
            DynamicEditor.BackgroundColor = Colors.Blue;
        }

        private void OnClearClicked(object? sender, EventArgs e)
        {
            DynamicEditor.BackgroundColor = null;
        }

        private void OnTextRedClicked(object? sender, EventArgs e)
        {
            DynamicTextColorEditor.TextColor = Colors.Red;
        }

        private void OnTextGreenClicked(object? sender, EventArgs e)
        {
            DynamicTextColorEditor.TextColor = Colors.Green;
        }

        private void OnTextBlueClicked(object? sender, EventArgs e)
        {
            DynamicTextColorEditor.TextColor = Colors.Blue;
        }

        private void OnTextResetClicked(object? sender, EventArgs e)
        {
            DynamicTextColorEditor.TextColor = null;
        }

        private void OnEditorCompleted(object? sender, EventArgs e)
        {
            CompletedLabel.Text = $"Completed! Text: \"{CompletedEditor.Text}\"";
            CompletedLabel.TextColor = Colors.Green;
        }

        private void OnSetCursorStart(object? sender, EventArgs e)
        {
            CursorEditor.CursorPosition = 0;
            CursorEditor.Focus();
        }

        private void OnSetCursorMiddle(object? sender, EventArgs e)
        {
            CursorEditor.CursorPosition = (CursorEditor.Text?.Length ?? 0) / 2;
            CursorEditor.Focus();
        }

        private void OnSetCursorEnd(object? sender, EventArgs e)
        {
            CursorEditor.CursorPosition = CursorEditor.Text?.Length ?? 0;
            CursorEditor.Focus();
        }

        private void OnSelectFirst5(object? sender, EventArgs e)
        {
            SelectionEditor.CursorPosition = 0;
            SelectionEditor.SelectionLength = Math.Min(5, SelectionEditor.Text?.Length ?? 0);
            SelectionEditor.Focus();
        }

        private void OnSelectAll(object? sender, EventArgs e)
        {
            SelectionEditor.CursorPosition = 0;
            SelectionEditor.SelectionLength = SelectionEditor.Text?.Length ?? 0;
            SelectionEditor.Focus();
        }

        private void OnClearSelection(object? sender, EventArgs e)
        {
            SelectionEditor.SelectionLength = 0;
            SelectionEditor.Focus();
        }
    }
}
