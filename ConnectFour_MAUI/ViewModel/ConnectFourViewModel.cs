using System.Collections.ObjectModel;
using ConnectFour.Model;
using ConnectFour_MAUI.ViewModel;

namespace ConnectFour.ViewModel
{
    public class ConnectFourViewModel : ViewModelBase
    {
        #region Private Fields

        private GameModel _model;
        private GameSizeViewModel _sizeViewModel = null!;
        private int _tableSize;

        #endregion


        #region Properties

        public DelegateCommand NewGameCommand { get; private set; }

        public DelegateCommand SaveGameCommand { get; private set; }

        public DelegateCommand LoadGameCommand { get; private set; }

        public DelegateCommand PauseGameCommand { get; private set; }

        public DelegateCommand ExitGameCommand { get; private set; }

        public ObservableCollection<ConnectFourField> Fields { get; set; }

        public ObservableCollection<GameSizeViewModel> Sizes { get; set; }

        public GameSizeViewModel SizeViewModel
        {
            get => _sizeViewModel;
            set
            {
                _sizeViewModel = value;
                _model.Size    = value.Size;
                OnPropertyChanged();
            }
        }

        public int TableSize
        {
            get => _tableSize;
            set
            {
                _tableSize = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GetTableSize));
                OnPropertyChanged(nameof(GameTableRows));
                OnPropertyChanged(nameof(GameTableColumns));
            }
        }

        public int GetTableSize // numebr of pixels
        {
            get => _tableSize * 50; // size times size of the cell (const)
        }

        public RowDefinitionCollection GameTableRows
        {
            get => new RowDefinitionCollection(Enumerable.Repeat(new RowDefinition(GridLength.Star), TableSize).ToArray());
        }

        public ColumnDefinitionCollection GameTableColumns
        {
            get => new ColumnDefinitionCollection(Enumerable.Repeat(new ColumnDefinition(GridLength.Star), TableSize).ToArray());
        }

        public bool AppTimerEnabled { get; set; }

        public bool HasMoves { get { return GameMovesCount > 0; } }

        public int GameMovesCount { get { return _model.Moves; } }
        public String PlayerString { get { return GameMovesCount % 2 == 0 ? "X" : "O"; } }

        public String ModelSize { get { return _model.Size.ToString(); } }

        public String TurnTime { get { return TimeSpan.FromSeconds(_model.TurnTime).ToString(@"mm\:ss"); } }
        public String XTime { get { return TimeSpan.FromSeconds(_model.X_Time).ToString("g"); } }
        public String OTime { get { return TimeSpan.FromSeconds(_model.O_Time).ToString("g"); } }

        #endregion


        #region Events

        public event EventHandler? NewGame;

        public event EventHandler? SaveGame;

        public event EventHandler? LoadGame;

        public event EventHandler? PauseGame;

        public event EventHandler? ExitGame;

        #endregion


        #region Constructor

        public ConnectFourViewModel(GameModel model)
        {
            // játék csatlakoztatása
            _model = model;
            _model.GameCreated += new EventHandler(Model_GameCreated);
            _model.GameLoaded += new EventHandler(Model_GameLoaded);
            _model.TimerAdvanced += new EventHandler<TimerAdvancedEventArgs>(Model_TimerAdvanced);
            _model.TileChanged += new EventHandler<TileChangedEventArgs>(Model_TileChanged);

            // parancsok kezelése
            NewGameCommand   = new DelegateCommand(param => OnNewGame());
            SaveGameCommand  = new DelegateCommand(param => OnSaveGame());
            LoadGameCommand  = new DelegateCommand(param => OnLoadGame());
            PauseGameCommand = new DelegateCommand(param => OnPauseGame());
            ExitGameCommand  = new DelegateCommand(param => OnExitGame());

            Sizes = new ObservableCollection<GameSizeViewModel>
            {
                new GameSizeViewModel {Size = GameSize.Small},
                new GameSizeViewModel {Size = GameSize.Medium},
                new GameSizeViewModel {Size = GameSize.Large}
            };
            SizeViewModel = Sizes[0];

            TableSize = _model.Height;

            // játéktábla létrehozása
            Fields = new ObservableCollection<ConnectFourField>();
            for (int i = 0; i < _model.Height; i++)
            {
                for (int j = 0; j < _model.Width; j++)
                {
                    var field = new ConnectFourField
                    {
                        Text = String.Empty,
                        Row = i,
                        Col = j,
                        Number = i * _model.Width + j, // gomb sorszáma
                        IsWinning = false,
                        StepCommand = new DelegateCommand(param => StepGame(Convert.ToInt32(param)))
                    };

                    Fields.Add(field);
                }
            }

            RefreshTable();
        }

        #endregion


        #region Private Methods

        private void InitializeTable()
        {
            if ((GameTableColumns.Count == 10 && SizeViewModel.Size == GameSize.Small)  ||
                (GameTableColumns.Count == 20 && SizeViewModel.Size == GameSize.Medium) ||
                (GameTableColumns.Count == 30 && SizeViewModel.Size == GameSize.Large))
            {
                foreach (var field in Fields)
                {
                    field.IsWinning = false;
                }

                RefreshTable();
            }
            else
            {
                Fields.Clear();

                TableSize = _model.Height;

                for (int i = 0; i < _model.Height; i++)
                {
                    for (int j = 0; j < _model.Width; j++)
                    {
                        Fields.Add(new ConnectFourField()
                        {
                            Text = String.Empty,
                            Row = i,
                            Col = j,
                            Number = i * _model.Width + j, // gomb sorszáma
                            IsWinning = false,
                            StepCommand = new DelegateCommand(param => StepGame(Convert.ToInt32(param)))
                        });
                    }
                }
            }
        }

        private void InitializeLoadedTable()
        {
            Fields.Clear();

            TableSize = _model.Height;

            for (int i = 0; i < _model.Height; i++)
            {
                for (int j = 0; j < _model.Width; j++)
                {
                    Fields.Add(new ConnectFourField()
                    {
                        Text = String.Empty,
                        Row = i,
                        Col = j,
                        Number = i * _model.Width + j, // gomb sorszáma
                        IsWinning = false,
                        StepCommand = new DelegateCommand(param => StepGame(Convert.ToInt32(param)))
                    });
                }
            }
        }

        private void StepGame(int index)
        {
            ConnectFourField field = Fields[index];

            _model.Move(field.Col);

            OnPropertyChanged(nameof(TurnTime));
            OnPropertyChanged(nameof(PlayerString));
            OnPropertyChanged(nameof(HasMoves));
        }

        #endregion


        #region Public Methods

        public void RefreshTable()
        {
            foreach (ConnectFourField field in Fields)
            {
                field.Text = _model.GetValue(field.Row, field.Col) == 'e'
                    ? String.Empty
                    : _model.GetValue(field.Row, field.Col).ToString().ToUpper();
            }

            OnPropertyChanged(nameof(TurnTime));
        }

        public void AppTimerChanged()
        {
            OnPropertyChanged(nameof(AppTimerEnabled));
        }

        #endregion


        #region Model Event Handlers

        private void Model_GameCreated(object? sender, EventArgs e)
        {
            InitializeTable();
            OnPropertyChanged(nameof(HasMoves));
            AppTimerEnabled = true; OnPropertyChanged(nameof(AppTimerEnabled));
        }

        private void Model_GameLoaded(object? sender, EventArgs e)
        {
            InitializeLoadedTable();
            OnPropertyChanged(nameof(HasMoves));
            AppTimerEnabled = true; OnPropertyChanged(nameof(AppTimerEnabled));
        }

        private void Model_TimerAdvanced(object? sender, TimerAdvancedEventArgs e)
        {
            OnPropertyChanged(nameof(TurnTime));
            OnPropertyChanged(nameof(XTime));
            OnPropertyChanged(nameof(OTime));
        }

        private void Model_TileChanged(object? sender, TileChangedEventArgs e)
        {
            String player = String.Empty;
            if (e.PlayerOnTile == Player.X)
            {
                player = "X";
            }
            if (e.PlayerOnTile == Player.O)
            {
                player = "O";
            }
            Fields[e.X * _model.Width + e.Y].Text = player;
        }

        #endregion


        #region Event Triggers

        private void OnNewGame()
        {
            NewGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnSaveGame()
        {
            SaveGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnLoadGame()
        {
            LoadGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnPauseGame()
        {
            PauseGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnExitGame()
        {
            ExitGame?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}