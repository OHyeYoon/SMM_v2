namespace SMM.Desktop.Services.Mods;

public sealed class ModToggleService
{
    public bool Toggle(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            return false;

        if (!Directory.Exists(folderPath))
            return false;

        var directory = new DirectoryInfo(folderPath);
        var parentPath = directory.Parent?.FullName;

        if (string.IsNullOrWhiteSpace(parentPath))
            return false;

        var currentName = directory.Name;

        var targetName = currentName.StartsWith(".")
            ? currentName.TrimStart('.')
            : "." + currentName;

        var targetPath = Path.Combine(parentPath, targetName);

        if (Directory.Exists(targetPath))
            return false;

        Directory.Move(folderPath, targetPath);

        return true;
    }
}