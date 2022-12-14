using System;
using System.Text;
using System.Drawing;
using ConnectFour.Persistence;

namespace ConnectFour.Model
{
    public enum Player
    {
        X, O
    }

    public enum GameState
    {
        None, XWon, OWon
    }

    public enum GameSize
    {
        Small, Medium, Large, Custom
    }

    // Model for the game
    public class GameModel
    {
        #region Private Fields

        private int _h, _w;
        private GameSize _gameSize;

        private Tile[,] Tiles; // Game table represented by tiles
        private Player _currentPlayer;
        private GameState _currentState;
        private IGameDataAccess _dataAccess;

        private int _tTime;   // Time elapsed in current turn
        private int _xTime, _oTime;  // Sum of players' times

        #endregion

        #region Public Properties

        public int Moves { get; private set; }

        public int TurnTime { get { return _tTime; } }
        public int X_Time { get { return _xTime; } }
        public int O_Time { get { return _oTime; } }

        public int Height { get { return _h; } }

        public int Width { get { return _w; } }

        public GameSize Size
        {
            get { return  _gameSize; }
            set { _gameSize = value; }
        }

        public Player CurrentPlayer { get { return _currentPlayer; } }

        public GameState CurrentState { get { return _currentState; } }

        public Stack<Point> WinningPointStack { get; private set; }

        #endregion

        #region Events

        public event EventHandler? GameEnd;
        public event EventHandler<GameWonEventArgs>? GameWon;
        public event EventHandler<TileChangedEventArgs>? TileChanged;
        public event EventHandler<TimerAdvancedEventArgs>? TimerAdvanced;
        public event EventHandler? GameCreated;
        public event EventHandler? GameLoaded;

        #endregion

        #region Constructor

        public GameModel(IGameDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
            _h = 10; _w = 10;
            Tiles = new Tile[_h, _w];

            WinningPointStack = new Stack<Point>();

            StartGame();
        }

        #endregion

        #region Public Methods

        public void StartGame()
        {
            SetHeightAndWidth(_gameSize);

            for (int i = 0; i < _h; i++)
                for (int j = 0; j < _w; j++)
                    Tiles[i, j] = new Tile();

            _currentState = GameState.None;
            _currentPlayer = Player.X;

            _tTime = 0;
            _xTime = 0;
            _oTime = 0;

            Moves = 0;

            OnGameCreated();
        }

        // Sets tile and changes current player
        public void Move(int col)
        {
            int row = GetRow(col);
            if (row > -1)
            {
                if (Tiles[row, col].IsEmpty())
                {
                    if (_currentPlayer == Player.X)
                    {
                        Tiles[row, col].Value = TileValue.X;
                    }
                    else
                    {
                        Tiles[row, col].Value = TileValue.O;
                    }
                    _tTime = 0; // Reset the turn time

                    OnTileChanged(row, col, _currentPlayer);

                    _currentPlayer = _currentPlayer == Player.X ? Player.O : Player.X; Moves++; // Set player
                    _currentState = GetCurrentState();  // Set state

                    if (CurrentState != GameState.None)
                    {
                        OnGameWon();
                    }
                    if (Moves == Height * Width)
                    {
                        OnGameEnd();
                    }
                }
            }
        }

