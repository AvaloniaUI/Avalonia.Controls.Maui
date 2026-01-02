using System.Diagnostics;
using _2048Game.Converters;
using _2048Game.Enums;
using _2048Game.Models;
using _2048Game.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace _2048Game;

public partial class MainPage : ContentPage
{
    private MainPageViewModel CurrentViewModel;
    private readonly Dictionary<Guid, Border> _tileViews = new();
    private bool _isAnimating = false;

    // Layout constants
    private const double BoardSize = 336.0;
    private const double TileSize = 70.0;
    private const double CellSpacing = 8.0;
    private const double BoardPadding = 8.0;
    private const double CellSize = (BoardSize - 2 * BoardPadding - 3 * CellSpacing) / 4.0; // ~73

    // Converters for tile colors
    private readonly StringToTileBackgroundColorConverter _bgConverter = new();
    private readonly StringToTileTextColorConverter _textConverter = new();

    public MainPage()
    {
        InitializeComponent();
        CurrentViewModel = (MainPageViewModel)BindingContext;

        // Subscribe to ViewModel events
        CurrentViewModel.TilesInitialized += OnTilesInitialized;
        CurrentViewModel.TileCreated += OnTileCreated;
        CurrentViewModel.MoveRequested += OnMoveRequested;

        // Create background grid cells
        InitializeBackgroundGrid();
    }

