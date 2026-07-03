using SMM.Desktop.Models;
using SMM.Desktop.Services.Game;
using SMM.Desktop.Services.Mods;
using SMM.Desktop.Services.Setting;

using System.IO.Compression;
using System.Text;

namespace SMM.Desktop.Services.Profiles;

public sealed class ProfileService
{
    private readonly SettingsService _settingsService;
    private readonly GameService _gameService;
    private readonly ProfileFinder _profileFinder;
    public ProfileService(
        SettingsService settingsService,
        GameService gameService,
        ProfileFinder profileFinder)
    {
        _settingsService = settingsService;
        _gameService = gameService;
        _profileFinder = profileFinder;
    }

    public List<ProfileInfo> GetProfiles()
    {
        var settings = _settingsService.Get();
        var game = _gameService.GetGameInfo();

        if (!game.IsInstalled)
            return [];

        return _profileFinder.FindProfiles(
            game.GamePath,
            settings.CurrentProfile
        );
    }

    public bool SwitchProfile(string targetProfile)
    {
        if (string.IsNullOrWhiteSpace(targetProfile))
            return false;

        var settings = _settingsService.Get();
        var game = _gameService.GetGameInfo();

        if (!game.IsInstalled)
            return false;

        var currentProfile = settings.CurrentProfile;

        if (string.Equals(currentProfile, targetProfile, StringComparison.OrdinalIgnoreCase))
            return true;

        var gamePath = game.GamePath;

        var modsPath = Path.Combine(gamePath, "Mods");
        var currentProfilePath = Path.Combine(gamePath, $"Mods_{currentProfile}");
        var targetProfilePath = Path.Combine(gamePath, $"Mods_{targetProfile}");

        if (Directory.Exists(modsPath))
        {
            if (Directory.Exists(currentProfilePath))
                Directory.Delete(currentProfilePath, true);

            Directory.Move(modsPath, currentProfilePath);
        }

        if (Directory.Exists(targetProfilePath))
        {
            Directory.Move(targetProfilePath, modsPath);
        }
        else
        {
            Directory.CreateDirectory(modsPath);
        }

        _settingsService.SetCurrentProfile(targetProfile);

        return true;
    }

    public bool CreateProfile(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
            return false;

        var cleanName = profileName.Trim();

        var invalidChars = Path.GetInvalidFileNameChars();

        if (cleanName.Any(c => invalidChars.Contains(c)))
            return false;

        var game = _gameService.GetGameInfo();

        if (!game.IsInstalled)
            return false;

        var profilePath = Path.Combine(game.GamePath, $"Mods_{cleanName}");

        if (Directory.Exists(profilePath))
            return false;

        Directory.CreateDirectory(profilePath);

        return true;
    }

    public bool RenameProfile(string oldName, string newName)
    {
        if (string.IsNullOrWhiteSpace(oldName))
            return false;

        if (string.IsNullOrWhiteSpace(newName))
            return false;

        var cleanNewName = newName.Trim();

        if (cleanNewName.Any(c => Path.GetInvalidFileNameChars().Contains(c)))
            return false;

        var settings = _settingsService.Get();
        var game = _gameService.GetGameInfo();

        if (!game.IsInstalled)
            return false;

        if (string.Equals(oldName, cleanNewName, StringComparison.OrdinalIgnoreCase))
            return true;

        var gamePath = game.GamePath;
        var newProfilePath = Path.Combine(gamePath, $"Mods_{cleanNewName}");

        if (Directory.Exists(newProfilePath))
            return false;

        var isActive = string.Equals(
            settings.CurrentProfile,
            oldName,
            StringComparison.OrdinalIgnoreCase
        );

        if (isActive)
        {
            _settingsService.SetCurrentProfile(cleanNewName);
            return true;
        }

        var oldProfilePath = Path.Combine(gamePath, $"Mods_{oldName}");

        if (!Directory.Exists(oldProfilePath))
            return false;

        Directory.Move(oldProfilePath, newProfilePath);

        return true;
    }

    private string GetProfileModsPath(string profileName)
    {
        var settings = _settingsService.Get();
        var game = _gameService.GetGameInfo();

        if (!game.IsInstalled)
            return string.Empty;

        var isActive = string.Equals(
            settings.CurrentProfile,
            profileName,
            StringComparison.OrdinalIgnoreCase
        );

        return isActive
            ? Path.Combine(game.GamePath, "Mods")
            : Path.Combine(game.GamePath, $"Mods_{profileName}");
    }

    public bool BackupProfile(string profileName)
    {
        var modsPath = GetProfileModsPath(profileName);

        if (string.IsNullOrWhiteSpace(modsPath))
            return false;

        if (!Directory.Exists(modsPath))
            return false;

        var game = _gameService.GetGameInfo();

        var backupFolder = Path.Combine(game.GamePath, "Backups");

        Directory.CreateDirectory(backupFolder);

        var fileName = $"{profileName}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
        var zipPath = Path.Combine(backupFolder, fileName);

        ZipFile.CreateFromDirectory(
            modsPath,
            zipPath,
            CompressionLevel.Optimal,
            includeBaseDirectory: true
        );

        return true;
    }

    public bool ExportProfile(string profileName, string exportZipPath)
    {
        var modsPath = GetProfileModsPath(profileName);

        if (string.IsNullOrWhiteSpace(modsPath))
            return false;

        if (!Directory.Exists(modsPath))
            return false;

        if (File.Exists(exportZipPath))
            File.Delete(exportZipPath);

        using var archive = ZipFile.Open(exportZipPath, ZipArchiveMode.Create);

        var files = Directory.GetFiles(
            modsPath,
            "*",
            SearchOption.AllDirectories
        );

        foreach (var file in files)
        {
            if (file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".rar", StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".7z", StringComparison.OrdinalIgnoreCase))
                continue;

            var relativePath = Path.GetRelativePath(modsPath, file);

            archive.CreateEntryFromFile(
                file,
                relativePath,
                CompressionLevel.Optimal
            );
        }

        var activeModList = BuildActiveModListText(modsPath);

        var entry = archive.CreateEntry("active_mods.txt");

        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, Encoding.UTF8);

        writer.Write(activeModList);

        return true;
    }

    private string BuildActiveModListText(string modsPath)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Active Mod List");
        sb.AppendLine($"Exported At: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        var folders = Directory.GetDirectories(modsPath)
            .Where(path => !Path.GetFileName(path).StartsWith("."))
            .OrderBy(path => Path.GetFileName(path));

        foreach (var folder in folders)
        {
            var folderName = Path.GetFileName(folder);
            var manifestPath = Path.Combine(folder, "manifest.json");

            if (!File.Exists(manifestPath))
            {
                sb.AppendLine($"- {folderName}");
                continue;
            }

            try
            {
                var manifest = new ModReader().Read(manifestPath);

                if (manifest == null)
                {
                    sb.AppendLine($"- {folderName}");
                    continue;
                }

                sb.AppendLine(
                    $"- {manifest.Name} / {manifest.Version} / {manifest.UniqueID}"
                );
            }
            catch
            {
                sb.AppendLine($"- {folderName}");
            }
        }

        return sb.ToString();
    }
}