        // Checks and sets game's state and returns it
        public GameState GetCurrentState()
        {
            // Horizontals
            for (int i = 0; i < _h; i++)
            {
                Stack<TileValue> stack = new();
                WinningPointStack.Clear();

                for (int j = 0; j < _w; j++)
                {
                    PushSameTile(i, j, stack, WinningPointStack);

                    if (stack.Count == 4)
                    {
                        if (stack.Peek() == TileValue.X) return GameState.XWon;
                        return GameState.OWon;
                    }
                }
            }
            // Verticals
            for (int j = 0; j < _w; j++) // cols
            {
                Stack<TileValue> stack = new();
                WinningPointStack.Clear();

                for (int i = 0; i < _h; i++) // rows
                {
                    PushSameTile(i, j, stack, WinningPointStack);

                    if (stack.Count == 4)
                    {
                        if (stack.Peek() == TileValue.X) return GameState.XWon;
                        return GameState.OWon;
                    }
                }
            }

            #region // Right diagonals
            for (int k = 0; k < _h + _w; k++)
            {
                Stack<TileValue> stack = new();
                WinningPointStack.Clear();

                for (int i = 0; i < _h; i++)
                {
                    for (int j = 0; j < _w; j++)
                    {
                        if (i + j == k)
                        {
                            PushSameTile(i, j, stack, WinningPointStack);

                            if (stack.Count == 4)
                            {
                                if (stack.Peek() == TileValue.X) return GameState.XWon;
                                return GameState.OWon;
                            }
                        }
                    }
                }
            }
            #endregion

            #region // Left diagonals
            // Upper diagonals (including the main diagonal)
            for (int k = 0; k < _w; k++)
            {
                Stack<TileValue> stack = new();
                WinningPointStack.Clear();

                for (int i = 0; i < _h; i++)
                {
                    for (int j = 0; j < _w; j++)
                    {
                        if (j - i == k)
                        {
                            PushSameTile(i, j, stack, WinningPointStack);

                            if (stack.Count == 4)
                            {
                                if (stack.Peek() == TileValue.X) return GameState.XWon;
                                return GameState.OWon;
                            }
                        }
                    }
                }
            }
            // Lower diagonals (excluding the main diagonal)
            for (int k = 1; k < _h; k++)
            {
                Stack<TileValue> stack = new();
                WinningPointStack.Clear();

                for (int i = 0; i < _h; i++)
                {
                    for (int j = 0; j < _w; j++)
                    {
                        if (i - j == k)
                        {
                            PushSameTile(i, j, stack, WinningPointStack);

                            if (stack.Count == 4)
                            {
                                if (stack.Peek() == TileValue.X) return GameState.XWon;
                                return GameState.OWon;
                            }
                        }
                    }
                }
            }
            #endregion

            return GameState.None;
        }

