using ConnectFour.Persistence;
using ConnectFour.Model;
using ConnectFour.ViewModel;
using ConnectFour_MAUI.View;
using ConnectFour_MAUI.ViewModel;

namespace ConnectFour_MAUI;

public partial class AppShell : Shell
{
    private readonly IGameDataAccess _dataAccess;
    private readonly GameModel _gameModel;
    private readonly ConnectFourViewModel _gameViewModel;

    private readonly IDispatcherTimer _timer;

    private readonly IStore _store;
    private readonly StoredGameBrowserModel _storedGameBrowserModel;
    private readonly StoredGameBrowserViewModel _storedGameBrowserViewModel;

    public AppShell
        (
            IStore store, IGameDataAccess dataAccess,
            GameModel gameModel, ConnectFourViewModel gameViewModel
        )
    {
        InitializeComponent();

        // init game
        _store = store;
        _dataAccess = dataAccess;
        _gameModel = gameModel;
        _gameViewModel = gameViewModel;

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (s, e) => { _gameModel.Timer_Tick(s, e); };

        // game model events
        _gameModel.GameEnd += GameModel_GameEnd;
        _gameModel.GameWon += GameModel_GameWon;

        // game view model events
        _gameViewModel.NewGame += GameViewModel_NewGame;
        _gameViewModel.SaveGame += GameViewModel_SaveGame;
        _gameViewModel.LoadGame += GameViewModel_LoadGame;
        _gameViewModel.PauseGame += GameViewModel_PauseGame;
        _gameViewModel.ExitGame += GameViewModel_ExitGame;

        // stored game browser view model
        _storedGameBrowserModel = new StoredGameBrowserModel(_store);
        _storedGameBrowserViewModel = new StoredGameBrowserViewModel(_storedGameBrowserModel);
        _storedGameBrowserViewModel.GameSaving += StoredGameBrowserViewModel_GameSaving;
        _storedGameBrowserViewModel.GameLoading += StoredGameBrowserViewModel_GameLoading;
    }

    private void GameViewModel_PauseGame(object? sender, EventArgs e)
    {
        if (_gameViewModel.AppTimerEnabled == true)
        {
            StopTimer();
        }
        else
        {
            StartTimer();
        }
        _gameViewModel.AppTimerChanged();
    }

    #region Internal methods

    internal void StartTimer()
    {
        _timer.Start();
        _gameViewModel.AppTimerEnabled = true;
    }

    internal void StopTimer()
    {
        _timer.Stop();
        _gameViewModel.AppTimerEnabled = false;
    }

    #endregion


    #region Game ViewModel Event Handlers

    private void GameViewModel_NewGame(object? sender, EventArgs e)
    {
        _gameModel.StartGame();

        StartTimer();
    }

    private async void GameViewModel_SaveGame(object? sender, EventArgs e)
    {
        await _storedGameBrowserModel.UpdateAsync(); // frissítjük a tárolt játékok listáját
        await Navigation.PushAsync(new SaveGamePage
        {
            BindingContext = _storedGameBrowserViewModel
        }); // átnavigálunk a lapra
    }

    private async void GameViewModel_LoadGame(object? sender, EventArgs e)
    {
        await _storedGameBrowserModel.UpdateAsync(); // frissítjük a tárolt játékok listáját
        await Navigation.PushAsync(new LoadGamePage
        {
            BindingContext = _storedGameBrowserViewModel
        }); // átnavigálunk a lapra
    }

    private async void GameViewModel_ExitGame(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage
        {
            BindingContext = _gameViewModel
        }); // átnavigálunk a beállítások lapra
    }

    #endregion


    #region Game Model Event Handlers

    private async void GameModel_GameEnd(object? sender, EventArgs e)
    {
        StopTimer();

        await DisplayAlert("Connect Four", "Draw!", "OK");

        _gameModel.StartGame();

        StartTimer();
    }

    private async void GameModel_GameWon(object? sender, GameWonEventArgs e)
    {
        StopTimer();

        // mark winning fields
        int w = _gameModel.Width;
        for (int i = 0; i < e.WinningCoordList.Count; i++)
        {
            _gameViewModel.Fields[e.WinningCoordList[i].X * w + e.WinningCoordList[i].Y].IsWinning = true;
        }
        _gameViewModel.RefreshTable();

        if (e.State == GameState.XWon)
        {
            await DisplayAlert("Connect Four", "X won!", "OK");
        }
        else
        {
            await DisplayAlert("Connect Four", "O won!", "OK");
        }

        _gameModel.StartGame();

        StartTimer();
    }

    #endregion


    #region Store Game Browser ViewModel Event Handlers

    private async void StoredGameBrowserViewModel_GameSaving(object? sender, StoredGameEventArgs e)
    {
        await Navigation.PopAsync(); // visszanavigálunk
        StopTimer();

        try
        {
            await _gameModel.SaveGameAsync(e.Name);
            await DisplayAlert("Connect Four", "Game saved successfully.", "OK");
        }
        catch
        {
            await DisplayAlert("Connect Four", "Failed saving game.", "OK");
        }
    }

    private async void StoredGameBrowserViewModel_GameLoading(object? sender, StoredGameEventArgs e)
    {
        await Navigation.PopAsync(); // visszanavigálunk

        try
        {
            await _gameModel.LoadGameAsync(e.Name);

            await Navigation.PopAsync(); // visszanavigálás
            await DisplayAlert("Connect Four", "Game saved successfully.", "OK");
            // csak akkor indul az időzítő, ha sikerült betölteni a játékot
            StartTimer();
            // refresh the game table in veiwmodel
            _gameViewModel.RefreshTable();

        }
        catch
        {
            await DisplayAlert("Connect Four", "Failed saving game.", "OK");
        }
    }

    #endregion
}