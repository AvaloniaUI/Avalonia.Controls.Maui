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
#pragma warning disable CS0414
        private bool gameOver = false;
#pragma warning restore CS0414

        // Undo state history
        private readonly Stack<GameState> _stateHistory = new();
        private const int MaxUndoHistory = 10;

        // Events for view communication
        public event EventHandler<IEnumerable<NumberTile>>? TilesInitialized;
        public event EventHandler<NumberTile>? TileCreated;
        public event EventHandler<MoveResult>? MoveRequested;

        // Attract mode auto-play
        private Timer? _attractModeTimer;
        private bool _isAttractModeAnimating = false;

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

            // Start in attract mode
            State = LevelState.AttractMode;

            _timer = new Timer(new TimerCallback((s) => UpdateTimerInUI()), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        public void StartAttractMode()
        {
            StopAttractMode();

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
            TotalMoves = 0;
            Score = "0";
            _activeTiles.Clear();
            _stateHistory.Clear();

            // Spawn initial tiles
            for (int i = 0; i < 2; i++)
            {
                SpawnRandomTile(notify: false);
            }

            State = LevelState.AttractMode;
            TilesInitialized?.Invoke(this, _activeTiles);

            // Start auto-play timer
            _attractModeTimer = new Timer(AttractModeAutoPlay, null, 800, 600);
        }

        public void StopAttractMode()
        {
            _attractModeTimer?.Dispose();
            _attractModeTimer = null;
            _isAttractModeAnimating = false;
        }

        private void AttractModeAutoPlay(object? state)
        {
            if (State != LevelState.AttractMode || _isAttractModeAnimating)
                return;

            _isAttractModeAnimating = true;

            // Pick a random direction
            var directions = new[] { Direction.Left, Direction.Right, Direction.Up, Direction.Down };
            var direction = directions[oR.Next(directions.Length)];

            var moveResult = CalculateMove(direction);

            // If no movement possible, try other directions
            if (!moveResult.HasMoved)
            {
                foreach (var altDir in directions.Where(d => d != direction))
                {
                    moveResult = CalculateMove(altDir);
                    if (moveResult.HasMoved) break;
                }
            }

            if (moveResult.HasMoved)
            {
                TotalMoves += 1;
                MoveRequested?.Invoke(this, moveResult);
            }
            else
            {
                // No valid moves - game over in attract mode, restart
                _isAttractModeAnimating = false;
                RestartAttractMode();
            }
        }

        public void AttractModeAnimationComplete()
        {
            _isAttractModeAnimating = false;
        }

        private void RestartAttractMode()
        {
            // Delay and restart
            Task.Delay(1000).ContinueWith(_ =>
            {
                if (State == LevelState.AttractMode)
                {
                    StartAttractMode();
                }
            });
        }

        public void SetStateForDebug(LevelState newState)
        {
            State = newState;
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
            StopAttractMode();

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

            // Remove merged tiles from active tiles (find by Id from TileData)
            foreach (var tileData in moveResult.TilesToRemove)
            {
                var tileToRemove = _activeTiles.FirstOrDefault(t => t.Id == tileData.Id);
                if (tileToRemove != null)
                {
                    _activeTiles.Remove(tileToRemove);
                }
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

            // In attract mode, handle game over/win differently
            if (State == LevelState.AttractMode)
            {
                AttractModeAnimationComplete();

                if (IsWon() || IsGameOver())
                {
                    RestartAttractMode();
                }
            }
            else
            {
                // Check win condition first
                if (IsWon())
                {
                    State = LevelState.Complete;
                }
                // Check game over
                else if (IsGameOver())
                {
                    gameOver = true;
                    State = LevelState.GameOver;
                }
            }
        }

        private MoveResult CalculateMove(Direction direction)
        {
            // Convert NumberTile objects to TileData for the shared calculator
            var tileDataList = _activeTiles.Select(TileData.FromNumberTile);

            // Use the shared movement calculator
            var result = GameMovementCalculator.CalculateMove(tileDataList, direction);

            // Populate the NumberTile references for backwards compatibility with view animations
            foreach (var movement in result.Movements)
            {
                movement.Tile = _activeTiles.FirstOrDefault(t => t.Id == movement.TileData.Id);
                if (movement.MergeTargetData.HasValue)
                {
                    movement.MergeTarget = _activeTiles.FirstOrDefault(t => t.Id == movement.MergeTargetData.Value.Id);
                }
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

        public bool IsWon()
        {
            // Check if any tile has reached 2048
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (iBoard[i][j] >= 2048) return true;
                }
            }

            return false;
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
