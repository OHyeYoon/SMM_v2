namespace SMM.Desktop.Models;

public sealed class SettingInfo
{
    public string Language { get; set; } = "en";
    public string Theme { get; set; } = "light";

    public string GamePath { get; set; } = string.Empty;

    public string SmapiPath =>
        string.IsNullOrWhiteSpace(GamePath)
            ? string.Empty
            : Path.Combine(GamePath, "StardewModdingAPI.exe");

    public string ModPath =>
        string.IsNullOrWhiteSpace(GamePath)
            ? string.Empty
            : Path.Combine(GamePath, "Mods");

    public string CurrentProfile { get; set; } = "Default";
}