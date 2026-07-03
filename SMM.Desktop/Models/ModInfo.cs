
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed record ModInfo
{
    public string Name { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string UniqueId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public string FolderName { get; init; } = string.Empty;
    public string FolderPath { get; init; } = string.Empty;
    public string NexusUrl { get; init; } = string.Empty;
    public string ManifestPath { get; init; } = string.Empty;

    public bool HasManifest { get; init; }
    public bool IsValidManifest { get; init; }
    public bool IsEnabled { get; init; }

    public string Status { get; init; } = "Normal";
    public string ErrorMessage { get; init; } = string.Empty;

    public List<ModIssue> Issues { get; init; } = [];

    public List<ModDependency> Dependencies { get; init; } = [];
    public string ContentPackForUniqueId { get; init; } = string.Empty;
}

public sealed class ModDependency
{
    public string UniqueID { get; set; } = string.Empty;

    [JsonPropertyName("IsRequired")]
    public JsonElement? IsRequiredRaw { get; set; }

    [JsonIgnore]
    public bool IsRequired
    {
        get
        {
            if (IsRequiredRaw is null)
                return true;

            var raw = IsRequiredRaw.Value;

            if (raw.ValueKind == JsonValueKind.True)
                return true;

            if (raw.ValueKind == JsonValueKind.False)
                return false;

            if (raw.ValueKind == JsonValueKind.String)
            {
                var value = raw.GetString();

                if (bool.TryParse(value, out var result))
                    return result;
            }

            return true;
        }
    }
}

public sealed class ModContentPackFor
{
    public string UniqueID { get; set; } = string.Empty;
}

public sealed class ModIssue
{
    public string Type { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Target { get; init; }
}

public sealed class ModManifest
{
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string UniqueID { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MinimumApiVersion { get; set; } = string.Empty;
    public string EntryDll { get; set; } = string.Empty;
     public List<JsonElement> UpdateKeys { get; set; } = [];
    public List<ModDependency> Dependencies { get; set; } = [];
    public ModContentPackFor? ContentPackFor { get; set; }
}