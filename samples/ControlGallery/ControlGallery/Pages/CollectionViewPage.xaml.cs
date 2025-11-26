using System.Collections.ObjectModel;

namespace ControlGallery.Pages;

public partial class CollectionViewPage : ContentPage
{
    // Simple list of strings
    public List<string> SimpleItems { get; } = new()
    {
        "Apple", "Banana", "Cherry", "Date", "Elderberry",
        "Fig", "Grape", "Honeydew"
    };

    // Grid items
    public List<string> GridItems { get; } = new()
    {
        "Item 1", "Item 2", "Item 3", "Item 4",
        "Item 5", "Item 6", "Item 7", "Item 8",
        "Item 9", "Item 10", "Item 11", "Item 12"
    };

    // Horizontal items
    public List<string> HorizontalItems { get; } = new()
    {
        "H1", "H2", "H3", "H4", "H5", "H6", "H7", "H8", "H9", "H10"
    };

    // Selectable items
    public List<string> SelectableItems { get; } = new()
    {
        "Option A", "Option B", "Option C", "Option D", "Option E"
    };

    // Selected item for binding
    private string? _selectedItem;
    public string? SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            OnPropertyChanged();
        }
    }

    // Empty view items - observable for dynamic add/remove
    public ObservableCollection<string> EmptyViewItems { get; } = new();

    // Grouped animals
    public List<AnimalGroup> GroupedAnimals { get; } = new()
    {
        new AnimalGroup("Mammals", new List<Animal>
        {
            new("Lion"), new("Elephant"), new("Dolphin"), new("Bear")
        }),
        new AnimalGroup("Birds", new List<Animal>
        {
            new("Eagle"), new("Penguin"), new("Owl")
        }),
        new AnimalGroup("Reptiles", new List<Animal>
        {
            new("Snake"), new("Crocodile"), new("Turtle"), new("Lizard"), new("Gecko")
        })
    };

    // People with custom template
    public List<Person> People { get; } = new()
    {
        new Person("Alice Johnson", "alice@example.com", "Developer", Colors.Purple),
        new Person("Bob Smith", "bob@example.com", "Designer", Colors.Blue),
        new Person("Carol White", "carol@example.com", "Manager", Colors.Green),
        new Person("David Brown", "david@example.com", "Analyst", Colors.Orange),
        new Person("Eve Davis", "eve@example.com", "Tester", Colors.Red)
    };

    // Large list for scrollbar demo
    public List<string> LargeList { get; } = Enumerable.Range(1, 50)
        .Select(i => $"Item number {i}")
        .ToList();

    // Header/Footer items
    public List<string> HeaderFooterItems { get; } = new()
    {
        "🍎 Apples", "🍌 Bananas", "🥛 Milk", "🍞 Bread",
        "🧀 Cheese", "🥚 Eggs", "🥕 Carrots", "🍅 Tomatoes"
    };

    // Multiple selection items
    public List<string> MultiSelectItems { get; } = new()
    {
        "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet"
    };

    // Infinite scroll items - observable for dynamic loading
    public ObservableCollection<string> InfiniteScrollItems { get; } = new();
    private bool _isLoadingMore = false;
    private int _infiniteScrollBatch = 0;

    public CollectionViewPage()
    {
        InitializeComponent();
        BindingContext = this;

        // Initialize with some items for empty view demo
        EmptyViewItems.Add("Initial Item 1");
        EmptyViewItems.Add("Initial Item 2");
        EmptyViewItems.Add("Initial Item 3");

        // Initialize infinite scroll items
        LoadInitialInfiniteScrollItems();
    }

    private void LoadInitialInfiniteScrollItems()
    {
        _infiniteScrollBatch = 0;
        InfiniteScrollItems.Clear();
        for (int i = 1; i <= 20; i++)
        {
            InfiniteScrollItems.Add($"Item {i}");
        }
        _infiniteScrollBatch = 1;
        UpdateInfiniteScrollCount();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string selected)
        {
            SelectionLabel.Text = $"Selected: {selected}";
        }
        else
        {
            SelectionLabel.Text = "No item selected";
        }
    }

    private void OnMultiSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedCount = e.CurrentSelection.Count;
        if (selectedCount > 0)
        {
            var selectedItems = string.Join(", ", e.CurrentSelection.Cast<string>());
            MultiSelectionLabel.Text = $"Selected ({selectedCount}): {selectedItems}";
        }
        else
        {
            MultiSelectionLabel.Text = "No items selected";
        }
    }

    private void OnClearItems(object? sender, EventArgs e)
    {
        EmptyViewItems.Clear();
    }

    private void OnAddItems(object? sender, EventArgs e)
    {
        var count = EmptyViewItems.Count;
        EmptyViewItems.Add($"New Item {count + 1}");
        EmptyViewItems.Add($"New Item {count + 2}");
        EmptyViewItems.Add($"New Item {count + 3}");
    }

    private void OnResetInfiniteList(object? sender, EventArgs e)
    {
        LoadInitialInfiniteScrollItems();
        LoadingLabel.Text = "";
    }

    private async void OnLoadMoreItems(object? sender, EventArgs e)
    {
        if (_isLoadingMore || _infiniteScrollBatch >= 5) // Limit to 5 batches (100 items total)
            return;

        _isLoadingMore = true;
        LoadingLabel.Text = "Loading...";

        // Simulate network delay
        await Task.Delay(500);

        _infiniteScrollBatch++;
        var startIndex = InfiniteScrollItems.Count + 1;
        for (int i = 0; i < 20; i++)
        {
            InfiniteScrollItems.Add($"Item {startIndex + i}");
        }

        UpdateInfiniteScrollCount();
        LoadingLabel.Text = _infiniteScrollBatch >= 5 ? "All items loaded" : "";
        _isLoadingMore = false;
    }

    private void UpdateInfiniteScrollCount()
    {
        InfiniteScrollCountLabel.Text = $"Items loaded: {InfiniteScrollItems.Count}";
    }
}

// Animal model for grouping
public class Animal
{
    public string Name { get; }

    public Animal(string name)
    {
        Name = name;
    }
}

// Animal group for grouped collection
public class AnimalGroup : List<Animal>
{
    public string Name { get; }

    public AnimalGroup(string name, List<Animal> animals) : base(animals)
    {
        Name = name;
    }
}

// Person model for custom template
public class Person
{
    public string Name { get; }
    public string Email { get; }
    public string Role { get; }
    public Color Color { get; }
    public string Initials => string.Join("", Name.Split(' ').Select(n => n[0]));

    public Person(string name, string email, string role, Color color)
    {
        Name = name;
        Email = email;
        Role = role;
        Color = color;
    }
}