    private void InitializeBackgroundGrid()
    {
        var emptyColor = Application.Current?.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#3c3a32")
            : Color.FromArgb("#cdc1b4");

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                var emptyCell = new Border
                {
                    BackgroundColor = emptyColor,
                    HeightRequest = TileSize,
                    WidthRequest = TileSize,
                    StrokeShape = new RoundRectangle { CornerRadius = 5 },
                    Stroke = Colors.Transparent
                };
                Grid.SetRow(emptyCell, row);
                Grid.SetColumn(emptyCell, col);
                BackgroundGrid.Children.Add(emptyCell);
            }
        }
    }

    private void OnTilesInitialized(object? sender, IEnumerable<NumberTile> tiles)
    {
        // Clear existing tile views
        TileLayer.Children.Clear();
        _tileViews.Clear();

        // Create views for initial tiles
        foreach (var tile in tiles)
        {
            CreateTileView(tile, animate: true);
        }
    }

    private void OnTileCreated(object? sender, NumberTile tile)
    {
        CreateTileView(tile, animate: true);
    }

    private async void OnMoveRequested(object? sender, MoveResult moveResult)
    {
        if (_isAnimating || !moveResult.HasMoved) return;

        _isAnimating = true;

        try
        {
            // Animate all movements in parallel
            var animations = new List<Task>();

            foreach (var movement in moveResult.Movements)
            {
                if (_tileViews.TryGetValue(movement.Tile.Id, out var tileView))
                {
                    var task = AnimateTileMovement(tileView, movement.Tile, movement.ToRow, movement.ToColumn);
                    animations.Add(task);
                }
            }

            await Task.WhenAll(animations);

            // Handle merges - remove merged-away tiles and animate merge targets
            var mergeAnimations = new List<Task>();
            foreach (var movement in moveResult.Movements.Where(m => m.WillMerge))
            {
                // Remove the tile that was merged away
                if (_tileViews.TryGetValue(movement.Tile.Id, out var mergedTile))
                {
                    TileLayer.Children.Remove(mergedTile);
                    _tileViews.Remove(movement.Tile.Id);
                }

                // Animate the merge target (scale pulse)
                if (movement.MergeTarget != null && _tileViews.TryGetValue(movement.MergeTarget.Id, out var targetView))
                {
                    mergeAnimations.Add(AnimateMergePulse(targetView));
                }
            }

            // Update merge target values
            foreach (var movement in moveResult.Movements.Where(m => m.WillMerge && m.MergeTarget != null))
            {
                var newValue = movement.Tile.Value + movement.MergeTarget!.Value;
                movement.MergeTarget.Number = newValue.ToString();

                // Update the visual
                if (_tileViews.TryGetValue(movement.MergeTarget.Id, out var targetView))
                {
                    UpdateTileViewAppearance(targetView, movement.MergeTarget);
                }
            }

            await Task.WhenAll(mergeAnimations);

            // Remove tiles marked for removal
            foreach (var tile in moveResult.TilesToRemove)
            {
                if (_tileViews.TryGetValue(tile.Id, out var view))
                {
                    TileLayer.Children.Remove(view);
                    _tileViews.Remove(tile.Id);
                }
            }

            // Notify ViewModel to apply the move and spawn new tile
            CurrentViewModel.ApplyMove(moveResult);
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private void CreateTileView(NumberTile tile, bool animate = false)
    {
        var position = GetTilePosition(tile.Row, tile.Column);

        var border = new Border
        {
            HeightRequest = TileSize,
            WidthRequest = TileSize,
            Padding = 0,
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = 5 }
        };

        var label = new Label
        {
            FontFamily = "PoppinsBold",
            Margin = new Thickness(0, 5, 0, 0),
            FontSize = 36,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };

        border.Content = label;
        UpdateTileViewAppearance(border, tile);

        AbsoluteLayout.SetLayoutBounds(border, new Rect(position.X, position.Y, TileSize, TileSize));
        AbsoluteLayout.SetLayoutFlags(border, AbsoluteLayoutFlags.None);

        _tileViews[tile.Id] = border;
        TileLayer.Children.Add(border);

        if (animate)
        {
            _ = AnimateSpawn(border);
        }
    }

    private void UpdateTileViewAppearance(Border border, NumberTile tile)
    {
        border.BackgroundColor = (Color)_bgConverter.Convert(tile.Number, typeof(Color), null!, null!)!;

        if (border.Content is Label label)
        {
            label.Text = tile.Number;
            label.TextColor = (Color)_textConverter.Convert(tile.Number, typeof(Color), null!, null!)!;
        }
    }

    private Point GetTilePosition(int row, int column)
    {
        // Calculate position within the AbsoluteLayout
        double x = BoardPadding + column * (CellSize + CellSpacing) + (CellSize - TileSize) / 2;
        double y = BoardPadding + row * (CellSize + CellSpacing) + (CellSize - TileSize) / 2;
        return new Point(x, y);
    }

    private async Task AnimateTileMovement(Border tileView, NumberTile tile, int targetRow, int targetColumn)
    {
        var targetPosition = GetTilePosition(targetRow, targetColumn);
        var currentBounds = AbsoluteLayout.GetLayoutBounds(tileView);

        double deltaX = targetPosition.X - currentBounds.X;
        double deltaY = targetPosition.Y - currentBounds.Y;

        // Animate using translation
        await tileView.TranslateToAsync(deltaX, deltaY, 100, Easing.CubicOut);

        // Reset translation and update actual position
        tileView.TranslationX = 0;
        tileView.TranslationY = 0;
        AbsoluteLayout.SetLayoutBounds(tileView, new Rect(targetPosition.X, targetPosition.Y, TileSize, TileSize));

        // Update tile's logical position
        tile.Row = targetRow;
        tile.Column = targetColumn;
    }

    private async Task AnimateSpawn(Border tileView)
    {
        tileView.Scale = 0;
        tileView.Opacity = 0;

        await Task.WhenAll(
            tileView.ScaleToAsync(1.0, 100, Easing.SpringOut),
            tileView.FadeToAsync(1.0, 50)
        );
    }

    private async Task AnimateMergePulse(Border tileView)
    {
        await tileView.ScaleToAsync(1.15, 50, Easing.CubicOut);
        await tileView.ScaleToAsync(1.0, 50, Easing.CubicIn);
    }

    void MainPageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var bindingContext = (MainPageViewModel)BindingContext;

        if (e.PropertyName == nameof(MainPageViewModel.State))
        {
            if (bindingContext.State == LevelState.GameOver)
            {
                // Game over handling
            }
            else if (bindingContext.State == LevelState.Playing)
            {
                // Game started handling
            }
        }
        else if (e.PropertyName == nameof(MainPageViewModel.AddedScore))
        {
            this.Dispatcher.DispatchAsync(async () =>
            {
                AddedScoreLabel.IsVisible = true;
                await AddedScoreLabel.TranslateToAsync(0, -40, 500, Easing.Linear);
                await AddedScoreLabel.TranslateToAsync(0, 0, 0, Easing.Linear);
                AddedScoreLabel.IsVisible = false;
            });
        }
    }

    private SwipeDirection? swipedDirection;
    private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (_isAnimating) return;

        switch (e.StatusType)
        {
            case GestureStatus.Running:
                HandleTouch(e.TotalX, e.TotalY);
                break;
            case GestureStatus.Completed:
                HandleTouchEnd(swipedDirection);
                break;
        }
    }

    private void HandleTouch(double eTotalX, double eTotalY)
    {
        swipedDirection = null;
        const int delta = 10;
        if (eTotalX > delta)
        {
            swipedDirection = SwipeDirection.Right;
        }
        else if (eTotalX < -delta)
        {
            swipedDirection = SwipeDirection.Left;
        }
        else if (eTotalY > delta)
        {
            swipedDirection = SwipeDirection.Down;
        }
        else if (eTotalY < -delta)
        {
            swipedDirection = SwipeDirection.Up;
        }
    }

    private void HandleTouchEnd(SwipeDirection? swiped)
    {
        if (swiped == null || _isAnimating) return;

        var direction = swiped switch
        {
            SwipeDirection.Right => Direction.Right,
            SwipeDirection.Left => Direction.Left,
            SwipeDirection.Up => Direction.Up,
            SwipeDirection.Down => Direction.Down,
            _ => throw new ArgumentOutOfRangeException()
        };

        // Request move - this will trigger animations via MoveRequested event
        CurrentViewModel.RequestMove(direction);
    }

    void UndoButton_Clicked(System.Object sender, System.EventArgs e)
    {
        if (CurrentViewModel.CanUndo)
        {
            CurrentViewModel.Undo();
        }
    }

    private void GameContainer_SizeChanged(object sender, EventArgs e)
    {
        const double originalWidth = 340.0;
        const double originalHeight = 336.0;

        var containerWidth = GameContainer.Width;
        var containerHeight = GameContainer.Height;

        if (containerWidth <= 0 || containerHeight <= 0)
            return;

        // Calculate scale to fit while maintaining aspect ratio
        var scaleX = containerWidth / originalWidth;
        var scaleY = containerHeight / originalHeight;
        var scale = Math.Min(scaleX, scaleY);

        // Apply uniform scale to both borders
        GameBorder.Scale = scale;
        GameOverBorder.Scale = scale;
    }
}
