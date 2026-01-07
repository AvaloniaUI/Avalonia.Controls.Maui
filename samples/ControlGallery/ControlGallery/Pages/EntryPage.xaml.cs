namespace ControlGallery.Pages
{
    public partial class EntryPage : ContentPage
    {
        public System.Windows.Input.ICommand ReturnCommand { get; }

        public EntryPage()
        {
            InitializeComponent();
            ReturnCommand = new Command((obj) => 
            {
                string msg = obj?.ToString() ?? "Return Key Pressed!";
                ReturnResultLabel.Text = $"Command Result: {msg}";
            });
            BindingContext = this;
        }

        private void OnRedClicked(object? sender, EventArgs e)
        {
            DynamicEntry.BackgroundColor = Colors.Red;
        }

        private void OnGreenClicked(object? sender, EventArgs e)
        {
            DynamicEntry.BackgroundColor = Colors.Green;
        }

        private void OnBlueClicked(object? sender, EventArgs e)
        {
            DynamicEntry.BackgroundColor = Colors.Blue;
        }

        private void OnClearClicked(object? sender, EventArgs e)
        {
            DynamicEntry.BackgroundColor = null;
        }

        private void OnTextColorRedClicked(object? sender, EventArgs e)
        {
            DynamicTextColorEntry.TextColor = Colors.Red;
        }

        private void OnTextColorGreenClicked(object? sender, EventArgs e)
        {
            DynamicTextColorEntry.TextColor = Colors.Green;
        }

        private void OnTextColorBlueClicked(object? sender, EventArgs e)
        {
            DynamicTextColorEntry.TextColor = Colors.Blue;
        }

        private void OnTextColorDefaultClicked(object? sender, EventArgs e)
        {
            DynamicTextColorEntry.TextColor = null;
        }

        private void OnPlaceholderRedClicked(object? sender, EventArgs e)
        {
            DynamicPlaceholderColorEntry.PlaceholderColor = Colors.Red;
        }

        private void OnPlaceholderGreenClicked(object? sender, EventArgs e)
        {
            DynamicPlaceholderColorEntry.PlaceholderColor = Colors.Green;
        }

        private void OnPlaceholderBlueClicked(object? sender, EventArgs e)
        {
            DynamicPlaceholderColorEntry.PlaceholderColor = Colors.Blue;
        }

        private void OnPlaceholderDefaultClicked(object? sender, EventArgs e)
        {
            DynamicPlaceholderColorEntry.PlaceholderColor = null;
        }

        private void OnSetCursorStart(object? sender, EventArgs e)
        {
            CursorEntry.CursorPosition = 0;
            CursorEntry.Focus();
        }

        private void OnSetCursorMiddle(object? sender, EventArgs e)
        {
            CursorEntry.CursorPosition = (CursorEntry.Text?.Length ?? 0) / 2;
            CursorEntry.Focus();
        }

        private void OnSetCursorEnd(object? sender, EventArgs e)
        {
            CursorEntry.CursorPosition = CursorEntry.Text?.Length ?? 0;
            CursorEntry.Focus();
        }

        private void OnSelectFirst5(object? sender, EventArgs e)
        {
            SelectionEntry.CursorPosition = 0;
            SelectionEntry.SelectionLength = Math.Min(5, SelectionEntry.Text?.Length ?? 0);
            SelectionEntry.Focus();
        }

        private void OnSelectAll(object? sender, EventArgs e)
        {
            SelectionEntry.CursorPosition = 0;
            SelectionEntry.SelectionLength = SelectionEntry.Text?.Length ?? 0;
            SelectionEntry.Focus();
        }

        private void OnClearSelection(object? sender, EventArgs e)
        {
            SelectionEntry.SelectionLength = 0;
            SelectionEntry.Focus();
        }

        private void OnEntryCompleted(object? sender, EventArgs e)
        {
            EventLabel.Text = $"Completed! Text: \"{EventEntry.Text}\"";
            EventLabel.TextColor = Colors.Green;
        }

        private void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
        {
            EventLabel.Text = $"Text Changed: \"{e.NewTextValue}\"";
            EventLabel.ClearValue(Label.TextColorProperty);
        }
    }
}