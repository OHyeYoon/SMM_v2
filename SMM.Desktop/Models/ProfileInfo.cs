namespace SMM.Desktop.Models;

public sealed record ProfileInfo
{
    public string Name { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public string FolderPath { get; init; } = string.Empty;

    public bool Exists { get; init; }
}