using System.Reflection;
using Microsoft.Extensions.Configuration;
using SMM.Desktop.Models;

namespace SMM.Desktop.Services.Setting;

public sealed class SettingsService
{
    private SettingInfo? _settings;

    public SettingInfo Get()
    {
        _settings ??= Load();
        return _settings;
    }

    private static SettingInfo Load()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "config.ini");

        if (!File.Exists(configPath))
        {
            return new SettingInfo();
        }

        var config = new ConfigurationBuilder()
            .AddIniFile(configPath, optional: true)
            .Build();

        var gamePath = config["Folders:GamePath"] ?? string.Empty;

        return new SettingInfo
        {
            Language = config["Settings:Language"] ?? "en",
            Theme = config["Settings:Theme"] ?? "light",
            GamePath = gamePath,
            CurrentProfile = config["General:CurrentProfile"] ?? "Default",
        };
    }

    public void SetCurrentProfile(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
            return;

        var configPath = Path.Combine(AppContext.BaseDirectory, "config.ini");

        var lines = File.Exists(configPath)
            ? File.ReadAllLines(configPath).ToList()
            : new List<string>();

        var sectionIndex = lines.FindIndex(line =>
            line.Trim().Equals("[General]", StringComparison.OrdinalIgnoreCase));

        if (sectionIndex < 0)
        {
            lines.Insert(0, "[General]");
            lines.Insert(1, $"CurrentProfile={profileName}");
        }
        else
        {
            var inserted = false;

            for (var i = sectionIndex + 1; i < lines.Count; i++)
            {
                var line = lines[i].Trim();

                if (line.StartsWith("[") && line.EndsWith("]"))
                    break;

                if (line.StartsWith("CurrentProfile=", StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = $"CurrentProfile={profileName}";
                    inserted = true;
                    break;
                }
            }

            if (!inserted)
            {
                lines.Insert(sectionIndex + 1, $"CurrentProfile={profileName}");
            }
        }

        File.WriteAllLines(configPath, lines);

        if (_settings != null)
        {
            _settings.CurrentProfile = profileName;
        }
    }

    public void SaveSettings(string language, string theme)
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "config.ini");

        var lines = File.Exists(configPath)
            ? File.ReadAllLines(configPath).ToList()
            : new List<string>();

        SetIniValue(lines, "Settings", "Language", language);
        SetIniValue(lines, "Settings", "Theme", theme);

        File.WriteAllLines(configPath, lines);

        _settings ??= new SettingInfo();
        _settings.Language = language;
        _settings.Theme = theme;
    }

    private static void SetIniValue(
        List<string> lines,
        string sectionName,
        string key,
        string value)
    {
        var sectionHeader = $"[{sectionName}]";

        var sectionIndex = lines.FindIndex(line =>
            line.Trim().Equals(sectionHeader, StringComparison.OrdinalIgnoreCase));

        if (sectionIndex < 0)
        {
            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                lines.Add(string.Empty);

            lines.Add(sectionHeader);
            lines.Add($"{key}={value}");
            return;
        }

        for (var i = sectionIndex + 1; i < lines.Count; i++)
        {
            var line = lines[i].Trim();

            if (line.StartsWith("[") && line.EndsWith("]"))
                break;

            if (line.StartsWith($"{key}=", StringComparison.OrdinalIgnoreCase))
            {
                lines[i] = $"{key}={value}";
                return;
            }
        }

        lines.Insert(sectionIndex + 1, $"{key}={value}");
    }

    public string GetVersion()
    {
        return Assembly
            .GetExecutingAssembly()
            .GetName()
            .Version?
            .ToString(3) ?? "Unknown";
    }
}