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
        });
    }

    private async Task RunAttractModeAnimation(CancellationToken ct)
    {
        var random = new Random();

        // Track tiles on the grid: [row, col] -> (Border, value)
        var gridTiles = new Dictionary<(int row, int col), (Border border, int value)>();

        // Spawn initial tiles (like starting a new game)
        await this.Dispatcher.DispatchAsync(() =>
        {
            for (int i = 0; i < 2; i++)
            {
                SpawnAttractTile(gridTiles, random);
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

                    // Calculate move result using real 2048 logic
                    var moveResult = CalculateAttractMove(gridTiles, direction);

                    if (!moveResult.HasMoved)
                    {
                        // Try other directions if this one doesn't work
                        foreach (var altDir in directions.Where(d => d != direction))
                        {
                            moveResult = CalculateAttractMove(gridTiles, altDir);
                            if (moveResult.HasMoved) break;
                        }
                    }

                    if (!moveResult.HasMoved)
                    {
                        // Game over state - reset the board
                        foreach (var tile in gridTiles.Values)
                        {
                            await tile.border.FadeToAsync(0, 200);
                            TileLayer.Children.Remove(tile.border);
                            _attractModeTiles.Remove(tile.border);
                        }
                        gridTiles.Clear();

                        // Spawn new initial tiles
                        for (int i = 0; i < 2; i++)
                        {
                            SpawnAttractTile(gridTiles, random);
                        }
                        return;
                    }

                    moved = true;

                    // Animate movements
                    var moveAnimations = new List<Task>();
                    foreach (var movement in moveResult.Movements)
                    {
                        var targetPosition = GetTilePosition(movement.ToRow, movement.ToCol);
                        var currentBounds = AbsoluteLayout.GetLayoutBounds(movement.Border);
                        double deltaX = targetPosition.X - currentBounds.X;
                        double deltaY = targetPosition.Y - currentBounds.Y;
                        moveAnimations.Add(AnimateAttractTileMove(movement.Border, deltaX, deltaY, ct));
                    }

                    if (moveAnimations.Count > 0)
                    {
                        await Task.WhenAll(moveAnimations);
                    }

                    // Process merges
                    var mergeAnimations = new List<Task>();
                    foreach (var merge in moveResult.Merges)
                    {
                        // Remove the source tile
                        TileLayer.Children.Remove(merge.SourceBorder);
                        _attractModeTiles.Remove(merge.SourceBorder);

                        // Update target tile with new value
                        int newValue = merge.TargetValue * 2;
                        UpdateAttractTileAppearance(merge.TargetBorder, newValue);
                        gridTiles[merge.TargetPos] = (merge.TargetBorder, newValue);

                        mergeAnimations.Add(AnimateAttractMergePulse(merge.TargetBorder, ct));
                    }

                    if (mergeAnimations.Count > 0)
                    {
                        await Task.WhenAll(mergeAnimations);
                    }

                    // Update grid positions after animations
                    var newGridTiles = new Dictionary<(int row, int col), (Border border, int value)>();
                    foreach (var movement in moveResult.Movements.Where(m => !m.WillMerge))
                    {
                        var pos = GetTilePosition(movement.ToRow, movement.ToCol);
                        movement.Border.TranslationX = 0;
                        movement.Border.TranslationY = 0;
                        AbsoluteLayout.SetLayoutBounds(movement.Border, new Rect(pos.X, pos.Y, TileSize, TileSize));
                        newGridTiles[(movement.ToRow, movement.ToCol)] = (movement.Border, movement.Value);
                    }

                    // Add merge targets (they stay in place but have new values)
                    foreach (var merge in moveResult.Merges)
                    {
                        newGridTiles[merge.TargetPos] = (merge.TargetBorder, merge.TargetValue * 2);
                    }

                    // Add tiles that didn't move
                    foreach (var kvp in gridTiles)
                    {
                        if (!moveResult.Movements.Any(m => m.FromRow == kvp.Key.row && m.FromCol == kvp.Key.col) &&
                            !moveResult.Merges.Any(m => m.SourcePos == kvp.Key))
                        {
                            newGridTiles[kvp.Key] = kvp.Value;
                        }
                    }

                    gridTiles = newGridTiles;

                    // Spawn a new tile
                    SpawnAttractTile(gridTiles, random);
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

    private void SpawnAttractTile(Dictionary<(int row, int col), (Border border, int value)> gridTiles, Random random)
    {
        var emptySpots = new List<(int row, int col)>();
        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 4; c++)
            {
                if (!gridTiles.ContainsKey((r, c)))
                {
                    emptySpots.Add((r, c));
                }
            }
        }

        if (emptySpots.Count == 0) return;

        var spot = emptySpots[random.Next(emptySpots.Count)];
        int value = random.Next(10) < 9 ? 2 : 4; // 90% chance of 2, 10% chance of 4

        var tile = CreateAttractGameTile(value);
        var tilePos = GetTilePosition(spot.row, spot.col);

        AbsoluteLayout.SetLayoutBounds(tile, new Rect(tilePos.X, tilePos.Y, TileSize, TileSize));
        AbsoluteLayout.SetLayoutFlags(tile, AbsoluteLayoutFlags.None);

        tile.Scale = 0;
        tile.Opacity = 0;

        TileLayer.Children.Add(tile);
        _attractModeTiles.Add(tile);
        gridTiles[spot] = (tile, value);

        _ = AnimateAttractSpawn(tile, CancellationToken.None);
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

    private class AttractMoveResult
    {
        public List<AttractMovement> Movements { get; } = new();
        public List<AttractMerge> Merges { get; } = new();
        public bool HasMoved => Movements.Count > 0;
    }

    private class AttractMovement
    {
        public required Border Border { get; init; }
        public int FromRow { get; init; }
        public int FromCol { get; init; }
        public int ToRow { get; init; }
        public int ToCol { get; init; }
        public int Value { get; init; }
        public bool WillMerge { get; init; }
    }

    private class AttractMerge
    {
        public required Border SourceBorder { get; init; }
        public required Border TargetBorder { get; init; }
        public (int row, int col) SourcePos { get; init; }
        public (int row, int col) TargetPos { get; init; }
        public int TargetValue { get; init; }
    }

    private static AttractMoveResult CalculateAttractMove(
        Dictionary<(int row, int col), (Border border, int value)> gridTiles,
        Direction direction)
    {
        var result = new AttractMoveResult();
        var merged = new HashSet<(int row, int col)>();
        var newPositions = new Dictionary<(int row, int col), (Border border, int value)>();

        // Sort positions based on direction
        var positions = GetSortedPositions(gridTiles.Keys.ToList(), direction);

        foreach (var pos in positions)
        {
            if (!gridTiles.TryGetValue(pos, out var tile)) continue;

            var (targetRow, targetCol) = GetFarthestPosition(pos.row, pos.col, direction, newPositions.Keys.ToList());

            // Check for merge possibility
            var nextPos = GetNextPosition(targetRow, targetCol, direction);
            if (IsValidPosition(nextPos.row, nextPos.col) &&
                newPositions.TryGetValue(nextPos, out var nextTile) &&
                nextTile.value == tile.value &&
                !merged.Contains(nextPos))
            {
                // Merge!
                result.Movements.Add(new AttractMovement
                {
                    Border = tile.border,
                    FromRow = pos.row,
                    FromCol = pos.col,
                    ToRow = nextPos.row,
                    ToCol = nextPos.col,
                    Value = tile.value,
                    WillMerge = true
                });

                result.Merges.Add(new AttractMerge
                {
                    SourceBorder = tile.border,
                    TargetBorder = nextTile.border,
                    SourcePos = pos,
                    TargetPos = nextPos,
                    TargetValue = tile.value
                });

                merged.Add(nextPos);
            }
            else if (targetRow != pos.row || targetCol != pos.col)
            {
                // Move without merge
                result.Movements.Add(new AttractMovement
                {
                    Border = tile.border,
                    FromRow = pos.row,
                    FromCol = pos.col,
                    ToRow = targetRow,
                    ToCol = targetCol,
                    Value = tile.value,
                    WillMerge = false
                });

                newPositions[(targetRow, targetCol)] = tile;
            }
            else
            {
                // Tile stays in place
                newPositions[pos] = tile;
            }
        }

        return result;
    }

    private static (int row, int col) GetFarthestPosition(int row, int col, Direction direction, List<(int row, int col)> occupied)
    {
        int dr = direction == Direction.Up ? -1 : (direction == Direction.Down ? 1 : 0);
        int dc = direction == Direction.Left ? -1 : (direction == Direction.Right ? 1 : 0);

        int newRow = row;
        int newCol = col;

        while (true)
        {
            int nextRow = newRow + dr;
            int nextCol = newCol + dc;

            if (!IsValidPosition(nextRow, nextCol) || occupied.Contains((nextRow, nextCol)))
                break;

            newRow = nextRow;
            newCol = nextCol;
        }

        return (newRow, newCol);
    }

    private static (int row, int col) GetNextPosition(int row, int col, Direction direction)
    {
        return direction switch
        {
            Direction.Up => (row - 1, col),
            Direction.Down => (row + 1, col),
            Direction.Left => (row, col - 1),
            Direction.Right => (row, col + 1),
            _ => (row, col)
        };
    }

    private static bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < 4 && col >= 0 && col < 4;
    }

    private static List<(int row, int col)> GetSortedPositions(List<(int row, int col)> positions, Direction direction)
    {
        return direction switch
        {
            Direction.Left => positions.OrderBy(p => p.col).ToList(),
            Direction.Right => positions.OrderByDescending(p => p.col).ToList(),
            Direction.Up => positions.OrderBy(p => p.row).ToList(),
            Direction.Down => positions.OrderByDescending(p => p.row).ToList(),
            _ => positions
        };
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
        AttractModeBorder.Scale = scale;
    }
}
