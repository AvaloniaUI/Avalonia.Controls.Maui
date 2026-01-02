using _2048Game.Data;
using _2048Game.Enums;
using _2048Game.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Specialized;

namespace _2048Game.ViewModels
{
    /// <summary>
    /// Game logic been refered from https://github.com/jakowskidev/u2048_Jakowski
    /// </summary>
    public partial class MainPageViewModel : ObservableObject
    {
        private int[][] iBoard;
        private int iScore = 0, iBest = 0, iAdded = 0;
        private int addNum = 2;
        private Random oR = new Random();
        private Boolean gameOver = false;
        private int iNewX, iNewY;
        private int guessedCount;
        private LevelState state;
        private string _mauiRobotPhrase = string.Empty;

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
                //need to notify even for same value.
                addedScore = value;
                OnPropertyChanged(nameof(AddedScore));
            }
        }

        public int GuessedCount
        {
            get => guessedCount;
            set => SetProperty(ref guessedCount, value);
        }

        public LevelState State
        {
            get => state;
            set => SetProperty(ref state, value);
        }
        public string MauiRobotPhrase
        {
            get => _mauiRobotPhrase;
            set => SetProperty(ref _mauiRobotPhrase, value);
        }
        
        public ObservableRangeCollection<NumberTile> GuessedTiles { get; } = new ObservableRangeCollection<NumberTile>();

       
        private ObservableCollection<NumberTile> tiles = new ObservableCollection<NumberTile>();
        public ObservableCollection<NumberTile> Tiles
        {
            get => tiles;
            set => SetProperty(ref tiles, value);
        }


        [RelayCommand]
        void LeftSwipe()
        {
            MoveBoard(Direction.Left);
            UpdateGame();
        }
        [RelayCommand]
        void RightSwipe()
        {
            MoveBoard(Direction.Right);
            UpdateGame();
        }
        [RelayCommand]
        void UpSwipe()
        {
            MoveBoard(Direction.Up);
            UpdateGame();
        }
        [RelayCommand]
        void DownSwipe()
        {
            MoveBoard(Direction.Down);
            UpdateGame();
        }
        [RelayCommand]
        void NewGame()
        {
            State = LevelState.Playing;
            ResetGame();
            Update();
            _ = LoadAsync();
        }
        public MainPageViewModel()
        {
            MauiRobotPhrase = "Hello! I am MauiRobots.";
            this.iBoard = new int[4][];
            for (int i = 0; i < 4; i++)
            {
                iBoard[i] = new int[4];
            }

            Update();
            _ = LoadAsync();
            _timer = new Timer(new TimerCallback((s) => UpdateTimerInUI()), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        void UpdateGame()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var index = 0;
                    if (i == 1) index += 3;
                    if (i == 2) index += 6;
                    if (i == 3) index += 9;
                    if (iBoard[i][j] > 0)
                    {
                        Tiles[i + j + index].Number = iBoard[i][j].ToString();
                        if(i == iNewX && j == iNewY)
                        {
                            Tiles[i + j + index].IsNewNumberGenerated = true;
                        }
                    }
                    else 
                    {
                        Tiles[i + j + index].Number = String.Empty;
                    }
                }
            }
            if (iAdded > 0)
            {
                AddedScore = $"+ {iAdded}";
            }
            Score = iScore.ToString();
            BestScore = iBest.ToString();
            //See if Game Over
            if(IsGameOver())
            {

            }
        }
        private DateTime startTime = DateTime.Now;
        Timer _timer;
        
        private void UpdateTimerInUI()
        {
            TimeSpan spent = DateTime.Now - startTime;
            string elapsedTime = string.Format("{0:00}:{1:00}",
                spent.Minutes, spent.Seconds);
            FormattedTime = elapsedTime;
        }
       
        public void Update()
        {
            while (!gameOver && addNum > 0)
            {
                int nX = oR.Next(0, 4), nY = oR.Next(0, 4);

                if (iBoard[nX][nY] == 0)
                {
                    iBoard[nX][nY] = oR.Next(0, 20) == 0 ? oR.Next(0, 15) == 0 ? 8 : 4 : 2;
                    iNewX = nX;
                    iNewY = nY;
                    --addNum;
                }
            }
        }

        private async Task LoadAsync()
        {
            var numberRepository = new GameRepository();

            var allShapes = await numberRepository.ListAsync();
            var actualTiles = new List<NumberTile>();

            foreach (var item in allShapes)
            {
                actualTiles.Add(item);
            }
            // 0,0 0,1 0,2 0,3
            // 1,0 1,1 1,2 1,3
            // 2,0 2,1 2,2 2,3
            // 3,0 3,1 3,2 3,3
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (iBoard[i][j] > 0)
                    {
                        var index = 0;
                        if (i == 1) index += 3;
                        if (i == 2) index += 6;
                        if (i == 3) index += 9;
                        actualTiles[i + j + index].Number = iBoard[i][j].ToString();
                    }
                }
            }

            State = LevelState.Playing;
            Tiles = new ObservableCollection<NumberTile>(actualTiles);
        }

        public void MoveBoard(Direction nDirection)
        {
            TotalMoves += 1;
            iNewX = -1;
            iNewY = -1;
            iAdded = 0;
            Boolean bAdd = false;

            switch (nDirection)
            {
                case Direction.Left:
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            for (int k = j + 1; k < 4; k++)
                            {
                                if (iBoard[i][k] == 0)
                                {
                                    continue;
                                }
                                else if (iBoard[i][k] == iBoard[i][j])
                                {
                                    iBoard[i][j] *= 2;
                                    Tiles[i + j + GetAddOnIndex(i)].IsNumberMultiplied = true;
                                    iAdded = iBoard[i][j];
                                    iScore += iBoard[i][j];
                                    iBoard[i][k] = 0;
                                    bAdd = true;
                                    break;
                                }
                                else
                                {
                                    if (iBoard[i][j] == 0 && iBoard[i][k] != 0)
                                    {
                                        iBoard[i][j] = iBoard[i][k];
                                        iBoard[i][k] = 0;
                                        j--;
                                        bAdd = true;
                                        break;
                                    }
                                    else if (iBoard[i][j] != 0)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case Direction.Down:
                    for (int j = 0; j < 4; j++)
                    {
                        for (int i = 3; i >= 0; i--)
                        {
                            for (int k = i - 1; k >= 0; k--)
                            {
                                if (iBoard[k][j] == 0)
                                {
                                    continue;
                                }
                                else if (iBoard[k][j] == iBoard[i][j])
                                {
                                    iBoard[i][j] *= 2;
                                    Tiles[i + j + GetAddOnIndex(i)].IsNumberMultiplied = true;
                                    iAdded = iBoard[i][j];
                                    iScore += iBoard[i][j];
                                    iBoard[k][j] = 0;
                                    bAdd = true;
                                    break;
                                }
                                else
                                {
                                    if (iBoard[i][j] == 0 && iBoard[k][j] != 0)
                                    {
                                        iBoard[i][j] = iBoard[k][j];
                                        iBoard[k][j] = 0;
                                        i++;
                                        bAdd = true;
                                        break;
                                    }
                                    else if (iBoard[i][j] != 0)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case Direction.Right:
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 3; j >= 0; j--)
                        {
                            for (int k = j - 1; k >= 0; k--)
                            {
                                if (iBoard[i][k] == 0)
                                {
                                    continue;
                                }
                                else if (iBoard[i][k] == iBoard[i][j])
                                {
                                    iBoard[i][j] *= 2;
                                    Tiles[i + j + GetAddOnIndex(i)].IsNumberMultiplied = true;
                                    iAdded = iBoard[i][j];
                                    iScore += iBoard[i][j];
                                    iBoard[i][k] = 0;
                                    bAdd = true;
                                    break;
                                }
                                else
                                {
                                    if (iBoard[i][j] == 0 && iBoard[i][k] != 0)
                                    {
                                        iBoard[i][j] = iBoard[i][k];
                                        iBoard[i][k] = 0;
                                        j++;
                                        bAdd = true;
                                        break;
                                    }
                                    else if (iBoard[i][j] != 0)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case Direction.Up:
                    for (int j = 0; j < 4; j++)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            for (int k = i + 1; k < 4; k++)
                            {
                                if (iBoard[k][j] == 0)
                                {
                                    continue;
                                }
                                else if (iBoard[k][j] == iBoard[i][j])
                                {
                                    iBoard[i][j] *= 2;
                                    Tiles[i + j + GetAddOnIndex(i)].IsNumberMultiplied = true;
                                    iAdded = iBoard[i][j];
                                    iScore += iBoard[i][j];
                                    iBoard[k][j] = 0;
                                    bAdd = true;
                                    break;
                                }
                                else
                                {
                                    if (iBoard[i][j] == 0 && iBoard[k][j] != 0)
                                    {
                                        iBoard[i][j] = iBoard[k][j];
                                        iBoard[k][j] = 0;
                                        i--;
                                        bAdd = true;
                                        break;
                                    }
                                    else if (iBoard[i][j] != 0)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            if (iScore > iBest)
            {
                iBest = iScore;
            }

            if (bAdd)
            {
                ++addNum;
            }

            if (IsGameOver())
            {
                State = LevelState.GameOver;
            }
            Update();
        }

        public bool IsGameOver()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i - 1 >= 0)
                    {
                        if (iBoard[i - 1][j] == iBoard[i][j])
                        {
                            return false;
                        }
                    }

                    if (i + 1 < 4)
                    {
                        if (iBoard[i + 1][j] == iBoard[i][j])
                        {
                            return false;
                        }
                    }

                    if (j - 1 >= 0)
                    {
                        if (iBoard[i][j - 1] == iBoard[i][j])
                        {
                            return false;
                        }
                    }

                    if (j + 1 < 4)
                    {
                        if (iBoard[i][j + 1] == iBoard[i][j])
                        {
                            return false;
                        }
                    }

                    if (iBoard[i][j] == 0)
                    {
                        return false;
                    }
                }
            }

            gameOver = true;
            return true;
        }

        private void ResetGame()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    this.iBoard[i][j] = 0;
                }
            }

            this.addNum = 2;
            this.iScore = 0;
            this.iAdded = 0;
            this.gameOver = false;
            startTime = DateTime.Now;
            TotalMoves = 0;
            Score = "0";
        }
        private int GetAddOnIndex(int i)
        {
            var index = 0;
            if (i == 1) index += 3;
            if (i == 2) index += 6;
            if (i == 3) index += 9;
            return index;
        }
    }

    /// <summary> 
	/// Represents a dynamic data collection that provides notifications when items get added, removed, or when the whole list is refreshed. 
	/// </summary> 
	/// <typeparam name="T"></typeparam> 
	public class ObservableRangeCollection<T> : ObservableCollection<T>
	{

		/// <summary> 
		/// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class. 
		/// </summary> 
		public ObservableRangeCollection()
			: base()
		{
		}

		/// <summary> 
		/// Initializes a new instance of the System.Collections.ObjectModel.ObservableCollection(Of T) class that contains elements copied from the specified collection. 
		/// </summary> 
		/// <param name="collection">collection: The collection from which the elements are copied.</param> 
		/// <exception cref="System.ArgumentNullException">The collection parameter cannot be null.</exception> 
		public ObservableRangeCollection(IEnumerable<T> collection)
			: base(collection)
		{
		}

		/// <summary> 
		/// Adds the elements of the specified collection to the end of the ObservableCollection(Of T). 
		/// </summary> 
		public void AddRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add)
		{
			if (notificationMode != NotifyCollectionChangedAction.Add && notificationMode != NotifyCollectionChangedAction.Reset)
				throw new ArgumentException("Mode must be either Add or Reset for AddRange.", nameof(notificationMode));
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));

			CheckReentrancy();

			var startIndex = Count;

			var itemsAdded = AddArrangeCore(collection);

			if (!itemsAdded)
				return;

			if (notificationMode == NotifyCollectionChangedAction.Reset)
			{
				RaiseChangeNotificationEvents(action: NotifyCollectionChangedAction.Reset);

				return;
			}

			var changedItems = collection is List<T> ? (List<T>)collection : new List<T>(collection);

			RaiseChangeNotificationEvents(
				action: NotifyCollectionChangedAction.Add,
				changedItems: changedItems,
				startingIndex: startIndex);
		}

		/// <summary> 
		/// Removes the first occurence of each item in the specified collection from ObservableCollection(Of T). NOTE: with notificationMode = Remove, removed items starting index is not set because items are not guaranteed to be consecutive.
		/// </summary> 
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
					changedItems.RemoveAt(i); //Can't use a foreach because changedItems is intended to be (carefully) modified
					i--;
				}
			}

			if (changedItems.Count == 0)
				return;

            RaiseChangeNotificationEvents(
                action: NotifyCollectionChangedAction.Remove,
                changedItems: changedItems);
		}

		/// <summary> 
		/// Clears the current collection and replaces it with the specified item. 
		/// </summary> 
		public void Replace(T item) => ReplaceRange(new T[] { item });

		/// <summary> 
		/// Clears the current collection and replaces it with the specified collection. 
		/// </summary> 
		public void ReplaceRange(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));

			CheckReentrancy();

			var previouslyEmpty = Items.Count == 0;

			Items.Clear();

			AddArrangeCore(collection);

			var currentlyEmpty = Items.Count == 0;

			if (previouslyEmpty && currentlyEmpty)
				return;

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
