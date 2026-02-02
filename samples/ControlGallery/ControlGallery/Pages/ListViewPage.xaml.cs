using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace ControlGallery.Pages;

internal partial class ListViewPage : ContentPage
{
    public ListViewPage()
    {
        // Basic
        BasicItems = new ObservableCollection<string>();
        for (int i = 1; i <= 20; i++) BasicItems.Add($"Basic Item {i}");

        // Refresh
        RefreshItems = new ObservableCollection<string>();
        for (int i = 1; i <= 20; i++) RefreshItems.Add($"Refresh Item {i}");

        // Events
        EventItems = new ObservableCollection<string>();
        for (int i = 1; i <= 20; i++) EventItems.Add($"Event Item {i}");

        // Separator
        SeparatorItems = new ObservableCollection<string>();
        for (int i = 1; i <= 10; i++) SeparatorItems.Add($"Separator Item {i}");

        // Performance
        PerformanceItems = new ObservableCollection<string>();
        for (int i = 1; i <= 1000; i++) PerformanceItems.Add($"Item {i}");

        // Cell Types
        CellTypeItems = new ObservableCollection<CellTypeItem>
        {
            new CellTypeItem { Name = "TextCell Item", Detail = "Detail text here", Type = "Text" },
            new CellTypeItem { Name = "ImageCell Item", Detail = "With image", Type = "Image" },
            new CellTypeItem { Name = "SwitchCell Item", IsToggled = true, Type = "Switch" },
            new CellTypeItem { Name = "EntryCell Item", Placeholder = "Type something...", Type = "Entry" },
            new CellTypeItem { Name = "ViewCell Item", Detail = "Custom View Content", Type = "View" }
        };

        // Selection
        SelectionItems = new ObservableCollection<string>();
        for (int i = 1; i <= 10; i++) SelectionItems.Add($"Selectable Item {i}");

        // Scrolling
        ScrollItems = new ObservableCollection<string>();
        for (int i = 1; i <= 50; i++) ScrollItems.Add($"Scroll Item {i}");

        // Grouping
        GroupedItems = new ObservableCollection<ListViewGroup>
        {
            new ListViewGroup("Mammals", new List<ItemModel> { new ItemModel("Cat"), new ItemModel("Dog") }),
            new ListViewGroup("Reptiles", new List<ItemModel> { new ItemModel("Snake"), new ItemModel("Lizard") })
        };

        // Header/Footer
        HeaderFooterItems = new ObservableCollection<string> { "A", "B", "C", "D" };

        // Template Selector
        RoleItems = new ObservableCollection<PersonItem>
        {
            new PersonItem("Alice", "Admin"),
            new PersonItem("Bob", "User"),
            new PersonItem("Charlie", "User"),
            new PersonItem("Dave", "Guest"),
            new PersonItem("Eve", "Admin")
        };

        // Runtime Template
        RuntimeTemplateItems = new ObservableCollection<string> { "Template A", "Template B", "Template C" };


        // Uneven Rows
        UnevenItems = new ObservableCollection<string>
        {
            "Short item",
            "This is a much longer item that will definitely take more space if it wraps or is displayed in a way that respects its content size.",
            "Short item 2",
            "Another very long item to demonstrate that when HasUnevenRows is set to true, the ListView should adjust the height of each row to fit the content, rather than using a fixed row height for every single item in the list. This is particularly useful when you have dynamic content.",
            "Short item 3"
        };

        ScrollToCommand = new Command<string>(OnScrollTo);
        LoadPerformanceCommand = new Command<string>(OnLoadPerformanceItems);

        ChangeTemplateCommand = new Command(OnChangeTemplate);
        FavoriteCommand = new Command<object>(s =>
        {
            LogEvent($"Favorite: {s}");
            ContextActionStatus = $"Add to Favorites tapped for {s}";
        });
        DeleteCommand = new Command<object>(s =>
        {
            LogEvent($"Delete: {s}");
            ContextActionStatus = $"Delete tapped for {s}";
        });
        RefreshCommand = new Command(async () =>
        {
            if (IsRefreshing) return;

            IsRefreshing = true;
            LogEvent("Refresh started...");

            await Task.Delay(2000);

            if (RefreshItems.Count > 0)
            {
                RefreshItems[0] = $"Refresh Item 1 (Refreshed {DateTime.Now:T})";
            }

            IsRefreshing = false;
            LogEvent("Refresh completed");
            ContextActionStatus = "Refreshed at " + DateTime.Now.ToLongTimeString();
        });

        EventsRefreshCommand = new Command(async () =>
        {
            if (IsEventsRefreshing) return;

            IsEventsRefreshing = true;
            LogEvent("Events Refresh started...");

            await Task.Delay(1500);

            if (EventItems.Count > 0)
            {
                EventItems[0] = $"Event Item 1 (Refreshed {DateTime.Now:T})";
            }

            IsEventsRefreshing = false;
            LogEvent("Events Refresh completed");
        });

        ContextItems = new ObservableCollection<ContextItemViewModel>();

        for (int i = 1; i <= 20; i++)
        {
            ContextItems.Add(new ContextItemViewModel
            {
                Text = $"Context Item {i}",
                FavoriteCommand = FavoriteCommand,
                DeleteCommand = DeleteCommand
            });
        }

        InitializeComponent();
        BindingContext = this;
    }

