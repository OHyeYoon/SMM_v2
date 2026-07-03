using SMM.Desktop.Models;

namespace SMM.Desktop.Services.Mods;

public sealed class ModValidator
{
    public List<ModInfo> Validate(List<ModInfo> mods)
    {
        var uniqueIdMap = mods
            .Where(m => !string.IsNullOrWhiteSpace(m.UniqueId))
            .GroupBy(m => m.UniqueId.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var result = new List<ModInfo>();

        foreach (var mod in mods)
        {
            var issues = mod.Issues.ToList();

            if (!string.IsNullOrWhiteSpace(mod.UniqueId) &&
                uniqueIdMap.TryGetValue(mod.UniqueId.Trim(), out var duplicatedMods) &&
                duplicatedMods.Count > 1)
            {
                issues.Add(new ModIssue
                {
                    Type = "DuplicateUniqueId",
                    Message = $"Duplicate UniqueID: {mod.UniqueId}",
                    Target = mod.UniqueId
                });
            }

            foreach (var dependency in mod.Dependencies)
            {
                if (!dependency.IsRequired)
                    continue;

                if (string.IsNullOrWhiteSpace(dependency.UniqueID))
                    continue;

                var dependencyExists = uniqueIdMap.TryGetValue(
                    dependency.UniqueID.Trim(),
                    out var dependencyMods
                );

                if (!dependencyExists)
                {
                    issues.Add(new ModIssue
                    {
                        Type = "MissingDependency",
                        Message = $"Required dependency is missing: {dependency.UniqueID}",
                        Target = dependency.UniqueID
                    });

                    continue;
                }

                var dependencyEnabled = dependencyMods.Any(m => m.IsEnabled);

                if (!dependencyEnabled)
                {
                    issues.Add(new ModIssue
                    {
                        Type = "DisabledDependency",
                        Message = $"Required dependency is disabled: {dependency.UniqueID}",
                        Target = dependency.UniqueID
                    });
                }
            }

            if (!string.IsNullOrWhiteSpace(mod.ContentPackForUniqueId))
            {
                var parentExists = uniqueIdMap.ContainsKey(mod.ContentPackForUniqueId.Trim());

                if (!parentExists)
                {
                    issues.Add(new ModIssue
                    {
                        Type = "MissingContentPackParent",
                        Message = $"Content pack parent is missing: {mod.ContentPackForUniqueId}",
                        Target = mod.ContentPackForUniqueId
                    });
                }
            }

            var status = ResolveStatus(mod, issues);

            result.Add(mod with
            {
                Issues = issues,
                Status = status,
                ErrorMessage = issues.FirstOrDefault()?.Message ?? string.Empty
            });
        }

        return result
            .OrderBy(m => !m.IsEnabled)
            .ThenByDescending(m => m.Status == "Error")
            .ThenByDescending(m => m.Status == "Warning")
            .ThenBy(m => m.Name)
            .ToList();
    }

    private static string ResolveStatus(ModInfo mod, List<ModIssue> issues)
    {
        if (issues.Any(i =>
                i.Type == "InvalidManifest" ||
                i.Type == "DuplicateUniqueId" ||
                i.Type == "MissingEntryDll"))
        {
            return "Error";
        }

        if (issues.Count > 0)
            return "Warning";

        if (!mod.IsEnabled)
            return "Disabled";

        return "Normal";
    }
}