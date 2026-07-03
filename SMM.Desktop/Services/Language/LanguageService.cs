using System.Text.Json;

namespace SMM.Desktop.Services.Language;

public sealed class LanguageService
{
    public Dictionary<string, string> GetLanguage(string language)
    {
        var lang = string.IsNullOrWhiteSpace(language)
            ? "en"
            : language.Trim().ToLower();

        var assembly = typeof(LanguageService).Assembly;

        var resourcePath = $"Languages/{lang}.json";

        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name =>
                name.Replace("\\", "/")
                    .Equals(resourcePath, StringComparison.OrdinalIgnoreCase));

        resourceName ??= assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name =>
                name.Replace("\\", "/")
                    .Equals("Languages/en.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
            return [];

        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
            return [];

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
    }
}