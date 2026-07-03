using SMM.Desktop.Models;
using SMM.Desktop.Services.Game;
using SMM.Desktop.Services.Setting;

namespace SMM.Desktop.Bridge;

public sealed class GameBridge
{
    private readonly GameService _gameService;

    public GameBridge()
    {
        var settingsService = new SettingsService();

        _gameService = new GameService(
            settingsService,
            new GameFinder(),
            new GameVersionReader());
    }

    public GameInfo GetInfo()
    {
        return _gameService.GetGameInfo();
    }
}