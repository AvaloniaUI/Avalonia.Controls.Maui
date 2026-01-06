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

    // Attract mode animation
    private CancellationTokenSource? _attractModeCts;
    private bool _isAttractModeRunning = false;
    private readonly List<Border> _attractModeTiles = new(); // Track attract mode tiles separately
    private readonly Dictionary<Guid, Border> _attractTileViews = new(); // Map tile IDs to Border elements


    public MainPage()
    {
        InitializeComponent();
        CurrentViewModel = (MainPageViewModel)BindingContext;

        // Subscribe to ViewModel events
        CurrentViewModel.TilesInitialized += OnTilesInitialized;
        CurrentViewModel.TileCreated += OnTileCreated;
        CurrentViewModel.MoveRequested += OnMoveRequested;
        CurrentViewModel.AttractModeStarted += OnAttractModeStarted;

        // Create background grid cells
        InitializeBackgroundGrid();

        // Start attract mode animation when page appears
        this.Appearing += OnPageAppearing;
    }

    private void OnPageAppearing(object? sender, EventArgs e)
    {
        if (CurrentViewModel.State == LevelState.AttractMode)
        {
            StartAttractModeAnimation();
        }
    }

    private void OnAttractModeStarted(object? sender, EventArgs e)
    {
        StartAttractModeAnimation();
    }

    private void StartAttractModeAnimation()
    {
        StopAttractModeAnimation();
        _attractModeCts = new CancellationTokenSource();
        _isAttractModeRunning = true;
        _ = RunAttractModeAnimation(_attractModeCts.Token);
    }

    private void StopAttractModeAnimation()
    {
        _isAttractModeRunning = false;
        _attractModeCts?.Cancel();
        _attractModeCts?.Dispose();
        _attractModeCts = null;

        // Clear attract mode tiles from the main TileLayer
        this.Dispatcher.Dispatch(() =>
        {
            foreach (var tile in _attractModeTiles)
            {
                TileLayer.Children.Remove(tile);
            }
            _attractModeTiles.Clear();
            _attractTileViews.Clear();
        });
    }

    private async Task RunAttractModeAnimation(CancellationToken ct)
    {
        var random = new Random();

        // Track tiles on the grid: TileData list (using TileData for shared calculator)
        var activeTiles = new List<TileData>();

        // Spawn initial tiles (like starting a new game)
        await this.Dispatcher.DispatchAsync(() =>
        {
            for (int i = 0; i < 2; i++)
            {
                SpawnAttractTile(activeTiles, random);
            }
        });

        while (!ct.IsCancellationRequested && _isAttractModeRunning)
        {
            try
            {
                bool moved = false;
                await this.Dispatcher.DispatchAsync(async () =>
                {
                    if (ct.IsCancellationRequested) return;

                    // Pick a random direction for this "move"
                    var directions = new[] { Direction.Left, Direction.Right, Direction.Up, Direction.Down };
                    var direction = directions[random.Next(directions.Length)];

                    // Calculate move result using the shared game logic
                    var moveResult = GameMovementCalculator.CalculateMove(activeTiles, direction);

                    if (!moveResult.HasMoved)
                    {
                        // Try other directions if this one doesn't work
                        foreach (var altDir in directions.Where(d => d != direction))
                        {
                            moveResult = GameMovementCalculator.CalculateMove(activeTiles, altDir);
                            if (moveResult.HasMoved) break;
                        }
                    }

                    if (!moveResult.HasMoved)
                    {
                        // Game over state - reset the board
                        foreach (var tileData in activeTiles)
                        {
                            if (_attractTileViews.TryGetValue(tileData.Id, out var border))
                            {
                                await border.FadeToAsync(0, 200);
                                TileLayer.Children.Remove(border);
                                _attractModeTiles.Remove(border);
                            }
                        }
                        activeTiles.Clear();
                        _attractTileViews.Clear();

                        // Spawn new initial tiles
                        for (int i = 0; i < 2; i++)
                        {
                            SpawnAttractTile(activeTiles, random);
                        }
                        return;
                    }

                    moved = true;

                    // Animate movements
                    var moveAnimations = new List<Task>();
                    foreach (var movement in moveResult.Movements)
                    {
                        if (_attractTileViews.TryGetValue(movement.TileData.Id, out var border))
                        {
                            var targetPosition = GetTilePosition(movement.ToRow, movement.ToColumn);
                            var currentBounds = AbsoluteLayout.GetLayoutBounds(border);
                            double deltaX = targetPosition.X - currentBounds.X;
                            double deltaY = targetPosition.Y - currentBounds.Y;
                            moveAnimations.Add(AnimateAttractTileMove(border, deltaX, deltaY, ct));
                        }
                    }

                    if (moveAnimations.Count > 0)
                    {
                        await Task.WhenAll(moveAnimations);
                    }

                    // Process merges
                    var mergeAnimations = new List<Task>();
                    foreach (var movement in moveResult.Movements.Where(m => m.WillMerge && m.MergeTargetData.HasValue))
                    {
                        // Remove the source tile
                        if (_attractTileViews.TryGetValue(movement.TileData.Id, out var sourceBorder))
                        {
                            TileLayer.Children.Remove(sourceBorder);
                            _attractModeTiles.Remove(sourceBorder);
                            _attractTileViews.Remove(movement.TileData.Id);
                        }

                        // Update target tile with new value
                        if (movement.MergeTargetData is { } mergeTarget && _attractTileViews.TryGetValue(mergeTarget.Id, out var targetBorder))
                        {
                            int newValue = movement.TileData.Value * 2;
                            UpdateAttractTileAppearance(targetBorder, newValue);
                            mergeAnimations.Add(AnimateAttractMergePulse(targetBorder, ct));
                        }
                    }

                    if (mergeAnimations.Count > 0)
                    {
                        await Task.WhenAll(mergeAnimations);
                    }

                    // Update tile positions after animations
                    var newActiveTiles = new List<TileData>();
                    var processedIds = new HashSet<Guid>();

                    // Handle non-merge movements - update position
                    foreach (var movement in moveResult.Movements.Where(m => !m.WillMerge))
                    {
                        if (_attractTileViews.TryGetValue(movement.TileData.Id, out var border))
                        {
                            var pos = GetTilePosition(movement.ToRow, movement.ToColumn);
                            // Batch the position update to prevent flicker
                            border.BatchBegin();
                            AbsoluteLayout.SetLayoutBounds(border, new Rect(pos.X, pos.Y, TileSize, TileSize));
                            border.TranslationX = 0;
                            border.TranslationY = 0;
                            border.BatchCommit();

                            newActiveTiles.Add(new TileData(
                                movement.TileData.Id,
                                movement.ToRow,
                                movement.ToColumn,
                                movement.TileData.Value));
                            processedIds.Add(movement.TileData.Id);
                        }
                    }

                    // Handle merges - update merge target value
                    foreach (var movement in moveResult.Movements.Where(m => m.WillMerge && m.MergeTargetData.HasValue))
                    {
                        var targetId = movement.MergeTargetData!.Value.Id;
                        if (!processedIds.Contains(targetId))
                        {
                            int newValue = movement.TileData.Value * 2;
                            newActiveTiles.Add(new TileData(
                                targetId,
                                movement.ToRow,
                                movement.ToColumn,
                                newValue));
                            processedIds.Add(targetId);
                        }
                    }

                    // Add tiles that didn't move
                    foreach (var tile in activeTiles)
                    {
                        if (!processedIds.Contains(tile.Id) &&
                            !moveResult.TilesToRemove.Any(t => t.Id == tile.Id))
                        {
                            newActiveTiles.Add(tile);
                        }
                    }

                    activeTiles.Clear();
                    activeTiles.AddRange(newActiveTiles);

                    // Spawn a new tile
                    SpawnAttractTile(activeTiles, random);
                });

                // Pause between moves
                await Task.Delay(moved ? 500 : 100, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // Ignore animation errors
            }
        }
    }

    private void SpawnAttractTile(List<TileData> activeTiles, Random random)
    {
        // Find occupied positions
        var occupiedPositions = new HashSet<(int row, int col)>(
            activeTiles.Select(t => (t.Row, t.Column)));

        var emptySpots = new List<(int row, int col)>();
        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 4; c++)
            {
                if (!occupiedPositions.Contains((r, c)))
                {
                    emptySpots.Add((r, c));
                }
            }
        }

        if (emptySpots.Count == 0) return;

        var spot = emptySpots[random.Next(emptySpots.Count)];
        int value = random.Next(10) < 9 ? 2 : 4; // 90% chance of 2, 10% chance of 4

        var tileId = Guid.NewGuid();
        var border = CreateAttractGameTile(value);
        var tilePos = GetTilePosition(spot.row, spot.col);

        AbsoluteLayout.SetLayoutBounds(border, new Rect(tilePos.X, tilePos.Y, TileSize, TileSize));
        AbsoluteLayout.SetLayoutFlags(border, AbsoluteLayoutFlags.None);

        border.Scale = 0;
        border.Opacity = 0;

        TileLayer.Children.Add(border);
        _attractModeTiles.Add(border);
        _attractTileViews[tileId] = border;

        // Add to active tiles list
        activeTiles.Add(new TileData(tileId, spot.row, spot.col, value));

        _ = AnimateAttractSpawn(border, CancellationToken.None);
    }

    private Border CreateAttractGameTile(int value)
    {
        var valueStr = value.ToString();

        var border = new Border
        {
            HeightRequest = TileSize,
            WidthRequest = TileSize,
            Padding = 0,
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = 5 },
            BackgroundColor = (Color)_bgConverter.Convert(valueStr, typeof(Color), null!, null!)!
        };

        var label = new Label
        {
            Text = valueStr,
            FontFamily = "PoppinsBold",
            Margin = new Thickness(0, 5, 0, 0),
            FontSize = value >= 1000 ? 24 : (value >= 100 ? 30 : 36),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            TextColor = (Color)_textConverter.Convert(valueStr, typeof(Color), null!, null!)!
        };

        border.Content = label;
        return border;
    }

    private void UpdateAttractTileAppearance(Border border, int value)
    {
        var valueStr = value.ToString();
        border.BackgroundColor = (Color)_bgConverter.Convert(valueStr, typeof(Color), null!, null!)!;

        if (border.Content is Label label)
        {
            label.Text = valueStr;
            label.FontSize = value >= 1000 ? 24 : (value >= 100 ? 30 : 36);
            label.TextColor = (Color)_textConverter.Convert(valueStr, typeof(Color), null!, null!)!;
        }
    }

    private static async Task AnimateAttractTileMove(Border tile, double deltaX, double deltaY, CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;
        try
        {
            await tile.TranslateToAsync(deltaX, deltaY, 100, Easing.CubicOut);
        }
        catch (OperationCanceledException) { }
    }

    private static async Task AnimateAttractMergePulse(Border tile, CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;
        try
        {
            await tile.ScaleToAsync(1.2, 75, Easing.CubicOut);
            await tile.ScaleToAsync(1.0, 75, Easing.CubicIn);
        }
        catch (OperationCanceledException) { }
    }

    private static async Task AnimateAttractSpawn(Border tile, CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;
        try
        {
            await Task.WhenAll(
                tile.ScaleToAsync(1.0, 100, Easing.SpringOut),
                tile.FadeToAsync(1.0, 50)
            );
        }
        catch (OperationCanceledException) { }
    }

    private void AttractMode_Tapped(object sender, TappedEventArgs e)
    {
        StopAttractModeAnimation();
        CurrentViewModel.NewGameCommand.Execute(null);
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
                if (movement.Tile != null && _tileViews.TryGetValue(movement.Tile.Id, out var tileView))
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
                if (movement.Tile != null && _tileViews.TryGetValue(movement.Tile.Id, out var mergedTile))
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
            foreach (var movement in moveResult.Movements.Where(m => m.WillMerge && m.MergeTarget != null && m.Tile != null))
            {
                var newValue = movement.Tile!.Value + movement.MergeTarget!.Value;
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

        // Set initial state BEFORE adding to visual tree to prevent flicker
        if (animate)
        {
            border.Scale = 0;
            border.Opacity = 0;
        }

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

        // Batch the position update to prevent flicker
        // Update layout bounds first, then reset translation in the same batch
        tileView.BatchBegin();
        AbsoluteLayout.SetLayoutBounds(tileView, new Rect(targetPosition.X, targetPosition.Y, TileSize, TileSize));
        tileView.TranslationX = 0;
        tileView.TranslationY = 0;
        tileView.BatchCommit();

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

    private void DebugButton_Clicked(object sender, EventArgs e)
    {
        DebugMenuBorder.IsVisible = true;
    }

    private void DebugMenuClose_Clicked(object sender, EventArgs e)
    {
        DebugMenuBorder.IsVisible = false;
    }

    private void DebugShowGameOver_Clicked(object sender, EventArgs e)
    {
        DebugMenuBorder.IsVisible = false;
        CurrentViewModel.SetStateForDebug(Enums.LevelState.GameOver);
    }

    private void DebugShowWin_Clicked(object sender, EventArgs e)
    {
        DebugMenuBorder.IsVisible = false;
        CurrentViewModel.SetStateForDebug(Enums.LevelState.Complete);
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

        // Apply uniform scale to all overlays
        GameBorder.Scale = scale;
        GameOverBorder.Scale = scale;
        AttractModeBorder.Scale = scale;
        WinBorder.Scale = scale;
        DebugMenuBorder.Scale = scale;
    }
}
