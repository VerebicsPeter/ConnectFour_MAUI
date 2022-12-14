using ConnectFour.Persistence;
using ConnectFour.Model;
using ConnectFour.ViewModel;
using ConnectFour_MAUI.Persistence;

namespace ConnectFour_MAUI;

public partial class App : Application
{
    private const string SuspendedGameSavePath = "SuspendedGame";

    private readonly AppShell _appShell;
    private readonly IStore _store;
    private readonly IGameDataAccess _gameDataAccess;
    private readonly GameModel _gameModel;
    private readonly ConnectFourViewModel _gameViewModel;

    public App()
	{
		InitializeComponent();
        // persistence
        _store          = new ConnectFourStore();
        _gameDataAccess = new SaveFileDataAcess(FileSystem.AppDataDirectory);
        // model and view-model
        _gameModel     = new GameModel(_gameDataAccess);
        _gameViewModel = new ConnectFourViewModel(_gameModel);

        _appShell = new AppShell(_store, _gameDataAccess, _gameModel, _gameViewModel)
        {
            BindingContext = _gameViewModel // binding
        };

		MainPage = _appShell; // view
	}

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = base.CreateWindow(activationState);

        window.Created += (s, e) =>
        {
            // új játékot indítunk
            _gameModel.StartGame();
            _appShell.StartTimer();
        };

        window.Activated += (s, e) =>
        {
            return;
            // TODO: Fix this!
            /*
            if (!File.Exists(Path.Combine(FileSystem.AppDataDirectory, SuspendedGameSavePath))) return;

            Task.Run(async () =>
            {
                // betöltjük a felfüggesztett játékot, amennyiben van
                try
                {
                    await _gameModel.LoadGameAsync(SuspendedGameSavePath);

                    // csak akkor indul az időzítő, ha sikerült betölteni a játékot
                    _appShell.StartTimer();
                }
                catch
                {
                }
            });
            */
        };

        window.Stopped += (s, e) =>
        {
            Task.Run(async () =>
            {
                try
                {
                    // elmentjük a jelenleg folyó játékot
                    _appShell.StopTimer();
                    await _gameModel.SaveGameAsync(SuspendedGameSavePath);
                }
                catch
                {
                }
            });
        };

        return window;
    }
}
