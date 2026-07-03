using System.Diagnostics;

namespace SMM.Desktop.Services.Smapi;

public sealed class SmapiVersionReader
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

        // 예: "4.5.2+82.branch.hash..." → "4.5.2"
        var plusIndex = version.IndexOf('+');

        if (plusIndex >= 0)
            version = version[..plusIndex];

        // 예: "4.5.2, 12345" → "4.5.2"
        var commaIndex = version.IndexOf(',');

        if (commaIndex >= 0)
            version = version[..commaIndex];

        return version.Trim();
    }
}