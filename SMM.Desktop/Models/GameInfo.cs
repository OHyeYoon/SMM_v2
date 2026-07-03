namespace SMM.Desktop.Models;

public sealed class GameInfo
{
    public bool IsInstalled { get; init; }

    public string GamePath { get; init; } = string.Empty;

    public string ExecutablePath { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;

    public string ErrorMessage { get; init; } = string.Empty;
}