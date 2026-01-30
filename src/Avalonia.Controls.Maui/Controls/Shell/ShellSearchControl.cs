using System.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Controls.Maui.Controls;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using AvaloniaControl = Avalonia.Controls.Control;
using MauiSearchHandler = Microsoft.Maui.Controls.SearchHandler;
using Avalonia.VisualTree;
using Avalonia.Controls.Maui.Extensions;

namespace Avalonia.Controls.Maui.Handlers.Shell
{
    public class ShellSearchControl : ContentControl
    {
        private readonly MauiSearchHandler _mauiSearchHandler;
        private readonly IMauiContext _mauiContext;
        
        public MauiSearchHandler SearchHandler => _mauiSearchHandler;

        private MauiSearchBar? _searchBar;
        private Control? _resultsOverlay;
        private ListBox? _resultsList;
        private bool _isNavigating;
        private IDisposable? _clickObserver;

        public ShellSearchControl(MauiSearchHandler mauiSearchHandler, IMauiContext mauiContext)
        {
            _mauiSearchHandler = mauiSearchHandler ?? throw new ArgumentNullException(nameof(mauiSearchHandler));
            _mauiContext = mauiContext ?? throw new ArgumentNullException(nameof(mauiContext));

            _mauiSearchHandler.PropertyChanged += OnMauiSearchHandlerPropertyChanged;
            _mauiSearchHandler.FocusChangeRequested += OnFocusChangeRequested;

            CreateControl();
        }

        private void CreateControl()
        {
            _searchBar = new MauiSearchBar
            {
                Text = _mauiSearchHandler.Query,
                Placeholder = _mauiSearchHandler.Placeholder,
                IsReadOnly = !_mauiSearchHandler.IsSearchEnabled,
                HorizontalAlignment = Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Layout.VerticalAlignment.Center,
                Margin = new Thickness(10, 5)
            };
            
            _searchBar.PropertyChanged += OnSearchBarPropertyChanged;
            _searchBar.SearchButtonPressed += OnSearchButtonPressed;
            _searchBar.KeyDown += OnSearchBarKeyDown;

            var container = new StackPanel
            {
                ClipToBounds = false
            };
            container.Children.Add(_searchBar);

            _resultsList = new ListBox
            {
                MaxHeight = 300,
                [ScrollViewer.HorizontalScrollBarVisibilityProperty] = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                Focusable = false,
                Padding = new Thickness(0)
            };
            
            UpdateBackground();
            
            _resultsList.SelectionChanged += OnResultsListSelectionChanged;

            var border = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(0, 0, 4, 4),
                Child = _resultsList,
                IsVisible = false
            };
            
            border.Bind(Avalonia.Controls.Border.BackgroundProperty, _resultsList.GetObservable(Avalonia.Controls.Primitives.TemplatedControl.BackgroundProperty));
            border.Width = Bounds.Width;

            var overlayCanvas = new Canvas
            {
                Height = 0,
                ClipToBounds = false,
                IsHitTestVisible = true,
                ZIndex = 999
            };
            
            overlayCanvas.Children.Add(border);
            container.Children.Add(overlayCanvas);

            _resultsOverlay = border;

            Content = container;
            this.ClipToBounds = false;

            UpdateItemsSource();
            UpdateItemTemplate();
            UpdateShowsResults();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == BoundsProperty && _resultsOverlay != null)
            {
                _resultsOverlay.Width = Bounds.Width;
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            
            UpdateShowsResults();
        }

        public void CleanUp()
        {
             if (_mauiSearchHandler != null)
             {
                 _mauiSearchHandler.PropertyChanged -= OnMauiSearchHandlerPropertyChanged;
                 _mauiSearchHandler.FocusChangeRequested -= OnFocusChangeRequested;
             }
             
             if (_searchBar != null)
             {
                 _searchBar.PropertyChanged -= OnSearchBarPropertyChanged;
                 _searchBar.SearchButtonPressed -= OnSearchButtonPressed;
                 _searchBar.KeyDown -= OnSearchBarKeyDown;
             }

             if (_resultsList != null)
             {
                 _resultsList.SelectionChanged -= OnResultsListSelectionChanged;
             }

             if (_resultsOverlay != null)
             {
                 SetPopupOpen(false);
             }
             
             _clickObserver?.Dispose();
             _clickObserver = null;
        }

        private void OnMauiSearchHandlerPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == MauiSearchHandler.QueryProperty.PropertyName)
            {
                 if (_searchBar != null && _searchBar.Text != _mauiSearchHandler.Query)
                 {
                     _searchBar.Text = _mauiSearchHandler.Query;
                 }
            }
            else if (e.PropertyName == MauiSearchHandler.PlaceholderProperty.PropertyName)
            {
                if (_searchBar != null)
                    _searchBar.Placeholder = _mauiSearchHandler.Placeholder;
            }
            else if (e.PropertyName == MauiSearchHandler.IsSearchEnabledProperty.PropertyName)
            {
                if (_searchBar != null)
                    _searchBar.IsReadOnly = !_mauiSearchHandler.IsSearchEnabled;
            }
            else if (e.PropertyName == MauiSearchHandler.ShowsResultsProperty.PropertyName)
            {
                UpdateShowsResults();
            }
            else if (e.PropertyName == MauiSearchHandler.ItemsSourceProperty.PropertyName)
            {
                UpdateItemsSource();
            }
            else if (e.PropertyName == MauiSearchHandler.ItemTemplateProperty.PropertyName)
            {
                UpdateItemTemplate();
            }
            else if (e.PropertyName == MauiSearchHandler.BackgroundColorProperty.PropertyName)
            {
                UpdateBackground();
            }
        }
        
        private void UpdateBackground()
        {
            if (_resultsList == null) return;

            var mauiColor = _mauiSearchHandler.BackgroundColor;
            if (mauiColor != null && mauiColor != Microsoft.Maui.Graphics.Colors.Transparent)
            {
                _resultsList.Background = mauiColor.ToPlatform();
            }
            else
            {
                // Fallback to theme-aware brush
                _resultsList.Bind(Avalonia.Controls.Primitives.TemplatedControl.BackgroundProperty, 
                    new Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension("SystemRegionBrush"));
            }
        }

        private void OnFocusChangeRequested(object? sender, VisualElement.FocusRequestArgs e)
        {
            if (e.Focus)
            {
                _searchBar?.Focus();
                e.Result = true;
            }
        }

        private void OnSearchBarPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            // Skip if we're navigating away to prevent query restoration
            if (_isNavigating) return;

            if (e.Property == MauiSearchBar.TextProperty && _mauiSearchHandler != null && _searchBar != null)
            {
                // Update MAUI property
                _mauiSearchHandler.SetValue(MauiSearchHandler.QueryProperty, _searchBar.Text);
            }
        }

        private void OnSearchBarTextChanged(object? sender, EventArgs e)
        {
            // Handled by OnSearchBarPropertyChanged
        }

        private void OnSearchButtonPressed(object? sender, RoutedEventArgs e)
        {
            // The search query is synchronized via OnSearchBarTextChanged. 
            // Any additional search action can be triggered here if needed.
        }
        
        private void OnSearchBarKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_mauiSearchHandler.Command != null && _mauiSearchHandler.Command.CanExecute(_mauiSearchHandler.CommandParameter))
                {
                    _mauiSearchHandler.Command.Execute(_mauiSearchHandler.CommandParameter);
                }
                
                SetPopupOpen(false);
            }
        }

        private void UpdateShowsResults()
        {
            if (_isNavigating) return;

            if (_resultsOverlay != null)
            {
                bool hasItems = false;
                if (_resultsList?.ItemsSource is IEnumerable list)
                {
                    if (list is ICollection collection)
                        hasItems = collection.Count > 0;
                    else
                        hasItems = list.Cast<object>().Any();
                }

                bool show = _mauiSearchHandler.ShowsResults && hasItems && !string.IsNullOrWhiteSpace(_mauiSearchHandler.Query);
                SetPopupOpen(show);
            }
        }

        private void UpdateItemsSource()
        {
            if (_resultsList == null) return;

            var itemsSource = _mauiSearchHandler.ItemsSource;
            _resultsList.ItemsSource = itemsSource;
            
            UpdateShowsResults();
        }

        private void UpdateItemTemplate()
        {
            if (_resultsList == null) return;
            
            var template = _mauiSearchHandler.ItemTemplate;
            if (template != null)
            {
                 _resultsList.ItemTemplate = new MauiDataTemplateWrapper(template, _mauiContext);
            }
            else
            {

                 // Default template (TextBlock)
                 _resultsList.ItemTemplate = new FuncDataTemplate<object>((item, ns) => 
                 {
                     var textBlock = new TextBlock 
                     { 
                         Margin = new Thickness(5),
                         VerticalAlignment = Layout.VerticalAlignment.Center
                     };

                     if (!string.IsNullOrEmpty(_mauiSearchHandler.DisplayMemberName))
                     {
                         // Use Avalonia Binding to avoid reflection
#pragma warning disable IL2026, IL3050
                         textBlock.Bind(TextBlock.TextProperty, new Avalonia.Data.Binding(_mauiSearchHandler.DisplayMemberName));
#pragma warning restore IL2026, IL3050
                     }
                     else
                     {
                         // Use simple string conversion
                         textBlock.Text = item?.ToString() ?? "";
                     }
                     
                     return textBlock;
                  });
            }
        }

        private void OnResultsListSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selectedItem = e.AddedItems[0];
                if (selectedItem == null) return;

                _isNavigating = true;

                SetPopupOpen(false);

                if (_searchBar != null)
                {
                    _searchBar.Text = string.Empty;
                    _searchBar.IsEnabled = false;
                    _searchBar.Focusable = false;
                    _searchBar.IsHitTestVisible = false;
                }

                var topLevel = TopLevel.GetTopLevel(this);
                topLevel?.FocusManager?.ClearFocus();

                if (_mauiSearchHandler != null)
                {
                    _mauiSearchHandler.SetValue(MauiSearchHandler.QueryProperty, string.Empty);
                }

                _mauiSearchHandler?.SetValue(MauiSearchHandler.SelectedItemProperty, selectedItem);
                
                var controller = (ISearchHandlerController?)_mauiSearchHandler;
                controller?.ItemSelected(selectedItem);

                Avalonia.Threading.Dispatcher.UIThread.Post(() => 
                {
                    if (_resultsList != null) _resultsList.SelectedItem = null;
                    if (_resultsList != null) _resultsList.SelectedItem = null;
                    if (_searchBar != null)
                    {
                        _searchBar.IsEnabled = true;
                        _searchBar.Focusable = true;
                        _searchBar.IsHitTestVisible = true;
                    }
                    _isNavigating = false;
                }, Avalonia.Threading.DispatcherPriority.Background);
            }
        }

        private void SetPopupOpen(bool open)
        {
            if (_resultsOverlay == null) return;
            
            if (open != _resultsOverlay.IsVisible)
            {
                _resultsOverlay.IsVisible = open;
                
                if (open)
                {
                     var topLevel = TopLevel.GetTopLevel(this);
                     if (topLevel != null)
                     {
                         _clickObserver = topLevel.AddDisposableHandler(
                             Avalonia.Input.InputElement.PointerPressedEvent, 
                             OnTopLevelPointerPressed, 
                             Avalonia.Interactivity.RoutingStrategies.Tunnel); 
                     }
                }
                else
                {
                     _clickObserver?.Dispose();
                     _clickObserver = null;
                     if (_resultsList != null) _resultsList.SelectedItem = null;
                }
            }
        }

        private void OnTopLevelPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_resultsOverlay?.IsVisible == true)
            {
                var source = e.Source as Visual;
                if (source != null)
                {
                    if (this.IsVisualAncestorOf(source) || _resultsOverlay.IsVisualAncestorOf(source))
                    {
                        return;
                    }
                }
                
                // Click was outside, close overlay
                SetPopupOpen(false);
            }
        }
    }
    
    public class MauiDataTemplateWrapper : IDataTemplate
    {
        private readonly DataTemplate _mauiTemplate;
        private readonly IMauiContext _mauiContext;

        public MauiDataTemplateWrapper(DataTemplate mauiTemplate, IMauiContext mauiContext)
        {
            _mauiTemplate = mauiTemplate;
            _mauiContext = mauiContext;
        }

        public AvaloniaControl Build(object? param)
        {
            var content = _mauiTemplate.CreateContent();
            if (content is View view)
            {
                view.BindingContext = param;
                var handler = view.ToHandler(_mauiContext);
                if (handler?.PlatformView is AvaloniaControl control)
                {
                    return control;
                }
            }
            
            return new TextBlock { Text = "Template Error" };
        }

        public bool Match(object? data) => true;
    }
}
