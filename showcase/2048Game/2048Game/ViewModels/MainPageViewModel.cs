using _2048Game.Enums;
using _2048Game.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace _2048Game.ViewModels
{
    /// <summary>
    /// Game logic refactored for tile movement animations
    /// Original game logic reference: https://github.com/jakowskidev/u2048_Jakowski
    /// </summary>
    public partial class MainPageViewModel : ObservableObject
    {
        // Board state
        private int[][] iBoard;
        private int iScore = 0, iBest = 0;
        private readonly Random oR = new();
        private bool gameOver = false;

        // Undo state history
        private readonly Stack<GameState> _stateHistory = new();
        private const int MaxUndoHistory = 10;

        // Events for view communication
        public event EventHandler<IEnumerable<NumberTile>>? TilesInitialized;
        public event EventHandler<NumberTile>? TileCreated;
        public event EventHandler<MoveResult>? MoveRequested;

        // Active tiles (entity-based, not fixed grid cells)
        private readonly List<NumberTile> _activeTiles = new();

        [ObservableProperty]
        private int totalMoves;

        private string formattedTime = string.Empty;
        public string FormattedTime
        {
            get => formattedTime;
            set => SetProperty(ref formattedTime, value);
        }

        private string score = "0";
        private string bestScore = "0";
        private string addedScore = "0";

        public string Score
        {
            get => score;
            set => SetProperty(ref score, value);
        }

        public string BestScore
        {
            get => bestScore;
            set => SetProperty(ref bestScore, value);
        }

        public string AddedScore
        {
            get => addedScore;
            set
            {
                addedScore = value;
                OnPropertyChanged(nameof(AddedScore));
            }
        }

        private LevelState state;
        public LevelState State
        {
            get => state;
            set => SetProperty(ref state, value);
        }

        private readonly Timer _timer;
        private DateTime startTime = DateTime.Now;

        public MainPageViewModel()
        {
            iBoard = new int[4][];
            for (int i = 0; i < 4; i++)
            {
                iBoard[i] = new int[4];
            }

            // Initialize with 2 random tiles
            SpawnInitialTiles();

            _timer = new Timer(new TimerCallback((s) => UpdateTimerInUI()), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private void SpawnInitialTiles()
        {
            _activeTiles.Clear();

            // Spawn 2 initial tiles
            for (int i = 0; i < 2; i++)
            {
                SpawnRandomTile(notify: false);
            }

            State = LevelState.Playing;
            TilesInitialized?.Invoke(this, _activeTiles);
        }

        private NumberTile? SpawnRandomTile(bool notify = true)
        {
            // Find empty cells
            var emptyCells = new List<(int row, int col)>();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (iBoard[i][j] == 0)
                    {
                        emptyCells.Add((i, j));
                    }
                }
            }

            if (emptyCells.Count == 0) return null;

            // Pick random empty cell
            var (row, col) = emptyCells[oR.Next(emptyCells.Count)];

            // Generate value: 90% = 2, 5% = 4, 5% = 8
            int value = oR.Next(0, 20) == 0 ? (oR.Next(0, 2) == 0 ? 8 : 4) : 2;

            // Update board
            iBoard[row][col] = value;

            // Create tile entity
            var tile = new NumberTile
            {
                Row = row,
                Column = col,
                Number = value.ToString()
            };

            _activeTiles.Add(tile);

            if (notify)
            {
                TileCreated?.Invoke(this, tile);
            }

            return tile;
        }

        [RelayCommand]
        void NewGame()
        {
            // Clear board
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    iBoard[i][j] = 0;
                }
            }

            iScore = 0;
            gameOver = false;
            startTime = DateTime.Now;
            TotalMoves = 0;
            Score = "0";

            SpawnInitialTiles();
        }

        public void RequestMove(Direction direction)
        {
            if (State != LevelState.Playing) return;

            var moveResult = CalculateMove(direction);

            if (moveResult.HasMoved)
            {
                // Save state before applying the move
                SaveState();

                TotalMoves += 1;
                MoveRequested?.Invoke(this, moveResult);
            }
        }

        private void SaveState()
        {
            // Clone the board
            var boardCopy = new int[4][];
            for (int i = 0; i < 4; i++)
            {
                boardCopy[i] = new int[4];
                for (int j = 0; j < 4; j++)
                {
                    boardCopy[i][j] = iBoard[i][j];
                }
            }

            // Clone the tiles
            var tilesCopy = _activeTiles.Select(t => new NumberTile
            {
                Id = t.Id,
                Row = t.Row,
                Column = t.Column,
                Number = t.Number
            }).ToList();

            var state = new GameState
            {
                Board = boardCopy,
                Tiles = tilesCopy,
                Score = iScore,
                TotalMoves = TotalMoves
            };

            _stateHistory.Push(state);

            // Limit history size
            if (_stateHistory.Count > MaxUndoHistory)
            {
                var temp = _stateHistory.ToArray().Take(MaxUndoHistory).ToArray();
                _stateHistory.Clear();
                foreach (var s in temp.Reverse())
                {
                    _stateHistory.Push(s);
                }
            }
        }

        public bool CanUndo => _stateHistory.Count > 0 && State == LevelState.Playing;

        public void Undo()
        {
            if (!CanUndo) return;

            var previousState = _stateHistory.Pop();

            // Restore board
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    iBoard[i][j] = previousState.Board[i][j];
                }
            }

            // Restore tiles
            _activeTiles.Clear();
            _activeTiles.AddRange(previousState.Tiles);

            // Restore score
            iScore = previousState.Score;
            Score = iScore.ToString();
            TotalMoves = previousState.TotalMoves;

            // Notify view to rebuild tiles
            TilesInitialized?.Invoke(this, _activeTiles);
        }

        public void ApplyMove(MoveResult moveResult)
        {
            // Update board state based on movements
            // First, clear all old positions
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    iBoard[i][j] = 0;
                }
            }

            // Remove merged tiles from active tiles
            foreach (var tile in moveResult.TilesToRemove)
            {
                _activeTiles.Remove(tile);
            }

            // Place remaining tiles at their new positions
            foreach (var tile in _activeTiles)
            {
                iBoard[tile.Row][tile.Column] = tile.Value;
            }

            // Update score
            if (moveResult.ScoreAdded > 0)
            {
                iScore += moveResult.ScoreAdded;
                AddedScore = $"+ {moveResult.ScoreAdded}";

                if (iScore > iBest)
                {
                    iBest = iScore;
                }
            }

            Score = iScore.ToString();
            BestScore = iBest.ToString();

            // Spawn new tile
            SpawnRandomTile();

            // Check game over
            if (IsGameOver())
            {
                gameOver = true;
                State = LevelState.GameOver;
            }
        }

        private MoveResult CalculateMove(Direction direction)
        {
            var result = new MoveResult();

            // Create a working copy of tile positions
            var tilePositions = _activeTiles
                .Select(t => (tile: t, row: t.Row, col: t.Column))
                .ToList();

            // Sort tiles based on direction to process them in order
            tilePositions = direction switch
            {
                Direction.Left => tilePositions.OrderBy(t => t.col).ToList(),
                Direction.Right => tilePositions.OrderByDescending(t => t.col).ToList(),
                Direction.Up => tilePositions.OrderBy(t => t.row).ToList(),
                Direction.Down => tilePositions.OrderByDescending(t => t.row).ToList(),
                _ => tilePositions
            };

            // Track which cells will be occupied and by which tile
            var targetGrid = new NumberTile?[4, 4];

            // Track which tiles have already merged (can only merge once per move)
            var mergedTiles = new HashSet<Guid>();

            foreach (var (tile, originalRow, originalCol) in tilePositions)
            {
                int targetRow = originalRow;
                int targetCol = originalCol;

                // Move tile as far as possible in the direction
                switch (direction)
                {
                    case Direction.Left:
                        while (targetCol > 0 && targetGrid[targetRow, targetCol - 1] == null)
                        {
                            targetCol--;
                        }
                        // Check for merge
                        if (targetCol > 0)
                        {
                            var adjacentTile = targetGrid[targetRow, targetCol - 1];
                            if (adjacentTile != null &&
                                adjacentTile.Value == tile.Value &&
                                !mergedTiles.Contains(adjacentTile.Id))
                            {
                                targetCol--;
                                result.Movements.Add(new TileMovement
                                {
                                    Tile = tile,
                                    FromRow = originalRow,
                                    FromColumn = originalCol,
                                    ToRow = targetRow,
                                    ToColumn = targetCol,
                                    WillMerge = true,
                                    MergeTarget = adjacentTile
                                });
                                result.TilesToRemove.Add(tile);
                                result.ScoreAdded += tile.Value * 2;
                                mergedTiles.Add(adjacentTile.Id);
                                continue;
                            }
                        }
                        break;

                    case Direction.Right:
                        while (targetCol < 3 && targetGrid[targetRow, targetCol + 1] == null)
                        {
                            targetCol++;
                        }
                        if (targetCol < 3)
                        {
                            var adjacentTile = targetGrid[targetRow, targetCol + 1];
                            if (adjacentTile != null &&
                                adjacentTile.Value == tile.Value &&
                                !mergedTiles.Contains(adjacentTile.Id))
                            {
                                targetCol++;
                                result.Movements.Add(new TileMovement
                                {
                                    Tile = tile,
                                    FromRow = originalRow,
                                    FromColumn = originalCol,
                                    ToRow = targetRow,
                                    ToColumn = targetCol,
                                    WillMerge = true,
                                    MergeTarget = adjacentTile
                                });
                                result.TilesToRemove.Add(tile);
                                result.ScoreAdded += tile.Value * 2;
                                mergedTiles.Add(adjacentTile.Id);
                                continue;
                            }
                        }
                        break;

                    case Direction.Up:
                        while (targetRow > 0 && targetGrid[targetRow - 1, targetCol] == null)
                        {
                            targetRow--;
                        }
                        if (targetRow > 0)
                        {
                            var adjacentTile = targetGrid[targetRow - 1, targetCol];
                            if (adjacentTile != null &&
                                adjacentTile.Value == tile.Value &&
                                !mergedTiles.Contains(adjacentTile.Id))
                            {
                                targetRow--;
                                result.Movements.Add(new TileMovement
                                {
                                    Tile = tile,
                                    FromRow = originalRow,
                                    FromColumn = originalCol,
                                    ToRow = targetRow,
                                    ToColumn = targetCol,
                                    WillMerge = true,
                                    MergeTarget = adjacentTile
                                });
                                result.TilesToRemove.Add(tile);
                                result.ScoreAdded += tile.Value * 2;
                                mergedTiles.Add(adjacentTile.Id);
                                continue;
                            }
                        }
                        break;

                    case Direction.Down:
                        while (targetRow < 3 && targetGrid[targetRow + 1, targetCol] == null)
                        {
                            targetRow++;
                        }
                        if (targetRow < 3)
                        {
                            var adjacentTile = targetGrid[targetRow + 1, targetCol];
                            if (adjacentTile != null &&
                                adjacentTile.Value == tile.Value &&
                                !mergedTiles.Contains(adjacentTile.Id))
                            {
                                targetRow++;
                                result.Movements.Add(new TileMovement
                                {
                                    Tile = tile,
                                    FromRow = originalRow,
                                    FromColumn = originalCol,
                                    ToRow = targetRow,
                                    ToColumn = targetCol,
                                    WillMerge = true,
                                    MergeTarget = adjacentTile
                                });
                                result.TilesToRemove.Add(tile);
                                result.ScoreAdded += tile.Value * 2;
                                mergedTiles.Add(adjacentTile.Id);
                                continue;
                            }
                        }
                        break;
                }

                // Record movement if position changed
                if (targetRow != originalRow || targetCol != originalCol)
                {
                    result.Movements.Add(new TileMovement
                    {
                        Tile = tile,
                        FromRow = originalRow,
                        FromColumn = originalCol,
                        ToRow = targetRow,
                        ToColumn = targetCol,
                        WillMerge = false
                    });
                }

                // Mark target cell as occupied
                targetGrid[targetRow, targetCol] = tile;
            }

            return result;
        }

        public bool IsGameOver()
        {
            // Check if any empty cells
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (iBoard[i][j] == 0) return false;
                }
            }

            // Check if any adjacent cells have same value
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i > 0 && iBoard[i - 1][j] == iBoard[i][j]) return false;
                    if (i < 3 && iBoard[i + 1][j] == iBoard[i][j]) return false;
                    if (j > 0 && iBoard[i][j - 1] == iBoard[i][j]) return false;
                    if (j < 3 && iBoard[i][j + 1] == iBoard[i][j]) return false;
                }
            }

            return true;
        }

        private void UpdateTimerInUI()
        {
            TimeSpan spent = DateTime.Now - startTime;
            string elapsedTime = string.Format("{0:00}:{1:00}", spent.Minutes, spent.Seconds);
            FormattedTime = elapsedTime;
        }

        // Keep legacy commands for backwards compatibility (they now use RequestMove)
        [RelayCommand]
        void LeftSwipe() => RequestMove(Direction.Left);

        [RelayCommand]
        void RightSwipe() => RequestMove(Direction.Right);

        [RelayCommand]
        void UpSwipe() => RequestMove(Direction.Up);

        [RelayCommand]
        void DownSwipe() => RequestMove(Direction.Down);
    }

    /// <summary>
    /// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed.
    /// </summary>
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        public ObservableRangeCollection() : base() { }

        public ObservableRangeCollection(IEnumerable<T> collection) : base(collection) { }

        public void AddRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add)
        {
            if (notificationMode != NotifyCollectionChangedAction.Add && notificationMode != NotifyCollectionChangedAction.Reset)
                throw new ArgumentException("Mode must be either Add or Reset for AddRange.", nameof(notificationMode));
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            CheckReentrancy();

            var startIndex = Count;
            var itemsAdded = AddArrangeCore(collection);

            if (!itemsAdded) return;

            if (notificationMode == NotifyCollectionChangedAction.Reset)
            {
                RaiseChangeNotificationEvents(action: NotifyCollectionChangedAction.Reset);
                return;
            }

            var changedItems = collection is List<T> list ? list : new List<T>(collection);
            RaiseChangeNotificationEvents(
                action: NotifyCollectionChangedAction.Add,
                changedItems: changedItems,
                startingIndex: startIndex);
        }

        public void RemoveRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Reset)
        {
            if (notificationMode != NotifyCollectionChangedAction.Remove && notificationMode != NotifyCollectionChangedAction.Reset)
                throw new ArgumentException("Mode must be either Remove or Reset for RemoveRange.", nameof(notificationMode));
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            CheckReentrancy();

            if (notificationMode == NotifyCollectionChangedAction.Reset)
            {
                var raiseEvents = false;
                foreach (var item in collection)
                {
                    Items.Remove(item);
                    raiseEvents = true;
                }

                if (raiseEvents)
                    RaiseChangeNotificationEvents(action: NotifyCollectionChangedAction.Reset);
                return;
            }

            var changedItems = new List<T>(collection);
            for (var i = 0; i < changedItems.Count; i++)
            {
                if (!Items.Remove(changedItems[i]))
                {
                    changedItems.RemoveAt(i);
                    i--;
                }
            }

            if (changedItems.Count == 0) return;

            RaiseChangeNotificationEvents(
                action: NotifyCollectionChangedAction.Remove,
                changedItems: changedItems);
        }

        public void Replace(T item) => ReplaceRange(new T[] { item });

        public void ReplaceRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            CheckReentrancy();

            var previouslyEmpty = Items.Count == 0;
            Items.Clear();
            AddArrangeCore(collection);
            var currentlyEmpty = Items.Count == 0;

            if (previouslyEmpty && currentlyEmpty) return;

            RaiseChangeNotificationEvents(action: NotifyCollectionChangedAction.Reset);
        }

        private bool AddArrangeCore(IEnumerable<T> collection)
        {
            var itemAdded = false;
            foreach (var item in collection)
            {
                Items.Add(item);
                itemAdded = true;
            }
            return itemAdded;
        }

        private void RaiseChangeNotificationEvents(NotifyCollectionChangedAction action, List<T>? changedItems = null, int startingIndex = -1)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

            if (changedItems is null)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action));
            else
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItems: changedItems, startingIndex: startingIndex));
        }
    }
}
