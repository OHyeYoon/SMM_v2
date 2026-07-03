namespace SMM.Desktop.Models;

public sealed class SmapiInfo
{
    public bool IsInstalled { get; init; }

    public string SmapiPath { get; init; } = string.Empty;

    public string ExecutablePath { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;

    public string ErrorMessage { get; init; } = string.Empty;
}