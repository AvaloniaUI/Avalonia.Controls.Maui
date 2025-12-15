using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ControlGallery.Pages;

public partial class SearchBarPage : ContentPage, INotifyPropertyChanged
{
    private string _searchText = string.Empty;
    private string _searchResultText = "Press Enter or tap search to search";
    private bool _isReadOnly;
    private string _editableText = "You can edit this";
    private string _eventLog = "Events will appear here...";

    public SearchBarPage()
    {
        InitializeComponent();

        ClearLogCommand = new Command(() => EventLog = "Events will appear here...");
        SearchCommand = new Command<object>(OnSearchCommand);

        BindingContext = this;
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }
    }

    public string SearchResultText
    {
        get => _searchResultText;
        set
        {
            if (_searchResultText != value)
            {
                _searchResultText = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set
        {
            if (_isReadOnly != value)
            {
                _isReadOnly = value;
                OnPropertyChanged();
            }
        }
    }

    public string EditableText
    {
        get => _editableText;
        set
        {
            if (_editableText != value)
            {
                _editableText = value;
                OnPropertyChanged();
            }
        }
    }

    public string EventLog
    {
        get => _eventLog;
        set
        {
            if (_eventLog != value)
            {
                _eventLog = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand ClearLogCommand { get; }

    public ICommand SearchCommand { get; }

    private void OnSearchCommand(object? parameter)
    {
        var text = CommandSearchBar?.Text ?? string.Empty;
        var param = parameter?.ToString() ?? "null";
        CommandResultLabel.Text = $"✓ Command executed! Query: '{text}', Parameter: '{param}'";
        CommandResultLabel.TextColor = Colors.Green;
    }

    private void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        if (sender is SearchBar searchBar)
        {
            var text = searchBar.Text ?? string.Empty;
            SearchResultText = string.IsNullOrWhiteSpace(text)
                ? "Please enter a search term"
                : $"Searching for: '{text}'";

            EventLog = $"SearchButtonPressed: '{text}'\n{EventLog}";
            TrimEventLog();
        }
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        EventLog = $"TextChanged: '{e.OldTextValue}' → '{e.NewTextValue}'\n{EventLog}";
        TrimEventLog();
    }

    private void OnSearchEventTriggered(object? sender, EventArgs e)
    {
        if (sender is SearchBar searchBar)
        {
            var text = searchBar.Text ?? "empty";
            SearchEventLabel.Text = $"✓ SearchButtonPressed! Query: '{text}'";
            SearchEventLabel.TextColor = Colors.Green;
        }
    }

    private void OnCursorStartClicked(object? sender, EventArgs e)
    {
        CursorSearchBar.CursorPosition = 0;
        CursorSearchBar.Focus();
    }

    private void OnCursorMiddleClicked(object? sender, EventArgs e)
    {
        var text = CursorSearchBar.Text;
        CursorSearchBar.CursorPosition = text.Length / 2;
        CursorSearchBar.Focus();
    }

    private void OnCursorEndClicked(object? sender, EventArgs e)
    {
        var text = CursorSearchBar.Text;
        CursorSearchBar.CursorPosition = text.Length;
        CursorSearchBar.Focus();
    }

    private void OnSelectAllClicked(object? sender, EventArgs e)
    {
        var text = SelectionSearchBar.Text;
        SelectionSearchBar.CursorPosition = 0;
        SelectionSearchBar.SelectionLength = text.Length;
        SelectionSearchBar.Focus();
    }

    private void OnSelect5Clicked(object? sender, EventArgs e)
    {
        SelectionSearchBar.CursorPosition = 0;
        SelectionSearchBar.SelectionLength = Math.Min(5, (SelectionSearchBar.Text?.Length ?? 0));
        SelectionSearchBar.Focus();
    }

    private void OnClearSelectionClicked(object? sender, EventArgs e)
    {
        SelectionSearchBar.SelectionLength = 0;
        SelectionSearchBar.Focus();
    }

    private void TrimEventLog()
    {
        var lines = EventLog.Split('\n');
        if (lines.Length > 10)
        {
            EventLog = string.Join('\n', lines.Take(10));
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}