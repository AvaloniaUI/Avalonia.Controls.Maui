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

namespace Avalonia.Controls.Maui.Handlers.Shell
{
    public class ShellSearchControl : ContentControl
    {
        private readonly MauiSearchHandler _mauiSearchHandler;
        private readonly IMauiContext _mauiContext;
        
        public MauiSearchHandler SearchHandler => _mauiSearchHandler;

        private MauiSearchBar? _searchBar;
        private Popup? _resultsPopup;
        private ListBox? _resultsList;

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
            
            // Apply colors if set
            if (_mauiSearchHandler.BackgroundColor != Microsoft.Maui.Graphics.Colors.Transparent) // Check default
            {
                 // _searchBar.Background = ... (MauiSearchBar might need property mapping for this)
            }

            if (_mauiSearchHandler.TextColor != null)
            {
                // _searchBar.Foreground = ...
            }

            _searchBar.PropertyChanged += OnSearchBarPropertyChanged;
            _searchBar.SearchButtonPressed += OnSearchButtonPressed;
            _searchBar.KeyDown += OnSearchBarKeyDown;

            // Container for SearchBar and Popup
            var container = new Grid();
            container.Children.Add(_searchBar);

            _resultsPopup = new Popup
            {
                PlacementTarget = _searchBar,
                Placement = PlacementMode.Bottom,
                IsLightDismissEnabled = true,
                OverlayInputPassThroughElement = _searchBar
            };

            _resultsList = new ListBox
            {
                MaxHeight = 300,
                [ScrollViewer.HorizontalScrollBarVisibilityProperty] = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
            };
            
            _resultsList.SelectionChanged += OnResultsListSelectionChanged;

            var border = new Border
            {
                Background = Brushes.White, 
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Child = _resultsList,
                MinWidth = 200
            };
            
            _resultsPopup.Child = border;
            container.Children.Add(_resultsPopup);

            Content = container;

            UpdateItemsSource();
            UpdateItemTemplate();
            UpdateShowsResults();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            
            // Re-evaluate results when attached (e.g. after navigation back)
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

             if (_resultsPopup != null)
             {
                 _resultsPopup.IsOpen = false;
             }
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
                
                if (_resultsPopup != null)
                {
                    _resultsPopup.IsOpen = false;
                }
            }
        }

        private void UpdateShowsResults()
        {
            if (_resultsPopup != null)
            {
                bool hasItems = false;
                if (_resultsList?.ItemsSource is IEnumerable list)
                {
                    // More reliable than ItemCount which might be 0 during synchronization
                    if (list is ICollection collection)
                        hasItems = collection.Count > 0;
                    else
                        hasItems = list.Cast<object>().Any();
                }

                bool show = _mauiSearchHandler.ShowsResults && hasItems;
                _resultsPopup.IsOpen = show;

                if (show)
                {
                    // Ensure width matches or is reasonable
                    if (_searchBar != null && _searchBar.IsVisible)
                    {
                        if (_resultsPopup.Child is Control child)
                        {
                            double width = _searchBar.Bounds.Width;
                            if (width > 0)
                            {
                                child.MinWidth = width;
                            }
                            else
                            {
                                // Fallback if bounds not ready
                                child.MinWidth = 200;
                            }
                        }
                    }
                }
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
                 // We need to wrap the MAUI DataTemplate into an Avalonia IDataTemplate
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
            if (_resultsList?.SelectedItem != null)
            {
                var selectedItem = _resultsList.SelectedItem;
                
                // Update MAUI handler property
                _mauiSearchHandler.SetValue(MauiSearchHandler.SelectedItemProperty, selectedItem);
                
                // Trigger the OnItemSelected method on the handler
                var controller = (ISearchHandlerController)_mauiSearchHandler;
                controller.ItemSelected(selectedItem);
                
                // Clear query after selection to prevent "filling the text and filtering"
                _mauiSearchHandler.Query = string.Empty;
                
                if (_resultsPopup != null) _resultsPopup.IsOpen = false;
                
                
                _resultsList.SelectedItem = null; // Reset selection
            }
        }
    }
    
    // Simple wrapper for MAUI DataTemplate
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
