namespace SMM.Desktop.Configuration;

public sealed class Config
{
    public GameConfig Game { get; set; } = new();
    public UiConfig UI { get; set; } = new();
    public StartupConfig Startup { get; set; } = new();
}

public sealed class GameConfig
{
    public string GamePath { get; set; } = string.Empty;
}

public sealed class UiConfig
{
    public string Theme { get; set; } = "Light";
}

public sealed class StartupConfig
{
    public bool CheckUpdate { get; set; } = true;
}