    public ObservableCollection<string> BasicItems { get; }
    public ObservableCollection<string> RefreshItems { get; }
    public ObservableCollection<string> EventItems { get; }
    public ObservableCollection<ContextItemViewModel> ContextItems { get; }
    public ObservableCollection<string> SeparatorItems { get; }


    public ObservableCollection<string> PerformanceItems { get; }
    public ObservableCollection<CellTypeItem> CellTypeItems { get; }
    public ObservableCollection<string> SelectionItems { get; }
    public ObservableCollection<string> ScrollItems { get; }
    public ObservableCollection<ListViewGroup> GroupedItems { get; }
    public ObservableCollection<string> HeaderFooterItems { get; }
    public ObservableCollection<PersonItem> RoleItems { get; }
    public ObservableCollection<string> RuntimeTemplateItems { get; }
    public ObservableCollection<string> UnevenItems { get; }

    private string _selectionStatus = "No selection";

    public string SelectionStatus
    {
        get => _selectionStatus;
        set
        {
            _selectionStatus = value;
            OnPropertyChanged();
        }
    }

    private ListViewSelectionMode _currentSelectionMode = ListViewSelectionMode.Single;

    public ListViewSelectionMode CurrentSelectionMode
    {
        get => _currentSelectionMode;
        set
        {
            _currentSelectionMode = value;
            OnPropertyChanged();
        }
    }

    private DataTemplate? _currentRuntimeTemplate;

    public DataTemplate? CurrentRuntimeTemplate
    {
        get => _currentRuntimeTemplate;
        set
        {
            _currentRuntimeTemplate = value;
            OnPropertyChanged();
        }
    }

    public ICommand ScrollToCommand { get; }
    public ICommand LoadPerformanceCommand { get; }

    public ICommand ChangeTemplateCommand { get; }
    public ICommand FavoriteCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand EventsRefreshCommand { get; }

    private string _contextActionStatus = "Context Action Status";
    private bool _isRefreshing;
    private bool _isEventsRefreshing;

