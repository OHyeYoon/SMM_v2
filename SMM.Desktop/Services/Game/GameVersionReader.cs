using System.Diagnostics;

namespace SMM.Desktop.Services.Game;

public sealed class GameVersionReader
{
    public string ReadVersion(string? executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
            return string.Empty;

        if (!File.Exists(executablePath))
            return string.Empty;

        var info = FileVersionInfo.GetVersionInfo(executablePath);

        var version = info.ProductVersion
            ?? info.FileVersion
            ?? string.Empty;

         return NormalizeVersion(version);
    }

    private static string NormalizeVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return string.Empty;

        // 예: "1.6.15, 24345" → "1.6.15"
        var commaIndex = version.IndexOf(',');

        if (commaIndex >= 0)
            version = version[..commaIndex];

        return version.Trim();
    }
}