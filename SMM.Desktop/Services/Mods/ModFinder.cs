using System.Text.Json;
using SMM.Desktop.Models;

namespace SMM.Desktop.Services.Mods;

public sealed class ModFinder
{
    private readonly ModReader _modReader;

    public ModFinder(ModReader modReader)
    {
        _modReader = modReader;
    }

    public List<ModInfo> Scan(string? modsPath)
    {
        var result = new List<ModInfo>();

        if (string.IsNullOrWhiteSpace(modsPath))
            return result;

        if (!Directory.Exists(modsPath))
            return result;

        var modFolders = Directory.GetDirectories(modsPath);

        foreach (var folderPath in modFolders)
        {
            var folderName = Path.GetFileName(folderPath);
            var displayName = folderName.TrimStart('.');
            var manifestPath = Path.Combine(folderPath, "manifest.json");
            var isEnabled = !folderName.StartsWith(".");

            if (!File.Exists(manifestPath))
            {
                result.Add(new ModInfo
                {
                    FolderName = folderName,
                    FolderPath = folderPath,
                    ManifestPath = manifestPath,

                    HasManifest = false,
                    IsValidManifest = false,
                    IsEnabled = isEnabled,

                    Name = displayName,

                    Status = "Warning",
                    ErrorMessage = "manifest.json not found.",

                    Issues =
                    [
                        new ModIssue
                        {
                            Type = "MissingManifest",
                            Message = "manifest.json not found."
                        }
                    ]
                });

                continue;
            }

            try
            {
                var manifest = _modReader.Read(manifestPath);
                if (manifest == null)
                {
                    result.Add(new ModInfo
                    {
                        FolderName = folderName,
                        FolderPath = folderPath,
                        ManifestPath = manifestPath,
                        HasManifest = true,
                        IsValidManifest = false,
                        IsEnabled = isEnabled,
                        Name = displayName,
                        Status = "Error",
                        ErrorMessage = "manifest.json could not be read.",

                        Issues =
                        [
                            new ModIssue
                            {
                                Type = "InvalidManifest",
                                Message = "manifest.json could not be read."
                            }
                        ]
                    });

                    continue;
                }

                var nexusUrl = GetNexusUrl(manifest);

                var issues = new List<ModIssue>();

                if (!string.IsNullOrWhiteSpace(manifest.EntryDll))
                {
                    var dllPath = Path.Combine(folderPath, manifest.EntryDll);

                    if (!File.Exists(dllPath))
                    {
                        issues.Add(new ModIssue
                        {
                            Type = "MissingEntryDll",
                            Message = $"Entry DLL not found: {manifest.EntryDll}",
                            Target = manifest.EntryDll
                        });
                    }
                }

                result.Add(new ModInfo
                {
                    FolderName = folderName,
                    FolderPath = folderPath,
                    ManifestPath = manifestPath,
                    NexusUrl = nexusUrl,
                    HasManifest = true,
                    IsValidManifest = true,
                    IsEnabled = isEnabled,
                    Name = string.IsNullOrWhiteSpace(manifest.Name) ? folderName : manifest.Name,
                    Author = manifest.Author,
                    Version = manifest.Version,
                    UniqueId = manifest.UniqueID,
                    Description = manifest.Description,
                    UpdateKeys = manifest.UpdateKeys,

                    Dependencies = manifest.Dependencies,
                    ContentPackForUniqueId = manifest.ContentPackFor?.UniqueID ?? string.Empty,

                    Status = issues.Count > 0 ? "Error" : "Normal",
                    ErrorMessage = issues.FirstOrDefault()?.Message ?? string.Empty,
                    Issues = issues
                });
            }
            catch (Exception ex)
            {
                result.Add(new ModInfo
                {
                    FolderName = folderName,
                    FolderPath = folderPath,
                    ManifestPath = manifestPath,
                    HasManifest = true,
                    IsValidManifest = false,
                    IsEnabled = isEnabled,
                    Name = displayName,
                    Status = "Error",
                    ErrorMessage = ex.Message,

                    Issues =
                    [
                        new ModIssue
                        {
                            Type = "InvalidManifest",
                            Message = ex.Message
                        }
                    ]
                });
            }
        }

        return result;
    }

    private static string GetNexusUrl(ModManifest manifest)
    {
        foreach (var value in manifest.UpdateKeys)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            if (!value.StartsWith("Nexus:", StringComparison.OrdinalIgnoreCase))
                continue;

            var nexusId = value["Nexus:".Length..].Trim();

            if (string.IsNullOrWhiteSpace(nexusId))
                continue;

            return $"https://www.nexusmods.com/stardewvalley/mods/{nexusId}";
        }

        return string.Empty;
    }
}