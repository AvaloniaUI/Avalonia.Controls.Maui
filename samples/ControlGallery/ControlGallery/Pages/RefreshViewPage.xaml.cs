using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ControlGallery.Pages;

/// <summary>
/// Sample page demonstrating the RefreshView control functionality.
/// </summary>
public partial class RefreshViewPage : ContentPage
{
    private bool _isRefreshing;
    private bool _refreshEnabled = true;
    private bool _colorDemoRefreshing;
    private bool _listRefreshing;
    private bool _interactiveRefreshing;
    private bool _canRefreshInteractive = true;
    private Color _refreshColor = Colors.Blue;
    private int _refreshCount;
    private int _basicItemCounter;
    private int _itemCounter;
    private int _interactiveRefreshCount;
    private int _eventHandlerCount;
    private string _lastCommandParameter = "None";

    /// <summary>
    /// Initializes a new instance of the RefreshViewPage class.
    /// </summary>
    public RefreshViewPage()
    {
        InitializeComponent();

        BasicItems = new ObservableCollection<string>();
        Items = new ObservableCollection<string>();
        InteractiveItems = new ObservableCollection<string>();
        LoadInitialItems();

        RefreshCommand = new Command(async () => await ExecuteRefreshAsync());
        StopRefreshCommand = new Command(() => IsRefreshing = false);
        SetColorCommand = new Command<string>(SetRefreshColor);
        RefreshListCommand = new Command(async () => await RefreshListAsync());
        ClearListCommand = new Command(() =>
        {
            Items.Clear();
            _itemCounter = 0;
            OnPropertyChanged(nameof(Items));
        });

        InteractiveRefreshCommand = new Command<string>(
            async (param) => await ExecuteInteractiveRefreshAsync(param),
            (param) => CanRefreshInteractive);

        ManualRefreshCommand = new Command(
            () => { InteractiveRefreshing = true; },
            () => CanRefreshInteractive);

        BindingContext = this;
    }

    /// <summary>
    /// Gets or sets whether the refresh operation is in progress.
    /// </summary>
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            if (_isRefreshing != value)
            {
                _isRefreshing = value;
                if (value)
                {
                    RefreshCount++;
                }
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the refresh view is enabled.
    /// </summary>
    public bool RefreshEnabled
    {
        get => _refreshEnabled;
        set
        {
            if (_refreshEnabled != value)
            {
                _refreshEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the color demo refresh view is refreshing.
    /// </summary>
    public bool ColorDemoRefreshing
    {
        get => _colorDemoRefreshing;
        set
        {
            if (_colorDemoRefreshing != value)
            {
                _colorDemoRefreshing = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the list refresh view is refreshing.
    /// </summary>
    public bool ListRefreshing
    {
        get => _listRefreshing;
        set
        {
            if (_listRefreshing != value)
            {
                _listRefreshing = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the color of the refresh indicator.
    /// </summary>
    public Color RefreshColor
    {
        get => _refreshColor;
        set
        {
            if (_refreshColor != value)
            {
                _refreshColor = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the number of times a refresh has been triggered.
    /// </summary>
    public int RefreshCount
    {
        get => _refreshCount;
        private set
        {
            if (_refreshCount != value)
            {
                _refreshCount = value;
                OnPropertyChanged();
            }
        }
    }

    public bool InteractiveRefreshing
    {
        get => _interactiveRefreshing;
        set
        {
            if (_interactiveRefreshing != value)
            {
                _interactiveRefreshing = value;
                OnPropertyChanged();
            }
        }
    }

    public bool CanRefreshInteractive
    {
        get => _canRefreshInteractive;
        set
        {
            if (_canRefreshInteractive != value)
            {
                _canRefreshInteractive = value;
                OnPropertyChanged();
                if (InteractiveRefreshCommand is Command interactiveCmd)
                {
                    interactiveCmd.ChangeCanExecute();
                }
                if (ManualRefreshCommand is Command manualCmd)
                {
                    manualCmd.ChangeCanExecute();
                }
            }
        }
    }

    public int InteractiveRefreshCount
    {
        get => _interactiveRefreshCount;
        set
        {
            if (_interactiveRefreshCount != value)
            {
                _interactiveRefreshCount = value;
                OnPropertyChanged();
            }
        }
    }

    public int EventHandlerCount
    {
        get => _eventHandlerCount;
        set
        {
            if (_eventHandlerCount != value)
            {
                _eventHandlerCount = value;
                OnPropertyChanged();
            }
        }
    }

    public string LastCommandParameter
    {
        get => _lastCommandParameter;
        set
        {
            if (_lastCommandParameter != value)
            {
                _lastCommandParameter = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the collection of items for the basic refresh demo.
    /// </summary>
    public ObservableCollection<string> BasicItems { get; }

    /// <summary>
    /// Gets the collection of items for the list demo.
    /// </summary>
    public ObservableCollection<string> Items { get; }

    public ObservableCollection<string> InteractiveItems { get; }

    /// <summary>
    /// Gets the command to trigger a refresh.
    /// </summary>
    public ICommand RefreshCommand { get; }

    /// <summary>
    /// Gets the command to stop a refresh.
    /// </summary>
    public ICommand StopRefreshCommand { get; }

    /// <summary>
    /// Gets the command to set the refresh color.
    /// </summary>
    public ICommand SetColorCommand { get; }

    /// <summary>
    /// Gets the command to refresh the list.
    /// </summary>
    public ICommand RefreshListCommand { get; }

    /// <summary>
    /// Gets the command to clear the list.
    /// </summary>
    public ICommand ClearListCommand { get; }

    public ICommand InteractiveRefreshCommand { get; }

    public ICommand ManualRefreshCommand { get; }


    private void LoadInitialItems()
    {
        for (int i = 1; i <= 8; i++)
        {
            _basicItemCounter++;
            BasicItems.Add($"Item {_basicItemCounter}");
        }

        for (int i = 1; i <= 10; i++)
        {
            _itemCounter++;
            Items.Add($"Item {_itemCounter}");
            InteractiveItems.Add($"Interactive Item {_itemCounter}");
        }
    }

    private async Task ExecuteRefreshAsync()
    {
        IsRefreshing = true;

        await Task.Delay(1500);

        _basicItemCounter++;
        BasicItems.Insert(0, $"New Item {_basicItemCounter}");

        IsRefreshing = false;
    }

    private void SetRefreshColor(string colorName)
    {
        RefreshColor = colorName switch
        {
            "Red" => Colors.Red,
            "Green" => Colors.Green,
            "Blue" => Colors.Blue,
            "Teal" => Colors.Teal,
            _ => Colors.Blue
        };
    }

    private async Task RefreshListAsync()
    {
        ListRefreshing = true;

        await Task.Delay(1500);

        for (int i = 0; i < 3; i++)
        {
            _itemCounter++;
            Items.Insert(0, $"New Item {_itemCounter}");
        }

        ListRefreshing = false;
    }

    private async Task ExecuteInteractiveRefreshAsync(string parameter)
    {
        if (InteractiveRefreshing) return;
        InteractiveRefreshing = true;
        LastCommandParameter = parameter ?? "Null";
        InteractiveRefreshCount++;

        await Task.Delay(1500);

        _itemCounter++;
        InteractiveItems.Insert(0, $"Refreshed Item {_itemCounter}");

        InteractiveRefreshing = false;
    }

    private void OnRefreshing(object sender, EventArgs e)
    {
        EventHandlerCount++;
    }
}
