using SMM.Desktop.Models;

namespace SMM.Desktop.Services.Profiles;

public sealed class ProfileFinder
{
    public List<ProfileInfo> FindProfiles(string gamePath, string currentProfile)
    {
        var profiles = new List<ProfileInfo>();

        if (string.IsNullOrWhiteSpace(currentProfile))
            currentProfile = "Default";

        var modsPath = string.IsNullOrWhiteSpace(gamePath)
            ? string.Empty
            : Path.Combine(gamePath, "Mods");

        profiles.Add(new ProfileInfo
        {
            Name = currentProfile,
            IsActive = true,
            FolderPath = modsPath,
            Exists = !string.IsNullOrWhiteSpace(modsPath) && Directory.Exists(modsPath)
        });

        if (string.IsNullOrWhiteSpace(gamePath))
            return profiles;

        if (!Directory.Exists(gamePath))
            return profiles;

        var profileFolders = Directory.GetDirectories(gamePath, "Mods_*");

        foreach (var folderPath in profileFolders)
        {
            var folderName = Path.GetFileName(folderPath);

            if (string.IsNullOrWhiteSpace(folderName))
                continue;

            var profileName = folderName["Mods_".Length..];

            if (string.IsNullOrWhiteSpace(profileName))
                continue;

            if (profileName.Equals(currentProfile, StringComparison.OrdinalIgnoreCase))
                continue;

            profiles.Add(new ProfileInfo
            {
                Name = profileName,
                IsActive = false,
                FolderPath = folderPath,
                Exists = true
            });
        }

        return profiles
            .OrderByDescending(p => p.IsActive)
            .ThenBy(p => p.Name)
            .ToList();
    }
}