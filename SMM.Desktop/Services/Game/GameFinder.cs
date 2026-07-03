using SMM.Desktop.Models;

namespace SMM.Desktop.Services.Game;

public sealed class GameFinder
{
    private const string GameExeName = "Stardew Valley.exe";

    public GameInfo Find(string? gamePath)
    {
        if (string.IsNullOrWhiteSpace(gamePath))
        {
            return new GameInfo
            {
                IsInstalled = false,
                ErrorMessage = "GamePath is empty."
            };
        }

        if (!Directory.Exists(gamePath))
        {
            return new GameInfo
            {
                IsInstalled = false,
                GamePath = gamePath,
                ErrorMessage = "Game folder does not exist."
            };
        }

        var exePath = Path.Combine(gamePath, GameExeName);

        if (!File.Exists(exePath))
        {
            return new GameInfo
            {
                IsInstalled = false,
                GamePath = gamePath,
                ExecutablePath = exePath,
                ErrorMessage = "Stardew Valley.exe was not found."
            };
        }

        return new GameInfo
        {
            IsInstalled = true,
            GamePath = gamePath,
            ExecutablePath = exePath
        };
    }
}