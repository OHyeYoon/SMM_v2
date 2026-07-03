using SMM.Desktop.Models;

namespace SMM.Desktop.Services.Smapi;

public sealed class SmapiFinder
{
    private const string SmapiExeName = "StardewModdingAPI.exe";

    public SmapiInfo Find(string? smapiPath)
    {
        if (string.IsNullOrWhiteSpace(smapiPath))
        {
            return new SmapiInfo
            {
                IsInstalled = false,
                ErrorMessage = "SmapiPath is empty."
            };
        }

        if (!Directory.Exists(smapiPath))
        {
            return new SmapiInfo
            {
                IsInstalled = false,
                SmapiPath = smapiPath,
                ErrorMessage = "Game folder does not exist."
            };
        }

        var exePath = Path.Combine(smapiPath, SmapiExeName);

        if (!File.Exists(exePath))
        {
            return new SmapiInfo
            {
                IsInstalled = false,
                SmapiPath = smapiPath,
                ExecutablePath = exePath,
                ErrorMessage = "Stardew Valley.exe was not found."
            };
        }

        return new SmapiInfo
        {
            IsInstalled = true,
            SmapiPath = smapiPath,
            ExecutablePath = exePath
        };
    }
}