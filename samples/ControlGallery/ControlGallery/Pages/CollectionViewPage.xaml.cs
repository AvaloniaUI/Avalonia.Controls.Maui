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

    // Selected item for SelectionChangedCommand demo
    private string? _selectionCommandSelectedItem;
    public string? SelectionCommandSelectedItem
    {
        get => _selectionCommandSelectedItem;
        set
        {
            _selectionCommandSelectedItem = value;
            OnPropertyChanged();
        }
    }

    // Empty view items using observable collection for dynamic updates
    public ObservableCollection<string> EmptyViewItems { get; } = new();

    // Empty view template items using observable collection for dynamic updates
    public ObservableCollection<string> EmptyViewTemplateItems { get; } = new();

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
        "Apples", "Bananas", "Milk", "Bread",
        "Cheese", "Eggs", "Carrots", "Tomatoes"
    };

    // Multiple selection items
    public List<string> MultiSelectItems { get; } = new()
    {
        "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet"
    };

    // Infinite scroll items using observable collection for dynamic loading
    public ObservableCollection<string> InfiniteScrollItems { get; } = new();
    private bool _isLoadingMore = false;
    private int _infiniteScrollBatch = 0;

    // Customized Grouping data
    public List<VegetableGroup> GroupedVegetables { get; } = new()
    {
        new VegetableGroup("Root Vegetables", new List<Vegetable>
        {
            new("Carrot", "Deep orange and crunchy", Colors.Orange),
            new("Potato", "Versatile and starchy", Colors.Tan),
            new("Beetroot", "Vibrant purple and earthy", Colors.Purple)
        }),
        new VegetableGroup("Leafy Greens", new List<Vegetable>
        {
            new("Spinach", "Nutrient-dense and tender", Colors.Green),
            new("Kale", "Hearty and fibrous", Colors.DarkGreen),
            new("Lettuce", "Crisp and refreshing", Colors.LightGreen)
        }),
        new VegetableGroup("Cruciferous", new List<Vegetable>
        {
            new("Broccoli", "Tree-like and healthy", Colors.ForestGreen),
            new("Cauliflower", "White and versatile", Colors.GhostWhite)
        })
    };

    // Inventory items for customized header/footer
    public List<InventoryItem> InventoryItems { get; } = new()
    {
        new InventoryItem("Laptop", 5, 1200),
        new InventoryItem("Monitor", 12, 300),
        new InventoryItem("Keyboard", 25, 50),
        new InventoryItem("Mouse", 30, 25),
        new InventoryItem("Printer", 3, 200)
    };

    public double TotalInventoryValue => InventoryItems.Sum(i => i.TotalValue);

    public CollectionViewPage()
    {
        InitializeComponent();

        // Initialize with some items for empty view demo
        EmptyViewItems.Add("Initial Item 1");
        EmptyViewItems.Add("Initial Item 2");
        EmptyViewItems.Add("Initial Item 3");

        // Initialize with some items for empty view template demo
        EmptyViewTemplateItems.Add("Template Item 1");
        EmptyViewTemplateItems.Add("Template Item 2");
        EmptyViewTemplateItems.Add("Template Item 3");

        // Initialize infinite scroll items
        LoadInitialInfiniteScrollItems();

        // Initialize layout variations
        UpdateListLayout();
        UpdateGridLayout();
        UpdateHGridLayout();

        SelectionCommand = new Command<string>(OnSelectionCommandExecuted);
        LoadMoreCommand = new Command(OnLoadMoreItemsCommandExecuted);

        BindingContext = this;
    }

    public Command<string> SelectionCommand { get; }
    public Command LoadMoreCommand { get; }

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

    // Layout Variation Properties
    private double _listSpacing = 0;
    public double ListSpacing
    {
        get => _listSpacing;
        set
        {
            _listSpacing = value;
            OnPropertyChanged();
            UpdateListLayout();
        }
    }

    private int _gridSpan = 2;
    public int GridSpan
    {
        get => _gridSpan;
        set
        {
            _gridSpan = value;
            OnPropertyChanged();
            UpdateGridLayout();
        }
    }

    private double _gridVerticalSpacing = 0;
    public double GridVerticalSpacing
    {
        get => _gridVerticalSpacing;
        set
        {
            _gridVerticalSpacing = value;
            OnPropertyChanged();
            UpdateGridLayout();
        }
    }

    private double _gridHorizontalSpacing = 0;
    public double GridHorizontalSpacing
    {
        get => _gridHorizontalSpacing;
        set
        {
            _gridHorizontalSpacing = value;
            OnPropertyChanged();
            UpdateGridLayout();
        }
    }

    private int _hGridSpan = 2;
    public int HGridSpan
    {
        get => _hGridSpan;
        set
        {
            _hGridSpan = value;
            OnPropertyChanged();
            UpdateHGridLayout();
        }
    }

    public List<ColorItem> LayoutItems { get; } = new()
    {
        new(Colors.Red, "Red"), new(Colors.Blue, "Blue"), new(Colors.Green, "Green"),
        new(Colors.Yellow, "Yellow"), new(Colors.Orange, "Orange"), new(Colors.Purple, "Purple"),
        new(Colors.Pink, "Pink"), new(Colors.Brown, "Brown"), new(Colors.Gray, "Gray"),
        new(Colors.Black, "Black"), new(Colors.Cyan, "Cyan"), new(Colors.Magenta, "Magenta")
    };

    private void UpdateListLayout()
    {
        if (VerticalListCollection == null) return;
        VerticalListCollection.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
        {
            ItemSpacing = ListSpacing
        };
    }

    private void UpdateGridLayout()
    {
        if (VerticalGridCollection == null) return;
        VerticalGridCollection.ItemsLayout = new GridItemsLayout(GridSpan, ItemsLayoutOrientation.Vertical)
        {
            VerticalItemSpacing = GridVerticalSpacing,
            HorizontalItemSpacing = GridHorizontalSpacing
        };
    }

    private void UpdateHGridLayout()
    {
        if (HorizontalGridCollection == null) return;
        HorizontalGridCollection.ItemsLayout = new GridItemsLayout(HGridSpan, ItemsLayoutOrientation.Horizontal)
        {
            VerticalItemSpacing = 5,
            HorizontalItemSpacing = 5
        };
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

    private void OnClearTemplateItems(object? sender, EventArgs e)
    {
        EmptyViewTemplateItems.Clear();
    }

    private void OnAddTemplateItems(object? sender, EventArgs e)
    {
        var count = EmptyViewTemplateItems.Count;
        EmptyViewTemplateItems.Add($"New 10 items {count + 1}");
        EmptyViewTemplateItems.Add($"New Template Item {count + 2}");
        EmptyViewTemplateItems.Add($"New Template Item {count + 3}");
    }

    private void OnResetInfiniteList(object? sender, EventArgs e)
    {
        LoadInitialInfiniteScrollItems();
        LoadingLabel.Text = "";
    }

    private async void OnLoadMoreItems(object? sender, EventArgs e)
    {
        if (_isLoadingMore || _infiniteScrollBatch >= 5) // Limit to 5 batches or 100 items total
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

    private void OnScrollToIndex(object? sender, EventArgs e)
    {
        // Use 1-based input for user intuitiveness where Item 1 corresponds to index 0
        if (int.TryParse(ScrollIndexEntry.Text, out int itemNumber) && itemNumber >= 1 && itemNumber <= LargeList.Count)
        {
            var index = itemNumber - 1;
            ScrollingCollection.ScrollTo(index, position: ScrollToPosition.Start);
        }
        else
        {
            DisplayAlert("Invalid Number", $"Please enter a number between 1 and {LargeList.Count}", "OK");
        }
    }

    private void OnScrollToItem(object? sender, EventArgs e)
    {
        var item = LargeList.FirstOrDefault(i => i.Contains("25"));
        if (item != null)
        {
            ScrollingCollection.ScrollTo(item, position: ScrollToPosition.Center);
        }
    }
    
    private void OnCollectionViewScrolled(object? sender, ItemsViewScrolledEventArgs e)
    {
        ScrollInfoLabel.Text = $"Scrolled: H:{e.HorizontalOffset:F2}, V:{e.VerticalOffset:F2}";
    }

    private void OnVisibilitySampleChanged(object? sender, EventArgs e)
    {
        if (VisibilityCollection == null || VisibilitySampleVerticalPicker == null || VisibilitySampleHorizontalPicker == null)
            return;

        if (VisibilitySampleVerticalPicker.SelectedItem is ScrollBarVisibility verticalVisibility)
        {
            VisibilityCollection.VerticalScrollBarVisibility = verticalVisibility;
        }

        if (VisibilitySampleHorizontalPicker.SelectedItem is ScrollBarVisibility horizontalVisibility)
        {
            VisibilityCollection.HorizontalScrollBarVisibility = horizontalVisibility;
        }
    }

    private void UpdateInfiniteScrollCount()
    {
        InfiniteScrollCountLabel.Text = $"Items loaded: {InfiniteScrollItems.Count}";
    }

    private void OnSelectionCommandExecuted(string parameter)
    {
        SelectionCommandLabel.Text = $"Command executed for: {parameter}";
    }

    private int _loadMoreCount = 0;
    private void OnLoadMoreItemsCommandExecuted()
    {
        _loadMoreCount++;
        LoadMoreCommandLabel.Text = $"LoadMoreCommand executed: {_loadMoreCount} times";
    }
    
    private void OnFavorite(object? sender, EventArgs e)
    {
        var item = (sender as SwipeItem)?.BindingContext as Person;
        if (item != null)
        {
            DisplayAlert("Favorite", $"{item.Name} added to favorites", "OK");
        }
    }

    private void OnDelete(object? sender, EventArgs e)
    {
        var item = (sender as SwipeItem)?.BindingContext as Person;
        if (item != null)
        {
            DisplayAlert("Delete", $"{item.Name} deleted", "OK");
        }
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

// Vegetable model for customized grouping
public class Vegetable
{
    public string Name { get; }
    public string Description { get; }
    public Color Color { get; }

    public Vegetable(string name, string description, Color color)
    {
        Name = name;
        Description = description;
        Color = color;
    }
}

public class VegetableGroup : List<Vegetable>
{
    public string Name { get; }

    public VegetableGroup(string name, List<Vegetable> vegetables) : base(vegetables)
    {
        Name = name;
    }
}

// Inventory item for customized header/footer
public class InventoryItem
{
    public string Name { get; }
    public int Quantity { get; }
    public double Price { get; }
    public double TotalValue => Quantity * Price;

    public InventoryItem(string name, int quantity, double price)
    {
        Name = name;
        Quantity = quantity;
        Price = price;
    }
}

public record ColorItem(Color Color, string Name);