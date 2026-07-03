using SMM.Desktop.Models;
using SMM.Desktop.Services.Game;

namespace SMM.Desktop.Services.Mods;

public sealed class ModService
{
    private readonly GameService _gameService;
    private readonly ModFinder _modFinder;

    private readonly ModToggleService _modToggleService;
    private readonly ModValidator _modValidator = new();

     public ModService(
        GameService gameService,
        ModFinder modFinder,
        ModValidator modValidator,
        ModToggleService modToggleService)
    {
        _gameService = gameService;
        _modFinder = modFinder;
        _modValidator = modValidator;
        _modToggleService = modToggleService;
    }


    public List<ModInfo> GetMods()
    {
        var game = _gameService.GetGameInfo();

        if (!game.IsInstalled)
            return [];

        var modsPath = Path.Combine(game.GamePath, "Mods");

        var mods = _modFinder.Scan(modsPath);

        return _modValidator.Validate(mods);
    }

    public bool ToggleMod(string folderPath)
    {
        return _modToggleService.Toggle(folderPath);
    }
}