    public string ContextActionStatus
    {
        get => _contextActionStatus;
        set
        {
            _contextActionStatus = value;
            OnPropertyChanged();
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public bool IsEventsRefreshing
    {
        get => _isEventsRefreshing;
        set
        {
            _isEventsRefreshing = value;
            OnPropertyChanged();
        }
    }

    private void OnSelectionChanged(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
            SelectionStatus = "No selection";
        else
            SelectionStatus = $"Selected: {e.SelectedItem}";
    }

    private void OnScrollTo(string param)
    {
        if (param == "Start")
            ScrollListView.ScrollTo(ScrollItems.First(), ScrollToPosition.Start, true);
        else if (param == "End")
            ScrollListView.ScrollTo(ScrollItems.Last(), ScrollToPosition.End, true);
        else if (param == "Index25")
        {
            if (ScrollItems.Count >= 25)
                ScrollListView.ScrollTo(ScrollItems[24], ScrollToPosition.Center, true);
        }
    }

    private async void OnLoadPerformanceItems(string countStr)
    {
        if (int.TryParse(countStr, out int count))
        {
            PerformanceItems.Clear();
            await Task.Run(() =>
            {
                var list = new List<string>(count);
                for (int i = 0; i < count; i++)
                {
                    list.Add($"Item {i + 1}");
                }

                Dispatcher.Dispatch(() =>
                {
                    foreach (var item in list) PerformanceItems.Add(item);
                });
            });
        }
    }

    private void OnChangeTemplate()
    {
        if (Resources.TryGetValue("SimpleTemplate", out object t1) &&
            Resources.TryGetValue("DetailedTemplate", out object t2))
        {
            CurrentRuntimeTemplate = (CurrentRuntimeTemplate == (DataTemplate)t1) ? (DataTemplate)t2 : (DataTemplate)t1;
        }
    }

    public void SetSelectionSingle(object sender, EventArgs e) => CurrentSelectionMode = ListViewSelectionMode.Single;

    public void SetSelectionNone(object sender, EventArgs e)
    {
        CurrentSelectionMode = ListViewSelectionMode.None;
        SelectionStatus = "None";
    }

    private void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        LogEvent($"Selected: {e.SelectedItem}");
    }

    private void OnItemTapped(object sender, ItemTappedEventArgs e)
    {
        LogEvent($"Tapped: {e.Item}");
    }

    private void OnItemAppearing(object sender, ItemVisibilityEventArgs e)
    {
        LogEvent($"Appearing: {e.Item}");
    }

    private void OnItemDisappearing(object sender, ItemVisibilityEventArgs e)
    {
        LogEvent($"Disappearing: {e.Item}");
    }

    private void OnScrolled(object sender, ScrolledEventArgs e)
    {
        if (Math.Abs(e.ScrollY % 50) < 5)
            LogEvent($"Scrolled: {e.ScrollX:F1}, {e.ScrollY:F1}");
    }

    private void LogEvent(string msg)
    {
        Debug.WriteLine(msg);

        if (EventLogLabel != null)
        {
            EventLogLabel.Text = $"{DateTime.Now:T}: {msg}\n" + EventLogLabel.Text;
            if (EventLogLabel.Text.Length > 500)
                EventLogLabel.Text = EventLogLabel.Text.Substring(0, 500);
        }
    }

    private void OnToggleSeparatorClicked(object sender, EventArgs e)
    {
        if (SeparatorVisibilityListView.SeparatorVisibility == SeparatorVisibility.Default)
            SeparatorVisibilityListView.SeparatorVisibility = SeparatorVisibility.None;
        else
            SeparatorVisibilityListView.SeparatorVisibility = SeparatorVisibility.Default;
    }

    private void OnSeparatorColorRedClicked(object sender, EventArgs e)
    {
        SeparatorColorListView.SeparatorColor = Colors.Red;
    }

    private void OnSeparatorColorGreenClicked(object sender, EventArgs e)
    {
        SeparatorColorListView.SeparatorColor = Microsoft.Maui.Graphics.Colors.Green;
    }

    private void OnSeparatorColorBlueClicked(object sender, EventArgs e)
    {
        SeparatorColorListView.SeparatorColor = Colors.Blue;
    }

    private void OnScrollNeverClicked(object sender, EventArgs e) =>
        ScrollBarVisibilityListView.VerticalScrollBarVisibility = ScrollBarVisibility.Never;

    private void OnScrollAlwaysClicked(object sender, EventArgs e) =>
        ScrollBarVisibilityListView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;

    private void OnScrollDefaultClicked(object sender, EventArgs e) =>
        ScrollBarVisibilityListView.VerticalScrollBarVisibility = ScrollBarVisibility.Default;
}

public class CellTypeItem
{
    public string? Name { get; set; }
    public string? Detail { get; set; }
    public bool IsToggled { get; set; } // SwitchCell state
    public string? Placeholder { get; set; } // EntryCell placeholder
    public string? Type { get; set; } // Cell type: Text, Image, Switch, Entry, View
}

public class ListViewGroup : ObservableCollection<ItemModel>
{
    public string Name { get; private set; }

    public ListViewGroup(string name, List<ItemModel> items) : base(items)
    {
        Name = name;
    }
}

public class ItemModel
{
    public string Name { get; set; }
    public string Description { get; set; }

    public ItemModel(string name)
    {
        Name = name;
        Description = $"Detail for {name}";
    }
}

public class PersonItem
{
    public string Name { get; }
    public string Role { get; }

    public PersonItem(string name, string role)
    {
        Name = name;
        Role = role;
    }
}

public class CellTypeTemplateSelector : DataTemplateSelector
{
    public DataTemplate? TextTemplate { get; set; }
    public DataTemplate? ImageTemplate { get; set; }
    public DataTemplate? SwitchTemplate { get; set; }
    public DataTemplate? EntryTemplate { get; set; }
    public DataTemplate? ViewTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var cellItem = item as CellTypeItem;
        return cellItem?.Type switch
        {
            "Text" => TextTemplate ?? new DataTemplate(),
            "Image" => ImageTemplate ?? new DataTemplate(),
            "Switch" => SwitchTemplate ?? new DataTemplate(),
            "Entry" => EntryTemplate ?? new DataTemplate(),
            "View" => ViewTemplate ?? new DataTemplate(),
            _ => TextTemplate ?? new DataTemplate()
        };
    }
}

public class ContextItemViewModel
{
    public string? Text { get; set; }
    public ICommand? FavoriteCommand { get; set; }
    public ICommand? DeleteCommand { get; set; }
}

public class RoleTemplateSelector : DataTemplateSelector
{
    public DataTemplate? AdminTemplate { get; set; }
    public DataTemplate? UserTemplate { get; set; }
    public DataTemplate? GuestTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var person = item as PersonItem;
        return person?.Role switch
        {
            "Admin" => AdminTemplate ?? new DataTemplate(),
            "User" => UserTemplate ?? new DataTemplate(),
            "Guest" => GuestTemplate ?? new DataTemplate(),
            _ => GuestTemplate ?? new DataTemplate()
        };
    }
}