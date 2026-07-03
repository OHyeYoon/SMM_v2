using System.Text.Json;
using SMM.Desktop.Models;

namespace SMM.Desktop.Services.Mods;

public sealed class ModReader
{
    public ModManifest? Read(string manifestPath)
    {
        if (!File.Exists(manifestPath))
            return null;

        var json = File.ReadAllText(manifestPath);

        return JsonSerializer.Deserialize<ModManifest>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            });
    }
}