using System.Diagnostics;
using SMM.Desktop.Models;
using SMM.Desktop.Services.Setting;

namespace SMM.Desktop.Services.Game;

public sealed class GameService
{
    private readonly SettingsService _settingsService;
    private readonly GameFinder _gameFinder;
    private readonly GameVersionReader _versionReader;

    public GameService(
        SettingsService settingsService,
        GameFinder gameFinder,
        GameVersionReader versionReader)
    {
        _settingsService = settingsService;
        _gameFinder = gameFinder;
        _versionReader = versionReader;
    }

    public GameInfo GetGameInfo()
    {
        var settings = _settingsService.Get();

        var game = _gameFinder.Find(settings.GamePath);

        if (!game.IsInstalled)
            return game;

        var version = _versionReader.ReadVersion(game.ExecutablePath);

        return new GameInfo
        {
            IsInstalled = true,
            GamePath = game.GamePath,
            ExecutablePath = game.ExecutablePath,
            Version = version,
            ErrorMessage = string.Empty
        };
    }

    public bool StartSmapi()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "steam://rungameid/413150",
                UseShellExecute = true
            });

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool OpenModsFolder()
    {
        var game = GetGameInfo();

        if (!game.IsInstalled)
            return false;

        var modsPath = Path.Combine(game.GamePath, "Mods");

        if (!Directory.Exists(modsPath))
            return false;

        Process.Start(new ProcessStartInfo
        {
            FileName = modsPath,
            UseShellExecute = true
        });

        return true;
    }

    public bool IsGameRunning()
    {
        return Process.GetProcessesByName("Stardew Valley").Any()
            || Process.GetProcessesByName("StardewValley").Any()
            || Process.GetProcessesByName("StardewModdingAPI").Any();
    }
}