        public int GetRow(int col)
        {
            if (0 <= col && col < _w)
            {
                for (int i = _h - 1; i > -1; i--)
                {
                    if (Tiles[i, col].IsEmpty())
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public char GetValue(int i, int j)
        {
            if (i >= 0 && i < _h && j >= 0 && j < _w)
            {
                if (Tiles[i, j].IsX()) return 'x';
                if (Tiles[i, j].ISO()) return 'o';
            }
            return 'e';
        }

        public int GetMoves()
        {
            int count = 0;
            for (int i = 0; i < _h; i++)
            {
                for (int j = 0; j < _w; j++)
                {
                    if (!Tiles[i, j].IsEmpty()) count++;
                }
            }
            return count;
        }

        public async Task<Point?> LoadGameAsync(string path) // also returns the times read
        {
            int height = _h, width = _w;

            try
            {
                if (_dataAccess == null) throw new InvalidOperationException("No data acesss provided.");
                GameTable table = await _dataAccess.LoadAsync(path);

                if (table.X < 3 || table.Y < 3) throw new InvalidOperationException("Table size corrupted.");

                Tiles = new Tile[table.X, table.Y];
                _h = table.X; _w = table.Y;

                // Initialize Tiles based on table
                for (int i = 0; i < table.X; i++)
                {
                    if (table.Table[i].Length == table.Y) // Check if the current string is the same length as the one provided in table.Y
                    {
                        char[] chars = table.Table[i].ToCharArray();

                        for (int j = 0; j < chars.Length; j++)
                        {
                            Tiles[i, j] = Tile.MakeTile(chars[j]);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Row length corrupted.");
                    }
                }
                Moves = GetMoves();

                SetSize();

                _tTime = 0;
                _xTime = table.XTime; _oTime = table.OTime;

                _currentPlayer = table.currPlayer == "x" ? Player.X : Player.O;
                _currentState  = GetCurrentState();

                if (_currentState != GameState.None)
                {
                    throw new InvalidOperationException("Game state corrupted.");
                }

                OnGameLoaded();

                return new Point(table.XTime, table.OTime);
            }
            catch
            {
                _h = height; _w = width;
                return null;
            }
        }

        public async Task SaveGameAsync(string path)
        {
            if (_dataAccess == null) throw new InvalidOperationException("No data acesss provided.");

            string curr = CurrentPlayer == Player.X ? "x" : "o";

            GameTable table = new(_h, _w, _xTime, _oTime, curr);

            for (int i = 0; i < _h; i++)
            {
                StringBuilder sb = new();

                for (int j = 0; j < _w; j++)
                {
                    switch (Tiles[i, j].Value)
                    {
                        case TileValue.X: sb.Append('x'); break;
                        case TileValue.O: sb.Append('o'); break;
                        default: sb.Append('e'); break;
                    }
                }
                table.Table[i] = sb.ToString();
            }

            await _dataAccess.SaveAsync(path, table);
        }

        #endregion

        #region Private Methods

        private void SetSize()
        {
            if (_h == 10 && _w == 10) { _gameSize = GameSize.Small; }
            if (_h == 20 && _w == 20) { _gameSize = GameSize.Medium; }
            if (_h == 30 && _w == 30) { _gameSize = GameSize.Large; }
        }

        private void SetHeightAndWidth(GameSize size)
        {
            switch (size)
            {
                case GameSize.Small:
                    _h = 10; _w = 10;
                    break;
                case GameSize.Medium:
                    _h = 20; _w = 20;
                    break;
                case GameSize.Large:
                    _h = 30; _w = 30;
                    break;
                case GameSize.Custom:
                    _h = 10; _w = 10; // TODO
                    break;
                default:
                    _h = 10; _w = 10;
                    break;
            }

            Tiles = new Tile[_h, _w];
        }

        // pushes into the stack (or clears it) to checks game state
        private void PushSameTile(int i, int j, Stack<TileValue> valueStack, Stack<Point> pointStack)
        {
            if (!Tiles[i, j].IsEmpty()) // if on an occupied tile
            {
                if (valueStack.Count == 0) // if the stack is empty
                {
                    valueStack.Push(Tiles[i, j].Value); pointStack.Push(new Point(i, j));
                }
                else // if the stack has elements
                {
                    if (valueStack.Peek() == Tiles[i, j].Value) // if top is the same as actual tile
                    {
                        valueStack.Push(Tiles[i, j].Value); pointStack.Push(new Point(i, j));
                    }
                    else // if the top is different
                    {
                        valueStack.Clear(); pointStack.Clear();
                        valueStack.Push(Tiles[i, j].Value); pointStack.Push(new Point(i, j));
                    }
                }
            }
            else { valueStack.Clear(); pointStack.Clear(); }
        }
        #endregion

        #region Event Handlers

        public void Timer_Tick(object? sender, EventArgs e)
        {
            _tTime++;
            if (_currentPlayer == Player.X) _xTime++; else _oTime++;
            OnTimerAdvanced();
        }

        #endregion

        #region Event Triggers
        //Event trigger for tile change
        private void OnTileChanged(int x, int y, Player player)
        {
            TileChanged?.Invoke(this, new TileChangedEventArgs(x, y, player));
        }
        // Event trigger for draw
        private void OnGameEnd()
        {
            GameEnd?.Invoke(this, EventArgs.Empty);
        }
        // Event trigger for win
        private void OnGameWon()
        {
            GameWon?.Invoke(this, new GameWonEventArgs(_currentState, WinningPointStack));
        }
        // Timer advanced
        private void OnTimerAdvanced()
        {
            TimerAdvanced?.Invoke(this, new TimerAdvancedEventArgs(_tTime, _xTime, _oTime));
        }

        private void OnGameCreated()
        {
            GameCreated?.Invoke(this, EventArgs.Empty);
        }

        private void OnGameLoaded()
        {
            GameLoaded?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}